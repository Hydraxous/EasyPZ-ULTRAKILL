using Configgy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EasyPZ
{
    public class SessionRecorder : LevelSessionBehaviour
    {
        private SessionRecording recording;

        private float timeStartedRecording;

        [Configgable("Config/Recorder", "Recording Frame Rate")]
        private static ConfigInputField<int> recorderFrameRate = new ConfigInputField<int>(300);
        
        private float lastFrameTime;
        private static float frameTime => 1f / recorderFrameRate.Value;
        protected override void OnSessionUpdate()
        {
            if (Time.time - lastFrameTime < frameTime)
                return;

            lastFrameTime = Time.time;
            recording.frames.Add(new SessionRecordingFrame()
            {
                position = NewMovement.Instance.transform.position,
                rotation = new Vector2(CameraController.Instance.transform.localEulerAngles.x, CameraController.Instance.transform.eulerAngles.y),
                animation = GetAnimationState(),
                time = Time.time-timeStartedRecording,
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

            if(moveInput.y > 0f)
                return 1;

            return 0;

        }


        protected override void OnStartSession()
        {
            timeStartedRecording = Time.time;
            recording = new SessionRecording();
            Debug.Log("STARTED RECORDING!");
        }

        protected override void OnStopSession()
        {
            Debug.Log("STOPPED RECORDING!");
            recording.SetStats(new StatGoal()
            {
                Deaths = StatsManager.Instance.restarts,
                Kills = StatsManager.Instance.kills,
                Style = StatsManager.Instance.stylePoints,
                Seconds = StatsManager.Instance.seconds,
            });
            SaveRecording();
        }

        private void SaveRecording()
        {
            recording.FixTimeOffset();
            string sceneName = SceneHelper.CurrentScene;
            string folderPath = HydraDynamics.DataPersistence.DataManager.GetDataPath("Recordings", sceneName);
            string date = DateTime.Now.ToString("yyyy-MM-dd-mm-ss");
            string ext = ".ukrun";

            string filePath = Path.Combine(folderPath, date + ext);
            Debug.Log($"Saved recording {filePath}");
            File.WriteAllBytes(filePath, recording.ToBytes());
        }
    }

    public class SessionRecording
    {
        public StatGoal stats;
        public List<SessionRecordingFrame> frames;

        public float GetTotalTime()
        {
            return frames[frames.Count - 1].time;
        }

        public SessionRecording()
        {
            frames = new List<SessionRecordingFrame>();
        }

        public void SetStats(StatGoal stats)
        {
            this.stats = stats;
        }

        public void FixTimeOffset()
        {
            float timeAtStart = frames[0].time;

            for(int i = 0; i < frames.Count; i++)
            {
                float time = frames[i].time;
                frames[i].time -= timeAtStart;
            }
        }

        public SessionRecordingFrame[] GetNearestTwoFrames(float time)
        {
            SessionRecordingFrame[] result = new SessionRecordingFrame[2];

            if (frames.Count < 2)
                return null;

            float totalTime = GetTotalTime();

            if(time >= frames[frames.Count-1].time)
                return new SessionRecordingFrame[2] { frames[frames.Count - 2], frames[frames.Count - 1] };

            if(time <= 0f)
                return new SessionRecordingFrame[2] { frames[0], frames[1] };
            
            for(int i = 1; i < frames.Count; i++)
            {
                SessionRecordingFrame lastFrame = frames[i-1];
                SessionRecordingFrame currentFrame = frames[i];

                if(lastFrame.time <= time && currentFrame.time >= time)
                {
                    result[0] = lastFrame;
                    result[1] = currentFrame;
                    return result;
                }

            }

            return null;
        }

        public static SessionRecording LoadFromBytes(byte[] data)
        {
            SessionRecording recording = new SessionRecording();

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    recording.stats.Kills = br.ReadInt32();
                    recording.stats.Deaths = br.ReadInt32();
                    recording.stats.Style = br.ReadInt32();
                    recording.stats.Seconds = br.ReadSingle();
                    
                    int frameCount = br.ReadInt32();
                    SessionRecordingFrame[] frames = new SessionRecordingFrame[frameCount];
                    for (int i = 0; i < frames.Length; i++)
                    {
                        frames[i] = new SessionRecordingFrame();
                        frames[i].time = br.ReadSingle();
                        frames[i].animation = br.ReadInt32();
                        frames[i].position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        frames[i].rotation = new Vector2(br.ReadSingle(), br.ReadSingle());
                    }

                    recording.frames = frames.ToList();
                }
            }

            return recording;
        }

        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(stats.Kills);
                    bw.Write(stats.Deaths);
                    bw.Write(stats.Style);
                    bw.Write(stats.Seconds);
                    bw.Write(frames.Count);
                    for(int i = 0; i < frames.Count; i++)
                    {
                        bw.Write(frames[i].time);
                        bw.Write(frames[i].animation);
                        bw.Write(frames[i].position.x);
                        bw.Write(frames[i].position.y);
                        bw.Write(frames[i].position.z);
                        bw.Write(frames[i].rotation.x);
                        bw.Write(frames[i].rotation.y);
                    }
                }

                return ms.ToArray();
            }
        }
    }

    public class SessionRecordingFrame
    {
        public Vector3 position;
        public Vector2 rotation;
        public int weapon;
        public int animation;
        public float time;
    }   

}
