using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Program
    {
        static bool protecthtml = false;
        static void Main(string[] args)
        {
            //测试：beifen 源目录 源根目录1 目标根目录1 ...
#if DEBUG
            args = new string[3];
            args[0] = @"F:\new_desktop\1";
            args[1] = @"F:\new_desktop\1";
            args[2] = @"F:\new_desktop\2";
#endif
            //检验参数是否正确
            int offset = 0;
            if (args.Length >= 1)
            {
                if (args[0] == "-protecthtml")
                {
                    offset++;
                    protecthtml = true;
                }
            }
            if ((args.Length - offset) >= 3 && (args.Length - offset) % 2 == 1)
            {
                DirectoryInfo from = new DirectoryInfo(args[0 + offset]);
                int length = (args.Length - offset) / 2;
                int i = 0;
                for (; i < length; i++)
                {
                    if (from.Exists && args[0 + offset].Substring(0, Math.Min(args[0 + offset].Length, args[2 * i + 1 + offset].Length)) == args[2 * i + 1 + offset])
                    {
                        DirectoryInfo to = new DirectoryInfo(args[2 * i + 2 + offset] + args[0 + offset].Substring(args[2 * i + 1 + offset].Length));
                        CopyDirectory(from, to);
                        break;
                    }
                }
                if (i == length)
                    Failed("失败：源目录必须在源根目录下！");
            }
            else
            {
                Failed("失败：参数不正确！");
            }
            Exit();
        }

        static void Failed(string reason)
        {
            Console.WriteLine(reason);
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        static void Exit()
        {
            Process.GetCurrentProcess().Kill();
        }

        static void CopyDirectory(DirectoryInfo from, DirectoryInfo to)
        {
            if (!to.Exists)
                to.Create();

            FileInfo[] allToFiles = to.GetFiles();
            List<string> allToFiles_names = new List<string>();
            foreach (FileInfo item in allToFiles)
            {
                allToFiles_names.Add(item.Name);
            }

            List<string> allFromMarkdownFiles_names = new List<string>();
            if (protecthtml)
            {
                FileInfo[] allFromMarkdownFiles = to.GetFiles("*.md");
                foreach (FileInfo item in allFromMarkdownFiles)
                {
                    allFromMarkdownFiles_names.Add(item.Name);
                }
            }

            FileInfo[] allFromFiles = from.GetFiles();
            foreach (FileInfo item in allFromFiles)
            {
                string file = item.Name;
                if (allToFiles_names.Contains(file))
                {
                    allToFiles_names.Remove(file);
                    FileInfo newFile = new FileInfo(to.FullName + "\\" + file);
                    if (newFile.LastWriteTime < item.LastWriteTime)
                    {
                        File.Copy(item.FullName, to.FullName + "\\" + file, true);
                    }
                }
                else
                {
                    File.Copy(item.FullName, to.FullName + "\\" + file, true);
                }
            }

            foreach (string item in allToFiles_names)
            {
                if (!protecthtml || !((item == "index.html") || (item.EndsWith(".html") && File.Exists(to.FullName + "\\" + item.Substring(0, item.LastIndexOf('.')) + ".md"))))
                {
                    File.Delete(to.FullName + "\\" + item);
                }
            }


            DirectoryInfo[] allToDirs = to.GetDirectories();
            List<string> allToDirs_names = new List<string>();
            foreach (DirectoryInfo item in allToDirs)
            {
                allToDirs_names.Add(item.Name);
            }

            DirectoryInfo[] allFromDirs = from.GetDirectories();
            foreach (DirectoryInfo item in allFromDirs)
            {
                string dir = item.Name;
                if (dir[0] == '.' || dir == "_git" || dir == "-git")
                    continue;
                if (allToDirs_names.Contains(dir))
                {
                    allToDirs_names.Remove(dir);
                }
                CopyDirectory(item, new DirectoryInfo(to.FullName + "\\" + item.Name));
            }

            foreach (string item in allToDirs_names)
            {
                Directory.Delete(to.FullName + "\\" + item, true);
            }
        }
    }
}
