using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPZ
{
    public static class RecordsManager
    {
        public static StatGoal GetStatGoal(string levelKey)
        {
            //Deserialize stat data and return it.
            return new StatGoal();
        }

        public static void SetStatGoal(string levelKey, StatGoal goal)
        {
            //Serialize stat data and save it.
            highScores[levelKey] = goal;
        }

        private static Dictionary<string, StatGoal> highScores = new Dictionary<string, StatGoal>();
    }
}
