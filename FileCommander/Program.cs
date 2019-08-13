using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCommander
{
    class Program
    {
        static private readonly int m = 35;
        static private readonly int n = 80;
        static private CPanel panel1;
        static private CPanel panel2;
        static private CPanel active_panel;
        static private CPanel passive_panel;
        static private Process process = null;

        static void Main(string[] args)
        {
            CommanderInit();

            while (true)
            {
                DetermWhichPanelIsActive();

                ConsoleKeyInfo keyInfo = Console.ReadKey();
                ConsoleKey key = keyInfo.Key;
                ConsoleModifiers modifiers = keyInfo.Modifiers;

                if (key == ConsoleKey.Tab)
                {
                    panel1.isActive = !panel1.isActive;
                    panel2.isActive = !panel2.isActive;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    active_panel.goDown();
                }
                else if (key == ConsoleKey.UpArrow)
                {
                    active_panel.goUp();
                }
                else if (key == ConsoleKey.Enter)
                {
                    active_panel.ProcessCurrentItem();
                }
                else if ((key == ConsoleKey.F1) && (modifiers & ConsoleModifiers.Alt) != 0)
                {
                    panel1.SelectDrive();
                }
                else if (key == ConsoleKey.F1)
                {
                    string[] helpLines =
                    {
                        " This is Help",
                        "Alt+F1-list of drives on left panel",
                        "Alt+F2-list of drives on right panel"
                    };
                    CLocalMenu help = new CLocalMenu(1, 10, 50, 25, helpLines);
                    help.Work();
                    panel1.Refresh();
                    panel2.Refresh();
                }
                else if ((key == ConsoleKey.F2) && (modifiers & ConsoleModifiers.Alt) != 0)
                {
                    panel2.SelectDrive();
                }
                else if (key == ConsoleKey.F2)
                {
                    DoRename();
                }
                else if (key == ConsoleKey.F3)
                {
                    string path = active_panel.GetCurrentItemPath();
                    var dirItemInfo = CCommon.GetDirectoryItemInfo(path);
                    CLocalMenu dirItemInfoShow = new CLocalMenu(3, 3, n / 2, m - 5, dirItemInfo);
                    dirItemInfoShow.Work();
                    panel1.Refresh();
                    panel2.Refresh();
                }
                else if ((key == ConsoleKey.F4) && (modifiers & ConsoleModifiers.Shift) != 0) 
                {
                    DoNewFile();
                }
                else if (key == ConsoleKey.F4)
                {
                    active_panel.ProcessCurrentItem();
                }
                else if (key == ConsoleKey.F5)
                {
                    DoCopy();
                }
                else if (key == ConsoleKey.Escape)
                {
                    break;
                }
                else if (key == ConsoleKey.F6)
                {
                    DoMove();
                }
                else if (key == ConsoleKey.F7)
                {
                    DoNewDir();
                }
                else if (key == ConsoleKey.F8)
                {
                    DoDelete();
                }
                else if (key == ConsoleKey.Spacebar)
                {
                    active_panel.ShowSizeOfCurrentItem();
                }
                SetCursorToCommandLine();
            }
        }

        static void CommanderInit()
        {
            Console.Title = "FileCommander";
            Console.WindowHeight = m;
            Console.WindowWidth = n;
            Console.BufferHeight = m;
            Console.BufferWidth = n;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Clear();
            panel1 = new CPanel(0, 0, n / 2, m - 1);
            panel2 = new CPanel(n / 2, 0, n / 2, m - 1);
            panel1.isActive = true;
            panel2.isActive = false;
            panel1.Show();
            panel2.Show();

            ShowHelpLine();
            SetCursorToCommandLine();
        }

        static void DetermWhichPanelIsActive()
        {
            active_panel = panel1;
            passive_panel = panel2;
            if (panel2.isActive)
            {
                active_panel = panel2;
                passive_panel = panel1;
            }
        }

        static private void DoCopy()
        {
            string path1 = panel1.GetPath();
            string path2 = panel2.GetPath();
            if (path1 == path2) return;

            string copedItemWithPath = active_panel.GetCurrentItemPath();
            string dstPath = passive_panel.GetPath() + Path.GetFileName(copedItemWithPath);

            if (CCommon.IsFile(copedItemWithPath))
            {
                File.Copy(copedItemWithPath, dstPath);
            }
            if (CCommon.IsDir(copedItemWithPath))
            {
                CCommon.CopyDirWithAllContent(copedItemWithPath, dstPath);
            }
            passive_panel.Refresh();
        }

        static private void DoMove()
        {
            string srcPath = active_panel.GetPath();
            string dstPath = passive_panel.GetPath();
            if (srcPath == dstPath) return;
            string srcFullName = active_panel.GetCurrentItemPath();
            string dstFullName = dstPath + Path.GetFileName(srcFullName);

            if (CCommon.IsFile(srcFullName))
            {
                try
                {
                    File.Move(srcFullName, dstFullName);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (CCommon.IsDir(srcFullName))
            {
                try
                {
                    DoCopy();
                    Directory.Delete(srcFullName, true);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            panel1.Refresh();
            panel2.Refresh();
        }

        static private void DoRename()
        {
            string oldname = active_panel.GetCurrentItemPath();

            Console.Write("New name: ");
            string newname = active_panel.GetPath() + Console.ReadLine();
            ClearCommandLine();

            if (CCommon.IsFile(oldname))
            {
                File.Move(oldname, newname);
            }
            if (CCommon.IsDir(oldname))
            {
                Directory.Move(oldname, newname);
            }
            active_panel.Refresh();
        }

        static private void DoNewFile()
        {
            Console.Write("New file name: ");
            string newFileName = active_panel.GetPath() + Console.ReadLine();
            ClearCommandLine();

            if (!File.Exists(newFileName))
            {
                var stream = File.Create(newFileName); stream.Close();
                process = Process.Start(newFileName);
                process.EnableRaisingEvents = true;
                process.Exited += RefreshActivePanel;
                active_panel.Refresh();
            }
        }

        static private void DoNewDir()
        {
            Console.Write("New dir name: ");
            string newDirName = active_panel.GetPath() + Console.ReadLine();
            ClearCommandLine();

            if (!Directory.Exists(newDirName))
            {
                Directory.CreateDirectory(newDirName);
                active_panel.Refresh();
            }
        }

        static private void DoDelete()
        {
            string name = active_panel.GetCurrentItemPath();

            if (CCommon.IsFile(name))
            {
                File.Delete(name);
            }
            else if (CCommon.IsDir(name))
            {
                Directory.Delete(name, true);
            }

            active_panel.Refresh();
        }

        static private void SetCursorToCommandLine()
        {
            Console.SetCursorPosition(0, m - 2);
        }

        static private void ClearCommandLine()
        {
            string s = new string(' ', n);
            SetCursorToCommandLine();
            Console.Write(s);
            SetCursorToCommandLine();
        }

        static void RefreshActivePanel(object sender, EventArgs e)
        {
            active_panel.Refresh();
        }

        static private void ShowHelpLine()
        {
            string helpLine = "";
            helpLine += " F1-Help  F2-ReName  F3-View  F4-Edit  F5-Copy  F6-Move  F7-MkDir  F8-Delete";
            CCommon.ShowLineInPosition(0, m - 1, helpLine, ConsoleColor.DarkBlue, ConsoleColor.Gray);
        }
    }
}
