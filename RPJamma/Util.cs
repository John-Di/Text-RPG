using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Data.SQLite;
using System.Data;

namespace RPJamma
{
    public class Battle
    {
        /// <summary>
        /// Two parties engage in a battle.
        /// </summary>
        /// <param name="p1">Party 1</param>
        /// <param name="p2">Party 2</param>
        public static void engage(Party p1, Party p2)
        {
            do
            {
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Text.color("Your party:", ConsoleColor.Cyan);
                Console.WriteLine("  " + p1);
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Text.color("Enemy party:", ConsoleColor.Red); 
                Console.WriteLine(" " + p2);


                Battle.battleCommands(p1, p2);
                Battle.battleCommands(p2, p1);

                Battle.attackSetup(Battle.arrangeAttackOrder(p1, p2));

                Console.WriteLine();

            }
            while (p1.getTotalHP() > 0 && p2.getTotalHP() > 0);
        }

        /// <summary>
        /// All members of a party are given commands. If all terms are met, the members are given a target.
        /// </summary>
        /// <param name="attacking">Attacking Party</param>
        /// <param name="receiver">Party being attacked</param>
        private static void battleCommands(Party attacking, Party receiver)
        {
            foreach (IEngageable member in attacking)
            {
                string target = "";

                if (!member.isDead())
                {
                    Party enemy = receiver;

                    if (member is Cleric)
                    {
                        receiver = attacking;
                    }

                    do
                    {
                        target = attackCommand(member, receiver);
                    }
                    while (receiver.getMember(target) == null || receiver.getMember(target).isDead());

                    member.setTarget(receiver.getMember(target));
                    receiver = enemy;
                }
            }
        }

        /// <summary>
        /// Decision is made whether this party is the user's party or an AI party before target select phase.
        /// </summary>
        /// <param name="member">Party memeber currently seeking an attack target.</param>
        /// <param name="receiver">Enemy party</param>
        /// <returns>string: Target's name.</returns>
        private static string attackCommand(IEngageable member, Party receiver)
        {
            if (member.getParty().isUser())
            {
                return playerCommandSelect(member, receiver);
            }
            else
            {
                return aiTargetSelect(receiver);
            }
        }

        /// <summary>
        /// The player is prompted to select an enemy party member to attack for their current member's turn.
        /// </summary>
        /// <param name="member">Player's current attacking member.</param>
        /// <param name="receiver">Enemy Party</param>
        /// <returns>The chosen target's name.</returns>
        /// <remarks>If the chosen target is dead or does not exist, an appropriate message is displayed.</remarks>
        /// <remarks>If the user's presses enter with no input, a randomly select target's name is returned.</remarks>
        private static string playerCommandSelect(IEngageable member, Party receiver)
        {
            string target;
            Console.WriteLine();

            Console.WriteLine("Please select a target for " + member);
            target = Console.ReadLine();

            if (target.Equals("") || target.Equals(null) || target.Trim().Equals(""))
            {
                if (member.getTarget() != null)
                {
                    target = member.getTarget().getName();
                }
                else
                {
                    target = receiver.getMember().getName();
                }
            }
            if (receiver.getMember(target) == null)
            {
                Console.WriteLine(target);


                Text.color("\n" + target + " is not a valid target.", ConsoleColor.Red); 
            }
            else if (receiver.getMember(target).isDead())
            {
                if(!(member is Cleric))
                {
                    Text.color("\n" + target + " is already dead. You're a horrible person!", ConsoleColor.Red);
                }
                else
                {
                    Text.color("\nYou cannot resurrect " + target + ". I'm sorry.", ConsoleColor.Red);
                }
            }

            return target;
        }

        /// <summary>
        /// AI Party selects a random enemy party member.
        /// </summary>
        /// <param name="receiver">Enemy party</param>
        /// <returns>string: Randomly selected enemy's name.</returns>
        private static string aiTargetSelect(Party receiver)
        {
            return receiver.getMemberName();
        }

