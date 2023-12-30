using Configgy;
using Configgy.UI;
using EasyPZ.Components;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;
using UnityEngine.UI;

namespace EasyPZ
{
    public class RunManagerInterface : IConfigElement
    {

        private ConfigBuilder config;
        public void BindConfig(ConfigBuilder configBuilder)
        {
            this.config = configBuilder;
        }

        private ConfiggableAttribute configgableAttribute;


        public void BindDescriptor(ConfiggableAttribute configgable)
        {
            //lol
            configgableAttribute = configgable;
        }

        public void BuildElement(RectTransform rect)
        {
            Vector2 size = rect.sizeDelta;
            size.y *= 20f;
            rect.sizeDelta = size;

            RectTransform runEditMenu = null;
            Text runEditLevelName = null;
            Text runEditDate = null;
            Text runEditRunnerName = null;

            Text runTime = null;
            Text runKills = null;
            Text runDeaths = null;
            Text runStyle = null;

            InputField runEditTitle = null;
            InputField runEditDescription = null;

            Button backButton = null;
            Button saveButton = null;
            Button deleteButton = null;

            //Editor
            DynUI.Div(rect, (editorRoot) =>
            {
                runEditMenu = editorRoot;
                VerticalLayoutGroup vlg = editorRoot.gameObject.AddComponent<VerticalLayoutGroup>();
                ContentSizeFitter csf = editorRoot.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.childControlHeight = false;

                DynUI.Div(editorRoot, (buttonsDiv) =>
                {
                    buttonsDiv.sizeDelta = new Vector2(buttonsDiv.sizeDelta.x, 40f);

                    HorizontalLayoutGroup hlg = buttonsDiv.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.childForceExpandWidth = true;
                    hlg.childForceExpandHeight = true;
                    hlg.childControlHeight = true;
                    hlg.childControlWidth = true;
                    hlg.childAlignment = TextAnchor.MiddleCenter;

                    DynUI.Button(buttonsDiv, (b) =>
                    {
                        b.GetComponentInChildren<Text>().text = "BACK";
                        backButton = b;
                    });

                    DynUI.Button(buttonsDiv, (b) =>
                    {
                        b.GetComponentInChildren<Text>().text = "SAVE";
                        saveButton = b;
                    });

                    DynUI.Button(buttonsDiv, (b) =>
                    {
                        b.GetComponentInChildren<Text>().text = "DELETE";
                        deleteButton = b;
                    });

                });

                DynUI.Div(editorRoot, (headerDiv) =>
                {
                    headerDiv.sizeDelta = new Vector2(headerDiv.sizeDelta.x, 40f);

                    HorizontalLayoutGroup hlg = headerDiv.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.childForceExpandWidth = true;
                    hlg.childForceExpandHeight = true;
                    hlg.childControlHeight = true;
                    hlg.childControlWidth = true;
                    hlg.childAlignment = TextAnchor.MiddleCenter;

                    DynUI.InputField(headerDiv, (inputField) =>
                    {
                        inputField.SetTextWithoutNotify("RUN_TITLE");
                        inputField.textComponent.fontSize = 18;
                        inputField.textComponent.alignment = TextAnchor.MiddleLeft;
                        runEditTitle = inputField;
                    });

                    DynUI.Label(headerDiv, (t) =>
                    {
                        t.text = "RUN_DATE";
                        t.fontSize = 18;
                        t.alignment = TextAnchor.MiddleRight;
                        runEditDate = t;
                    });

                });

                DynUI.Div(editorRoot, (headerDiv) =>
                {
                    headerDiv.sizeDelta = new Vector2(headerDiv.sizeDelta.x, 20f);

                    HorizontalLayoutGroup hlg = headerDiv.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.childForceExpandWidth = true;
                    hlg.childForceExpandHeight = true;
                    hlg.childControlHeight = true;
                    hlg.childControlWidth = true;
                    hlg.childAlignment = TextAnchor.MiddleCenter;

                    DynUI.Label(headerDiv, (t) =>
                    {
                        t.text = "RUNNER_NAME";
                        t.fontSize = 18;
                        t.alignment = TextAnchor.MiddleLeft;
                        runEditRunnerName = t;
                    });

                    DynUI.Label(headerDiv, (t) =>
                    {
                        t.text = "LEVEL_NAME";
                        t.fontSize = 18;
                        t.alignment = TextAnchor.MiddleRight;
                        runEditLevelName = t;
                    });

                });


                DynUI.Div(editorRoot, (subDiv) =>
                {
                    VerticalLayoutGroup vlg = subDiv.gameObject.AddComponent<VerticalLayoutGroup>();
                    ContentSizeFitter csf = subDiv.gameObject.AddComponent<ContentSizeFitter>();
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    vlg.childForceExpandWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.childControlHeight = false;

                    DynUI.InputField(subDiv, (inputField) =>
                    {
                        RectTransform rt = inputField.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 100f);

                        inputField.textComponent.fontSize = 18;
                        inputField.textComponent.alignment = TextAnchor.UpperLeft;
                        inputField.SetTextWithoutNotify("RUN_DESCRIPTION");

                        runEditDescription = inputField;
                    });
                });

                DynUI.Div(editorRoot, (subDiv) =>
                {
                    VerticalLayoutGroup vlg = subDiv.gameObject.AddComponent<VerticalLayoutGroup>();
                    ContentSizeFitter csf = subDiv.gameObject.AddComponent<ContentSizeFitter>();
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    vlg.childForceExpandWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.childControlHeight = false;

                    DynUI.Label(subDiv, (timeText) =>
                    {
                        RectTransform rt = timeText.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 25f);

                        timeText.fontSize = 18;
                        timeText.alignment = TextAnchor.MiddleLeft;
                        timeText.text = "RUN_TIME";

                        runTime = timeText;
                    });

                    DynUI.Label(subDiv, (killsText) =>
                    {
                        RectTransform rt = killsText.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 25f);

                        killsText.fontSize = 18;
                        killsText.alignment = TextAnchor.MiddleLeft;
                        killsText.text = "RUN_KILLS";

                        runKills = killsText;
                    });

                    DynUI.Label(subDiv, (styleText) =>
                    {
                        RectTransform rt = styleText.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 25f);

                        styleText.fontSize = 18;
                        styleText.alignment = TextAnchor.MiddleLeft;
                        styleText.text = "RUN_STYLE";

                        runStyle = styleText;
                    });

                    DynUI.Label(subDiv, (deathText) =>
                    {
                        RectTransform rt = deathText.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 25f);

                        deathText.fontSize = 18;
                        deathText.alignment = TextAnchor.MiddleLeft;
                        deathText.text = "RUN_STYLE";

                        runDeaths = deathText;
                    });
                });
            });

            runEditMenu.gameObject.SetActive(false);

            RectTransform levelFolders = null;

            List<SessionRecordingMetadata> metadatas = GhostFileManager.FetchMetadata();

            List<string> levels = metadatas.Select(x => x.LevelName).Distinct().OrderBy(x => x).ToList();
            Dictionary<string, List<SessionRecordingMetadata>> levelsDict = levels.ToDictionary(x => x, x => metadatas.Where(y => y.LevelName == x).ToList());

            //Folders List
            DynUI.Div(rect, (foldersRoot) =>
            {
                levelFolders = foldersRoot;

                VerticalLayoutGroup vlg = foldersRoot.gameObject.AddComponent<VerticalLayoutGroup>();
                ContentSizeFitter csf = foldersRoot.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.childControlHeight = false;

                DynUI.Label(foldersRoot, (t) =>
                {
                    t.text = "--- Levels ---";
                    t.fontSize = 18;
                    t.alignment = TextAnchor.MiddleCenter;
                });

                foreach (KeyValuePair<string, List<SessionRecordingMetadata>> levelRuns in levelsDict)
                {
                    Button levelFolderButton = null;
                    RectTransform runList = null;

                    DynUI.Div(rect, (levelRunListRoot) =>
                    {
                        runList = levelRunListRoot;
                        VerticalLayoutGroup vlg = levelRunListRoot.gameObject.AddComponent<VerticalLayoutGroup>();
                        ContentSizeFitter csf = levelRunListRoot.gameObject.AddComponent<ContentSizeFitter>();
                        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        vlg.childForceExpandWidth = true;
                        vlg.childForceExpandHeight = false;
                        vlg.childControlHeight = false;

                        DynUI.Div(runList, (buttonsDiv) =>
                        {
                            buttonsDiv.sizeDelta = new Vector2(buttonsDiv.sizeDelta.x, 40f);

                            HorizontalLayoutGroup hlg = buttonsDiv.gameObject.AddComponent<HorizontalLayoutGroup>();
                            hlg.childForceExpandWidth = true;
                            hlg.childForceExpandHeight = true;
                            hlg.childControlHeight = true;
                            hlg.childControlWidth = true;
                            hlg.childAlignment = TextAnchor.MiddleCenter;

                            DynUI.Button(buttonsDiv, (b) =>
                            {
                                b.GetComponentInChildren<Text>().text = "BACK";
                                b.onClick.AddListener(() =>
                                {
                                    runList.gameObject.SetActive(false);
                                    levelFolders.gameObject.SetActive(true);
                                });
                            });
                        });


                        DynUI.Label(levelRunListRoot, (t) =>
                        {
                            t.text = $"--- {levelRuns.Key} Runs ---";
                            t.fontSize = 18;
                            t.alignment = TextAnchor.MiddleCenter;
                        });

                        runList.gameObject.SetActive(false);
                    });

                    DynUI.Button(levelFolders, (b) =>
                    {
                        levelFolderButton = b;

                        RectTransform buttonRect = b.GetComponent<RectTransform>();
                        buttonRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x, 35f);
                        Text buttonText = b.GetComponentInChildren<Text>();
                        buttonText.text = $"{levelRuns.Key} ({levelRuns.Value.Count})";
                        b.onClick.AddListener(() =>
                        {
                            levelFolders.gameObject.SetActive(false);
                            runList.gameObject.SetActive(true);
                            Canvas.ForceUpdateCanvases();
                        });
                    });

                    bool spawnToggles = levelRuns.Key == SceneHelper.CurrentScene;

                    //Order each run by date created.
                    foreach (SessionRecordingMetadata metadata in levelRuns.Value.OrderByDescending(x=>x.DateCreated.Ticks))
                    {
                        List<Toggle> soloToggles = new List<Toggle>();

                        DynUI.Div(runList, (listElement) =>
                        {
                            listElement.sizeDelta = new Vector2(listElement.sizeDelta.x, 35f);

                            HorizontalLayoutGroup hlg = listElement.gameObject.AddComponent<HorizontalLayoutGroup>();
                            hlg.childForceExpandWidth = false;
                            hlg.childForceExpandHeight = true;
                            hlg.childControlHeight = true;
                            hlg.childControlWidth = false;
                            hlg.childAlignment = TextAnchor.MiddleCenter;

                            if (spawnToggles)
                            {
                                DynUI.Toggle(listElement, (toggle) =>
                                {
                                    RectTransform rt = toggle.GetComponent<RectTransform>();
                                    rt.sizeDelta = new Vector2(35f, 35f);
                                    soloToggles.Add(toggle);

                                    toggle.onValueChanged.AddListener((state) =>
                                    {
                                        if (state)
                                        {
                                            foreach (Toggle t in soloToggles)
                                            {
                                                if (t == toggle)
                                                    continue;

                                                t.SetIsOnWithoutNotify(false);
                                            }

                                            GhostManager gm = GameObject.FindObjectOfType<GhostManager>();
                                            if(gm != null)
                                            {

                                            }
                                        }
                                    });
                                });
                            }
                            

                            DynUI.Button(listElement, (b) =>
                            {
                                RectTransform buttonRect = b.GetComponent<RectTransform>();

                                float size = listElement.sizeDelta.x;
                                if (spawnToggles)
                                    size -= 40f;
                                buttonRect.sizeDelta = new Vector2(size, 35f);
                                Text buttonText = b.GetComponentInChildren<Text>();

                                buttonText.text = $"{metadata.Title} | {metadata.GetTimeString()} | {metadata.GetFileName()}";

                                b.onClick.AddListener(() =>
                                {
                                    runList.gameObject.SetActive(false);
                                    runEditMenu.gameObject.SetActive(true);

                                    bool editable = (metadata.SteamID == SteamClient.SteamId.Value);
                                    bool isDirty = false;

                                    runEditTitle.SetTextWithoutNotify(metadata.Title);
                                    runEditTitle.interactable = editable;

                                    string updatedTitle = metadata.Title;
                                    runEditTitle.onEndEdit.RemoveAllListeners();
                                    if (editable)
                                    {
                                        runEditTitle.onEndEdit.AddListener((v) =>
                                        {
                                            updatedTitle = v;
                                            isDirty = true;
                                        });
                                    }

                                    runEditDescription.SetTextWithoutNotify(metadata.Description);
                                    runEditDescription.interactable = editable;

                                    string updatedDesc = metadata.Description;
                                    runEditDescription.onEndEdit.RemoveAllListeners();
                                    if (editable)
                                    {
                                        runEditDescription.onEndEdit.AddListener((v) =>
                                        {
                                            updatedDesc = v;
                                            isDirty = true;
                                        });
                                    }

                                    runEditLevelName.text = metadata.LevelName;
                                    runEditDate.text = metadata.DateCreated.ToString("MM/dd/yyyy hh:mm:ss");

                                    Friend f = new Friend(metadata.SteamID);
                                    runEditRunnerName.text = f.Name;

                                    backButton.onClick.RemoveAllListeners();
                                    backButton.onClick.AddListener(() =>
                                    {
                                        runEditMenu.gameObject.SetActive(false);
                                        runList.gameObject.SetActive(true);
                                    });

                                    saveButton.onClick.RemoveAllListeners();
                                    if (editable)
                                    {
                                        saveButton.onClick.AddListener(() =>
                                        {
                                            if (isDirty)
                                            {
                                                isDirty = false;
                                                metadata.Title = updatedTitle;
                                                metadata.Description = updatedDesc;
                                                GhostFileManager.UpdateMetadata(metadata);
                                            }
                                        });
                                    }

                                    deleteButton.onClick.RemoveAllListeners();
                                    deleteButton.onClick.AddListener(() =>
                                    {
                                        GhostFileManager.DeleteRun(metadata);
                                        runList.gameObject.SetActive(true);
                                        runEditMenu.gameObject.SetActive(false);

                                        metadatas.Remove(metadata);
                                        //Remove the button.
                                        GameObject.Destroy(b.gameObject);

                                        Canvas.ForceUpdateCanvases();
                                    });

                                    runTime.text = $"TIME: {metadata.GetTimeString()}";
                                    runStyle.text = $"STYLE: {metadata.StatGoal.Style}";
                                    runKills.text = $"KILLS: {metadata.StatGoal.Kills}";
                                    runDeaths.text = $"DEATHS: {metadata.StatGoal.Deaths}";
                                });
                            });

                        });

                        
                    }

                }

            });



        }




        public ConfiggableAttribute GetDescriptor()
        {
            return configgableAttribute;
        }

        public void OnMenuClose()
        {

        }

        public void OnMenuOpen()
        {

        }
    }
}
