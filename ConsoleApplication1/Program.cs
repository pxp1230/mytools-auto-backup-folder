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
        static void Main(string[] args)
        {
            //测试：beifen 源目录 源根目录 目标根目录
#if DEBUG
            args = new string[2];
            args[0] = @"F:\pxp1230.github.io";
            args[1] = @"pxp1230.github.io";
#endif
            //检验参数是否正确
            int offset = 0;
            if (args.Length >= 1)
            {
                //预留的参数位置
                if (args[0] == "-XXX")
                {
                    offset++;
                }
            }
            string from = null, fromRoot = null, toRoot = null;
            if ((args.Length - offset) == 2)
            {
                from = args[0 + offset];
                fromRoot = args[0 + offset];
                if (args[1 + offset].Length < 2 || args[1 + offset][1] != ':')
                    toRoot = Environment.CurrentDirectory + @"\" + args[1 + offset];
                else
                    toRoot = args[1 + offset];
            }
            else if ((args.Length - offset) == 3)
            {
                from = args[0 + offset];
                fromRoot = args[1 + offset];
                if (args[2 + offset].Length < 2 || args[2 + offset][1] != ':')
                    toRoot = Environment.CurrentDirectory + @"\" + args[2 + offset];
                else
                    toRoot = args[2 + offset];
            }
            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(fromRoot) && !string.IsNullOrEmpty(toRoot))
            {
                DirectoryInfo fromDir = new DirectoryInfo(from);
                if (fromDir.Exists && from.Substring(0, Math.Min(from.Length, fromRoot.Length)) == fromRoot)
                {
                    DirectoryInfo to = new DirectoryInfo(toRoot + from.Substring(fromRoot.Length));
                    CopyDirectory(fromDir, to);
                }
                else
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
                File.Delete(to.FullName + "\\" + item);
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
                if (dir[0] == '.')
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
