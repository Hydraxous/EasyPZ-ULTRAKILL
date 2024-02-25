using Configgy;
using EasyPZ.Components;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace EasyPZ.Ghosts
{
    public static class GhostFileManager
    {
        public static List<GhostRecordingMetadata> FetchMetadata()
        {

            string folderPath = Paths.ghostRecordingPath.Value;

            if (!Directory.Exists(folderPath))
            {
                return new List<GhostRecordingMetadata>();
            }

            DirectoryInfo info = new DirectoryInfo(folderPath);

            List<GhostRecordingMetadata> metadatas = new List<GhostRecordingMetadata>();

            foreach (var x in info.GetFiles("*.ukrun", SearchOption.AllDirectories))
            {
                try
                {
                    GhostRecordingMetadata metaData = GhostRecording.LoadMetadataOnlyFromFilePath(x.FullName);
                    metaData.LocatedFilePath = x.FullName;
                    metadatas.Add(metaData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"EASYPZ: Error loading metadata for file {x.FullName}");
                    Debug.LogException(e);
                }
            }

            return metadatas;
        }

        internal static void UpdateMetadata(GhostRecordingMetadata metadata)
        {
            if(string.IsNullOrEmpty(metadata.LocatedFilePath))
            {
                throw new Exception("Cannot update metadata without a file path");
            }

            if (!File.Exists(metadata.LocatedFilePath))
            {
                throw new Exception("File does not exist or was moved.");
            }

            try
            {
                GhostRecording recording = GhostRecording.LoadFromBytes(File.ReadAllBytes(metadata.LocatedFilePath));
                recording.Metadata = metadata;
                File.WriteAllBytes(metadata.LocatedFilePath, recording.ToBytes());
            } catch (Exception e)
            {
                Debug.LogError($"EASYPZ: Error updating metadata for file {metadata.LocatedFilePath}");
                Debug.LogException(e);
            }
        }

        internal static void RenameRun(GhostRecordingMetadata metadata, string newFileName)
        {
            if (string.IsNullOrEmpty(metadata.LocatedFilePath))
            {
                throw new Exception("Cannot update metadata without a file path");
            }

            if (!File.Exists(metadata.LocatedFilePath))
            {
                throw new Exception("File does not exist or was moved.");
            }

            string oldFilePath = metadata.LocatedFilePath;
            string newFilePath = Path.Combine(Path.GetDirectoryName(oldFilePath), newFileName + ".ukrun");

            try
            {
                byte[] bytes = File.ReadAllBytes(oldFilePath);
                File.WriteAllBytes(newFilePath, bytes);

                if (!File.Exists(newFilePath))
                    throw new Exception("Failed to write new file. Unknown issue? File not found?");

                File.Delete(oldFilePath);
                metadata.LocatedFilePath = newFilePath;
            }
            catch (Exception e)
            {
                Debug.LogError($"EASYPZ: Error renaming run for file {oldFilePath}");
                Debug.LogException(e);
            }

            Debug.Log($"EasyPZ: Renamed run {Path.GetFileNameWithoutExtension(oldFilePath)} to {newFileName}");
        }

        internal static void DeleteRun(GhostRecordingMetadata metadata)
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
                    GhostRecordingMetadata metaData = GhostRecording.LoadMetadataOnlyFromFilePath(x.FullName);

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
