using Configgy;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EasyPZ.Components
{
    public class SessionRecorder : LevelSessionBehaviour
    {
        private SessionRecording recording;

        private float timeStartedRecording;

        [Configgable("Ghosts/Recording", "Recording Frame Rate", description:frameRateDescription)]
        private static ConfigInputField<int> recorderFrameRate = new ConfigInputField<int>(16);
        private const string frameRateDescription = "The frame rate at which the ghost will be recorded. Higher frame rates will result in smoother playback, but larger file sizes. Frames can only be recorded as fast as your system can render them, so setting this higher than your max fps won't record additional frames.";

        [Configgable("Ghosts/Recording", "Recording Enabled")]
        private static ConfigToggle recordingEnabled = new ConfigToggle(true);

        private bool recordCurrentSession = false;
        private float lastFrameTime;
        private static float frameTime => 1f / recorderFrameRate.Value;

        private bool cheatsUsedInSession;
        private bool assistsUsedInSession;

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

            recording.frames.Add(new SessionRecordingFrame()
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
            recording = new SessionRecording();
            
            recording.Metadata.DateCreated = DateTime.Now;
            recording.Metadata.ModVersion = ConstInfo.VERSION;
            recording.Metadata.GameVersion = Application.version;

            if (SteamClient.IsValid)
            {
                recording.Metadata.SteamID = SteamClient.SteamId.Value;
            }
            else
            {
                recording.Metadata.SteamID = 0;
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
