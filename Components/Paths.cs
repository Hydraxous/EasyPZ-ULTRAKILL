using Configgy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace EasyPZ
{
    internal static class Paths
    {
        public static string ExecutionPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string DataFolder => Path.Combine(ExecutionPath, "GhostRecordings");

        [Configgable("Ghosts/Recording", "Recording Path")]
        public static ConfigInputField<string> ghostRecordingPath = new ConfigInputField<string>(DataFolder);

        [Configgable("Ghosts/Recording", "Open Recordings Folder")]
        public static void OpenDataFolder()
        {
            Application.OpenURL(ghostRecordingPath.Value);
        }
    }
}
