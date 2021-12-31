using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;
namespace Server
{
    public enum ActiveWork
    {
        
    }
  public  class ClientObject:Client
    {

        //0-LoadActiveVersionsGame
        //1-LoadGame
        //2- ChekID
        //3-SendAuthorAndDescription
        private ModInfo _localModInfo = new ModInfo();
        private byte[] receiveBuffer=new byte[1] { 255 };
        public ClientObject(TcpClient tcpClient):base(tcpClient)
        {
           
            Console.WriteLine("Клиент {0} подключился", Ip);
            Work();
        }
       
        public void EndRead(IAsyncResult result)
        {
            try
            {
                int ID = receiveBuffer[0];

             //   Stream.ReadTimeout = 5000;
               


                switch (ID)
                {
                    case 0:
                        Console.WriteLine("{0} - Загрузка версий игры", Ip);
                        LoadActiveVersionsGame();

                        break;
                    case 1:
                        Console.WriteLine("{0} - Скачивание игры", Ip);
                        LoadGame();
                        break;
                    case 2:
                        Console.WriteLine("{0} - Проверка имени и пароля", Ip);
                        CheckIDAndPassword();
                        break;
                    case 3:
                        Console.WriteLine("{0} - Отправка мода на сервер", Ip);
                        SendFileOnServer();
                        break;
                    case 4:
                        Console.WriteLine("{0} - Удаление мода с сервера", Ip);
                        DeleteMod();
                        break;
                    case 5:
                        Console.WriteLine("{0} - Скачивание списка всех модов", Ip);
                        LoadListMods();
                        break;
                    case 6:
                        Console.WriteLine("{0} - Скачивание мода", Ip);
                        LoadModFromServer();
                        break;
                    case 7:
                        Console.WriteLine("{0} - Установка новой версии", Ip);
                        LoadLauncher();
                        break;
                }
              
                Work();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}\n{1}",e.Message,e.ToString());
                MainServer.KillClient(this,TypeClient.BaseClient);
            }
        }
        
