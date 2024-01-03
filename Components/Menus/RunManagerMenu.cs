using Configgy;
using Configgy.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ.Components
{
    public class RunManagerMenu : MonoBehaviour
    {
        [SerializeField] private GameObject container;

        [SerializeField] private RunEditor runEditor;
        [SerializeField] private RectTransform listRoot;

        [SerializeField] private Button discordButton;
        [SerializeField] private Button backButton;

        [SerializeField] private RectTransform folderButtonContentBody;
        [SerializeField] private RectTransform foldersRoot;

        [SerializeField] private GameObject folderListPrefab;

        private List<GameObject> instancedMenus;
        private List<GameObject> instancedFolderButtons;

        private static RunManagerMenu instance;

        [Configgable("Extras/Advanced", "Notify on update available")]
        private static ConfigToggle notifyOnUpdateAvailable = new ConfigToggle(true);
        private static bool openedOnce;

        private void Awake()
        {
            instance = this;

            discordButton.onClick.AddListener(() =>
            {
                Application.OpenURL(ConstInfo.DISCORD_URL);
            });

            backButton.onClick.AddListener(() =>
            {
                Close();
            });

            RebuildMenu();

            container.SetActive(false);
            listRoot.gameObject.SetActive(false);
            foldersRoot.gameObject.SetActive(false);
            runEditor.gameObject.SetActive(false);
        }

        [Configgable("Ghosts", "Manage Runs")]
        private static void OpenMenuStatic()
        {
            if(instance == null)
            {
                Debug.LogError("RunManagerMenu could not be found.");
                return;
            }

            ConfigurationMenu.Close();
            instance.Open();
        }

        public void RebuildMenu()
        {
            ClearMenus();

            List<SessionRecordingMetadata> metadatas = GhostFileManager.FetchMetadata();
            List<string> levels = metadatas.Select(x => x.LevelName).Distinct().OrderBy(x => x).ToList();
            Dictionary<string, List<SessionRecordingMetadata>> levelsDict = levels.ToDictionary(x => x, x => metadatas.Where(y => y.LevelName == x).ToList());

            levelsDict = levelsDict.OrderByDescending(x => x.Key == SceneHelper.CurrentScene).ToDictionary(x => x.Key, x => x.Value);

            foreach (var levelFolder in levelsDict)
            {
                GameObject folder = Instantiate(folderListPrefab, foldersRoot);
                RunList list = folder.GetComponent<RunList>();

                bool isCurrent = levelFolder.Key == SceneHelper.CurrentScene;

                list.SetName(levelFolder.Key);
                list.SetList(levelFolder.Value, (d) =>
                {
                    runEditor.BackButton.onClick.RemoveAllListeners();
                    runEditor.BackButton.onClick.AddListener(() =>
                    {
                        runEditor.gameObject.SetActive(false);
                        foldersRoot.gameObject.SetActive(true);
                        if (folder != null)
                        {
                            folder.SetActive(true);
                        }
                        else
                        {
                            foldersRoot.gameObject.SetActive(false);
                            listRoot.gameObject.SetActive(true);
                        }
                    });
                    OpenRunInEditor(d);
                });

                folder.SetActive(false);

                GameObject folderButton = Instantiate(Configgy.Assets.PluginAssets.ButtonPrefab, folderButtonContentBody);
                instancedFolderButtons.Add(folderButton);

                Button b = folderButton.GetComponent<Button>();
                Text t = folderButton.GetComponentInChildren<Text>();
                RectTransform rt = folderButton.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 35f);
                t.text = $"{levelFolder.Key} ({levelFolder.Value.Count})";
                if (isCurrent)
                {
                    t.text = $"<color=orange>{t.text}</color>";
                }

                instancedMenus.Add(folder);

                b.onClick.AddListener(() =>
                {
                    OpenFolder(list);
                });
            }
        }

        private void Update()
        {
            if (!container.activeInHierarchy)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(runEditor.gameObject.activeInHierarchy)
                {
                    runEditor.gameObject.SetActive(false);
                    foldersRoot.gameObject.SetActive(true);
                    return;
                }

                if (foldersRoot.gameObject.activeInHierarchy)
                {
                    foldersRoot.gameObject.SetActive(false);
                    listRoot.gameObject.SetActive(true);
                    return;
                }

                if (container.activeInHierarchy)
                {
                    Close();
                }
            }
        }

        private void OpenRunInEditor(SessionRecordingMetadata metadata)
        {
            runEditor.SetRecording(metadata);
            runEditor.gameObject.SetActive(true);
            listRoot.gameObject.SetActive(false);
            foldersRoot.gameObject.SetActive(false);
        }

        private void OpenFolder(RunList list)
        {
            list.SetBackAction(() =>
            {
                listRoot.gameObject.SetActive(true);
                foldersRoot.gameObject.SetActive(false);
                list.gameObject.SetActive(false);
            });

            foldersRoot.gameObject.SetActive(true);
            listRoot.gameObject.SetActive(false);
            list.gameObject.SetActive(true);
        }

        private void ClearMenus()
        {
            if(instancedMenus != null)
            {
                for (int i = 0; i < instancedMenus.Count; i++)
                {
                    if (instancedMenus[i] == null)
                        continue;

                    GameObject g = instancedMenus[i];
                    instancedMenus[i] = null;
                    Destroy(g);
                }

                instancedMenus.Clear();
            }
            else
            {
                instancedMenus = new List<GameObject>();
            }

            if(instancedFolderButtons != null)
            {
                for (int i = 0; i < instancedFolderButtons.Count; i++)
                {
                    if (instancedFolderButtons[i] == null)
                        continue;

                    GameObject g = instancedFolderButtons[i];
                    instancedFolderButtons[i] = null;
                    Destroy(g);
                }

                instancedFolderButtons.Clear();
            }
            else
            {
                instancedFolderButtons = new List<GameObject>();
            }

        }

        public void Open()
        {
            container.SetActive(true);
            Pauser.Pause(container);
            runEditor.gameObject.SetActive(false);
            foldersRoot.gameObject.SetActive(false);

            if(instancedMenus != null)
            {
                foreach (var m in instancedMenus)
                {
                    m.gameObject.SetActive(false);
                }
            }
            listRoot.gameObject.SetActive(true);

            if (!openedOnce)
            {
                openedOnce = true;
                if(!EasyPZ.UsingLatestVersion && notifyOnUpdateAvailable.Value)
                {
                    NotifyUpdateAvailable();
                }
            }
        }

        private void NotifyUpdateAvailable()
        {
            ModalDialogue.ShowDialogue(new ModalDialogueEvent()
            {
                Title = "Outdated",
                Message = $"You are using an outdated version of {ConstInfo.NAME}: (<color=red>{ConstInfo.VERSION}</color>). Please update to the latest version: (<color=green>{Plugin.LatestVersion}</color>)",
                Options = new DialogueBoxOption[]
                        {
                            new DialogueBoxOption()
                            {
                                Name = "Open Browser",
                                Color = Color.white,
                                OnClick = () => Application.OpenURL(ConstInfo.GITHUB_URL+"/releases/latest")
                            },
                            new DialogueBoxOption()
                            {
                                Name = "Later",
                                Color = Color.white,
                                OnClick = () => { }
                            },
                            new DialogueBoxOption()
                            {
                                Name = "Don't Ask Again.",
                                Color = Color.red,
                                OnClick = () =>
                                {
                                    notifyOnUpdateAvailable.SetValue(false);
                                }
                            }
                        }
            });
        }

        public void Close()
        {
            container.SetActive(false);
            ConfigurationMenu.Open();
        }
    }
}
