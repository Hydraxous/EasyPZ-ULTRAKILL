using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EasyPZ.Ghosts
{
    public class GhostFileReaderV1 : IGhostFileReader
    {
        public GhostRecording Read(BinaryReader br)
        {
            GhostRecording recording = new GhostRecording();
            recording.fileTypeVersion = 1;
            recording.Metadata = ReadMetadata(br);

            int frameCount = br.ReadInt32();

            recording.frames = new List<GhostRecordingFrame>();

            for (int i = 0; i < frameCount; i++)
            {
                recording.frames.Add(ReadFrame(br));
            }
            return recording;
        }

        public GhostRecordingMetadata ReadMetadata(BinaryReader br)
        {
            GhostRecordingMetadata metaData = new GhostRecordingMetadata();

            metaData.LevelName = br.ReadString();
            metaData.Difficulty = br.ReadInt32();
            metaData.SteamID = br.ReadUInt64();
            metaData.RunnerColor = Color.red;

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

        public GhostRecordingFrame ReadFrame(BinaryReader br)
        {
            GhostRecordingFrame frame = new GhostRecordingFrame();
            frame.time = br.ReadSingle();
            frame.animation = br.ReadInt32();
            frame.position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            frame.rotation = new Vector2(br.ReadSingle(), br.ReadSingle());
            return frame;
        }
    }
}