        /// <summary>
        /// Combines and orders two parties by highest speed first.
        /// </summary>
        /// <param name="p1">Party 1</param>
        /// <param name="p2">Party 2</param>
        /// <returns>A HashSet of combined IEngageables from both parties sorted by descending speed.</returns>
        private static HashSet<IEngageable> arrangeAttackOrder(HashSet<IEngageable> p1, HashSet<IEngageable> p2)
        {
            HashSet<IEngageable> bothParties = new HashSet<IEngageable>();

            IEnumerable<IEngageable> concat = p1.Concat(p2).OrderByDescending(member => member.getSpeed());

            foreach (IEngageable member in concat)
            {
                bothParties.Add(member);
            }

            return bothParties;
        }

        /// <summary>
        /// Members of both parties fufill their commands.
        /// </summary>
        /// <param name="engagers"></param>
        /// <remarks>Any member attacking a dead target is redirected to a random living enemy target</remarks>
        private static void attackSetup(HashSet<IEngageable> engagers)
        {
            foreach (IEngageable attacker in engagers)
            {
                IEngageable receiver = attacker.getTarget();

                if (!attacker.isDead() && !receiver.isDead())
                {
                    Battle.attack(attacker, receiver);

                    if (receiver.isDead())
                    {
                        Console.WriteLine();
                        Text.color(receiver.ToString() + ": ", Text.allianceColor(receiver));
                        Text.color(Engageable.deathCry(), ConsoleColor.DarkGray);
                        Text.userRead();
                    }
                }
                else if (!attacker.isDead() && receiver.isDead())
                {
                    if (!receiver.getParty().isDefeated())
                    {
                        IEngageable newTarget;

                        do
                        {
                            newTarget = receiver.getParty().getMember();
                        }
                        while (newTarget.isDead());

                        Battle.attack(attacker, newTarget);
                    }
                }
            }
        }

        /// <summary>
        /// Damage is dealt from one Fighter to another.
        /// </summary>
        /// <param name="attacker">Attacking Fighter</param>
        /// <param name="receiver">Defending Fighter</param>
        private static void attack(IEngageable attacker, IEngageable receiver)
        {
            Console.WriteLine();
            Text.color(attacker.ToString(), Text.allianceColor(attacker));
            if (!(attacker is Cleric))
            {
                Console.Write(" attacks ");
            }
            else
            { 
                Console.Write(" heals "); 
            }

            Text.color(
                    ((receiver.getName().Equals(attacker.getName())) ? "themselves" : receiver.getName()) + "\n", Text.allianceColor(receiver));


            int newHP = (!(attacker is Cleric)) ? 
                (receiver.getHP() - damage(attacker.getAttack(), receiver.getDefense(), attacker.getCritChance())) :
                (receiver.getHP() + damage(attacker.getAttack(), 0, 0));

            if (newHP <= 0)
            {
                newHP = 0;
            }
            if (newHP >= receiver.getMaxHP())
            {
                newHP = receiver.getMaxHP();
            }

            Text.color(receiver.ToString(), Text.allianceColor(receiver));            
            Console.Write("'s health goes from ");
            Text.color(receiver.getHP().ToString(), Text.healthColor(receiver));
            Console.Write(" to ");
            receiver.setHP(newHP);
            Text.color(newHP.ToString(), Text.healthColor(receiver));

            if (attacker.getParty().isUser())
            {
                Console.WriteLine();
                Console.Write(attacker + " earned ");
                Text.color(attacker.earnEXP(receiver).ToString(), ConsoleColor.Yellow);
            }


            Text.userRead();
        }

