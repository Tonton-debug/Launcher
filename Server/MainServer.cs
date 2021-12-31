using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.IO.Compression;

namespace Server
{
    public enum TypeClient
    {
        ChatClient,
        BaseClient,
        InstallerClient
    }
    static class MainServer
    {
        public static SettingsServer SettingsServer { get; set; }
        public const int VERSION = 2;
        public const string PATH = "/home/tonton/Debug/";
        public const string DELIMITER=@"/";
        private const int PORT_CLIENT = 4512;
        private const int PORT_CHAT_CLIENT = 4511;
        private const int PORT_INSTALLER_CLIENT = 4510;
        private static TcpListener _listenerClient;
        private static TcpListener _listerChatClient;
        private static TcpListener _listerInstallerClient;
        private static List<ClientObject> _clientObjest = new List<ClientObject>();
        private static List<ChatClient> _chatClients = new List<ChatClient>();
      
        
      
        public static void KillClient<T>(T client,TypeClient typeClient)
        {
           if(client is ClientObject)
            {
                _clientObjest.Remove(client as ClientObject);
                Console.WriteLine("{1} {0} отключился", (client as ClientObject).Ip, typeClient.ToString());
            }else if (client is ChatClient)
            {
                _chatClients.Remove(client as ChatClient);
                Console.WriteLine("{1} {0} отключился", (client as ChatClient).Ip, typeClient.ToString());
            }
               
          
        }
        public static byte[] GetColor()
        {
            Random random = new Random();
           byte[] MyColor = new byte[3];
            do
            {
                random.NextBytes(MyColor);
            }
            while (_chatClients.Find((t) => t.MyColor == MyColor) != null);
            return MyColor;
        }
        public static void SendBytesData(byte[] _getChatBytesData)
        {
            try
            {
                foreach (var item in _chatClients)
                {
                    NetworkStream stream = item.MyClient.GetStream();
                    stream.Write(_getChatBytesData, 0, _getChatBytesData.Length);
                    Console.WriteLine("{0} - отправлено сообщение", item.Ip);
                    Thread.Sleep(1);
                }
            }
            catch
            {

            }
        }
        
