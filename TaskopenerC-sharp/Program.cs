using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace TaskopenerC_sharp
{
    class Program
    {
        static string path = "urls.txt";

        void Show()
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                Console.WriteLine("         "+line);
            }
            Mainloop();
        }

        void Help()
        {
            Console.WriteLine(".show shows all shortcuts");
            Console.WriteLine(".exit closes program");
            Console.WriteLine(".delete deletes shortcut (.delete then enter then write short cut to delete)");
            Console.WriteLine(".add command to add shortcut (shortcut,webbadress/filepath)");
            Console.WriteLine(".cancel cancel current proces(takes you to normal mode)");
        }

        void AddNewShortCut()
        {
            Console.WriteLine("Add mode");
            Console.WriteLine("Add by writing shortcut and then the filepath or the webadress ");
            Console.WriteLine("Like this: ex,www.example.com, or: ex,,C:/example.exe");
            string newInput = Console.ReadLine();
            if (newInput == ".cancel")
                Mainloop();
            if (newInput == ".show")
                Show();
            if (newInput == ".help")
                Help();
            if (newInput == ".add")
                AddNewShortCut();
            if (newInput == ".delete")
                DeleteShortCut();
            if (newInput == ".exit")
                Environment.Exit(0);
            if (newInput == ".clear")
                Console.Clear();

            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.Write(newInput + "\r\n");
            }

        }

        void DeleteShortCut()
        {
            Console.WriteLine("Delete mode");
            Console.WriteLine("Write the shortcut you want to delete");
            string input = Console.ReadLine();
            string[] lines = File.ReadAllLines(path);
            ArrayList newList = new ArrayList();

            if (input == ".cancel")
                Mainloop();
            if (input == ".show")
                Show();
            if (input == ".help")
                Help();
            if (input == ".add")
                AddNewShortCut();
            if (input == ".delete")
                DeleteShortCut();
            if (input == ".exit")
                Environment.Exit(0);
            if (input == ".clear")
                Console.Clear();
            foreach (string line in lines)
            {
                if (input != line.Split(',')[0])
                {
                    newList.Add(line);
                    Console.WriteLine(line);
                }
            }
            System.IO.File.WriteAllText(path, string.Empty);

            foreach (string line in newList)
            {
                using (StreamWriter sw = new StreamWriter(path,true))
                {
                    sw.Write(line+"\r\n");
             
                }
            }

        }

        public void Mainloop()
        {
            Console.WriteLine("Open mode");
            Console.WriteLine(".help for help");
            string input = Console.ReadLine();
            if (input == ".show")
                Show();
            if (input == ".help")
                Help();
            if (input == ".add")
                AddNewShortCut();
            if (input == ".delete")
                DeleteShortCut();
            if (input == ".exit")
                Environment.Exit(0);
            if (input == ".clear")
                Console.Clear();


            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] linesplitted = line.Split(',');
                if (input== linesplitted[0])
                {
                    if (CheckIfShortCut(linesplitted[1]))
                    {
                        Process.Start(GetShortcutTarget(linesplitted[1]));
                    }
                    else
                    {

                        Console.WriteLine(linesplitted[1]);
                        var psi = new ProcessStartInfo
                        {
                            FileName = linesplitted[1],
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                }
            }

            Mainloop();
        }

        private bool CheckIfShortCut(string fileName)
        {
            if (fileName.Contains(".lnk")) { return true; }
            else { return false; }


        }
        private string GetShortcutTarget(string file)
        {
            try
            {
                if (System.IO.Path.GetExtension(file).ToLower() != ".lnk")
                {
                    throw new Exception("Supplied file must be a .LNK file");
                }

                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream))
                {
                    fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                    uint flags = fileReader.ReadUInt32();        // Read flags
                    if ((flags & 1) == 1)
                    {                      // Bit 1 set means we have to
                                           // skip the shell item ID list
                        fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                        uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                        fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                    }

                    long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                                                                 // structure begins
                    uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                    fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                    uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                                                               // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                    fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                                                                                        // base pathname (target)
                    long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                                                                                                        // the base pathname. I don't need the 2 terminating nulls.
                    char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                    var link = new string(linkTarget);

                    int begin = link.IndexOf("\0\0");
                    if (begin > -1)
                    {
                        int end = link.IndexOf("\\\\", begin + 2) + 2;
                        end = link.IndexOf('\0', end) + 1;

                        string firstPart = link.Substring(0, begin);
                        string secondPart = link.Substring(end);

                        return firstPart + secondPart;
                    }
                    else
                    {
                        return link;
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Mainloop();
        }
    }
}
