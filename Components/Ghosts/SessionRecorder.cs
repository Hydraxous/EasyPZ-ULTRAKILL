using Configgy;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using EasyPZ.Ghosts;

namespace EasyPZ.Components
{
    public class SessionRecorder : LevelSessionBehaviour
    {
        private GhostRecording recording;

        private float timeStartedRecording;

        [Configgable("Ghosts/Recording", "Recording Frame Rate", description:frameRateDescription)]
        private static ConfigInputField<int> recorderFrameRate = new ConfigInputField<int>(16);
        private const string frameRateDescription = "The frame rate at which the ghost will be recorded. Higher frame rates will result in smoother playback, but larger file sizes. Frames can only be recorded as fast as your system can render them, so setting this higher than your max fps won't record additional frames.";

        [Configgable("Ghosts/Recording", "Recording Enabled")]
        private static ConfigToggle recordingEnabled = new ConfigToggle(false);

        [Configgable("Ghosts/Recording", "My Ghost Color", description:defaultGhostColorDescription)]
        private static ConfigColor defaultGhostColor = new ConfigColor(UnityEngine.Color.red);
        private const string defaultGhostColorDescription = "The default color of the ghost in your recorded runs. Please note, this feature is still a work in progress and is not functional yet.";

        private bool recordCurrentSession = false;
        private float lastFrameTime;
        private static float frameTime => 1f / recorderFrameRate.Value;

        private bool cheatsUsedInSession;
        private bool assistsUsedInSession;



        private void Awake()
        {
            if (Data.Cache.AskedAboutRecording)
                return;

            Data.Cache.AskedAboutRecording = true;
            Data.SaveCache();

            ModalDialogue.ShowSimple("Ghosts!?", "EasyPZ will record your movements as you play, save them when you complete a level, and play them back to you as rival ghosts to race against. Would you like to enable this feature? (This can be changed in the EasyPZ options later.)", (r) =>
            {
                recordingEnabled.SetValue(r);
                GhostManager.GhostsEnabled.SetValue(r);
            },
            "Enable Ghosts", "Disable Ghosts");
        }

        protected override void OnSessionUpdate()
        {
            if (!recordCurrentSession)
                return;

            if (Time.time - lastFrameTime < frameTime)
                return;

            lastFrameTime = Time.time;

            if (AssistController.Instance.cheatsEnabled)
            {
                cheatsUsedInSession = true;
            }

            if (AssistController.Instance.majorEnabled)
            {
                assistsUsedInSession = true;
            }

            recording.frames.Add(new GhostRecordingFrame()
            {
                position = NewMovement.Instance.transform.position,
                rotation = new Vector2(CameraController.Instance.transform.localEulerAngles.x, CameraController.Instance.transform.eulerAngles.y),
                animation = GetAnimationState(),
                time = Time.time - timeStartedRecording,
            });
        }

        private int GetAnimationState()
        {
            Vector2 moveInput = InputManager.Instance.InputSource.Actions.Movement.Move.ReadValue<Vector2>();
            bool slideInput = InputManager.Instance.InputSource.Actions.Movement.Slide.IsPressed();
            bool grounded = NewMovement.Instance.gc.onGround;

            if (slideInput)
                return 2;

            if (!grounded)
                return 3;

            if (moveInput.y > 0f)
                return 1;

            return 0;

        }

        protected override void OnStartSession()
        {
            recordCurrentSession = recordingEnabled.Value;
            if (!recordCurrentSession)
            {
                enabled = false;
                return;
            }

            timeStartedRecording = Time.time;
            recording = new GhostRecording();
            
            recording.Metadata.DateCreated = DateTime.Now;
            recording.Metadata.ModVersion = ConstInfo.VERSION;
            recording.Metadata.GameVersion = Application.version;

            recording.Metadata.RunnerColor = defaultGhostColor.Value; 

            if (SteamClient.IsValid)
            {
                recording.Metadata.SteamID = SteamClient.SteamId.Value;
                recording.Metadata.RunnerName = SteamClient.Name;
            }
            else
            {
                recording.Metadata.SteamID = 0;
                recording.Metadata.RunnerName = null;
            }

            recording.Metadata.Difficulty = PrefsManager.Instance.GetInt("difficulty", 0);
            recording.Metadata.LevelName = SceneHelper.CurrentScene;
            recording.Metadata.Description = $"My run of {recording.Metadata.LevelName}";
            recording.Metadata.Title = $"My {recording.Metadata.LevelName} run";

            Debug.Log("Ghost Recording Started");
        }

        protected override void OnStopSession()
        {
            if (!recordCurrentSession)
                return;

            recording.Metadata.NoDamage = !StatsManager.Instance.tookDamage;
            recording.Metadata.CheatsUsed = cheatsUsedInSession;
            recording.Metadata.MajorAssistsUsed = assistsUsedInSession;

            Debug.Log("Ghost Recording Stopped");
            recording.SetStats(new StatGoal()
            {
                Deaths = StatsManager.Instance.restarts,
                Kills = StatsManager.Instance.kills,
                Style = StatsManager.Instance.stylePoints,
                Seconds = StatsManager.Instance.seconds,
            });

            SaveRecording();
            GhostNotifier.Notify();
        }

        private void SaveRecording()
        {
            recording.FixTimeOffset();
            string sceneName = SceneHelper.CurrentScene;

            string folderPath = Path.Combine(Paths.ghostRecordingPath.Value, sceneName);

            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string date = DateTime.Now.ToString("yyyy-MM-dd-mm-ss");
            string ext = ".ukrun";

            string filePath = Path.Combine(folderPath, date + ext);
            Debug.Log($"Saved new ghost: {filePath}");
            File.WriteAllBytes(filePath, recording.ToBytes());
        }
    }

}