        /// <summary>
        /// Calculates damage to be dealt
        /// </summary>
        /// <param name="attack">Base attack damage</param>
        /// <param name="defense">Base defense stat</param>
        /// <returns>int: Base attack +- 10% * a possible 1.5 factor.
        /// If the defense stat is greater than this value, it returns 0</returns>
        private static int damage(int attack, int defense, int crit)
        {
            Random random = new Random();

            int TEN_PERCENT = (int)(attack * 0.1);

            //System.Threading.Thread.Sleep(1000);
            int difference = RandomPlus.genIntInc(-TEN_PERCENT, TEN_PERCENT);

            //System.Threading.Thread.Sleep(1000);
            double critValue;

            if (RandomPlus.genBoolean(crit))
            {
                critValue = 1.5;
                Text.color(Engageable.criticalCry() + "\n", ConsoleColor.Yellow);
                Text.color("Critical Hit!\n", ConsoleColor.Magenta);
            }
            else
            {
                critValue = 1.0;
                string cry = "";

                if (crit != 0)
                {
                    Text.color(Engageable.cry() + "\n", ConsoleColor.Yellow);
                }
                else
                {
                    Text.color(Engageable.supportCry() + "\n", ConsoleColor.Green);
                }
            }

            int damage = (int)((attack + difference) * critValue);
            //Console.WriteLine("From (" + attack + " + " + difference + ") to " + damage + " damage - " + defense + " = " + (damage - defense));

            return (defense < attack) ? damage - defense: 1;
        }

        /// <summary>
        /// Two Fighters engage in a battle where both parties attack each other. The first to attack is determined by highest speed
        /// </summary>
        /// <param name="one">The first Fighter to strike</param>
        /// <param name="two">The second Fighter to strike (if not defeated this round)</param>
        private static void attackAndReceive(IEngageable one, IEngageable two)
        {
            arrangeAttackOrder(one, two);

            attack(one, two);

            if (two.getHP() > 0)
            {
                attack(two, one);
            }
        }

        /// <summary>
        /// Determines the first to attack between two Fighters.
        /// </summary>
        /// <param name="one">Fighter one</param>
        /// <param name="two">Fighter two</param>
        private static void arrangeAttackOrder(IEngageable one, IEngageable two)
        {
            if (one.getSpeed() < two.getSpeed() || (one.getSpeed() == two.getSpeed() && RandomPlus.genBoolean()))
            {
                IEngageable temp = one;
                one = two;
                two = temp;
            }
        }

    }

    public static class Text
    {
        /// <summary>
        /// Displays a message with a ">>>" sequence to allow the user time to read and waits for a key to be pressed before continuing.
        /// </summary>
        /// <param name="message">Message to be read by the user.</param>
        public static void userRead()
        {
            Console.WriteLine(" >>>");
            Console.ReadKey();
        }


        /// <summary>
        /// Changes the color of the text output to the console.
        /// </summary>
        /// <param name="message">Message to be colored.</param>
        public static void color(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static ConsoleColor allianceColor(IEngageable engageable)
        {
            return (engageable.getParty().isUser()) ? ConsoleColor.Cyan : ConsoleColor.Red;
        }

        public static ConsoleColor healthColor(IEngageable engageable)
        {
            if (engageable.getHP() == 0)
            {
                return ConsoleColor.DarkRed;
            }
            else if (engageable.getHP() < engageable.getMaxHP() * (1.0 / 3.0))
            {
                return ConsoleColor.Red;
            }
            else if (engageable.getHP() < engageable.getMaxHP() * (2.0 / 3.0))
            {
                return ConsoleColor.Yellow;
            }
            else
            {
                return ConsoleColor.Green;
            }
        }

    }

    public static class RandomPlus
    {
        private static Random random = new Random();
        private static string[] names = {
                                                     "Raven", "Zona", "Angelita", "Neida", "Leeanna", "Doran", "Sean", "Charlie", "Herbert", "Yelena", 
                                                     "Opal", "Sherwood", "Camila", "Glennie", "Denis", "Naida", "Jesse", "Tessie", "Talitha", "Grady", 
                                                     "Hector", "Edwardo", "Marybelle", "Carlo", "Weston", "Gregg", "Caroyln", "Mira", "Milly", "Cristen", 
                                                     "Antonette", "Keeley", "Ling", "Diego", "Anja", "Myron", "Micaela", "Latisha", "Darryl", "Margery", 
                                                     "Karren", "Meghan", "Reginald", "Casey", "Lessie", "Coy", "Tameika", "Carolina", "Alonzo", "Camie"
                                        };
        /// <summary>
        /// Randomly generates 0 or 1.
        /// </summary>
        /// <returns>Boolean: True if the value is 1, false if 0</returns>
        public static bool genBoolean()
        {
            return random.Next(2) == 1;
        }

