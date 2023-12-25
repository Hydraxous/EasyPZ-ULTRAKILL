using Configgy;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ.Components
{
    public class ClassicTracker : MonoBehaviour, IUIEditable, IEZPZTracker
    {
        [SerializeField] private Text pModeStatusText;
        [SerializeField] private Text timeGoalText;
        [SerializeField] private Text killGoalText;
        [SerializeField] private Text styleGoalText;
        [SerializeField] private Text speedMetricText;
        [SerializeField] private Text speedSuffixText;
        [SerializeField] private Text pModeStatusPrefix;

        [SerializeField] private Image timeIcon;
        [SerializeField] private Image styleIcon;
        [SerializeField] private Image killsIcon;

        [SerializeField] private RectTransform trackerRoot;

        private int lastEnemyCount = 0;

        [Configgable("Customization/Classic Tracker", "X Position")]
        private static ConfigInputField<float> trackerXPosition = new ConfigInputField<float>(540);

        [Configgable("Customization/Classic Tracker", "Y Position")]
        private static ConfigInputField<float> trackerYPosition = new ConfigInputField<float>(-220);

        [Configgable("Customization/Classic Tracker", "Background Color")]
        private static ConfigColor backgroundColor = new ConfigColor(new Color(0, 0, 0, 0.35f));

        [Configgable("Customization/Classic Tracker", "Primary Color")]
        private static ConfigColor primaryColor = new ConfigColor(new Color(1, 1, 1, 1f));

        [Configgable("Customization/Classic Tracker", "Complete Icon Color")]
        private static ConfigColor completeColor = new ConfigColor(new Color(1, 0, 0, 1f));

        [Configgable("Customization/Classic Tracker", "Incomplete Icon Color")]
        private static ConfigColor incompleteColor = new ConfigColor(new Color(1, 1, 1, 0.3f));

        [Configgable("Customization/Classic Tracker", "Display Speed")]
        private static ConfigToggle showSpeed = new ConfigToggle(true);

        [Configgable("Customization/Classic Tracker", "Icon Completion Colors")]
        private static ConfigToggle iconCompletionColors = new ConfigToggle(true);

        [Configgable("Customization/Classic Tracker", "Text Highlight Color")]
        private static Color textHighlightColor = new Color(255, 0, 0, 255);

        private StatsManager statsManager;
        private StatGoal goal;

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

            SetYPos(trackerYPosition.Value);
            SetXPos(trackerXPosition.Value);
            SetBackgroundColor(backgroundColor.Value);
            SetTextColor(primaryColor.Value);
        }

        private void SetBackgroundColor(Color color)
        {
            trackerRoot.GetComponent<Image>().color = color;
        }

        private bool completed;

        private void Update()
        {
            UpdateKillCheck();
            UpdateStyleCheck();
            UpdateTimeCheck();
            UpdateSpeedMetric();
            UpdatePMode();

            if(!completed)
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

            bool complete = seconds > 0f || !iconCompletionColors.Value;
            timeIcon.color = complete ? completeColor.Value : incompleteColor.Value;
            timeGoalText.text = minutes + ":" + seconds.ToString("00.00");
        }

        private void UpdateKillCheck()
        {
            int enemyCount = EnemyTracker.Instance.GetCurrentEnemies().Count;
            int neededKills = Mathf.Max(goal.Kills - statsManager.kills, 0);

            bool complete = neededKills <= 0 || !iconCompletionColors.Value;
            killsIcon.color = complete ? completeColor.Value : incompleteColor.Value;

            if (enemyCount > 0)
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
            bool complete = statsManager.stylePoints >= goal.Style || !iconCompletionColors.Value;
            styleIcon.color = complete ? completeColor.Value : incompleteColor.Value;
            styleGoalText.text = Mathf.Clamp((goal.Style - statsManager.stylePoints), 0, Mathf.Infinity).ToString();
        }

        private void UpdateSpeedMetric()
        {
            if (showSpeed.Value)
            {
                speedMetricText.text = NewMovement.Instance.rb.velocity.magnitude.ToString("00.00");
                speedSuffixText.text = "m/s";
            }
            else
            {
                speedMetricText.text = "";
                speedSuffixText.text = "";
            }
        }

        private void UpdatePMode()
        {
            bool autoReset = TrackerManager.AutoRestartEnabled;
            string htmlColor = (autoReset) ? ColorUtility.ToHtmlStringRGB(completeColor.Value) : ColorUtility.ToHtmlStringRGB(incompleteColor.Value);
            pModeStatusPrefix.text = $"<color=#{ColorUtility.ToHtmlStringRGB(textHighlightColor)}>RESET</color>:";
            pModeStatusText.text = TrackerManager.AutoRestartEnabled ? "ON" : "OFF";
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

        public void StartEditMode()
        {
            Debug.Log("Start edit mode");
        }

        public void EndEditMode()
        {
            Debug.Log("End edit mode");
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
            primaryColor.OnValueChanged -= SetTextColor;
        }
    }
}
