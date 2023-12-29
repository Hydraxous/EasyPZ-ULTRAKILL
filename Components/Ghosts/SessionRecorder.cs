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

        [Configgable("Ghosts/Recording", "Recording Frame Rate")]
        private static ConfigInputField<int> recorderFrameRate = new ConfigInputField<int>(16);

        [Configgable("Ghosts/Recording", "Recording Enabled")]
        private static ConfigInputField<bool> recordingEnabled = new ConfigInputField<bool>(true);

        private bool recordCurrentSession = false;
        private float lastFrameTime;
        private static float frameTime => 1f / recorderFrameRate.Value;

        protected override void OnSessionUpdate()
        {
            if (!recordCurrentSession)
                return;

            if (Time.time - lastFrameTime < frameTime)
                return;

            lastFrameTime = Time.time;
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
            recording.Metadata.SteamID = SteamClient.SteamId.Value;
            recording.Metadata.Difficulty = PrefsManager.Instance.GetInt("difficulty", 0);
            recording.Metadata.LevelName = SceneHelper.CurrentScene;
            recording.Metadata.Description = $"My run of {recording.Metadata.LevelName}";
            recording.Metadata.Title = $"My {recording.Metadata.LevelName} run";

            Debug.Log("STARTED RECORDING!");
        }

        protected override void OnStopSession()
        {
            if (!recordCurrentSession)
                return;

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

            string folderPath = Path.Combine(Paths.ghostRecordingPath.Value, sceneName);

            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string date = DateTime.Now.ToString("yyyy-MM-dd-mm-ss");
            string ext = ".ukrun";

            string filePath = Path.Combine(folderPath, date + ext);
            Debug.Log($"Saved recording {filePath}");
            File.WriteAllBytes(filePath, recording.ToBytes());
        }
    }

    public class SessionRecordingMetadata
    {
        public string LevelName;
        public int Difficulty;
        public ulong SteamID;

        public string Title;
        public string Description;
        public DateTime DateCreated;

        public string ModVersion;
        public string GameVersion;
        public float TotalLength;

        public string LocatedFilePath;

        public StatGoal StatGoal;

        public static SessionRecordingMetadata LoadFromBytes(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    return Read(br);
                }
            }
        }

        public string GetFileName()
        {
            if (string.IsNullOrEmpty(LocatedFilePath))
                return "FILE_PATH_INVALID";

            return Path.GetFileNameWithoutExtension(LocatedFilePath);
        }

        public string GetTimeString(bool letters = false)
        {
            TimeSpan time = TimeSpan.FromSeconds(StatGoal.Seconds);

            if(letters)
            {
                return string.Format("{0:D2}m:{1:D2}s:{2:D3}ms",
                time.Minutes,
                time.Seconds,
                time.Milliseconds);
            }
            else
            {
                return string.Format("{0:D2}:{1:D2}:{2:D3}",
                time.Minutes,
                time.Seconds,
                time.Milliseconds);
            }
        }

        public static SessionRecordingMetadata Read(BinaryReader br)
        {
            SessionRecordingMetadata metaData = new SessionRecordingMetadata();

            metaData.LevelName = br.ReadString();
            metaData.Difficulty = br.ReadInt32();
            metaData.SteamID = br.ReadUInt64();

            metaData.Title = br.ReadString();
            metaData.Description = br.ReadString();
            metaData.DateCreated = new DateTime(br.ReadInt64());

            metaData.ModVersion = br.ReadString();
            metaData.GameVersion = br.ReadString();
            metaData.TotalLength = br.ReadSingle();

            metaData.StatGoal = new StatGoal()
            {
                Kills = br.ReadInt32(),
                Deaths = br.ReadInt32(),
                Style = br.ReadInt32(),
                Seconds = br.ReadSingle(),
            };

            return metaData;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(LevelName);
            bw.Write(Difficulty);
            bw.Write(SteamID);

            bw.Write(Title);
            bw.Write(Description);
            bw.Write(DateCreated.Ticks);

            bw.Write(ModVersion);
            bw.Write(GameVersion);

            bw.Write(TotalLength); //Set externally
            bw.Write(StatGoal.Kills);
            bw.Write(StatGoal.Deaths);
            bw.Write(StatGoal.Style);
            bw.Write(StatGoal.Seconds);
        }

        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    Write(bw);
                }

                return ms.ToArray();
            }
        }
    }

    public class SessionRecording
    {
        public int fileTypeVersion = 0;
        public SessionRecordingMetadata Metadata;
        public List<SessionRecordingFrame> frames;

        public float GetTotalTime()
        {
            return frames[frames.Count - 1].time;
        }

        public SessionRecording()
        {
            Metadata = new SessionRecordingMetadata();
            frames = new List<SessionRecordingFrame>();
        }

        public void SetStats(StatGoal stats)
        {
            Metadata.StatGoal = stats;
        }

        public void FixTimeOffset()
        {
            float timeAtStart = frames[0].time;

            for (int i = 0; i < frames.Count; i++)
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

            if (time >= frames[frames.Count - 1].time)
                return new SessionRecordingFrame[2] { frames[frames.Count - 2], frames[frames.Count - 1] };

            if (time <= 0f)
                return new SessionRecordingFrame[2] { frames[0], frames[1] };

            for (int i = 1; i < frames.Count; i++)
            {
                SessionRecordingFrame lastFrame = frames[i - 1];
                SessionRecordingFrame currentFrame = frames[i];

                if (lastFrame.time <= time && currentFrame.time >= time)
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
                    int fileVersionType = br.ReadInt32();

                    recording.Metadata = SessionRecordingMetadata.Read(br);

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

        public static SessionRecordingMetadata LoadMetadataOnlyFromFilePath(string filePath)
        {
            SessionRecordingMetadata metaData = null;

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File {filePath} does not exist.");

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int fileVersionType = br.ReadInt32();
                        metaData = SessionRecordingMetadata.Read(br);
                        return metaData;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to read file {filePath}");
                Debug.LogException(ex);
            }

            return null;
        }


        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(0);

                    Metadata.TotalLength = GetTotalTime();
                    Metadata.Write(bw);

                    bw.Write(frames.Count);
                    for (int i = 0; i < frames.Count; i++)
                    {
                        frames[i].Write(bw);
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
        public int animation;
        public float time;

        public void Write(BinaryWriter w)
        {
            w.Write(time);
            w.Write(animation);
            w.Write(position.x);
            w.Write(position.y);
            w.Write(position.z);
            w.Write(rotation.x);
            w.Write(rotation.y);
        }

        public SessionRecordingFrame Read(BinaryReader r)
        {
            time = r.ReadSingle();
            animation = r.ReadInt32();
            position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            rotation = new Vector2(r.ReadSingle(), r.ReadSingle());
            return this;
        }
    }

}
