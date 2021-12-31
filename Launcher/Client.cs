using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Threading;

namespace Launcher
{
    public enum ClientActions
    {
        LoadActiveVersionsGame,
        LoadGame,
        CheckID,
        SendFileAntOtherSettings, 
        DeleteFile,
        UpdateListMods,
        InstallMod,
        InstallLauncher

    }
    public class Client
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private MainWindow _mainWindow;
        public Client(string address, int port,MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            try
            {
                _client = new TcpClient();
                _client.Connect(address, port);
                _stream = _client.GetStream();
                _stream.ReadTimeout = 10000;
                _stream.WriteTimeout = 10000;
                SendTextToLog("Подключено к серверу");
            }
            catch (Exception e)
            {
                _mainWindow.Dispatcher.Invoke(
                       () =>
                       {
                           _mainWindow.ErrorConnection(e.Message);
                       }
                       );
                
                _client.Close();
           //     _stream.Close();
                
            }
           
        }
        private void SendTextToLog(string text)
        {
            _mainWindow.Dispatcher.Invoke(
                        () =>
                        {
                            _mainWindow.AddToLog(text);
                        });
        }
        private void DeactiveOrActiveElements(InactiveElement[] inactiveElements, bool isEnable)
        {
            _mainWindow.Dispatcher.Invoke(
                        () =>
                        {
                            _mainWindow.DeactiveOrActiveElements(inactiveElements,isEnable);
                        });
        }
        private void LoadActiveVersionsGame()
        {
            
            string versions=default;
            List<byte> get_bytes = new List<byte>();
            do
            {
                get_bytes.Add((byte)_stream.ReadByte());

            } while (_stream.DataAvailable);
            versions = Encoding.Unicode.GetString(get_bytes.ToArray());
            SendTextToLog(versions);
            _mainWindow.Dispatcher.Invoke(
                       () =>
                       {
                           foreach (var item in versions.Split(':'))
                           {
                               if(item.Length>0)
                               _mainWindow.AddComboBoxItemInVersions(item);
                           }
                           _mainWindow.InstallLauncher();
                       });
        }
        private void LoadGame(object[] args)
        {
          
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.GameLoaderStackPanel,InactiveElement.ModsTabItem, InactiveElement.ProjectTabItem }, false);
            string version = args[0].ToString();
            byte[] versionInBytes = Encoding.Unicode.GetBytes(version);
            _stream.Write(versionInBytes, 0, versionInBytes.Length);
            List<byte> fileBytes = new List<byte>();
            int result = _stream.ReadByte();
            int position = 0;
            int size = 0;
            int maxSize = 0;
          
            if (result == 1)
            {

                _stream.WriteByte(1);
                if (_stream.ReadByte() == 1)
                {
                    byte[] buffer = new byte[4096];
                    size = _stream.Read(buffer, 0, buffer.Length);
                    buffer = buffer.Take(size).ToArray();
                    int sizeFile = int.Parse(Encoding.Unicode.GetString(buffer));
                    size = 0;
                    _stream.WriteByte(1);
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.LoadingGameProgressBar.Maximum = sizeFile;
                        _mainWindow.LoadingGameProgressBar.Value = 0;
                    });
                
                    using (var fileStream =new FileStream(Directory.GetCurrentDirectory() + @"\test.zip", FileMode.OpenOrCreate,FileAccess.ReadWrite))
                    {
                            do
                            {
                               
                                buffer = new byte[4000096];
                           
                                size = _stream.Read(buffer, 0, buffer.Length);
                                buffer = buffer.Take(size).ToArray();
                                fileStream.Write(buffer, 0, buffer.Length);
                                if (maxSize % 1000 < 5)
                                {
                                    _mainWindow.Dispatcher.Invoke(() =>
                                    {
                                        _mainWindow.LoadingGameProgressBar.Value = maxSize;

                                    });
                                }
                            maxSize += size;

                        } while (maxSize< sizeFile);
                        
                        
                    }
                 
                }
            }
            SendTextToLog("Скачиваем архив");
            SendTextToLog("Архив установлен");

            if (File.Exists(Directory.GetCurrentDirectory() + @"\mainDLL.dll"))
            {
                File.Delete(Directory.GetCurrentDirectory() + @"\mainDLL.dll");
            }

            if (Directory.Exists(Directory.GetCurrentDirectory() + @"\online test"))
            {
                SendTextToLog("Удаляем старую версию");
                Directory.Delete(Directory.GetCurrentDirectory() + @"\online test", true);
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\online test");
            }
            SendTextToLog("Распаковываем архив");
            ZipFile.ExtractToDirectory("test.zip", Directory.GetCurrentDirectory() + @"\online test\");
            File.Delete(Directory.GetCurrentDirectory() + @"\test.zip");
            _mainWindow.Dispatcher.Invoke(
                     () =>
                     {
                         _mainWindow.DownloadButton.IsEnabled = true;
                     });
            SendTextToLog("Игра установлена");
            File.Copy(Directory.GetCurrentDirectory() + @"\online test\online game_Data\Managed\Assembly-CSharp.dll", Directory.GetCurrentDirectory() + @"\mainDLL.dll");
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.GameLoaderStackPanel, InactiveElement.ModsTabItem, InactiveElement.ProjectTabItem }, true);
            ModPanel.SetNameChosenModPanel("");
            RunActionAsync(ClientActions.UpdateListMods, null);
        }
        private void InstallMod(object[] args)
        {
            byte[] NameModBytes = Encoding.Unicode.GetBytes(args[0].ToString());

            _stream.Write(NameModBytes, 0, NameModBytes.Length);
            int result = _stream.ReadByte();
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.UpdateListModsButton }, false);
            if ((bool)args.Last() == true)
                DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.MyModsStackPanel }, false);
            else
                DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.OtherModsStackPanel }, false);
            switch (result)
            {
                case 0:
                    List<byte> bytes2 = new List<byte>();
                    byte[] sizeInBytes = new byte[1024];
                    int size = _stream.Read(sizeInBytes, 0, sizeInBytes.Length);
                    int sizeFile = int.Parse(Encoding.Unicode.GetString(sizeInBytes.Take(size).ToArray()));
                    if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\mods"))
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\mods");
                    _stream.WriteByte(1);
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.ModDownloadProgressBar.Maximum = sizeFile;
                        _mainWindow.ModDownloadProgressBar.Value = 0;
                    });
                    using (var fileStream = new FileStream(Directory.GetCurrentDirectory() + @"\mods\" + args[0].ToString() + ".dll", FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        int currentSize = 0;
                        do
                        {
                            byte[] buffer = new byte[8096];
                            int size2 = _stream.Read(buffer, 0, buffer.Length);
                            buffer = buffer.Take(size2).ToArray();
                            fileStream.Write(buffer, 0, buffer.Length);
                            currentSize += size2;
                            _mainWindow.Dispatcher.Invoke(() =>
                            {
                                _mainWindow.ModDownloadProgressBar.Value = currentSize;
                            });
                          
                        } while (currentSize < sizeFile);
                    }
                    SendTextToLog("Файл успешно загружен");
                    _stream.WriteByte(1);
                    List<byte> bytes2Info = new List<byte>();

                    do
                    {
                        bytes2Info.Add((byte)_stream.ReadByte());

                    } while (_stream.DataAvailable);
                    string[] info = Encoding.Unicode.GetString(bytes2Info.ToArray()).Split('~');
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        Mod mod = new Mod();
                        mod.Name = info[0].ToString();
                        mod.Author = info[1].ToString();
                        mod.Description = info[2].ToString();
                        mod.Version = int.Parse(info[3].ToString());
                        if (_mainWindow.MyProgramSettings.GetMod(mod.Name, false) != null)
                        {
                            _mainWindow.MyProgramSettings.RemoveMod(mod.Name, false);
                        }
                        _mainWindow.MyProgramSettings.AddMod(mod, false);
                    });

                    RunActionAsync(ClientActions.UpdateListMods, null);
                    break;
                case 1:
                    SendTextToLog("Не удалось загрузить файл");
                    break;
                default:
                    break;
                    DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.UpdateListModsButton }, true);
            }
            if ((bool)args.Last() == true)
                DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.MyModsStackPanel }, true);
            else
                DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.OtherModsStackPanel }, true);
        }
        private bool FindModWithParameters(string parameter,string parameter2)
        { 
                if (parameter == parameter2)
                    return true;
            if (parameter2.StartsWith(parameter))
                return true;
            foreach (var charInNameMod in parameter2)
                {
                    if (parameter.Contains(charInNameMod))
                    {
                        return true;
                    }
                }
                return false;
        }
        private bool CanGetMod(object[] args,string nameMod,string author)
        {
            if (args != null)
            {
                
                if (args.Length == 2)
                {
                    if (args[1].ToString() == "")
                        return FindModWithParameters(args[0].ToString(), nameMod);
                    else if (args[0].ToString() == "")
                        return FindModWithParameters(args[1].ToString(), author);
                    else if (args[0].ToString() != "" && args[1].ToString() != "")
                        return FindModWithParameters(args[1].ToString(), author) && FindModWithParameters(args[0].ToString(), nameMod);
                }
                return false;
            }
            else
                return true;
        }
        private void UpdateListMods(object[] args)
        {
           
            if (_stream.ReadByte() == 0)
            {
                return;
            }
            _mainWindow.Dispatcher.Invoke(
                    () =>
                    {
                        _mainWindow.OtherModsStackPanel.Children.Clear();
                        _mainWindow.MyModsStackPanel.Children.Clear();
                    });
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.UpdateListModsButton }, false);
            List<byte> bytes2 = new List<byte>();
            _stream.ReadByte();
                do
                {
                    bytes2.Add((byte)_stream.ReadByte());
                } while (_stream.DataAvailable);
             string[] infoFiles = Encoding.Unicode.GetString(bytes2.ToArray()).Split('|');
            
            foreach (var item in infoFiles)
            {
                string[] info = item.Split('~');
                if (item == "")
                    continue;
                _mainWindow.Dispatcher.Invoke(
                     () =>
                     {
                         if (_mainWindow.MyProgramSettings.GetMod(info[1], false) == null)
                         {
                             if (CanGetMod(args, info[1], info[0]))
                             {
                                 _mainWindow.OtherModsStackPanel.Children.Add(new ModPanel(info[0], info[1], info[2], int.Parse(info[3]), false, _mainWindow));
                             }
                         }
                         else
                         {
                             Mod mod = _mainWindow.MyProgramSettings.GetMod(info[1], false);
                             ModPanel modPanel;
                             if (mod.Version != int.Parse(info[3]))
                                 modPanel = new ModPanel(mod.Author, mod.Name, mod.Description, mod.Version, true, _mainWindow, true);
                             else
                                 modPanel = new ModPanel(mod.Author, mod.Name, mod.Description, mod.Version, true, _mainWindow);
                             _mainWindow.MyModsStackPanel.Children.Add(modPanel);
                         }

                     });
            }
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.UpdateListModsButton }, true);
        }
        private void CheckIDAndPassword(object[] arguments)
        {
            string IdAndPassword = arguments[0].ToString() + "~" + arguments[1].ToString();
            byte[] IdAndPassword_bytes = Encoding.Unicode.GetBytes(IdAndPassword);
            _stream.Write(IdAndPassword_bytes, 0, IdAndPassword_bytes.Length);
           
             int result = _stream.ReadByte();
           
            SendTextToLog("result:" + result.ToString());
            switch (result)
            {
                case 0:
                    SendTextToLog("Файл успешно создан");
                    _mainWindow.Dispatcher.Invoke(
                     () =>
                     {
                         _mainWindow.MyModsComboBox.IsEnabled = false;
                         _mainWindow.AuthorWrapPanel.Visibility = Visibility.Visible;
                         Mod mod = new Mod();
                         mod.Name = arguments[0].ToString();
                         mod.Password = arguments[1].ToString();
                         _mainWindow.MyProgramSettings.AddMod(mod,true);
                         _mainWindow.VisualMyModsComboBox();
                     });

                     break;
                case 1:
                    SendTextToLog("Вы вошли в настройки файла!");
                    List<byte> authorAndDescriptionBytes = new List<byte>();
                    do
                    {
                        authorAndDescriptionBytes.Add((byte)_stream.ReadByte());
                    } while (_stream.DataAvailable);
                    string authorAndDescription = Encoding.Unicode.GetString(authorAndDescriptionBytes.ToArray());
                    _mainWindow.Dispatcher.Invoke(
                     () =>
                     {
                      
                         _mainWindow.MyModsComboBox.IsEnabled = false;
                         _mainWindow.AuthorWrapPanel.Visibility = Visibility.Visible;
                         _mainWindow.InfoWrapPanel.Visibility = Visibility.Visible;
                         _mainWindow.AuthorTextBox.Text = authorAndDescription.Split('~')[0];
                         _mainWindow.InfoTextBox.Text = authorAndDescription.Split('~')[1];
                         _mainWindow.SelectFileWrapPanel.Visibility = Visibility.Visible;
                         _mainWindow.ActionWrapPanel.Visibility = Visibility.Visible;
                         _mainWindow.AcceptButton.Visibility = Visibility.Collapsed;

                     });

                    break;
                case 2:
                    SendTextToLog("Ошибка! Возможно вы неверно написали пароль, или вы превысили лимит символов "+ IdAndPassword.Length + @"/" + 50);
                    _mainWindow.Dispatcher.Invoke(()=>_mainWindow.ResetMyProjectPanel());
                    break;
                default:
                    break;
            }
            if (result!=2)
                DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.IdWrapPanel, InactiveElement.PasswordWrapPanel }, false);
        }
        private void SendFile(object[] arguments)
        {
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.ProjectStackPanel, InactiveElement.ModsTabItem,InactiveElement.GameLoaderStackPanel }, false);
            string AuthorAndDescription = arguments[0].ToString() + "~" + arguments[1].ToString()+"~"+ arguments[3].ToString();
            byte[] AuthorAndDescriptionBytes = Encoding.Unicode.GetBytes(AuthorAndDescription);
            _stream.Write(AuthorAndDescriptionBytes, 0, AuthorAndDescriptionBytes.Length);
            byte[] file = File.ReadAllBytes(arguments[2].ToString());
            int result = _stream.ReadByte();
            byte[] sizeFileInBytes =Encoding.Unicode.GetBytes(file.Length.ToString());
            _stream.Write(sizeFileInBytes, 0, sizeFileInBytes.Length);
            Thread.Sleep(10);
            _stream.Write(file, 0, file.Length);
           
           result = _stream.ReadByte();

            switch (result)
            {
                case 1:
                    SendTextToLog("Скорее всего вы загрузили слишком большой файл. Файл не должен превышать 1мб.");
                    break;

                case 0:
                    SendTextToLog("Файл успешно загружен на сервер");
                    break;
                case 2:
                    SendTextToLog("Ты думал, что я не добавлю проверку на пароль? Попробуй другой способ взлома бд");
                    break;
                case 3:
                    SendTextToLog("Слишком длинное описание/название/имя автора\n"+AuthorAndDescription.Length+@"/"+100);
                    break;
                default:
                    break;
            }

            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.ProjectStackPanel, InactiveElement.ModsTabItem, InactiveElement.GameLoaderStackPanel }, true);
            _mainWindow.Dispatcher.Invoke(() => _mainWindow.ResetMyProjectPanel());
        }
        private void DeleteFile(object[] arguments)
        {
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.ProjectStackPanel }, false);
            string AuthorAndDescription = arguments[0].ToString();
            byte[] AuthorAndDescriptionBytes = Encoding.Unicode.GetBytes(AuthorAndDescription);
            _stream.Write(AuthorAndDescriptionBytes, 0, AuthorAndDescriptionBytes.Length);
            int result = _stream.ReadByte();
            switch (result)
            {
                case 0:
                    SendTextToLog("Файл успешно удалён с сервера");
                    break;
                case 2:
                    SendTextToLog("Ты думал, что я не добавлю проверку на пароль? Попробуй другой способ взлома бд");
                    break;
                default:
                    break;
            }
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.ProjectStackPanel }, true);
            _mainWindow.Dispatcher.Invoke(() => { _mainWindow.ResetMyProjectPanel(); _mainWindow.MyProgramSettings.RemoveMod(arguments[1].ToString(), true); _mainWindow.VisualMyModsComboBox(); });
        }
        private void InstallLauncher(object[] arguments)
        {
            byte[] versionInBytes = Encoding.Unicode.GetBytes(arguments[0].ToString());
            _stream.Write(versionInBytes, 0, versionInBytes.Length);
            if (_stream.ReadByte() == 1)
            {
                byte[] sizeInBytes = new byte[1024];
                int size = _stream.Read(sizeInBytes, 0, sizeInBytes.Length);
                sizeInBytes = sizeInBytes.Take(size).ToArray();
                int maxSize = int.Parse(Encoding.Unicode.GetString(sizeInBytes));
                _stream.WriteByte(0);
                int currentSize = 0;
                DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.GameLoaderStackPanel, InactiveElement.ProjectTabItem, InactiveElement.ModsTabItem }, false);
                using (var fileStream=new FileStream(Directory.GetCurrentDirectory() + "/launcher.zip", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    do
                    {
                        byte[] buffer = new byte[4096];
                        size = _stream.Read(buffer, 0, buffer.Length);
                        buffer = buffer.Take(size).ToArray();
                        currentSize += size;
                        fileStream.Write(buffer, 0, buffer.Length);
                    } while (currentSize < maxSize);
                }
                _stream.WriteByte(0);
                byte[] versionInBytes2 = new byte[1024];
                size = _stream.Read(versionInBytes2, 0, versionInBytes2.Length);
                versionInBytes2 = versionInBytes2.Take(size).ToArray();
                string versionInBytes2String = Encoding.Unicode.GetString(versionInBytes2);
                _mainWindow.Dispatcher.Invoke(() => { 
                    _mainWindow.MyProgramSettings.Version = int.Parse(versionInBytes2String);
                    _mainWindow.UpdateLauncher();
                });
            }
        }
        private void RunAction(ClientActions actions, object[] arguments)
        {
            try
            {
                _stream.WriteByte((byte)actions);

                switch (actions)
                {
                    case ClientActions.LoadActiveVersionsGame:
                        LoadActiveVersionsGame();
                        break;
                    case ClientActions.LoadGame:
                        LoadGame(arguments);
                        break;
                    case ClientActions.CheckID:
                        CheckIDAndPassword(arguments);
                        break;
                    case ClientActions.SendFileAntOtherSettings:
                        SendFile(arguments);
                        break;
                    case ClientActions.DeleteFile:
                        DeleteFile(arguments);
                        break;
                    case ClientActions.UpdateListMods:
                        UpdateListMods(arguments);
                        break;
                    case ClientActions.InstallMod:
                        InstallMod(arguments);
                        break;
                    case ClientActions.InstallLauncher:
                        InstallLauncher(arguments);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                _mainWindow.Dispatcher.Invoke(
                      () =>
                      {
                          _mainWindow.ErrorConnection(e.Message);
                      }
                      );

                _client.Close();
            }
            
        }
       
        public async void RunActionAsync(ClientActions actions, object[] arguments)
        {
            
            await Task.Run(() => RunAction(actions, arguments));

        }
    }
}

