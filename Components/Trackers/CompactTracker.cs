using Configgy;
using EasyPZ.UIEdit;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ.Components
{
    public class CompactTracker : MonoBehaviour, IUIEditable, IEZPZTracker
    {
        [SerializeField] private Text timeText;
        [SerializeField] private Text killsText;
        [SerializeField] private Text dynamicKillsText;
        [SerializeField] private Text styleText;

        [SerializeField] private Image[] dividers;
        [SerializeField] private Image background;

        [SerializeField] private RectTransform trackerRoot;
        [SerializeField] private MovableWindow movableWindow;

        [Configgable("Tracker/Compact Tracker", "X Position")]
        private static ConfigInputField<float> trackerXPosition = new ConfigInputField<float>(0);

        [Configgable("Tracker/Compact Tracker", "Y Position")]
        private static ConfigInputField<float> trackerYPosition = new ConfigInputField<float>(-270);

        [Configgable("Tracker/Compact Tracker", "Background Color")]
        private static ConfigColor backgroundColor = new ConfigColor(new Color(0, 0, 0, 0.70f));

        [Configgable("Tracker/Compact Tracker", "Text Color")]
        private static ConfigColor textColor = new ConfigColor(new Color(1, 1, 1, 1f));

        [Configgable("Tracker/Compact Tracker", "Alt Color")]
        private static ConfigColor altColor = new ConfigColor(new Color(1, 1, 1, 1));

        [Configgable("Tracker/Compact Tracker", "Display Reset Status")]
        private static ConfigToggle showResetStatus = new ConfigToggle(true);

        [Configgable("Tracker/Compact Tracker", "Text Highlight Color")]
        private static Color highlightColor = new Color(255, 0, 0, 255);

        private StatsManager statsManager;
        private StatGoal goal;

        private void Start()
        {
            statsManager = StatsManager.Instance;

            movableWindow.enabled = false;
            background.raycastTarget = false;

            LoadCustomization();
        }

        private void LoadCustomization()
        {
            trackerXPosition.OnValueChanged += SetXPos;
            trackerYPosition.OnValueChanged += SetYPos;
            backgroundColor.OnValueChanged += SetBackgroundColor;
            textColor.OnValueChanged += SetTextColor;

            SetYPos(trackerYPosition.Value);
            SetXPos(trackerXPosition.Value);
            SetBackgroundColor(backgroundColor.Value);
            SetTextColor(textColor.Value);
        }

        private void SetBackgroundColor(Color color)
        {
            background.color = color;
        }

        private bool completed;

        private void Update()
        {
            UpdateKillCheck();
            UpdateStyleCheck();
            UpdateTimeCheck();
            UpdatePMode();

            if (!completed)
            {
                if (goal.IsComplete())
                {
                    completed = true;
                    //Play cool little animation.
                    //Like a shine swipe using a mask
                }
            }
        }

        private void UpdateTimeCheck()
        {
            float secondsLeft = goal.Seconds - statsManager.seconds;
            float seconds = Mathf.Max(secondsLeft, 0.0f);

            float minutes = 0f;
            while (seconds >= 60f)
            {
                seconds -= 60f;
                minutes += 1f;
            }

            timeText.text = minutes + ":" + seconds.ToString("00.00");
        }

        private void UpdateKillCheck()
        {
            int enemyCount = EnemyTracker.Instance.GetCurrentEnemies().Count;
            int neededKills = Mathf.Max(goal.Kills - statsManager.kills, 0);

            dynamicKillsText.gameObject.SetActive(enemyCount > 0);

            if (enemyCount > 0)
            {
                string htmlHighlightColor = GetTextHighlightHtml();
                dynamicKillsText.text = $"[<color=#{htmlHighlightColor}><b>{enemyCount}</b></color>]";
            }

            killsText.text = neededKills.ToString();


        }

        private string GetTextHighlightHtml()
        {
            return ColorUtility.ToHtmlStringRGB(highlightColor);
        }

        private void UpdateStyleCheck()
        {
            styleText.text = Mathf.Clamp((goal.Style - statsManager.stylePoints), 0, Mathf.Infinity).ToString();
        }

        private void UpdatePMode()
        {
            bool enabled = showResetStatus.Value;
            bool autoReset = TrackerManager.AutoRestartEnabled.Value;

            foreach (Image divider in dividers)
            {
                divider.color = (!enabled) ? altColor.Value : (autoReset) ? highlightColor : altColor.Value;  
            }
        }


        private void SetXPos(float xPosition)
        {
            trackerRoot.anchoredPosition = new Vector2(xPosition, trackerRoot.anchoredPosition.y);
        }

        private void SetYPos(float yPosition)
        {
            trackerRoot.anchoredPosition = new Vector2(trackerRoot.anchoredPosition.x, yPosition);
        }

        private void SetTextColor(Color color)
        {
            timeText.color = color;
            styleText.color = color;
            killsText.color = color;
            dynamicKillsText.color = color;
        }

        public void StartEditMode()
        {
            movableWindow.enabled = true;
            background.raycastTarget = true;
        }

        public void EndEditMode()
        {
            background.raycastTarget = false;
            movableWindow.enabled = false;
            Vector2 position = trackerRoot.anchoredPosition;
            trackerXPosition.SetValue(position.x);
            trackerYPosition.SetValue(position.y);
        }

        public void SetStatGoal(StatGoal goal)
        {
            this.goal = goal;
        }

        private void OnDestroy()
        {
            trackerXPosition.OnValueChanged -= SetXPos;
            trackerYPosition.OnValueChanged -= SetYPos;
            backgroundColor.OnValueChanged -= SetBackgroundColor;
            textColor.OnValueChanged -= SetTextColor;
        }
    }
}
