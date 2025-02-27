using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EasyPZ.Ghosts
{
    public class GhostRecording
    {
        public const int FILE_TYPE_VERSION = 2;

        public int fileTypeVersion = FILE_TYPE_VERSION;
        public GhostRecordingMetadata Metadata;
        public List<GhostRecordingFrame> frames;

        public float GetTotalTime()
        {
            return frames[frames.Count - 1].time;
        }

        public GhostRecording()
        {
            Metadata = new GhostRecordingMetadata();
            frames = new List<GhostRecordingFrame>();
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

        public GhostRecordingFrame[] GetNearestTwoFrames(float time)
        {
            GhostRecordingFrame[] result = new GhostRecordingFrame[2];

            if (frames.Count < 2)
                return null;

            float totalTime = GetTotalTime();

            if (time >= frames[frames.Count - 1].time)
                return new GhostRecordingFrame[2] { frames[frames.Count - 2], frames[frames.Count - 1] };

            if (time <= 0f)
                return new GhostRecordingFrame[2] { frames[0], frames[1] };

            for (int i = 1; i < frames.Count; i++)
            {
                GhostRecordingFrame lastFrame = frames[i - 1];
                GhostRecordingFrame currentFrame = frames[i];

                if (lastFrame.time <= time && currentFrame.time >= time)
                {
                    result[0] = lastFrame;
                    result[1] = currentFrame;
                    return result;
                }

            }

            return null;
        }

        public static GhostRecording LoadFromBytes(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    int fileTypeVersion = br.ReadInt32();
                    return GhostFileReaderFactory.GetReader(fileTypeVersion).Read(br);
                }
            }
        }

        public static GhostRecordingMetadata LoadMetadataOnlyFromFilePath(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File {filePath} does not exist.");

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int fileVersionType = br.ReadInt32();
                        return GhostFileReaderFactory.GetReader(fileVersionType).ReadMetadata(br);
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

    public class GhostRecordingFrame
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
    }

    public class GhostRecordingMetadata
    {
        public string LevelName;
        public int Difficulty;
        public ulong SteamID;
        public string RunnerName;
        public Color RunnerColor;

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

        public void Write(BinaryWriter bw)
        {
            bw.Write(LevelName);
            bw.Write(Difficulty);
            bw.Write(SteamID);
            bw.Write(RunnerName);

            bw.Write(RunnerColor.r);
            bw.Write(RunnerColor.g);
            bw.Write(RunnerColor.b);

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
