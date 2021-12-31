using System.Windows;
using System.Text.Json;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace Installer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NetworkStream _stream;
        private TcpClient _client;
        private MySettings _settings;
        private byte[] _fileBytes = new byte[10000000];
        public void ConnectToServer()
        {
            try
            {
                _client = new TcpClient();
                _client.Connect("88.85.171.249", 4510);
                _stream = _client.GetStream();
                if (File.Exists(Directory.GetCurrentDirectory() + "/settings.json"))
                { 
                   
                    PathStackPanel.IsEnabled = false;
                    if (_settings == null)
                        _settings = JsonSerializer.Deserialize<MySettings>(File.ReadAllText(Directory.GetCurrentDirectory() + "/settings.json"));
                    PathTextBlock.Text = _settings.Path;
                    int version = 0;
                    if (!HasVersion(out version, _settings.Version))
                    {
                        StartLauncherButton.IsEnabled = false;
                        _settings.Version = version;
                    }
                    else
                    {
                        StartLauncherButton.IsEnabled = true;
                        UpdateOrDownloadLauncherButton.IsEnabled = false;
                    }
                }
                else
                {
                    _settings = new MySettings();
                    _settings.Path = "";
                    UpdateOrDownloadLauncherButton.IsEnabled = true;
                  UpdateOrDownloadLauncherButton.Content = "Установить лаунчер";
                    PathStackPanel.IsEnabled = true;
                    StartLauncherButton.IsEnabled = false;
                }
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }
        public MainWindow()
        {
            InitializeComponent();
          
            ConnectToServer();

        }
        private bool HasVersion(out int newVersion,int oldVersion)
        {
            _stream.WriteByte(0);
            newVersion = _stream.ReadByte();

            return newVersion==oldVersion;
        }

        private void SelectedPathButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
            if (openFileDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                PathTextBlock.Text = openFileDialog.SelectedPath;
                if (PathTextBlock.Text != "")
                    _settings.Path = PathTextBlock.Text+ @"\launcher\";


            }
        }
        private void UpdateOrDownloadLauncherButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Устанавливаем файл");
            if (_settings.Path != "")
            {
                _stream.WriteByte(1);
                using (var fileStream = new FileStream(Directory.GetCurrentDirectory() + @"\launcher.zip", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    do
                    {
                        int readBytes = _stream.Read(_fileBytes, 0, _fileBytes.Length);
                        _fileBytes = _fileBytes.ToList().Take(readBytes).ToArray();
                        fileStream.Write(_fileBytes, 0, _fileBytes.Length);
                        Thread.Sleep(100);
                    } while (_stream.DataAvailable);
                }
                if (Directory.Exists(_settings.Path))
                
                    Directory.Delete(_settings.Path, true);
                    else
                        Directory.CreateDirectory(_settings.Path);

                    ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + @"\launcher.zip", _settings.Path);
               
                System.Windows.MessageBox.Show("Файл установлен");
                File.Delete(Directory.GetCurrentDirectory() + @"\laucher.zip");
                UpdateOrDownloadLauncherButton.IsEnabled = false;
                UpdateOrDownloadLauncherButton.Content = "Обновить лаунчер";
                StartLauncherButton.IsEnabled = true;
            }
            else
                System.Windows.MessageBox.Show("Укажите путь к папке");
            
        }

        private void StartLauncherButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(_settings.Path+"launcher.exe");
            Process.GetCurrentProcess().Kill();
        }

        private void DeleteLauncherButton_Click(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(_settings.Path))
            Directory.Delete(_settings.Path, true);
            File.Delete(Directory.GetCurrentDirectory() + "/settings.json");
            System.Windows.MessageBox.Show("Файл удалён");
            _client.Close();
            _stream.Close();
            PathTextBlock.Text = "";
            
            ConnectToServer();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
         File.WriteAllText(Directory.GetCurrentDirectory()+@"/settings.json",JsonSerializer.Serialize(_settings));
         
        }
    }
}
