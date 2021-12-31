using System.IO;
using System.Text.Json;
namespace Server
{
    
    class ModInfo
    {
        
       public string NameFile { get; set; }
        public string NameFileBytes { get; set; }
        public string Author { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public static ModInfo LoadModInfo(string file)
        {
            return JsonSerializer.Deserialize<ModInfo>(file);
        }
        public void SaveModInfo(string path)
        {
         File.WriteAllText(path,JsonSerializer.Serialize(this));
        }
    }
}
