using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPJamma
{
    public class Party : HashSet<IEngageable>, IPrintable
    {
        private bool isPlayer;

        public Party() : this(false) { }

        public Party(bool isUser) 
        {
            isPlayer = isUser;
        }

        public Party(IEngageable fighter, bool isUser)
        {
            this.isPlayer = isUser;
            addMember(fighter);
        }

        public Party(HashSet<IEngageable> party, bool isUser)
        {
            this.isPlayer = isUser;
            addParty(party);
        }

        public Party(Party party, bool isUser)
        {
            this.isPlayer = isUser;
            addParty(party);
        }

        public Party(Party p1, Party p2, bool isUser)
        {
            this.isPlayer = isUser;
            addParty(p1);
            addParty(p2);
        }

        public Party(IEnumerable<Party> parties, bool isUser)
        {
            this.isPlayer = isUser;
            foreach (Party party in parties)
            {
                addParty(party);
            }
        }

        /// <summary>
        /// Adds a new member to the party.
        /// </summary>
        /// <param name="newMember">New member to be added.</param>
        public void addMember(IEngageable newMember)
        {
            Add(newMember);
            newMember.setParty(this);
        }

        /// <summary>
        /// Adds an already formed party to this party.
        /// </summary>
        /// <param name="party">Party to be added.</param>
        public void addParty(HashSet<IEngageable> party)
        {
            foreach (IEngageable member in party)
            {
                addMember(member);
            }
        }

        public void addParty(Party party)
        {
            foreach (IEngageable member in party)
            {
                addMember(member);
            }
        }
        
        /// <summary>
        /// Sums up the HP of all members in a Party
        /// </summary>
        /// <returns>int: total HP of party members</returns>
        public int getTotalHP()
        {
            int totalHP = 0;

            foreach (IEngageable member in this)
            {
                totalHP += member.getHP();
            }

            return totalHP;
        }

        /// <summary>
        /// Determines if all members in this party have 0 HP.
        /// </summary>
        /// <returns>bool</returns>
        public bool isDefeated()
        {
            return getTotalHP() == 0;
        }

        /// <summary>
        /// Get a member of a party by name
        /// </summary>
        /// <param name="name">Name of member to fetch</param>
        /// <returns>IEngageable: If the member is in the party, returns the member, otherwise returns null</returns>
        public IEngageable getMember(string name)
        {
            if (Contains(name))
            {
                foreach (IEngageable member in this)
                {
                    if (member.getName().Equals(name))
                    {
                        return member;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Randomly fetches a member from this by name.
        /// </summary>
        /// <returns>string: Member's name of this Party</returns>
        public string getMemberName()
        {
            return getMember().getName();
        }

        /// <summary>
        /// Randomly fetches a member from this.
        /// </summary>
        /// <returns>IEngageable: member of this</returns>
        public IEngageable getMember()
        {
            IEngageable[] members = this.ToArray();

            return members[RandomPlus.genInt(members.Length)];
        }

        /// <summary>
        /// Sorts the party members by highest speed first
        /// </summary>
        /// <returns>Returns a Party of members sorted by descending speed values</returns>
        public HashSet<IEngageable> sortBySpeed()
        {
            HashSet<IEngageable> sortedParty = new HashSet<IEngageable>();
            
            IEnumerable<IEngageable> sorted = this.OrderByDescending(member => member.getSpeed());

            foreach (IEngageable member in sorted)
            {
                sortedParty.Add(member);
            }

            return sortedParty;
        }

        /// <summary>
        /// Checks if a fighter is in the party
        /// </summary>
        /// <param name="fighter">Fighter to be searched</param>
        /// <returns>bool</returns>
        public bool Contains(string fighter)
        {
            List<string> members = new List<string>();

            foreach (IEngageable member in this)
            {
                members.Add(member.getName());
            }

            return members.Contains(fighter);
        }

        /// <summary>
        /// Displays members of a party coupled with their current HP
        /// </summary>
        /// <returns>string format "[name]/[hp]"</returns>
        public override string ToString()
        {
            string members = "";

            foreach (IEngageable member in this)
            {
                members += member.getClass() + "/" + member.getName() + "/" + member.getHP() + "hp  " ;
            }

            return members;
        }

        /// <summary>
        /// If this party belongs to the user.
        /// </summary>
        /// <returns>bool</returns>
        public bool isUser()
        {
            return isPlayer;
        }
    }

    public abstract class Engageable : IEngageable, IEquatable<Engageable>, IPrintable
    {
        protected int level, attack, defense, speed, hp, maxHP, accuracy, crit, exp;
        protected string name;
        protected Party party;
        protected IEngageable target;

        protected static readonly string[] 
                            
                                     CRIES = {
                                                  "Take this!",
                                                  "How do you like this one!",
                                                  "Give up already!",
                                                  "You'll never prevail!",
                                                  "Hold Still!",
                                                  "Forfeit!",
                                                  "You're wasting your time!",
                                                  "You bore me..."
                                             },

                           SUPPORT_CRIES = {
                                                  "Here you go!",
                                                  "Hope this helps!",
                                                  "Hang in there!",
                                                  "Don't give up!",
                                                  "You're doing great!",
                                                  "Keep it up!",
                                                  "Be careful!",
                                                  "We need you alive!"
                                           },

                           CRITICAL_STRIKE = {
                                                  "You're finished!",
                                                  "This is the end!",
                                                  "This is the end for you!",
                                                  "You're out of luck!",
                                                  "Say GoodBye!",
                                                  "You lose!"
                                             },

                                    DEATH =  {
                                                 "This... This is it...",
                                                 "I'm... done.",
                                                 "How... How could I... lose.",
                                                 "I... failed.",
                                                 "How... How could this... be.",
                                                 "Gahhhhh!"
                                             };

        //public Engageable() : this("Bob", 10, 10, 10, 100) { }

        public Engageable(string name, int level, int attack, int defense, int speed, int hp, int acc, int crit)
        {
            this.name = name;
            this.attack = attack;
            this.defense = defense;
            this.speed = speed;
            this.hp = hp;
            this.maxHP = hp;
            this.accuracy = acc;
            this.crit = crit;
            this.level = level;
        }

        public void setName(string name)
        {
            this.name = name;
        }
        public string getName()
        {
            return name;
        }

        public int getHP()
        {
            return hp;
        }
        public void setHP(int hp)
        {
            this.hp = hp;
        }

        public int getMaxHP()
        {
            return maxHP;
        }
        public void setMaxHP(int maxHP)
        {
            this.maxHP = maxHP;
        }

        public int getLevel()
        {
            return level;
        }
        public void setLevel(int level)
        {
            this.level = level;
        }
        public void incLevel()
        {
            ++level;
            incStats();
            Console.WriteLine("LEVEL UP!");
        }
        public void incLevel(int level)
        {
            for (int i = 1; i <= level; i++)
            {
                incLevel();
            }
        }

        public int getAttack()
        {
            return attack;
        }
        public void setAttack(int attack)
        {
            this.attack = attack;
        }

        public int getDefense()
        {
            return defense;
        }
        public void setDefense(int attack)
        {
            this.attack = attack;
        }

        public int getSpeed()
        {
            return speed;
        }
        public void setSpeed(int attack)
        {
            this.attack = attack;
        }

        public int getAccuracy()
        {
            return accuracy;
        }
        public void setAccuracy(int accuracy)
        {
            this.accuracy = accuracy;
        }

        public int getCritChance()
        {
            return crit;
        }
        public void setCritChance(int crit)
        {
            this.crit = crit;
        }

        public string getClass()
        {
            return this.GetType().Name;
        }

        public int getEXP()
        {
            return exp;
        }
        public void setEXP(int exp)
        {
            this.exp = exp;
        }
        public void gainEXP(int exp)
        {
            int totalEXP = this.exp + exp;

            if (totalEXP >= Math.Pow(Math.E, getLevel()) * 5)
            {
                incLevel();
            }

            setEXP(totalEXP % 100);
        }
        public int earnEXP(IEngageable target)
        {
            int exp;

            if(target.isDead())
            {
                exp = (int)Math.Pow(Math.E, target.getLevel());
            }
            else
            {
                exp = (int)(Math.Pow(Math.E, target.getLevel())/10);
            }
            
            gainEXP(exp);

            return exp;
        }

        public abstract void incStats();

        public Party getParty()
        {
            return party;
        }
        public void setParty(Party party)
        {
            this.party = party;
        }

        public IEngageable getTarget()
        {
            return target;
        }
        public void setTarget(IEngageable target)
        {
            this.target = target;
        }

        public bool isDead()
        {
            return hp == 0;
        }

        public string getStats()
        {
            return "Name:  " + name       + "\n" +
                   "Class: " + getClass() + "\n" +
                   "Level: " + level      + "\n" +
                   "Hp:    " + maxHP      + "\n" +
                   "Att:   " + attack     + "\n" +
                   "Def:   " + defense    + "\n" +
                   "Spd:   " + speed      + "\n";                   
        }
        
        public static string cry()
        {
            return "\"" + CRIES[RandomPlus.genInt(CRIES.Length)] + "\"";
        }
        public static string criticalCry()
        {
            return "\"" + CRITICAL_STRIKE[RandomPlus.genInt(CRITICAL_STRIKE.Length)] + "\"";
        }
        public static string deathCry()
        {
            return "\"" + DEATH[RandomPlus.genInt(DEATH.Length)] + "\"";
        }
        public static string supportCry()
        {
            return "\"" + SUPPORT_CRIES[RandomPlus.genInt(SUPPORT_CRIES.Length)] + "\"";
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is Engageable))
            {
                return false;
            }

            Engageable casted = (Engageable)obj;

            return Equals(casted);
        }
        public bool Equals(Engageable fighter)
        {
            return attack == fighter.getAttack()
                && defense == fighter.getDefense()
                && speed == fighter.getSpeed()
                && name == fighter.getName();
        }

        public override string ToString()
        {
            return name;
        }

    }

    public class Swordsman : Engageable
    {
        public Swordsman() 
            : this(RandomPlus.genFirstName(), 1, RandomPlus.genIntInc(75, 85), RandomPlus.genIntInc(55, 65), RandomPlus.genIntInc(55, 65), RandomPlus.genIntInc(90, 100), 90, 10){ }

        public Swordsman(int level)
            : this(RandomPlus.genFirstName(), level, RandomPlus.genIntInc(75, 85), RandomPlus.genIntInc(55, 65), RandomPlus.genIntInc(55, 65), RandomPlus.genIntInc(90, 100), 90, 10)
        {
            this.level = level;
        }

        public Swordsman(string name, int level)
            : this(name, level, RandomPlus.genIntInc(75, 85), RandomPlus.genIntInc(55, 65), RandomPlus.genIntInc(55, 65), RandomPlus.genIntInc(90, 100), 90, 10)
        {
            this.name = name;
            this.level = level;
        }

        public Swordsman(string name, int level, int attack, int defense, int speed, int hp, int acc, int crit) 
            : base(name, level, attack, defense, speed, hp, acc, crit) { }

        public override void incStats()
        {
            attack += RandomPlus.genIntInc(1,3);
            defense += RandomPlus.genIntInc(1, 3);
            speed += RandomPlus.genIntInc(1, 2);
            int moreHP = RandomPlus.genIntInc(5,10);
            hp += moreHP;
            maxHP += moreHP;
        }
    }

    public class Cleric : Engageable
    {
        public Cleric()
            : this(RandomPlus.genFirstName(), 1, RandomPlus.genIntInc(25, 35), RandomPlus.genIntInc(35, 45), RandomPlus.genIntInc(35, 45), RandomPlus.genIntInc(70, 80), 100, 0) { }

        public Cleric(int level)
            : this(RandomPlus.genFirstName(), level, RandomPlus.genIntInc(25, 35), RandomPlus.genIntInc(35, 45), RandomPlus.genIntInc(35, 45), RandomPlus.genIntInc(70, 80), 100, 0)
        {
            this.level = level;
        }

        public Cleric(string name, int level)
            : this(name, level, RandomPlus.genIntInc(25, 35), RandomPlus.genIntInc(35, 45), RandomPlus.genIntInc(35, 45), RandomPlus.genIntInc(70, 80), 100, 0)
        {
            this.name = name;
            this.level = level;
        }

        public Cleric(string name, int level, int attack, int defense, int speed, int hp, int acc, int crit)
            : base(name, level, attack, defense, speed, hp, acc, crit) { }

        public override void incStats()
        {
            attack += RandomPlus.genIntInc(7, 10);
            defense += RandomPlus.genIntInc(1, 2);
            speed += RandomPlus.genIntInc(1, 2);
            int moreHP = RandomPlus.genIntInc(5, 10);
            hp += moreHP;
            maxHP += moreHP;
        }
    }

    public class Tank : Engageable
    {
        public Tank()
            : this(RandomPlus.genFirstName(), 1, RandomPlus.genIntInc(65, 75), RandomPlus.genIntInc(65, 75), RandomPlus.genIntInc(25, 35), RandomPlus.genIntInc(100, 120), 60, 10) { }

        public Tank(int level)
            : this(RandomPlus.genFirstName(), level, RandomPlus.genIntInc(65, 75), RandomPlus.genIntInc(65, 75), RandomPlus.genIntInc(25, 35), RandomPlus.genIntInc(100, 120), 60, 10)
        {
            this.level = level;
        }

        public Tank(string name, int level)
            : this(name, level, RandomPlus.genIntInc(65, 75), RandomPlus.genIntInc(65, 75), RandomPlus.genIntInc(25, 35), RandomPlus.genIntInc(100, 120), 60, 10)
        {
            this.name = name;
            this.level = level;
        }

        public Tank(string name, int level, int attack, int defense, int speed, int hp, int acc, int crit)
            : base(name, level, attack, defense, speed, hp, acc, crit) { }

        public override void incStats()
        {
            attack += RandomPlus.genIntInc(1, 3);
            defense += RandomPlus.genIntInc(5, 10);
            speed += RandomPlus.genIntInc(0, 1);
            int moreHP = RandomPlus.genIntInc(8, 12);
            hp += moreHP;
            maxHP += moreHP;
        }
    }

    public class Assassin : Engageable
    {
        public Assassin()
            : this(RandomPlus.genFirstName(), 1, RandomPlus.genIntInc(85, 95), RandomPlus.genIntInc(45, 55), RandomPlus.genIntInc(75, 85), RandomPlus.genIntInc(60, 70), 90, 20) { }

        public Assassin(int level)
            : this(RandomPlus.genFirstName(), level, RandomPlus.genIntInc(85, 95), RandomPlus.genIntInc(45, 55), RandomPlus.genIntInc(75, 85), RandomPlus.genIntInc(60, 70), 90, 20)
        {
            this.level = level;
        }

        public Assassin(string name, int level)
            : this(name, level, RandomPlus.genIntInc(85, 95), RandomPlus.genIntInc(45, 55), RandomPlus.genIntInc(75, 85), RandomPlus.genIntInc(60, 70), 90, 20)
        {
            this.name = name;
            this.level = level;
        }

        public Assassin(string name, int level, int attack, int defense, int speed, int hp, int acc, int crit)
            : base(name, level, attack, defense, speed, hp, acc, crit) { }

        public override void incStats()
        {
            attack += RandomPlus.genIntInc(1, 3);
            defense += RandomPlus.genIntInc(0, 2);
            speed += RandomPlus.genIntInc(5, 7);
            int moreHP = RandomPlus.genIntInc(5, 10);
            hp += moreHP;
            maxHP += moreHP;
        }
    }
}
