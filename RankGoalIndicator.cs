using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ
{
    public class RankGoalIndicator : MonoBehaviour
    {
        Text pModeStatusText, timeGoalText, killGoalText, styleGoalText, speedMetricText, pModeStatusPrefix;

        private StatsManager sman;
        private NewMovement player;
        private EasyPZUIPatch ezpz;
        private EnemyTracker eTrack;
        private UIAutoPositioner positioner;

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
            bool BRUH = true;
            if (timeUntilCountEnemies < Time.time)
            {
                if (BRUH)
                {
                    lastEnemyCount = eTrack.GetCurrentEnemies().Count;
                    
                }
                else
                {
                    timeUntilCountEnemies = Time.time + enemyCountUpdateInterval;
                    EnemyIdentifier[] enemies = GameObject.FindObjectsOfType<EnemyIdentifier>();
                    int enemyCount = 0;
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        if (!enemies[i].dead)
                        {
                            ++enemyCount;
                        }
                    }
                    lastEnemyCount = enemyCount;
                }
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
            eTrack = MonoSingleton<EnemyTracker>.Instance;
            ezpz = gameObject.GetComponentInParent<EasyPZUIPatch>();
            sman = MonoSingleton<StatsManager>.Instance;
            player = MonoSingleton<NewMovement>.Instance;
            positioner = GetComponent<UIAutoPositioner>();
        }

        private void FindTextObjects()
        {
            pModeStatusPrefix = transform.Find("StatusContainer/PModeStatusPrefix").gameObject.GetComponent<Text>();
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
            GetCurrentEnemyCount();
            float seconds = sman.timeRanks[3] - sman.seconds;
            float minutes = 0f;
            while (seconds >= 60f)
            {
                seconds -= 60f;
                minutes += 1f;
            }
            pModeStatusPrefix.text = String.Format("<color=#{0}>P</color>-Mode:", ColorUtility.ToHtmlStringRGB(positioner.uIData.highlightColor));
            timeGoalText.text = minutes + ":" + seconds.ToString("00.00");
            pModeStatusText.text = ezpz.PMode.ToString();
            
            if (lastEnemyCount > 0)
            {
                killGoalText.text = String.Format("[<color=#{2}><b>{0}</b></color>] {1}", lastEnemyCount, Mathf.Clamp((sman.killRanks[3] - sman.kills), 0, Mathf.Infinity),ColorUtility.ToHtmlStringRGB(positioner.uIData.highlightColor));
            }else
            {
                killGoalText.text = Mathf.Clamp((sman.killRanks[3] - sman.kills), 0, Mathf.Infinity).ToString();
            }
            styleGoalText.text = Mathf.Clamp((sman.styleRanks[3] - sman.stylePoints), 0, Mathf.Infinity).ToString();
            speedMetricText.text = player.transform.GetComponent<Rigidbody>().velocity.magnitude.ToString("00.00");
        }

    }
}
