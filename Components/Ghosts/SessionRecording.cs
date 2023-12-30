using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;

namespace EasyPZ.Components
{
    public class SessionRecording
    {
        public const int FILE_TYPE_VERSION = 1;
        public int fileTypeVersion = FILE_TYPE_VERSION;
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
                    recording.fileTypeVersion = br.ReadInt32();

                    switch (recording.fileTypeVersion)
                    {
                        case 0:
                            LoadFileTypeZero(recording, br);
                            break;
                        case 1:
                            LoadFileTypeOne(recording, br);
                            break;
                    }
                }
            }

            return recording;
        }

        private static void LoadFileTypeOne(SessionRecording recording, BinaryReader br)
        {
            recording.Metadata = SessionRecordingMetadata.ReadFileTypeOne(br);

            int frameCount = br.ReadInt32();

            recording.frames = new List<SessionRecordingFrame>();

            for (int i = 0; i < frameCount; i++)
            {
                recording.frames.Add(new SessionRecordingFrame().Read(br));
            }
        }

        private static void LoadFileTypeZero(SessionRecording recording, BinaryReader br)
        {
            recording.Metadata = SessionRecordingMetadata.ReadFileTypeZero(br);

            int frameCount = br.ReadInt32();

            recording.frames = new List<SessionRecordingFrame>();

            for (int i = 0; i < frameCount; i++)
            {
                recording.frames.Add(new SessionRecordingFrame().Read(br));
            }
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
                        switch (fileVersionType) 
                        {
                            case 0:
                                metaData = SessionRecordingMetadata.ReadFileTypeZero(br);
                                break;
                            case 1:
                                metaData = SessionRecordingMetadata.ReadFileTypeOne(br);
                                break;
                        }
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
                    
                    bw.Write(FILE_TYPE_VERSION);

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

        public bool CheatsUsed;
        public bool MajorAssistsUsed;
        public bool NoDamage;

        public string LocatedFilePath; //Non Serialized

        public StatGoal StatGoal;

        public string GetFileName()
        {
            if (string.IsNullOrEmpty(LocatedFilePath))
                return "FILE_PATH_INVALID";

            return Path.GetFileNameWithoutExtension(LocatedFilePath);
        }

        public string GetTimeString(bool letters = false)
        {
            TimeSpan time = TimeSpan.FromSeconds(StatGoal.Seconds);

            if (letters)
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

        public static SessionRecordingMetadata ReadFileTypeOne(BinaryReader br)
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

            metaData.CheatsUsed = br.ReadBoolean();
            metaData.MajorAssistsUsed = br.ReadBoolean();
            metaData.NoDamage = br.ReadBoolean();

            metaData.StatGoal = new StatGoal()
            {
                Kills = br.ReadInt32(),
                Deaths = br.ReadInt32(),
                Style = br.ReadInt32(),
                Seconds = br.ReadSingle(),
            };

            return metaData;
        }

        public static SessionRecordingMetadata ReadFileTypeZero(BinaryReader br)
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

            bw.Write(CheatsUsed);
            bw.Write(MajorAssistsUsed);
            bw.Write(NoDamage);

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
}
