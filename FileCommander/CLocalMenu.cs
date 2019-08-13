using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCommander
{
    class CLocalMenu
    {
        private List<string> items;
        private int left, top, width, height;
        private  int maxWidth;
        private int maxHeight;
        private int selectedIdx;
        private int start = 0;
        private ConsoleColor bkColor = ConsoleColor.DarkGreen;
        private ConsoleColor frColor = ConsoleColor.Gray;
        private ConsoleColor selBkColor = ConsoleColor.Gray;
        ConsoleColor selFrColor = ConsoleColor.Black;

        public CLocalMenu(int left, int top, int maxWidth, int maxHeight, params string[] items)
        {
            this.left = left;
            this.top = top;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            this.items = items.ToList();
            height = items.Length+2;
            if (height > maxHeight) height = maxHeight;
            int maxLen = items.Select(s => s.Length).Max();
            width = maxLen+2;
            if (width > maxWidth) width = maxWidth;
            selectedIdx = 0;
        }

        public int Work()
        {
            int choise = -1;
            Console.CursorVisible = false;
            while (true)
            {
                Show();
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                ConsoleKey key = keyInfo.Key;
                ConsoleModifiers modifiers = keyInfo.Modifiers;
                if (key == ConsoleKey.Escape)
                {
                    choise = -1;
                    break;
                }
                if (key == ConsoleKey.Enter)
                {
                    choise = selectedIdx;
                    break;
                }
                if (key == ConsoleKey.UpArrow)
                {
                    if (selectedIdx > 0) selectedIdx--;
                    if (selectedIdx - start == -1)
                    {
                        start--;
                    }
                }
                if (key == ConsoleKey.DownArrow)
                {
                    int N = height - 2;
                    if (selectedIdx < items.Count-1) selectedIdx++;
                    if (selectedIdx - start > N-1)
                    {
                        start++;
                    }
                }
            }
            Console.CursorVisible = true;
            return choise;
        }

        public void Show()
        {
            CCommon.ShowFrame(left, top, width, height, bkColor, frColor);
            ShowItemsList();
        }

        public void ShowItemsList()
        {
            int N = height - 2;
            for (int i=0; i<N; i++)
            {
                if (start + i == selectedIdx)
                {
                    CCommon.ShowLineInPosition(left + 1, top + i + 1, items[start+i], selBkColor, selFrColor);
                }
                else
                {
                    CCommon.ShowLineInPosition(left + 1, top + i + 1, items[start + i], bkColor, frColor);
                }
            }
        }
    }
}
