using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPJamma
{
    public interface IEngageable
    {
        string getName();
        void setName(string name);

        int getHP();
        void setHP(int hp);

        int getMaxHP();
        void setMaxHP(int maxHP);

        int getLevel();
        void setLevel(int level);
        void incLevel();

        int getAttack();
        void setAttack(int attack);

        int getDefense();
        void setDefense(int defense);

        int getSpeed();
        void setSpeed(int speed);

        int getAccuracy();
        void setAccuracy(int accuracy);

        int getCritChance();
        void setCritChance(int crit);

        int getEXP();
        void setEXP(int exp);
        void gainEXP(int exp);
        int earnEXP(IEngageable target);

        Party getParty();
        void setParty(Party party);

        IEngageable getTarget();
        void setTarget(IEngageable target);

        bool isDead();

        string getClass();
        string getStats();
    }

    public interface IPrintable
    {
        string ToString();
    }

    public interface IEquatable<T>
    {
        bool Equals(Object obj);
        bool Equals(T t);
    }
}