       private static void EndAcceptTcpClient(IAsyncResult result)
        {
            try
            {
                TcpClient tcpClient;
                switch ((TypeClient)result.AsyncState)
                {
                    case TypeClient.ChatClient:

                        tcpClient = _listerChatClient.EndAcceptTcpClient(result);
                        ChatClient chatClient = new ChatClient(tcpClient);
                        _chatClients.Add(chatClient);
                        _listerChatClient.BeginAcceptTcpClient(new AsyncCallback(EndAcceptTcpClient), TypeClient.ChatClient);
                        break;
                    case TypeClient.BaseClient:
                        tcpClient = _listenerClient.EndAcceptTcpClient(result);
                        ClientObject clientObject = new ClientObject(tcpClient);
                        _clientObjest.Add(clientObject);
                        _listenerClient.BeginAcceptTcpClient(new AsyncCallback(EndAcceptTcpClient), TypeClient.BaseClient);
                        break;
                    case TypeClient.InstallerClient:
                        tcpClient = _listerInstallerClient.EndAcceptTcpClient(result);
                        new InstallerClient(tcpClient);
                        _listerInstallerClient.BeginAcceptTcpClient(new AsyncCallback(EndAcceptTcpClient), TypeClient.InstallerClient);
                        break;
                    default:
                        break;
                }
            }
            catch
            {

            }
           
          
        }
        private static void StartListersClient()
        {
            _listerInstallerClient = new TcpListener(IPAddress.Any, PORT_INSTALLER_CLIENT);
            _listerInstallerClient.Start();
            _listenerClient = new TcpListener(IPAddress.Any, PORT_CLIENT);
            _listenerClient.Start();
            _listerChatClient = new TcpListener(IPAddress.Any, PORT_CHAT_CLIENT);
            _listerChatClient.Start();
            
            _listenerClient.BeginAcceptTcpClient(new AsyncCallback(EndAcceptTcpClient), TypeClient.BaseClient);
            _listerChatClient.BeginAcceptTcpClient(new AsyncCallback(EndAcceptTcpClient), TypeClient.ChatClient);
            _listerInstallerClient.BeginAcceptTcpClient(new AsyncCallback(EndAcceptTcpClient), TypeClient.InstallerClient);
        }
        private static void ChangeFileSettings()
        {
            if (File.Exists(Directory.GetCurrentDirectory() + DELIMITER+ "settings.json"))
            {
                SettingsServer = JsonSerializer.Deserialize<SettingsServer>(File.ReadAllText(Directory.GetCurrentDirectory() + DELIMITER+"settings.json"));
                ThreadPool.SetMaxThreads(SettingsServer.MaxThreadPool, SettingsServer.MaxAsyncThreadPool);
            }
               
            else
                SettingsServer.Version = 0;
        }
        public static int GetVersion()
        {
            return SettingsServer.Version;
        }
        private static void ZippingFiles()
        {
            foreach (var path in Directory.GetDirectories(PATH + DELIMITER + "Game" + DELIMITER))
            {
                if(!File.Exists(path+DELIMITER+"test.zip"))
                ZipFile.CreateFromDirectory(path+ DELIMITER + "online test", path + DELIMITER + "test.zip");
            }
            if(!File.Exists(PATH+DELIMITER+"launcher.zip"))
                ZipFile.CreateFromDirectory(PATH + DELIMITER + "launcher", PATH + DELIMITER + "launcher.zip");
            Console.WriteLine("End zipping");
        }
        static void Main(string[] args)
        {
            ChangeFileSettings();
            StartListersClient();
            ZippingFiles();


             _chatClients = new List<ChatClient>();
            int maxThread;
            int maxAsyncThread;
            ThreadPool.GetMaxThreads(out maxThread, out maxAsyncThread);
            Console.WriteLine("Версия сервера:{3}\nВерсия лаунчера:{0}\nПотоки:{1},{2}", SettingsServer.Version, maxThread, maxAsyncThread, VERSION);
            Console.WriteLine("Ожидание подключений...");
          
            while (true)
            {
                string commandString = Console.ReadLine();
                commandString = commandString.Replace(" ", "");
                try
                {
                    switch (commandString)
                    {
                        case "clear":
                            Console.Clear();
                            break;
                        case "zip":
                            ZippingFiles();
                            break;
                        case "stop":
                            File.WriteAllText(Directory.GetCurrentDirectory() + DELIMITER + "settings.json", JsonSerializer.Serialize<SettingsServer>(SettingsServer));
                           
                            foreach (var item in _clientObjest)
                            {
                                item.MyClient.Close();
                                item.Stream.Flush();
                                item.Stream.Close();
                            }
                            foreach (var item in _chatClients)
                            {
                                item.MyClient.Close();
                                item.Stream.Flush();
                                item.Stream.Close();
                            }
                            _listerInstallerClient.Stop();

                            _listerChatClient.Stop();
                            _listenerClient.Stop();
                            return;
                        default:
                            if (commandString.StartsWith("version:"))
                            {
                                int version;
                                if (commandString.Split(':').Length > 1 && int.TryParse(commandString.Split(':')[1], out version))
                                {

                                    SettingsServer.Version = version;

                                    Console.WriteLine("Текущая версия лаунчера обновлена");
                                }
                                Console.WriteLine("Версия лаунчера:{0}", SettingsServer.Version);
                            }
                            else if (commandString.StartsWith("ThreadPool:"))
                            {
                                int maxThreadPool;
                                int maxAsync;
                                commandString = commandString.Substring(11);
                                if (commandString.Split(',').Length > 1 && int.TryParse(commandString.Split(',')[0], out maxThreadPool) && int.TryParse(commandString.Split(',')[1], out maxAsync))
                                {
                                    if (ThreadPool.SetMaxThreads(maxThreadPool, maxAsync))
                                    {
                                        SettingsServer.MaxThreadPool = maxThreadPool;
                                        SettingsServer.MaxAsyncThreadPool = maxAsync;
                                        Console.WriteLine("Успешно");
                                    }
                                    else
                                        Console.WriteLine("Не получилось настроить заданное число потоков");
                                }

                            }
                            else if (commandString.StartsWith("DeleteMod:"))
                            {
                                string name;
                                if (commandString.Split(':').Length > 1)
                                {
                                    name = commandString.Split(':')[1];
                                    if (File.Exists(Directory.GetCurrentDirectory() + DELIMITER + "Mods" + DELIMITER + name + ".json"))
                                    {
                                        File.Delete(Directory.GetCurrentDirectory() + DELIMITER + "Mods" + DELIMITER + name + ".json");
                                        File.Delete(Directory.GetCurrentDirectory() + DELIMITER + "Mods" + DELIMITER + name + ".dll");
                                        Console.WriteLine("Мод удалён");
                                    }
                                   
                                }else
                                    Console.WriteLine("Не удалось найти мод");
                            }
                            else
                                Console.WriteLine("ААААА ЧТО ЭТО ЗА БУКВЫ\nЯ НЕ ПОНИМАЮ АААААА");
                            break;
                    }
                }catch(Exception e)
                {
                    Console.WriteLine("Произошла ошибка\n{0}", e);
                }
            }
        }
    }
}