        private byte[] GetVersions()
        {
            List<string> versions=new List<string>();
            foreach (var item in Directory.GetDirectories(MainServer.PATH + "Game"))
            {
                versions.Add(item.Replace(MainServer.PATH + "Game"+ MainServer.DELIMITER, ""));
            }
            List<byte> return_bytes = new List<byte>();
            foreach (var item in versions)
            {
                foreach (var bytes in Encoding.Unicode.GetBytes(item+":"))
                {
                    return_bytes.Add(bytes);
                }
               
            }
            return return_bytes.ToArray();
        }
        private bool SizeLessThanMaximumSize(out byte[] resultBytes,int maxSize)
        {
            int bytesRead;
           resultBytes = new byte[maxSize];
            bytesRead = Stream.Read(resultBytes, 0, resultBytes.Length);

            while (Stream.DataAvailable)
            {
                Stream.ReadByte();
            }
            if (bytesRead == maxSize)
                return true;
            resultBytes = resultBytes.ToList().Take(bytesRead).ToArray();
            return false;
        }
        private void LoadLauncher()
        {
            byte[] get_version_bytes = GetDataOld();
            string version = Encoding.Unicode.GetString(get_version_bytes.ToArray());
            if (MainServer.SettingsServer.Version!= int.Parse(version))
            {
                Stream.WriteByte(1);
                byte[] send_bytes;
                using (var fileStream = new FileStream(MainServer.PATH+"launcher.zip", FileMode.Open, FileAccess.Read))
                {
                    send_bytes = new byte[(int)fileStream.Length];
                    fileStream.Read(send_bytes, 0, (int)fileStream.Length);
                }
                SendData(Encoding.Unicode.GetBytes(send_bytes.Length.ToString()), true);
                SendData(send_bytes, true);
                SendData(Encoding.Unicode.GetBytes(MainServer.SettingsServer.Version.ToString()), false);
            }
            else
            {
                Stream.WriteByte(0);
            }
        }
        private void LoadGame()
        {
           byte[] get_version_bytes =GetDataOld();
            Stream.WriteByte(1);
            string version = Encoding.Unicode.GetString(get_version_bytes.ToArray());
            byte[] send_bytes;
            using (var fileStream=new FileStream(Directory.GetFiles(MainServer.PATH + "Game" + MainServer.DELIMITER + version + MainServer.DELIMITER)[0],FileMode.Open,FileAccess.Read))
            {
                send_bytes = new byte[(int)fileStream.Length];
                fileStream.Read(send_bytes, 0, (int)fileStream.Length);
            }
         
            if (Stream.ReadByte() == 1)
            {
                Console.WriteLine(MainServer.PATH + "Game" + MainServer.DELIMITER + version + MainServer.DELIMITER + "\n" + send_bytes.Length);
                Stream.WriteByte(1);
            
                SendData(Encoding.Unicode.GetBytes(send_bytes.Length.ToString()), true);
                SendDataAsync(send_bytes);
               
            }

        }
        private void LoadActiveVersionsGame()
        {
            byte[] get_bytes = GetVersions();
            SendData(get_bytes, false);
           
        }
        private void SendFileOnServer()
        {

            byte[] getAuthorAndDescriptionBytes = GetDataOld();
            if (getAuthorAndDescriptionBytes.Length >= 200)
            {
                Stream.WriteByte(3);
                return;
            }
            //if (SizeLessThanMaximumSize(out getAuthorAndDescriptionBytes, 200))
            //{
            //    Stream.WriteByte(2);
            //    return;
            //}
            string getAuthorAndDescription = Encoding.Unicode.GetString(getAuthorAndDescriptionBytes.ToArray());
            if (_localModInfo.Password != getAuthorAndDescription.Split('~')[2])
            {
                Stream.WriteByte(2);
                return;
            }
            Stream.WriteByte(0);
            int maxSize = int.Parse(Encoding.Unicode.GetString(GetDataOld()));
            int sizeLocal = 0;
            int size;
            if (maxSize >= 1000000)
            {
                Stream.WriteByte(1);
                return;
            }
           
            Console.WriteLine("DSD");
            if (File.Exists(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + _localModInfo.NameFileBytes + ".dll"))
            {
                    _localModInfo.Version++;
                File.Delete(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + _localModInfo.NameFileBytes + ".dll");
            }
                
            using (var fileStream = new FileStream(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + _localModInfo.NameFileBytes + ".dll", FileMode.OpenOrCreate, FileAccess.Write))
            {
                do
                {
                    byte[] localBufer = new byte[512];
                    size = Stream.Read(localBufer, 0, localBufer.Length);
                    localBufer = localBufer.Take(size).ToArray();
                  fileStream.Write(localBufer,0,localBufer.Length);
                    Console.WriteLine(sizeLocal);
                    sizeLocal += size;

                } while (sizeLocal < maxSize);
            }
            
            //if (SizeLessThanMaximumSize(out getDLLFileDataBytes, 1000000))
            //{
            //    Stream.WriteByte(1);
            //    return;
            //}           

            Console.WriteLine("Файл получен");
            _localModInfo.Author = getAuthorAndDescription.Split('~')[0];
            _localModInfo.Description = getAuthorAndDescription.Split('~')[1];
            
            
            _localModInfo.SaveModInfo(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + _localModInfo.NameFileBytes + ".json");

            Stream.WriteByte(0);
        }
        private void CheckIDAndPassword()
        { 
            byte[] getFileID;
            bool k = SizeLessThanMaximumSize(out getFileID, 100);
            if (k)
            {               
                Stream.WriteByte(2);
                
                return;
            }
            
            string idFile = Encoding.Unicode.GetString(getFileID.ToArray()).Split('~')[0];
            string passwordFile = Encoding.Unicode.GetString(getFileID.ToArray()).Split('~')[1];
           // Console.WriteLine(Encoding.Unicode.GetBytes(idFile) + ".json");
            if (File.Exists(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + StringToBytesString(Encoding.Unicode.GetBytes(idFile)) + ".json"))
            {
                _localModInfo = ModInfo.LoadModInfo(File.ReadAllText(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + StringToBytesString(Encoding.Unicode.GetBytes(idFile)) + ".json"));
                if (_localModInfo.Password == passwordFile)
                {
                    Stream.WriteByte(1);
                    byte[] bytesAuthorAndDescription = Encoding.Unicode.GetBytes(_localModInfo.Author + "~" + _localModInfo.Description);
                    SendData(bytesAuthorAndDescription, false);
                  
                }
                else
                    Stream.WriteByte(2);
            }
            else
            {
                Stream.WriteByte(0);
                _localModInfo = new ModInfo();
                _localModInfo.NameFile = idFile;
                _localModInfo.NameFileBytes = StringToBytesString(Encoding.Unicode.GetBytes(idFile));
                _localModInfo.Password = passwordFile;
                _localModInfo.Version = 0;
            }
        }
        private void DeleteMod()
        {
            byte[] getPasswordBytes = GetDataOld();
            string getPassword = Encoding.Unicode.GetString(getPasswordBytes.ToArray());
            if (_localModInfo.Password != getPassword)
            {
                Stream.WriteByte(2);
                return;
            }
            Console.WriteLine(_localModInfo.NameFileBytes);
            File.Delete(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + _localModInfo.NameFileBytes + ".dll");
            File.Delete(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + _localModInfo.NameFileBytes + ".json");
            Stream.WriteByte(0);
        }
        private void LoadListMods()
        {
          
            int size = Directory.GetFiles(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER).Length;
            if (size == 0)
                Stream.WriteByte(0);
            else
                Stream.WriteByte(1);
            string resultString = "";
            for (int q = 0; q < size; q++)
            {
                string item = Directory.GetFiles(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER)[q];
              
                if (new FileInfo(item).Extension == ".json")
                {
                    ModInfo _modInfo = ModInfo.LoadModInfo(File.ReadAllText(item));
                    Console.WriteLine("Sending mod:{0} {1} {2}\n{3}/{4}", _modInfo.Author, _modInfo.NameFile, _modInfo.Description,q,size);
                    resultString+= _modInfo.Author + "~" + _modInfo.NameFile + "~" + _modInfo.Description + "~" + _modInfo.Version + "|";
                  
                }
                
            }
            byte[] sendTextBytes = Encoding.Unicode.GetBytes(resultString);
            Stream.WriteByte(0);
            SendData(sendTextBytes, false);

        }
        private void LoadModFromServer()
        {
            byte[] getFileNameBytes;
            string fileName;
            SizeLessThanMaximumSize(out getFileNameBytes, 100);
            fileName = Encoding.Unicode.GetString(getFileNameBytes);
            byte[] finalNameBytes = Encoding.Unicode.GetBytes(fileName);
            fileName = "";
            foreach (var item in finalNameBytes)
            {
                fileName += item;
            }
            if (File.Exists(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + fileName + ".dll"))
            {
                Stream.WriteByte(0);
                byte[] getDllBytes= File.ReadAllBytes(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + fileName + ".dll");
                SendData(Encoding.Unicode.GetBytes(getDllBytes.Length.ToString()), true);
               
                SendData(getDllBytes,false);
                ModInfo modInfo = ModInfo.LoadModInfo(File.ReadAllText(MainServer.PATH + MainServer.DELIMITER + "Mods" + MainServer.DELIMITER + fileName + ".json"));
                if (Stream.ReadByte() == 1)
                {
                    byte[] getJsonBytes = Encoding.Unicode.GetBytes(modInfo.NameFile + "~" + modInfo.Author + "~" + modInfo.Description + "~" + modInfo.Version);
                    SendData(getJsonBytes, false);
                  
                }
            }
            else
                Stream.WriteByte(1);
        }

        public void Work()
        { 
                AsyncCallback asyncCallback = new AsyncCallback(EndRead);
                Stream.BeginRead(receiveBuffer, 0, 1, asyncCallback, null);            
        }
    }
}