        /// <summary>
        /// Generates a random boolean given a percent success rate
        /// </summary>
        /// <param name="percent">Percent for success</param>
        /// <returns></returns>
        public static bool genBoolean(int percent)
        {
            return (percent == 100) ? true : RandomPlus.genInt(100) < percent;
        }

        /// <summary>
        /// Randomly generates a positive integer
        /// </summary>
        /// <returns>int</returns>
        public static int genInt()
        {
            return random.Next();
        }
        /// <summary>
        /// Randomly generates a positive integer given an exclusive upperbound
        /// </summary>
        /// <param name="max">Exclusive Upperbound</param>
        /// <returns>int</returns>
        public static int genInt(int max)
        {
            return random.Next(max);
        }
        /// <summary>
        /// Randomly generates a positive integer given an inclusive lowerbound and exclusive upperbound
        /// </summary>
        /// <param name="min">Incusive Lowerbound</param>
        /// <param name="max">Exclusive Upperbound</param>
        /// <returns>int</returns>
        public static int genInt(int min, int max)
        {
            return random.Next(min, max);
        }
        /// <summary>
        /// Randomly generates a positive integer given an inclusive upperbound
        /// </summary>
        /// <param name="max">Inclusive Upper Bound</param>
        /// <returns>int</returns>
        public static int genIntInc(int max)
        {
            return genInt(max + 1);
        }
        /// <summary>
        /// Randomly generates a positive integer given an inclusive lowerbound and inclusive upperbound
        /// </summary>
        /// <param name="min">Incusive Lowerbound</param>
        /// <param name="max">Incusive Upperbound</param>
        /// <returns>int</returns>
        public static int genIntInc(int min, int max)
        {
            return genInt(min, max + 1);
        }

        /// <summary>
        /// Randomly generates a value between 0.0 and 1.0
        /// </summary>
        /// <returns>double</returns>
        public static double genTenth()
        {
            return random.NextDouble();
        }
        /// <summary>
        /// Generates a value between 0.0 and 1.0 given a max in this interval
        /// </summary>
        /// <param name="max">Maximum value (between 0.0 and 1.0)</param>
        /// <returns>double</returns>
        public static double genTenth(double max)
        {
            return genIntInc((int)(max * 10)) / 10.0;
        }
        /// <summary>
        /// Randomly generates a positive double value
        /// </summary>
        /// <returns>double</returns>
        public static double genDouble()
        {
            return genInt() + genTenth();
        }
        /// <summary>
        /// Randomly generates a positive double value given an inclusive integer upperbound
        /// </summary>
        /// <param name="max">Exclusive int Upperbound</param>
        /// <returns>double</returns>
        public static double genDouble(int max)
        {
            return genInt(max) + genTenth();
        }
        /// <summary>
        /// Randomly generates a positive double value given an inclusive double upperbound
        /// </summary>
        /// <param name="max">Exclusive double Upperbound</param>
        /// <returns>double</returns>
        public static double genDouble(double max)
        {
            int rounded = (int)max;
            double fraction = 1.0 - Math.Abs(rounded - max);

            return genIntInc(rounded) + genTenth(fraction);
        }

