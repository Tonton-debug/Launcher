using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Update
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
   
                foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Name != "mainDLL.dll" && fileInfo.Name != "launcher.zip" && fileInfo.Name != "ProgramSettings.bin"&& fileInfo.Name != "Update.pdb" && fileInfo.Name != "Update.dll" && fileInfo.Name != "Update.exe"&&
                        fileInfo.Name != "Update.runtimeconfig.dev.json" && fileInfo.Name != "Update.runtimeconfig.json" && fileInfo.Name != "Update.deps.json")
                        File.Delete(file);
                }
                ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "/launcher.zip", Directory.GetCurrentDirectory() + "/");
                foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()+"/launcher/"))
                {
                    FileInfo fileInfo = new FileInfo(file);
                   
                    File.Move(file, Directory.GetCurrentDirectory() + "/"+fileInfo.Name);
                }
               Process.Start(Directory.GetCurrentDirectory() + @"\Launcher.exe");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
           
        }
    }
}
