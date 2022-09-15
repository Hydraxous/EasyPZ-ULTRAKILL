using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ
{
    public class RankGoalIndicator : MonoBehaviour
    {
        Text pModeStatusText, timeGoalText, killGoalText, styleGoalText, speedMetricText;

        private StatsManager sman;
        private NewMovement player;
        private EasyPZUIPatch ezpz;
        private EnemyTracker enemyTracker;

        private int lastEnemyCount = 0;
        private float enemyCountUpdateInterval = 1.5f;
        private float timeUntilCountEnemies = 0.0f;

        private void Start()
        {
            FindThings();
        }

        private void Update()
        {
            UpdateDisplay();
        }

        private int GetCurrentEnemyCount()
        {
            if (timeUntilCountEnemies < Time.time)
            {
                timeUntilCountEnemies = Time.time + enemyCountUpdateInterval;
                lastEnemyCount = enemyTracker.GetCurrentEnemies().Count;
                return lastEnemyCount;
            }
            return lastEnemyCount;
        }

        private void FindThings()
        {
            FindDependants();
            FindTextObjects();
        }

        private void FindDependants()
        {

            ezpz = gameObject.GetComponentInParent<EasyPZUIPatch>();
            sman = MonoSingleton<StatsManager>.Instance;
            player = MonoSingleton<NewMovement>.Instance;
            enemyTracker = MonoSingleton<EnemyTracker>.Instance;
        }

        private void FindTextObjects()
        {
            pModeStatusText = transform.Find("StatusContainer/PModeStatusText").gameObject.GetComponent<Text>();
            timeGoalText = transform.Find("GoalsContainer/TimeGoalContainer/TimeGoalText").gameObject.GetComponent<Text>();
            //timeGoalText.fontSize = 18;
            killGoalText = transform.Find("GoalsContainer/KillGoalContainer/KillGoalText").gameObject.GetComponent<Text>();
            // killGoalText.fontSize = 21;
            styleGoalText = transform.Find("GoalsContainer/StyleGoalContainer/StyleGoalText").gameObject.GetComponent<Text>();
            //styleGoalText.fontSize = 21;
            speedMetricText = transform.Find("SpeedometerContainer/SpeedMetricText").gameObject.GetComponent<Text>();
            //speedMetricText.fontSize = 16;
        }

        public void UpdateDisplay()
        {
            float seconds = sman.timeRanks[3] - sman.seconds;
            float minutes = 0f;
            while (seconds >= 60f)
            {
                seconds -= 60f;
                minutes += 1f;
            }
            timeGoalText.text = minutes + ":" + seconds.ToString("00.00");
            pModeStatusText.text = ezpz.PMode.ToString();
            killGoalText.text = String.Format("[<color=red>{0}</color>] ", lastEnemyCount + Mathf.Clamp((sman.killRanks[3] - sman.kills),0,Mathf.Infinity).ToString());
            styleGoalText.text = Mathf.Clamp((sman.styleRanks[3] - sman.stylePoints), 0, Mathf.Infinity).ToString();
            speedMetricText.text = player.transform.GetComponent<Rigidbody>().velocity.magnitude.ToString("00.00");
        }

    }
}
