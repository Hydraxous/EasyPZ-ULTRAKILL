using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ.Components
{
    public class RunList : MonoBehaviour
    {
        [SerializeField] private GameObject listElementPrefab;
        [SerializeField] private RectTransform contentBody;
        [SerializeField] private Button selectAllButton;
        [SerializeField] private Button deselectAllButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Text levelNameText;

        private List<GameObject> instancedElements;

        private bool isInLevelOfFolder;

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
            }
            else
            {
                instancedElements = new List<GameObject>();
            }
        }

        public void SetList(List<SessionRecordingMetadata> metadatas, Action<SessionRecordingMetadata> onRunSelected)
        {
            ClearList();

            foreach (var metadata in metadatas)
            {
                GameObject element = Instantiate(listElementPrefab, contentBody);

                Button button = element.GetComponentInChildren<Button>();
                Text text = element.GetComponentInChildren<Text>();
                Toggle toggle = element.GetComponentInChildren<Toggle>();

                instancedElements.Add(element);

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