        /// <summary>
        /// Randomly chooses a first name from a list of predetermined first names.
        /// </summary>
        /// <returns>string of a first name.</returns>
        public static string genFirstName()
        {
            string cs = "URI=file:names.db";

            using (SQLiteConnection sqlite = new SQLiteConnection(cs))
            {
                SQLiteDataAdapter adapter;
                DataTable dt = new DataTable();
                sqlite.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(sqlite))
                {
                    cmd.CommandText = "SELECT Id FROM Names";  //set the passed query
                    adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dt); //fill the datasource

                    int size = dt.Rows.Count;

                    cmd.CommandText = "SELECT Name FROM Names where Id = " + RandomPlus.genIntInc(1, size);
                    cmd.CommandType = CommandType.Text;
                    string name = cmd.ExecuteScalar().ToString();

                    return name;
                }
            }


            //HtmlWeb htmlWeb = new HtmlWeb();
            //HtmlDocument doc;
            //string node = "";

            //try
            //{
            //    doc = htmlWeb.Load("http://onerandomname.com/");
            //    node = "//div[@id='randomname']"; 

            //    if (doc.DocumentNode != null)
            //    {
            //        string name = (doc.DocumentNode.SelectSingleNode(node).InnerHtml).Split(' ')[0];

            //        List<string> temp = names.ToList();

            //        if (!names.Contains(name))
            //        {
            //            temp.Add(name);
            //            names = temp.ToArray();
            //        }

            //        return name;
            //    }
            //}
            //catch (Exception e) { e.ToString(); }
            //finally
            //{
            //    doc = htmlWeb.Load("http://www.namegenerator.biz/random-name-generator.php");
            //    node = "//strong[@id='rname']";
            //}


            //if (doc.DocumentNode != null)
            //{
            //    string name = (doc.DocumentNode.SelectSingleNode(node).InnerHtml).Split(' ')[0];
                
            //    List<string> temp = names.ToList();

            //    if (!names.Contains(name))
            //    {
            //        temp.Add(name);
            //        names = temp.ToArray();
            //    }

            //    return name;
            //}

            return names[genInt(names.Length)];
        }
    }

    public class DatabaseHelper
    {
        string db;
        SQLiteConnection connection;
        SQLiteCommand cmd;

        private delegate void QueryDelegate();

        public DatabaseHelper(string path)
        {
            db = path;
        }

        public string getDBPath()
        {
            return db;
        }

        public void insert(string tableName, string[] parametersList)
        {            
            QueryDelegate insertDel = delegate()
            {
                insertQuery(tableName, parametersList);
            };
                
            connect(insertDel);
        }

        public void select(string columnName, string tableName, string[] constraints)
        {
            QueryDelegate selectDel = delegate()
            {
                selectQuery(columnName, tableName, constraints);
            };

            connect(selectDel);
        }

        public void create(string tableName, string[] parametersList, bool exists)
        {
            QueryDelegate createDel = delegate()
            {
                createQuery(tableName, parametersList, exists);
            };

            connect(createDel);
        }

        public void connect(Delegate query)
        {
            using (connection = new SQLiteConnection(db))
            {
                connection.Open();

                using (cmd = new SQLiteCommand(connection))
                {
                    query.DynamicInvoke();
                }
            }
        }

        private void insertQuery(string tableName, string[] parametersList)
        {
            string parameters = String.Join(",", parametersList);

            cmd.CommandText = "INSERT INTO " + tableName + " VALUES(" + parameters + ")";
            cmd.ExecuteNonQuery();
        }

        
        private void selectQuery(string columnName, string tableName, string[] constraintsList)
        {
            string constraints = String.Join(",", constraintsList);

            cmd.CommandText = "SELECT " + columnName + " FROM " + tableName + constraints;  //set the passed query
        }

        private void createQuery(string tableName, string[] parametersList, bool exists)
        {
            string tableExists = (!exists) ? "IF NOT EXISTS" : "",
                   parameters = String.Join(",", parametersList);

            cmd.CommandText = @"CREATE TABLE " + tableExists + " " + tableName + "(" + parameters + ")";
            cmd.ExecuteNonQuery();
        }
    }


}
