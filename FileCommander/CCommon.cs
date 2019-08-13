using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCommander
{
    static class CCommon
    {
        static public bool IsDir(string path)
        {
            FileAttributes fileAttributes = File.GetAttributes(path);
            if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }
            return false;
        }

        static public bool IsFile(string path)
        {
            return !IsDir(path);
        }

        static public bool IsDirectoryRoot(string path)
        {
            DirectoryInfo d = new DirectoryInfo(path);
            return d.Parent == null;
        }

        static public string GetParentRoot(string path)
        {
            DirectoryInfo d = new DirectoryInfo(path);
            return d.Parent.FullName;
        }

        static public void CopyDirWithAllContent(string SourcePath, string DestinationPath)
        {
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

            foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
        }

        static public long GetFolderSizeInBytes(string folderFullName)
        {
            long sum = 0;
            try
            {
                string[] D = Directory.GetFiles(folderFullName, "*.*", SearchOption.AllDirectories);
                foreach (string newPath in D)
                {
                    FileInfo fileInfo = new FileInfo(newPath);
                    sum += fileInfo.Length;
                }
            }
            catch { }
            return sum;
        }

        static public long GetNumberOfFilesInFolder(string folderFullName)
        {
            long cnt = 0;
            try
            {
                string[] dirFilesFullNames = Directory.GetFiles(folderFullName, "*.*", SearchOption.AllDirectories);
                cnt = dirFilesFullNames.Length;
            }
            catch { }
            return cnt;
        }

        static public void ShowFrame(int left, int top, int width, int height,
            ConsoleColor bkColor = ConsoleColor.Blue,
            ConsoleColor frColor = ConsoleColor.Black
            )
        {
            char up_left = '\u2554';
            char up_right = '\u2557';
            char down_left = '\u255A';
            char down_right = '\u255D';
            char dbl_vert = '\u2551';
            char dbl_horz = '\u2550';
            string top_str = "" + up_left;
            for (int i = 0; i < width - 2; i++)
            {
                top_str += dbl_horz;
            }
            top_str += up_right;

            ShowLineInPosition(left, top, top_str, bkColor, frColor);
            string mid_str = "" + dbl_vert;
            for (int i = 0; i < width - 2; i++)
            {
                mid_str += " ";
            }
            mid_str += dbl_vert;

            for (int i = 0; i < height - 2; i++)
            {
                ShowLineInPosition(left, top + i + 1, mid_str, bkColor, frColor);
            }
            string bottom_str = "" + down_left;
            for (int i = 0; i < width - 2; i++)
            {
                bottom_str += dbl_horz;
            }
            bottom_str += down_right;

            ShowLineInPosition(left, top + height - 1, bottom_str, bkColor, frColor);
        }

        static public void ShowLineInPosition(int left, int top, string Line,
            ConsoleColor bkColor = ConsoleColor.Blue,
            ConsoleColor frColor = ConsoleColor.Black)
        {
            ConsoleColor tmpBkColor = Console.BackgroundColor;
            ConsoleColor tmpFrColor = Console.ForegroundColor;

            Console.BackgroundColor = bkColor;
            Console.ForegroundColor = frColor;

            Console.SetCursorPosition(left, top);
            Console.Write(Line);

            Console.BackgroundColor = tmpBkColor;
            Console.ForegroundColor = tmpFrColor;
        }

        static public string GetFirstAvaibleDrivePath()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (drive.IsReady)
                {
                    return drive.Name;
                }
            }
            return null;
        }

        static public string[] GeAllAvaibleDrivesPaths()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            List<string> drivesNames = new List<string>();

            foreach (var drive in drives)
            {
                if (drive.IsReady)
                {
                    drivesNames.Add(drive.Name);
                }
            }

            return drivesNames.ToArray();
        }

        static public string[] GetDirectoryItemInfo(string path)
        {
            List<string> results = new List<string>();

            if (IsDir(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                results.Add("Directory");
                results.Add("Name: " + directoryInfo.Name);
                results.Add("CreationTime: " + directoryInfo.CreationTime);
                results.Add("LastWriteTime: " + directoryInfo.LastWriteTime);
            }

            if (IsFile(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                results.Add("File");
                results.Add("Name: " + fileInfo.Name);
                results.Add("Extension: " + fileInfo.Extension);
                results.Add("Size: " + fileInfo.Length);
            }
            return results.ToArray();
        }
    }
}


