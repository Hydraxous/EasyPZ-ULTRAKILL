using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPZ
{
    [Serializable]
    public struct StatGoal
    {
        public int Difficulty;
        public int Kills;
        public float Seconds;
        public int Deaths;
        public int Style;

        public bool NotEmpty()
        {
            return Kills > 0 || Seconds > 0 || Deaths > 0 || Style > 0;
        }

        public bool IsFailed()
        {
            if (NewMovement.Instance.dead)
            {
                if (StatsManager.Instance.restarts + 1 > Deaths)
                    return true;
            }
            else if (StatsManager.Instance.restarts > Deaths)
                return true;

            if (StatsManager.Instance.seconds > Seconds)
                return true;

            if (StatsManager.Instance.infoSent)
            {
                if (StatsManager.Instance.kills < Kills)
                    return true;    

                if (StatsManager.Instance.stylePoints < Style)
                    return true;
            }

            return false;
        }

        public bool IsComplete()
        {
            if (StatsManager.Instance.seconds > Seconds)
                return false;

            if (StatsManager.Instance.restarts > Deaths)
                return false;
            
            if (StatsManager.Instance.stylePoints < Style)
                return false;

            if (StatsManager.Instance.kills < Kills)
                return false;
         
            return true;
        }
    }

}
