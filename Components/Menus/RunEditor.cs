using Configgy;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using EasyPZ.Ghosts;

namespace EasyPZ.Components
{
    public class RunEditor : MonoBehaviour
    {
        [SerializeField] private RunManagerMenu runManager;

        [SerializeField] private Text levelText;
        [SerializeField] private Text dateCreatedText;
        [SerializeField] private Text difficultyNameText;

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

        [SerializeField] private InputField fileNameField;
        [SerializeField] private InputField runnerNameField;
        [SerializeField] private InputField titleField;
        [SerializeField] private InputField descriptionField;

        [SerializeField] private Text finalRankText;
        [SerializeField] private Image finalRankImage;

        public Button BackButton => backButton;

        public void SetRecording(GhostRecordingMetadata metadata)
        {
            bool steamValid = SteamClient.IsValid;
            bool isOwner = false;

            if (steamValid)
            {
                isOwner = metadata.SteamID == SteamClient.SteamId;
            }

            bool inLevelOfRun = metadata.LevelName == SceneHelper.CurrentScene;

            levelText.text = metadata.LevelName;

            

            dateCreatedText.text = metadata.DateCreated.ToString("MM/dd/yyyy hh:mm:ss");

            goalKillsText.text = metadata.StatGoal.Kills.ToString("000");
            goalStyleText.text = metadata.StatGoal.Style.ToString("000");
            goalTimeText.text = metadata.GetTimeString();
            goalDeathsText.text = metadata.StatGoal.Deaths.ToString("000");

            difficultyNameText.text = "-- " + ParseDifficultyName(metadata.Difficulty) + " --";

            string fileName = Path.GetFileNameWithoutExtension(metadata.LocatedFilePath);

            fileNameField.SetTextWithoutNotify(fileName);
            fileNameField.onEndEdit.RemoveAllListeners();
            fileNameField.interactable = isOwner;

            titleField.SetTextWithoutNotify(metadata.Title);
            titleField.onEndEdit.RemoveAllListeners();
            titleField.interactable = isOwner;

            string title = metadata.Title;

            descriptionField.SetTextWithoutNotify(metadata.Description);
            descriptionField.onEndEdit.RemoveAllListeners();
            descriptionField.interactable = isOwner;

            string desc = metadata.Description;

            string runnerName = metadata.RunnerName;

            //Possible old file.
            if (string.IsNullOrEmpty(runnerName))
            {
                if (SteamClient.IsValid)
                    runnerName = new Friend(metadata.SteamID).Name;
                else
                    runnerName = $"Player ({metadata.SteamID.ToString().Substring(0, 5)}...)";
            }
            
            runnerNameField.SetTextWithoutNotify(runnerName);
            runnerNameField.onEndEdit.RemoveAllListeners();
            runnerNameField.interactable = isOwner;
            
            Func<bool> changesMade = () =>
            {
                return desc != metadata.Description || title != metadata.Title || fileName != Path.GetFileNameWithoutExtension(metadata.LocatedFilePath) || runnerName != metadata.RunnerName;
            };

            if (isOwner)
            {
                descriptionField.onEndEdit.AddListener((string text) =>
                {
                    desc = text;
                    saveButton.gameObject.SetActive(changesMade());
                });

                titleField.onEndEdit.AddListener((string text) =>
                {
                    title = text;
                    saveButton.gameObject.SetActive(changesMade());
                });

                fileNameField.onEndEdit.AddListener((string text) =>
                {
                    bool valid = true;

                    valid &= !string.IsNullOrEmpty(text);
                    valid &= text.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

                    if (!valid)
                    {
                        fileNameField.SetTextWithoutNotify(fileName);
                        return;
                    }

                    fileName = text;
                    saveButton.gameObject.SetActive(changesMade());
                });

                runnerNameField.onEndEdit.AddListener((string text) =>
                {
                    runnerName = text;
                    saveButton.gameObject.SetActive(changesMade());
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
                    metadata.RunnerName = runnerName;

                    GhostFileManager.UpdateMetadata(metadata);
                    
                    if(fileName != Path.GetFileNameWithoutExtension(metadata.LocatedFilePath))
                    {
                        GhostFileManager.RenameRun(metadata, fileName);
                    }

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

        private string ParseDifficultyName(int difficulty)
        {
            switch (difficulty)
            {
                case 0:
                    return "HARMLESS";
                case 1:
                    return "LENIENT";
                case 2:
                    return "STANDARD";
                case 3:
                    return "VIOLENT";
                case 4:
                    return "BRUTAL";
                case 5:
                    return "ULTRAKILL MUST DIE";
                default:
                    return "UNKNOWN DIFFICULTY";
            }
        }
    }
}
