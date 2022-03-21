using System;
using System.Collections.Generic;

namespace ClickFree
{
    public static class Constants
    {
        public const string ClickFreeFolderName = "ClickFree";
        public const string WindowsBackupFolderName = "Windows Backup";
        public const string FacebookFolderName = "Photos from Facebook";
        public static string[] AppleFileExtensions = new[] { ".heic" };
        public static string[] IgnoreFileExtensions = new string[0];

        public static List<string> DefaultBackUpFolders = new List<string>(new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
        });
    }
}
