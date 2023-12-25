using Configgy;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ.Components
{
    public class ClassicTracker : MonoBehaviour, IUIEditable
    {
        [SerializeField] private Text pModeStatusText;
        [SerializeField] private Text timeGoalText;
        [SerializeField] private Text killGoalText;
        [SerializeField] private Text styleGoalText;
        [SerializeField] private Text speedMetricText;
        [SerializeField] private Text speedSuffixText;
        [SerializeField] private Text pModeStatusPrefix;
        [SerializeField] private Image[] icons;
        [SerializeField] private RectTransform trackerRoot;

        private int lastEnemyCount = 0;

        [Configgable("Options/ClassicTracker", "X Position")]
        private static ConfigInputField<float> trackerXPosition = new ConfigInputField<float>(-10);

        [Configgable("Options/ClassicTracker", "Y Position")]
        private static ConfigInputField<float> trackerYPosition = new ConfigInputField<float>(10);

        [Configgable("Options/ClassicTracker", "Background Color")]
        private static ConfigColor backgroundColor = new ConfigColor(new Color(0, 0, 0, 0.35f));

        [Configgable("Options/ClassicTracker", "Primary Color")]
        private static ConfigColor primaryColor = new ConfigColor(new Color(1, 1, 1, 1f));

        [Configgable("Options/ClassicTracker", "Secondary Color")]
        private static ConfigColor secondaryColor = new ConfigColor(new Color(1, 1, 1, 1f));

        [Configgable("Options/ClassicTracker", "Text Highlight Color")]
        private static Color textHighlightColor = new Color(255, 0, 0, 255);

        private StatsManager statsManager;

        private void Start()
        {
            statsManager = StatsManager.Instance;
            LoadCustomization();
        }

        private void LoadCustomization()
        {
            trackerXPosition.OnValueChanged += SetXPos;
            trackerYPosition.OnValueChanged += SetYPos;
            backgroundColor.OnValueChanged += SetBackgroundColor;
            primaryColor.OnValueChanged += SetTextColor;
            secondaryColor.OnValueChanged += SetIconColor;

            SetYPos(trackerYPosition.Value);
            SetXPos(trackerXPosition.Value);
            SetBackgroundColor(backgroundColor.Value);
            SetTextColor(primaryColor.Value);
            SetIconColor(secondaryColor.Value);
        }

        private void SetBackgroundColor(Color color)
        {
            trackerRoot.GetComponent<Image>().color = color;
        }

        private void Update()
        {
            UpdateKillCheck();
            UpdateStyleCheck();
            UpdateTimeCheck();
            UpdateSpeedMetric();
            UpdatePMode();
        }

        private void UpdateTimeCheck() 
        {
            float seconds = Mathf.Max(statsManager.timeRanks[3] - statsManager.seconds, 0.0f);
            float minutes = 0f;
            while (seconds >= 60f)
            {
                seconds -= 60f;
                minutes += 1f;
            }

            timeGoalText.text = minutes + ":" + seconds.ToString("00.00");
        }

        private void UpdateKillCheck()
        {
            int enemyCount = EnemyTracker.Instance.GetCurrentEnemies().Count;
            int neededKills = Mathf.Max(statsManager.killRanks[3] - statsManager.kills, 0);


            if (lastEnemyCount > 0)
            {
                string htmlColor = ColorUtility.ToHtmlStringRGB(textHighlightColor);
                killGoalText.text = $"[<color=#{htmlColor}><b>{enemyCount}</b></color>] {neededKills}";
            }
            else
            {
                killGoalText.text = neededKills.ToString();
            }
        }

        private void UpdateStyleCheck()
        {
            styleGoalText.text = Mathf.Clamp((statsManager.styleRanks[3] - statsManager.stylePoints), 0, Mathf.Infinity).ToString();
        }

        private void UpdateSpeedMetric()
        {
            speedMetricText.text = NewMovement.Instance.rb.velocity.magnitude.ToString("00.00");
        }

        private void UpdatePMode()
        {
            pModeStatusPrefix.text = $"<color=#{ColorUtility.ToHtmlStringRGB(textHighlightColor)}>P</color>-Mode:";
            pModeStatusText.text = EasyPZ.PModeEnabled.ToString();
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
            timeGoalText.color = color;
            killGoalText.color = color;
            styleGoalText.color = color;
            speedMetricText.color = color;
            pModeStatusText.color = color;
            pModeStatusPrefix.color = color;
            speedSuffixText.color = color;
        }

        private void SetIconColor(Color color)
        {
            foreach (Image icon in icons)
            {
                icon.color = color;
            }
        }

        public void StartEditMode()
        {
            Debug.Log("Start edit mode");
        }

        public void EndEditMode()
        {
            Debug.Log("End edit mode");
        }

        private void OnDestroy()
        {
            trackerXPosition.OnValueChanged -= SetXPos;
            trackerYPosition.OnValueChanged -= SetYPos;
            backgroundColor.OnValueChanged -= SetBackgroundColor;
            primaryColor.OnValueChanged -= SetTextColor;
            secondaryColor.OnValueChanged -= SetIconColor;
        }
    }
}
