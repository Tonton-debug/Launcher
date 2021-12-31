using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Launcher
{
    [Serializable]
    public  class ProgramSettings 
    {
        private List<Mod> _myMods = new List<Mod>();
        private List<Mod> _downloadMods = new List<Mod>();
        public int Version { get; set; }
        public bool HasChosenMod(out Mod get)
        {
            get = _downloadMods.Find((t) => t.ChoseMod);
            return get != null;
        }
        public void AddMod(Mod mod,bool isMyMode)
        {
            if (isMyMode)
                _myMods.Add(mod);
            else
                _downloadMods.Add(mod);

        }
        public void RemoveMod(string name, bool isMyMode)
        {
            if (isMyMode)
                _myMods.Remove(_myMods.Find((t)=>t.Name==name));
            else
                _downloadMods.Remove(_downloadMods.Find((t) => t.Name == name));
        }
        public List<Mod> GetListAllMods(bool isMyMode)
        {
            if (isMyMode)
                return _myMods;
            else
                return _downloadMods;
        }
        public bool HasMods(bool isMyMode)
        {
            if (isMyMode)
                return _myMods.Count!=0;
            else
                return _downloadMods.Count != 0;
        }
        public Mod GetMod(string name,bool isMyMode)
        {
            if (isMyMode)
              return  _myMods.Find((t) => t.Name == name);
            else
                return _downloadMods.Find((t) => t.Name == name);
        }
        public void SaveSettings(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, this);
            }
        }
        public static ProgramSettings LoadSettings(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
              return (ProgramSettings)formatter.Deserialize(fs);
            }
        }
    }
}
