using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FileCommander
{
    class CPanel
    {
        private int top;
        private int left;
        private int width;
        private int height;
        private int N;
        private int start;
        private int current;
        private string path;
        private string[] directoryItems;
        private bool is_active = false;
        public bool IsActive
        {
            set
            {
                is_active = value;
                ShowListOfItems();
            }
            get { return is_active; }
        }

        public string GetPath()
        {
            return path;
        }

        public void SetPath(string path)
        {
            this.path = path;
            Refresh();
        }

        public string GetCurrentItemPath()
        {
            return directoryItems[current];
        }

        public CPanel(int left, int top, int width, int height)
        {
            this.top = top;
            this.left = left;
            this.width = width;
            this.height = height;
            N = height - 8;
            path = CCommon.GetFirstAvaibleDrivePath();
            RefreshPathFiles();
        }

        public void Show()
        {
            ShowFrame();
            ShowListOfItems();
        }

        string prepareItemLine(int idx, bool needSizeForFolder = false)
        {
            string currentItemFullName = directoryItems[idx];
            CFileInfo fInfo = Get_File_Info(currentItemFullName);

            string name = System.IO.Path.GetFileNameWithoutExtension(fInfo.name);
            if (name.Length > 22) name = name.Substring(0, 19) + "...";
            while (name.Length < 22) name += " ";

            string ext = fInfo.extOrDir;
            if (ext.StartsWith(".")) ext = ext.Remove(0, 1);
            while (ext.Length < 3) ext += " ";

            string size = fInfo.size.ToString();
            if (needSizeForFolder & CCommon.IsDir(currentItemFullName))
            {
                size = CCommon.GetFolderSizeInBytes(currentItemFullName).ToString();
            }

            return name + " " + ext + " " + String.Format("{0,10}", size);
        }

        public void ShowSizeOfCurrentItem()
        {
            string currentLine = prepareItemLine(current, true);
            int lineNumberInPanel = current - start;
            bool isDir = CCommon.IsDir(directoryItems[lineNumberInPanel]);
            bool isFile = CCommon.IsFile(directoryItems[lineNumberInPanel]);

            ConsoleColor tmp = Console.ForegroundColor;
            if (isDir) Console.ForegroundColor = ConsoleColor.White;
            if (isFile) Console.ForegroundColor = ConsoleColor.Green;
            ShowLineInPosition(left + 1, top + 4 + lineNumberInPanel, currentLine, true);
            Console.ForegroundColor = tmp;
        }

        private void ShowFrame()
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

            ShowLineInPosition(left, top, top_str);
            for (int i = 0; i < height - 3; i++)
            {
                Console.SetCursorPosition(left, top + i + 1);
                Console.Write(dbl_vert);
                Console.SetCursorPosition(left + width - 1, top + i + 1);
                Console.Write(dbl_vert);
            }
            string bottom_str = "" + down_left;
            for (int i = 0; i < width - 2; i++)
            {
                bottom_str += dbl_horz;
            }
            bottom_str += down_right;

            ShowLineInPosition(left, top + height - 2, bottom_str);
            string mid_line = "" + '\u255F';
            for (int i = 0; i < width - 2; i++)
            {
                mid_line += '\u2500';
            }
            mid_line += '\u2562';

            ShowLineInPosition(left, top + 2, mid_line);
            ShowLineInPosition(left, top + height - 4, mid_line);
        }

        private void ShowListOfItems()
        {
            Console.CursorVisible = false;
            string titul = "Name                  Type       Size ";
            ShowLineInPosition(left + 1, top + 1, path.PadRight(width - 2, ' '));
            ShowLineInPosition(left + 1, top + 3, titul);
            int NN = Math.Min(N, directoryItems.Length);
            for (int i = 0; i < NN; i++)
            {
                ShowAnItem(i);
            }

            string spaces = new string(' ', width - 2);
            for (int i = NN + 1; i <= N; i++)
            {
                CCommon.ShowLineInPosition(left + 1, top + 3 + i, spaces, ConsoleColor.DarkBlue);
            }
            Console.CursorVisible = true;
        }

        private void ShowAnItem(int i)
        {
            int idx = start + i;
            string itemLine = prepareItemLine(idx);
            bool isSelect = (idx) == current;

            bool isDir = CCommon.IsDir(directoryItems[idx]);
            bool isFile = CCommon.IsFile(directoryItems[idx]);

            ConsoleColor tmp = Console.ForegroundColor;
            if (isDir) Console.ForegroundColor = ConsoleColor.White;
            if (isFile) Console.ForegroundColor = ConsoleColor.Green;

            ShowLineInPosition(left + 1, top + 4 + i, itemLine, isSelect);
            if (isSelect)
            {
                ShowItemInfo(idx);
            }
            Console.ForegroundColor = tmp;
        }

        private void ShowItemInfo(int itemIdx)
        {
            string item = directoryItems[itemIdx];
            if (item == "...")
            {
                CCommon.ShowLineInPosition(left + 1, top + height - 3, new string(' ', width - 2), ConsoleColor.DarkBlue, ConsoleColor.White);
                return;
            }

            string itemInfoLine = "";
            if (CCommon.IsDir(item))
            {
                long dirSize = CCommon.GetFolderSizeInBytes(item);
                long numberOfFiles = CCommon.GetNumberOfFilesInFolder(item);
                itemInfoLine = $"size: {dirSize}  num of files: {numberOfFiles}";
            }

            if (CCommon.IsFile(item))
            {
                FileInfo fileInfo = new FileInfo(item);
                long fileSize = fileInfo.Length;
                itemInfoLine = $"size: {fileSize}";
            }

            itemInfoLine = itemInfoLine.PadRight(width - 2, ' ');
            CCommon.ShowLineInPosition(left + 1, top + height - 3, itemInfoLine, ConsoleColor.DarkBlue, ConsoleColor.White);
        }

        private void ShowLineInPosition(int left, int top, string Line, bool isSelect = false)
        {
            ConsoleColor tmp = ConsoleColor.Black;
            if (isSelect && IsActive)
            {
                tmp = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.Gray;
            }

            Console.SetCursorPosition(left, top);
            Console.Write(Line);

            if (isSelect && IsActive)
            {
                Console.BackgroundColor = tmp;
            }
        }

        private void RefreshPathFiles()
        {
            try
            {
                directoryItems = Directory.GetFileSystemEntries(path);
                var list = new List<string>(directoryItems);

                if (!CCommon.IsDirectoryRoot(path))
                {
                    list.Insert(0, "...");
                }

                directoryItems = list.ToArray();
            }
            catch
            {
                directoryItems = new[] { "..." };
            }
            start = 0;
            current = 0;
        }

        public void Refresh()
        {
            RefreshPathFiles();
            Show();
        }

        private CFileInfo Get_File_Info(string filePath)
        {
            bool isDir = CCommon.IsDir(filePath);

            CFileInfo file_info = new CFileInfo();

            file_info.name = System.IO.Path.GetFileName(filePath);

            file_info.extOrDir = System.IO.Path.GetExtension(filePath);
            if (isDir)
            {
                file_info.extOrDir = "Dir";
            }

            if (!isDir)
            {
                file_info.size = new FileInfo(filePath).Length;
            }
            return file_info;
        }

        public void goDown()
        {
            if (current < directoryItems.Length - 1)
            {
                current++;
                if (current - start > N - 1)
                {
                    start++;
                }
                ShowListOfItems();
            }
        }

        public void goUp()
        {
            if (current > 0)
            {
                current--;
                if (current - start == -1)
                {
                    start--;
                }
                ShowListOfItems();
            }
        }

        public void changeDir()
        {
            string targetDirName = directoryItems[current];
            if (targetDirName == "...")
            {
                path = CCommon.GetParentRoot(path);
            }
            else
            {
                path = targetDirName + "\\";
            }
            RefreshPathFiles();
            Show();
        }

        public void ProcessCurrentItem()
        {
            string selectedItem = directoryItems[current];

            if (CCommon.IsDir(selectedItem))
            {
                changeDir();
            }
            else
            {
                Process.Start(selectedItem);
            }
        }

        public void SelectDrive()
        {
            string[] drvNames = CCommon.GeAllAvaibleDrivesPaths();
            CLocalMenu localMenu = new CLocalMenu(left + 1, top + 3, 20, 5, drvNames);
            int choice = localMenu.Work();
            if (choice != -1)
            {
                SetPath(drvNames[choice].Trim());
            }
            else
            {
                ShowListOfItems();
            }
        }
    }

    struct CFileInfo
    {
        public string name;
        public string extOrDir;
        public long size;
    }
}
