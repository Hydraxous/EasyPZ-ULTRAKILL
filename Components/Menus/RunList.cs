using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using EasyPZ.Ghosts;


namespace EasyPZ.Components
{
    public class RunList : MonoBehaviour
    {
        [SerializeField] private GameObject listElementPrefab;
        [SerializeField] private RectTransform contentBody;
        [SerializeField] private Button selectAllButton;
        [SerializeField] private Button deselectAllButton;
        [SerializeField] private Button backButton;
        [SerializeField] private InputField searchField;
        [SerializeField] private Text levelNameText;

        private Dictionary<GameObject, HashSet<string>> namedElements;
        private List<GameObject> instancedElements;

        private bool isInLevelOfFolder;

        private void Awake()
        {
            searchField.onEndEdit.AddListener((string text) =>
            {
                UpdateSearch(text);
            });
        }

        public void SetName(string name)
        {
            isInLevelOfFolder = name == SceneHelper.CurrentScene;
            levelNameText.text = name;
        }

        private void ClearList()
        {
            if(instancedElements != null)
            {
                for (int i = 0; i < instancedElements.Count; i++)
                {
                    if (instancedElements[i] == null)
                        continue;

                    GameObject element = instancedElements[i];
                    instancedElements[i] = null;
                    Destroy(element);
                }

                instancedElements.Clear();
                namedElements.Clear();
            }
            else
            {
                instancedElements = new List<GameObject>();
                namedElements = new Dictionary<GameObject, HashSet<string>>();
            }
        }

        public void SetList(List<GhostRecordingMetadata> metadatas, Action<GhostRecordingMetadata> onRunSelected)
        {
            ClearList();

            foreach (var metadata in metadatas.OrderByDescending(x=>x.DateCreated.Ticks))
            {
                GameObject element = Instantiate(listElementPrefab, contentBody);

                Button button = element.GetComponentInChildren<Button>();
                Text text = element.GetComponentInChildren<Text>();
                Toggle toggle = element.GetComponentInChildren<Toggle>();

                instancedElements.Add(element);
                namedElements.Add(element, new HashSet<string>());

                namedElements[element].Add(metadata.GetFileName());

                if (!string.IsNullOrEmpty(metadata.Description))
                    namedElements[element].Add(metadata.Description);

                if (!string.IsNullOrEmpty(metadata.Title))
                    namedElements[element].Add(metadata.Title);

                if (!string.IsNullOrEmpty(metadata.RunnerName))
                    namedElements[element].Add(metadata.RunnerName);

                string playerName = "Player (" + metadata.SteamID.ToString().Substring(0,5) +"...)";

                if (SteamClient.IsValid)
                {
                    playerName = new Friend(metadata.SteamID).Name;
                }

                text.text = $"{playerName} // {metadata.Title} // {Path.GetFileNameWithoutExtension(metadata.LocatedFilePath)}";

                button.onClick.AddListener(() =>
                {
                    onRunSelected.Invoke(metadata);
                });

                toggle.SetIsOnWithoutNotify(GhostManager.IsGhostEnabled(metadata.LocatedFilePath));

                toggle.gameObject.SetActive(isInLevelOfFolder);
                toggle.onValueChanged.AddListener((bool value) =>
                {
                    GhostManager.SetGhostEnabled(metadata.LocatedFilePath, value);
                });
            }

            selectAllButton.onClick.AddListener(() =>
            {
                foreach (var element in instancedElements)
                {
                    Toggle toggle = element.GetComponentInChildren<Toggle>();
                    toggle.isOn = true;
                }
            });

            deselectAllButton.onClick.AddListener(() =>
            {
                foreach (var element in instancedElements)
                {
                    Toggle toggle = element.GetComponentInChildren<Toggle>();
                    toggle.isOn = false;
                }
            });
        }

        private void UpdateSearch(string text)
        {
            if (instancedElements == null || namedElements == null)
                return;

            if (string.IsNullOrEmpty(text))
            {
                foreach(var x in instancedElements)
                {
                    x.SetActive(true);
                }
                return;
            }

            foreach (var x in namedElements)
            {
                bool found = false;

                foreach (var y in x.Value)
                {
                    if (y.ToLower().Contains(text.ToLower()))
                    {
                        found = true;
                        break;
                    }
                }

                x.Key.SetActive(found);
            }
        }


        public void SetBackAction(Action onBackPressed)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                onBackPressed?.Invoke();
            });
        }

    }
}
