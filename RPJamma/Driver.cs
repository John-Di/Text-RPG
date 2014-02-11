using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace RPJamma
{
    class Driver
    {
        static void TomVsStan()
        {
            //Swordsman tom = new Swordsman("Tom", 70, 55, 50, 670);
            //Swordsman stan = new Swordsman("Stan", 80, 40, 50, 560);

            //do
            //{
            //    Battle.engage(new Party(tom, false), new Party(stan, false));

            //} while (tom.getHP() > 0 && stan.getHP() > 0);

            Console.WriteLine(Engageable.deathCry());
            Console.ReadKey();
        }

        static Party initPlayerParty()
        {
            Party p1 = new Party(true);

            List<IEngageable> draft = new List<IEngageable>();
            Console.WriteLine("Select 4 Fighters for your party: \n");

            for (int members = 0; members < 10; members++)
            {
                switch (RandomPlus.genIntInc(5))
                {
                    case 0: draft.Add(new Swordsman()); break;
                    case 1: draft.Add(new Assassin()); break;
                    case 2: draft.Add(new Tank()); break;
                    case 3: draft.Add(new Cleric()); break;
                    case 4: draft.Add(new Swordsman()); break;
                    case 5: draft.Add(new Cleric()); break;
                }

                Console.WriteLine((members + 1) + ". \n" + draft[members].getStats());
            }

            for (int i = 0; i < 4; ++i)
            {
                int index = -1;
                string input;
                do
                {
                    Console.WriteLine("Character {0}'s index:", (i + 1));
                    input = Console.ReadLine();

                    try
                    {
                        index = int.Parse(input) - 1;
                    }
                    catch (Exception e)
                    {
                        index = -1;
                    }

                    if (index >= 0 && index < draft.Count)
                    {
                        if (draft.ElementAt(index) == null)
                        {
                            Text.color("You've already selected that character.\n\n", ConsoleColor.Red);
                        }
                    }
                    else
                    {
                        Text.color("Really? '" + input + "'? Hahahahha! -_-' That wasn't funny.\n\n", ConsoleColor.Red);
                        index = -1;
                    }
                }
                while (index == -1 || draft.ElementAt(index) == null);

                p1.addMember(draft.ElementAt(index));
                draft[index] = null;
                Console.WriteLine();
            }

            return p1;
        }

        static void heal(Party p1)
        {
            foreach (IEngageable member in p1)
            {
                member.setHP(member.getMaxHP());
                Console.WriteLine(member.getStats());
            }
        }

        static bool fight(Party p1, Party p2)
        {
            int averageLevel = 0;

            foreach (IEngageable member in p1)
            {
                averageLevel += member.getLevel();
            }

            averageLevel /= p1.Count;

            for (int i = 0; i < 3; i++)
            {
                switch (RandomPlus.genIntInc(5))
                {
                    case 0: p2.addMember(new Swordsman(averageLevel));  break;
                    case 1: p2.addMember(new Assassin(averageLevel));  break;
                    case 2: p2.addMember(new Tank(averageLevel));       break;
                    case 3: p2.addMember(new Tank(averageLevel));       break;
                    case 4: p2.addMember(new Assassin(averageLevel));  break;
                    case 5: p2.addMember(new Swordsman(averageLevel));  break;
                }
            }

            p2.addMember(new Cleric(averageLevel));

            Battle.engage(p1, p2);

            Console.WriteLine("-----------");

            if (!p1.isDefeated())
            {
                Text.color("You win!\n", ConsoleColor.Cyan);
                return true;
            }
            else
            {
                Text.color("You lose...\n", ConsoleColor.Red);
                return false;
            }
        }

        public static void run()
        {
            string answer;
            bool success = false;
            Party p1 = new Party(true),
                  p2 = new Party();

            do
            {
                if (success)
                {
                    heal(p1);
                }
                else
                {
                    p1 = initPlayerParty();
                }
                
                success = fight(p1, p2);

                Console.WriteLine("\nGo another round? (y/n)");
                answer = Console.ReadLine().ToLower();
                Console.WriteLine();

            } while (answer.Equals("y"));

            Console.Write("Until the next time!");
            Text.userRead();
        }

        static void fillDB()
        {
            //ripFromWeb();
            ripFromFile();

            Console.ReadKey();

        }

        static void ripFromWeb()
        {

            string cs = "URI=file:names.db";

            using (SQLiteConnection con = new SQLiteConnection(cs))
            {
                con.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Names(Id INTEGER PRIMARY KEY, Name TEXT UNIQUE ON CONFLICT IGNORE NOT NULL ON CONFLICT IGNORE)";
                    cmd.ExecuteNonQuery();
                    for (int i = 0; i < 30000; i++)
                    {
                        string name = RandomPlus.genFirstName();
                        cmd.CommandText = "INSERT INTO Names VALUES(null,'" + name + "')";
                        cmd.ExecuteNonQuery();
                        Console.WriteLine((i + 1) + ": " + name);
                    }
                }
            }


            //SQLiteConnection sqlite = new SQLiteConnection(cs);
            //SQLiteDataAdapter ad;
            //DataTable dt = new DataTable();

            //try
            //{
            //    SQLiteCommand cmd;
            //    sqlite.Open();  //Initiate connection to the db
            //    cmd = sqlite.CreateCommand();
            //    cmd.CommandText = "SELECT Name FROM Names";  //set the passed query
            //    ad = new SQLiteDataAdapter(cmd);
            //    ad.Fill(dt); //fill the datasource
            //}
            //catch (SQLiteException ex)
            //{
            //    //Add your exception code here.
            //}
            //sqlite.Close();

            //foreach (DataRow row in dt.Select())
            //{
            //    Console.WriteLine(row[0]);
            //}
        }

        static void ripFromFile()
        {
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\johnd\Documents\Visual Studio 2012\Projects\RPJamma\RPJamma\bin\Debug\NAMES.DIC");

            string cs = "URI=file:names.db";

            using (SQLiteConnection con = new SQLiteConnection(cs))
            {
                con.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Names(Id INTEGER PRIMARY KEY, Name TEXT UNIQUE ON CONFLICT IGNORE NOT NULL ON CONFLICT IGNORE)";
                    cmd.ExecuteNonQuery();
                    int i = 0;

                    foreach (string line in lines)
                    {
                        string name = char.ToUpper(line[0]) + line.Substring(1);
                        cmd.CommandText = "INSERT INTO Names VALUES(null,'" + name + "')";
                        cmd.ExecuteNonQuery();
                        Console.WriteLine((i++) + ": " + name);
                    }
                }
            }
            
            
        }

        static void Main(string[] args)
        {
            //TomVsStan();
            run();
            //fillDB();
        }
    }
}
