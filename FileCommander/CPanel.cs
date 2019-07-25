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
        private string[] files;
        private bool is_active = false;
        public bool isActive
        {
            set {
                is_active = value;
                Show();
            }
            get { return is_active; }
        }

        public CPanel(int left, int top, int width, int height)
        {
            this.top = top;
            this.left = left;
            this.width = width;
            this.height = height;
            N = height - 7;
            path = CCommon.GetFirstAvaibleDrivePath();
            RefreshPathFiles();
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
            return files[current];
        }

        public void Show()
        {
            ShowFrame();
            ShowListOfItems();
        }

        string prepareItemLine(int idx)
        {
            CFileInfo fInfo = Get_File_Info(files[idx]);
            string name = System.IO.Path.GetFileNameWithoutExtension(fInfo.name);
            if (name.Length > 22) name = name.Substring(0, 19) + "...";
            while (name.Length < 22) name += " ";
            string ext = fInfo.extOrDir;
            if (ext.StartsWith(".")) ext = ext.Remove(0, 1);
            while (ext.Length < 3) ext += " ";
            string size = fInfo.size.ToString();
            return name + " " + ext + " " + String.Format("{0,10}", size);
        }

        private void ShowFrame()
        {
            char up_left = '\u2554';
            char up_right = '\u2557';
            char down_left = '\u255A';
            char down_right = '\u255D';
            char dbl_vert = '\u2551';
            char dbl_horz = '\u2550';
            //-----------------------------------
            string top_str = "" + up_left;
            for(int i=0; i<width-2; i++)
            {
                top_str += dbl_horz;
            }
            top_str += up_right;

            ShowLineInPosition(left, top, top_str);
            //-----------------------------------
            string mid_str = "" + dbl_vert;
            for (int i = 0; i < width - 2; i++)
            {
                mid_str += " ";
            }
            mid_str += dbl_vert;

            for (int i = 0; i < height - 3; i++)
            {
                ShowLineInPosition(left, top+i+1, mid_str);
            }
            //-----------------------------------
            string bottom_str = "" + down_left;
            for (int i = 0; i < width - 2; i++)
            {
                bottom_str += dbl_horz;
            }
            bottom_str += down_right;

            ShowLineInPosition(left, top + height - 2, bottom_str);
            //-----------------------------------
            string mid_line = "" + '\u255F';
            for (int i = 0; i < width - 2; i++)
            {
                mid_line += '\u2500';
            }
            mid_line += '\u2562';

            ShowLineInPosition(left, top + 2, mid_line);
        }

        private void ShowListOfItems()
        {
            string titul = "Name                  Type       Size ";
            ShowLineInPosition(left + 1, top + 1, path);
            ShowLineInPosition(left + 1, top + 3, titul);
            int NN = Math.Min(N, files.Length);
            for (int i=0; i<NN; i++)
            {
                ShowAnItem(i);
            }
        }

        private void ShowAnItem(int i)
        {
            int idx = start + i;
            string itemLine = prepareItemLine(idx);
            bool isSelect = (idx) == current;

            bool isDir = CCommon.IsDir(files[idx]);
            bool isFile = CCommon.IsFile(files[idx]);

            ConsoleColor tmp = Console.ForegroundColor;
            if (isDir) Console.ForegroundColor = ConsoleColor.White;
            if (isFile) Console.ForegroundColor = ConsoleColor.Green;
            ShowLineInPosition(left + 1, top + 4 + i, itemLine, isSelect);
            Console.ForegroundColor = tmp;
        }

        private void ShowLineInPosition(int left, int top, string Line, bool isSelect=false)
        {
            ConsoleColor tmp = ConsoleColor.Black;

            if (isSelect && isActive)
            {
                tmp = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.Gray;
            }
            Console.SetCursorPosition(left, top);
            Console.Write(Line);

            if (isSelect && isActive)
            {
                Console.BackgroundColor = tmp;
            }
        }

        private void RefreshPathFiles()
        {
            files = Directory.GetFileSystemEntries(path);
            var list = new List<string>(files);

            if (!CCommon.IsDirectoryRoot(path))
            {
                list.Insert(0, "...");
            }

            files = list.ToArray();
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
            if (current < files.Length - 1)
            {
                current++;
                if (current-start>N-1)
                {
                    start++;
                }
                Show();
            }
        }

        public void goUp()
        {
            if (current > 0)
            {
                current--;
                if (current-start == -1)
                {
                    start--;
                }
                Show();
            }
        }

        public void changeDir()
        {
            string targetDirName = files[current];
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
            string selectedItem = files[current];

            if (CCommon.IsDir(selectedItem))
            {
                changeDir();
            }
            else
            {
                Process.Start(selectedItem);
            }
        }

        public void SelectDrive() {
            string[] drvNames = CCommon.GeAllAvaibleDrivesPaths();
            CLocalMenu localMenu = new CLocalMenu(left+1, top+3, 20, 5, drvNames);
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
