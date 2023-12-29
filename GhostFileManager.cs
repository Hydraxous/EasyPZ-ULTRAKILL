using Configgy;
using EasyPZ.Components;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace EasyPZ
{
    public static class GhostFileManager
    {
        public static List<SessionRecordingMetadata> FetchMetadata()
        {

            string folderPath = Paths.ghostRecordingPath.Value;

            if (!Directory.Exists(folderPath))
            {
                return new List<SessionRecordingMetadata>();
            }

            DirectoryInfo info = new DirectoryInfo(folderPath);

            List<SessionRecordingMetadata> metadatas = new List<SessionRecordingMetadata>();

            foreach (var x in info.GetFiles("*.ukrun", SearchOption.AllDirectories))
            {
                try
                {
                    SessionRecordingMetadata metaData = SessionRecording.LoadMetadataOnlyFromFilePath(x.FullName);
                    metaData.LocatedFilePath = x.FullName;
                    metadatas.Add(metaData);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return metadatas;
        }

        internal static void UpdateMetadata(SessionRecordingMetadata metadata)
        {
            if(string.IsNullOrEmpty(metadata.LocatedFilePath))
            {
                throw new Exception("Cannot update metadata without a file path");
            }

            if (!File.Exists(metadata.LocatedFilePath))
            {
                throw new Exception("File does not exist or was moved.");
            }

            SessionRecording recording = SessionRecording.LoadFromBytes(File.ReadAllBytes(metadata.LocatedFilePath));
            recording.Metadata = metadata;
            File.WriteAllBytes(metadata.LocatedFilePath, recording.ToBytes());
        }

        internal static void DeleteRun(SessionRecordingMetadata metadata)
        {
            if (string.IsNullOrEmpty(metadata.LocatedFilePath))
            {
                throw new Exception("Cannot update metadata without a file path");
            }

            if (!File.Exists(metadata.LocatedFilePath))
                return;

            File.Delete(metadata.LocatedFilePath);
        }

        [Configgable("Extras/Advanced", "Print Metadata")]
        public static void PrintFileMetaData() 
        {
            string folderPath = Paths.ghostRecordingPath.Value;

            DirectoryInfo info = new DirectoryInfo(folderPath);

            StringBuilder sb = new StringBuilder();

            foreach (var x in info.GetFiles("*.ukrun", SearchOption.AllDirectories))
            {
                try
                {
                    sb.Clear();
                    SessionRecordingMetadata metaData = SessionRecording.LoadMetadataOnlyFromFilePath(x.FullName);

                    sb.AppendLine($"FILE: {x.FullName}");
                    sb.AppendLine($"LEVEL: {metaData.LevelName}");
                    sb.AppendLine($"STEAMID: {metaData.SteamID}");
                    sb.AppendLine($"LENGTH: {metaData.TotalLength}");
                    sb.AppendLine($"GAMEVERSION: {metaData.GameVersion}");
                    sb.AppendLine($"MODVERSION: {metaData.ModVersion}");
                    sb.AppendLine($"TITLE: {metaData.Title}");
                    sb.AppendLine($"DESC: {metaData.Description}");
                    sb.AppendLine($"DIFF: {metaData.Difficulty}");
                    sb.AppendLine();

                    Debug.Log(sb.ToString());
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }
}
