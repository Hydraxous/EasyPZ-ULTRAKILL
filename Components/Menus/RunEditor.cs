using Configgy;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ.Components
{
    public class RunEditor : MonoBehaviour
    {
        [SerializeField] private RunManagerMenu runManager;

        [SerializeField] private Text runFileName;

        [SerializeField] private Text levelText;
        [SerializeField] private Text runnerNameText;
        [SerializeField] private Text dateCreatedText;

        [SerializeField] private Text goalKillsText;
        [SerializeField] private Text goalStyleText;
        [SerializeField] private Text goalTimeText;
        [SerializeField] private Text goalDeathsText;

        [SerializeField] private GameObject flagsContainer;
        [SerializeField] private GameObject cheatsBubble;
        [SerializeField] private GameObject majorAssistsBubble;
        [SerializeField] private GameObject noDamageBubble;

        [SerializeField] private Button openInFolderButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button setCustomGoalButton;

        [SerializeField] private InputField titleField;
        [SerializeField] private InputField descriptionField;

        [SerializeField] private Text finalRankText;
        [SerializeField] private Image finalRankImage;

        public Button BackButton => backButton;

        public void SetRecording(SessionRecordingMetadata metadata)
        {
            bool steamValid = SteamClient.IsValid;
            bool isOwner = false;

            if (steamValid)
            {
                isOwner = metadata.SteamID == SteamClient.SteamId;
            }

            bool inLevelOfRun = metadata.LevelName == SceneHelper.CurrentScene;

            runFileName.text = "-- "+Path.GetFileNameWithoutExtension(metadata.LocatedFilePath) + " --";
            levelText.text = metadata.LevelName;

            if (SteamClient.IsValid)
                runnerNameText.text = new Friend(metadata.SteamID).Name;
            else
                runnerNameText.text = $"Player ({metadata.SteamID.ToString().Substring(0,5)}...)";

            dateCreatedText.text = metadata.DateCreated.ToString("MM/dd/yyyy hh:mm:ss");

            goalKillsText.text = metadata.StatGoal.Kills.ToString("000");
            goalStyleText.text = metadata.StatGoal.Style.ToString("000");
            goalTimeText.text = metadata.GetTimeString();
            goalDeathsText.text = metadata.StatGoal.Deaths.ToString("000");

            titleField.SetTextWithoutNotify(metadata.Title);
            titleField.onEndEdit.RemoveAllListeners();
            titleField.interactable = isOwner;

            string title = metadata.Title;

            descriptionField.SetTextWithoutNotify(metadata.Description);
            descriptionField.onEndEdit.RemoveAllListeners();
            descriptionField.interactable = isOwner;

            string desc = metadata.Description;
            if (isOwner)
            {
                descriptionField.onEndEdit.AddListener((string text) =>
                {
                    desc = text;
                    saveButton.gameObject.SetActive(desc != metadata.Description || title != metadata.Title);
                });

                titleField.onEndEdit.AddListener((string text) =>
                {
                    title = text;
                    saveButton.gameObject.SetActive(desc != metadata.Description || title != metadata.Title);
                });
            }

            bool assists = metadata.CheatsUsed || metadata.MajorAssistsUsed || metadata.NoDamage;
            
            flagsContainer.SetActive(assists);
            
            cheatsBubble.SetActive(metadata.CheatsUsed);
            majorAssistsBubble.SetActive(metadata.MajorAssistsUsed);
            noDamageBubble.SetActive(metadata.NoDamage);

            openInFolderButton.onClick.RemoveAllListeners();
            openInFolderButton.onClick.AddListener(() =>
            {
                Application.OpenURL(Path.GetDirectoryName(metadata.LocatedFilePath));
            });

            saveButton.onClick.RemoveAllListeners();
            saveButton.gameObject.SetActive(false);

            if (isOwner)
                saveButton.onClick.AddListener(() =>
                {
                    metadata.Title = title;
                    metadata.Description = desc;
                    GhostFileManager.UpdateMetadata(metadata);
                    saveButton.gameObject.SetActive(false);

                    runManager.RebuildMenu();
                });

            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() =>
            {
                ModalDialogue.ShowSimple("WARNING", "You are about to permanently delete this run. Confirm?", (r) =>
                {
                    if (!r)
                        return;

                    //close the editor
                    GhostFileManager.DeleteRun(metadata);
                    runManager.RebuildMenu();
                    gameObject.SetActive(false);
                    runManager.Open();

                }, "Delete", "Cancel");
            });


            setCustomGoalButton.onClick.RemoveAllListeners();
            setCustomGoalButton.gameObject.SetActive(inLevelOfRun);

            if (inLevelOfRun)
            {
                setCustomGoalButton.onClick.AddListener(() =>
                {
                    TrackerManager.SetCustomGoal(metadata.StatGoal);
                    setCustomGoalButton.onClick.RemoveAllListeners();
                });
            }
        }
    }
}
