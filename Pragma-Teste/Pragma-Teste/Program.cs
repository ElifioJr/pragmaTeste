using System.Text;
using System.Text.Json;

namespace SerializeExtra
{
    public class Game
    {
        public int game { get; set; }
        public Status? status { get; set; }
    }
    public class Status
    {
        public int total_kills { get; set; }
        public IList<Player>? players { get; set; }
    }
    public class Player
    {
        public int id { get; set; }
        public string nome { get; set; }
        public int kills { get; set; }
        public List<string>? old_names { get; set; }

        public Player(int Id, string Nome, int Kills)
        {
            id = Id;
            nome = Nome;
            kills = Kills;
            old_names = new List<string>();
        }
    }
    public class Program
    {
        public static void Main()
        {
            string path = @"Quake.txt";
            // Leitura do arquivo por linha  
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            // Verifica se o arquivo teve exito na leitrua
            int gamecount = 1;
            bool addNext = false;
            IList<Game>? games;
            games = new List<Game>();

            foreach (string line in lines)
            {
                string linhaDados = line.Substring(7);
                // Remocao da hora nas linhas
                if (linhaDados.Contains(":"))
                {
                    // INICIO DO JOGO !
                    if (linhaDados.Substring(0, linhaDados.IndexOf(":")).Equals("InitGame"))
                    {
                        games.Add(new Game
                        {
                            game = gamecount,
                            status = new Status
                            {
                                total_kills = 0,
                                players = new List<Player>()
                            },
                        });
                    }
                    // Condicional que adiciona players
                    else if (linhaDados.Substring(0, linhaDados.IndexOf(":")).Equals("ClientConnect"))
                    {
                        addNext = true;
                    }
                    // Adiciona o jogador
                    else if (addNext)
                    {
                        // verifica se conseguiu pegar o nome inicial do jogador
                        string nomeInicial = linhaDados.Substring(linhaDados.IndexOf("n\\") + 2, linhaDados.IndexOf("\\t") - linhaDados.IndexOf("n\\") - 2);
                        // verifica se cosneguiu pegar o player ID
                        int playerid = int.Parse(linhaDados.Substring(linhaDados.IndexOf("n\\") - 2, 1)) - 1;
                        if (games[gamecount - 1].status.players.Count < playerid)
                        {
                            games[gamecount - 1].status.players.Add(new Player(playerid, nomeInicial, 0));
                        }
                        addNext = false;
                    }
                    // Cond. que contabiliza as mortes
                    else if (linhaDados.Substring(0, linhaDados.IndexOf(":")).Equals("Kill"))
                    {
                        games[gamecount - 1].status.total_kills++;                
                        string killer = linhaDados.Substring(linhaDados.IndexOf(":", 5) + 2, linhaDados.IndexOf("killed") - linhaDados.IndexOf(":", 5) - 3);                     
                        string morto = linhaDados.Substring(linhaDados.IndexOf("killed") + 7, linhaDados.IndexOf("by") - linhaDados.IndexOf("killed") - 7);
                        if (killer.Equals("<world>") || killer.Equals(morto))
                        {
                            // -1 kills para o morto
                            int mortoid = int.Parse(linhaDados.Substring(linhaDados.IndexOf(' ', 7) + 1, 1)) - 1;
                            if (mortoid - 1 >= 0 && mortoid - 1 < games[gamecount - 1].status.players.Count)
                            {
                                games[gamecount - 1].status.players[mortoid - 1].kills -= 1;
                            }
                            else
                            {
                                games[gamecount - 1].status.players[mortoid - 2].kills -= 1;
                            }
                        }
                        else
                        {
                            // +1 kills para o killer
                            int killerid = int.Parse(linhaDados.Substring(linhaDados.IndexOf(' ', 4) + 1, 1)) - 1;
                            games[gamecount - 1].status.players[killerid - 1].kills += 1;
                        }
                    }
                    // Guarda a o nome antigo dos players apos alteracao na lista old_players
                    else if (linhaDados.Substring(0, linhaDados.IndexOf(":")).Equals("ClientUserinfoChanged"))
                    {
                        string playername = "";
                        int playerid = int.Parse(linhaDados.Substring(linhaDados.IndexOf("n\\") - 2, 1)) - 1;
                        if (playerid - 1 >= 0 && playerid - 1 < games[gamecount - 1].status.players.Count)
                        {
                            playername = games[gamecount - 1].status.players[playerid - 1].nome;
                            string newName = linhaDados.Substring(linhaDados.IndexOf("n\\") + 2, linhaDados.IndexOf("\\t") - linhaDados.IndexOf("n\\") - 2);
                            if (!newName.Equals(playername))
                            {
                                games[gamecount - 1].status.players[playerid - 1].old_names.Add(playername);
                                games[gamecount - 1].status.players[playerid - 1].nome = newName;
                            }
                        }
                    }
                    // Contabiliza o quantidade de jogos ao fim de cada partida
                    else if (linhaDados.Substring(0, linhaDados.IndexOf(":")).Equals("ShutdownGame"))
                    {
                        gamecount++;
                    }
                }
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(games, options);
            // Cria um arquivo json contedo o formato em .txt
            File.WriteAllText("Quake.json", jsonString);
            // Imprime o json no console
            Console.WriteLine(jsonString);
        }
    }
}