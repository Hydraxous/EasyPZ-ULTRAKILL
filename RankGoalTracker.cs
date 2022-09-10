using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ
{
    public class RankGoalTracker : MonoBehaviour
    {
        Text pModeStatusText, timeGoalText, killGoalText, styleGoalText, speedMetricText;

        private StatsManager sman;
        private NewMovement player;
        public EasyPZ ezpz;

        private bool missingObjects = true;
        private bool missingTexts = true;

        private void Start()
        {
            FindThings();
        }

        private void Update()
        {         
            if (missingObjects || missingTexts)
            {
                FindThings();
            }
            else
            { 
                try
                {
                    UpdateDisplay();
                }catch(Exception e)
                {
                    missingObjects = true;
                }
            }
        }

        private void FindThings()
        {
            if (missingObjects)
            {
                FindDependants();
            }

            if (missingTexts)
            {
                FindTextObjects();
            }
        }

        private void FindDependants()
        {
            try
            {
                ezpz = GameObject.FindObjectOfType<EasyPZ>();
                sman = MonoSingleton<StatsManager>.Instance;
                player = MonoSingleton<NewMovement>.Instance;
                missingObjects = false;
            }
            catch (System.Exception e)
            {
                missingObjects = true;
            }
        }

        private void FindTextObjects()
        {
            try
            {
                pModeStatusText = transform.Find("StatusContainer/PModeStatusText").gameObject.GetComponent<Text>();
                timeGoalText = transform.Find("GoalsContainer/TimeGoalContainer/TimeGoalText").gameObject.GetComponent<Text>();
                killGoalText = transform.Find("GoalsContainer/KillGoalContainer/KillGoalText").gameObject.GetComponent<Text>();
                killGoalText.fontSize = 20;
                styleGoalText = transform.Find("GoalsContainer/StyleGoalContainer/StyleGoalText").gameObject.GetComponent<Text>();
                styleGoalText.fontSize = 20;
                speedMetricText = transform.Find("SpeedometerContainer/SpeedMetricText").gameObject.GetComponent<Text>();
                speedMetricText.fontSize = 16;
                missingTexts = false;
            }catch (System.Exception e)
            {
                missingTexts = true;
            }
            
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
            timeGoalText.text = minutes + ":" + seconds.ToString("00.000");
            pModeStatusText.text = ezpz.PMode.ToString();
            killGoalText.text = (sman.killRanks[3] - sman.kills).ToString();
            styleGoalText.text = Mathf.Clamp((sman.styleRanks[3] - sman.stylePoints), 0, Mathf.Infinity).ToString();
            speedMetricText.text = player.transform.GetComponent<Rigidbody>().velocity.magnitude.ToString("00.00");
        }

        private void OnEnable()
        {
            FindThings();
        }
    }
}
