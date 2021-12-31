using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace Launcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public enum InactiveElement
    {
        GameLoaderStackPanel,
        IdWrapPanel,
        PasswordWrapPanel,
        AuthorWrapPanel,
        InfoWrapPanel,
        SelectFileWrapPanel,
        ProjectTabItem,
        ProjectStackPanel,
        UpdateListModsButton,
        OtherModsStackPanel,
        MyModsStackPanel,
        MyModsComboBox,
        MessageStackPanel,
        NameInChatStackPanel,
        ModsTabItem
    }
    public partial class MainWindow : Window
    {
        public Client Client { get; private set; }
        public ChatClient ChatClient { get; private set; }
        public ProgramSettings MyProgramSettings { get; private set; }

        private void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            System.Windows.Forms.MessageBox.Show(e.Message + "\n" + e);
         
        }
        public void UpdateLauncher()
        {
            try
            {
                MyProgramSettings.SaveSettings(Directory.GetCurrentDirectory() + "/ProgramSettings.bin");
            }
            catch
            {

            }
            if (!File.Exists(Directory.GetCurrentDirectory() + @"\Update.exe"))
            {
                System.Windows.Forms.MessageBox.Show("Не удалось найти установщик");
                return;
            }
            Process.Start(Directory.GetCurrentDirectory() + @"\Update.exe");
            Process.GetCurrentProcess().Kill();
        }
        public MainWindow()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
           
            if (!File.Exists("ProgramSettings.bin"))
            {
                MyProgramSettings = new ProgramSettings();
            }else
            {
                try
                {
                    MyProgramSettings = ProgramSettings.LoadSettings(Directory.GetCurrentDirectory() + "/ProgramSettings.bin");
                }
                catch
                {
                    MyProgramSettings = new ProgramSettings();
                }
               
            }
            Mod mod;
            if (MyProgramSettings.HasChosenMod(out mod))
            {
                ModPanel.SetNameChosenModPanel(mod.Name);
            }
            VisualMyModsComboBox();
      
        }
        public void VisualMyModsComboBox()
        {
            MyModsComboBox.Items.Clear();
            if (MyProgramSettings.HasMods(true))
            {
                foreach (var item in MyProgramSettings.GetListAllMods(true))
                {
                    ComboBoxItem comboBoxItemMod = new ComboBoxItem();
                    comboBoxItemMod.Tag = item.Password;
                    comboBoxItemMod.Content = item.Name;
                    MyModsComboBox.Items.Add(comboBoxItemMod);
                }
            }
        }
        public void DeactiveOrActiveElements(InactiveElement[] inactiveElements,bool isEnable)
        {
            foreach (var item in inactiveElements)
            {
                (FindName(item.ToString()) as FrameworkElement).IsEnabled = isEnable;
            }
            
        }
        public void AddToLog(string text)
        {
            TextBlock textBox = new TextBlock();
            textBox.MaxWidth = 115;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.Text = text;
            textBox.Foreground =(Brush)new BrushConverter().ConvertFromString("Gainsboro");
            LogStackPanel.Children.Add(textBox);
            LogScrollViewer.ScrollToEnd();
        }
        public void ErrorConnection(string error)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Не удалось подключиться к серверу. Попробовать переподключиться?\n"+ error, "Ошибка", MessageBoxButtons.YesNo);
          if(dialogResult==System.Windows.Forms.DialogResult.No)
            Process.GetCurrentProcess().Kill();
            else
            {
              
                ProcessStartInfo Info = new ProcessStartInfo();
                Info.Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetEntryAssembly().Location + "\"";
                Info.WindowStyle = ProcessWindowStyle.Hidden;
                Info.CreateNoWindow = true;
                Info.FileName = "cmd.exe";
                Process.Start(Info);
                Process.GetCurrentProcess().Kill();
            }
        }
        public void AddComboBoxItemInVersions(string version)
        {
            ComboBoxItem listBoxItem = new ComboBoxItem();
            listBoxItem.Content = version;
            listBoxItem.Foreground =(Brush)new BrushConverter().ConvertFromString("Gainsboro");
            ComboBoxVersions.Items.Add(listBoxItem);
            ComboBoxVersions.SelectedItem = listBoxItem;
           
        }
        public void InstallLauncher()
        {
            Client.RunActionAsync(ClientActions.InstallLauncher, new object[] { MyProgramSettings.Version });
        }
        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            Client = new Client("88.85.171.249", 4512,this);
            ChatClient = new ChatClient("88.85.171.249", 4511,this);
          //  Client.RunActionAsync(ClientActions.InstallLauncher, new object[] {0});
            Client.RunActionAsync(ClientActions.LoadActiveVersionsGame,null);
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem acitiveComboBoxItem = (ComboBoxItem)ComboBoxVersions.Items[ComboBoxVersions.SelectedIndex];
           
            Client.RunActionAsync(ClientActions.LoadGame,new object[] { acitiveComboBoxItem.Content });
           
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if(!File.Exists(Directory.GetCurrentDirectory() + @"\online test\online game.exe"))
            {
                System.Windows.Forms.MessageBox.Show("Не удалось найти игру");
                return;
            }
            try
            {
                MyProgramSettings.SaveSettings(Directory.GetCurrentDirectory() + "/ProgramSettings.bin");
            }
            catch
            {

            }
            Process.Start(Directory.GetCurrentDirectory() + @"\online test\online game.exe");
            Process.GetCurrentProcess().Kill();
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog()==true)
            {
                SelectedFileTextBlock.Text=openFileDialog.FileName;
            }
        }
        public void ResetMyProjectPanel()
        {
            InfoWrapPanel.Visibility = Visibility.Collapsed;
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.IdWrapPanel, InactiveElement.PasswordWrapPanel,InactiveElement.InfoWrapPanel,InactiveElement.SelectFileWrapPanel,InactiveElement.AuthorWrapPanel }, true);
            SelectFileWrapPanel.Visibility = Visibility.Collapsed;
            AcceptButton.Content = "Применить";
            AcceptButton.Visibility = Visibility.Visible;
            ActionWrapPanel.Visibility = Visibility.Collapsed;
            AuthorWrapPanel.Visibility = Visibility.Collapsed;
            MyModsComboBox.IsEnabled = true;
            AcceptButton.Tag = 0;
        }
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            int state = int.Parse(AcceptButton.Tag.ToString());
            AddToLog(state.ToString());
            switch (state)
            {
                case 0:
                    Client.RunActionAsync(ClientActions.CheckID, new object[] { IdTextBox.Text.Replace('~', ' '), PasswordTextBox.Text.Replace('~', ' ') });
                   
                    break;
                case 1:
                    DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.AuthorWrapPanel }, false);
                    InfoWrapPanel.Visibility = Visibility.Visible;
                    break;
                case 2:
                    DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.InfoWrapPanel }, false);
                    SelectFileWrapPanel.Visibility = Visibility.Visible;
                    AcceptButton.Content = "Загрузить мод";
                    break;
                case 3:
                    if(SelectedFileTextBlock.Text!="")
                    Client.RunActionAsync(ClientActions.SendFileAntOtherSettings, new object[] { AuthorTextBox.Text.Replace('~', ' '), InfoTextBox.Text.Replace('~', ' '), SelectedFileTextBlock.Text.Replace('~', ' '), PasswordTextBox.Text.Replace('~', ' ') });
                    else
                    {
                        System.Windows.MessageBox.Show("Вы не выбрали файл");
                        AcceptButton.Tag = --state;
                    }
                    break;
                default:
                    break;
            }
            AcceptButton.Tag = ++state;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Client.RunActionAsync(ClientActions.SendFileAntOtherSettings, new object[] { AuthorTextBox.Text.Replace('~', ' '), InfoTextBox.Text.Replace('~', ' '), SelectedFileTextBlock.Text.Replace('~', ' '),PasswordTextBox.Text.Replace('~', ' ') });

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Client.RunActionAsync(ClientActions.DeleteFile, new object[] {PasswordTextBox.Text.Replace('~', ' '),IdTextBox.Text.Replace('~',' ') });
        }

        

        private void UpdateListModsButton_Click(object sender, RoutedEventArgs e)
        {
            Client.RunActionAsync(ClientActions.UpdateListMods, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                MyProgramSettings.SaveSettings(Directory.GetCurrentDirectory() + "/ProgramSettings.bin");
            }
            catch
            {

            }
          
        }

        private void MyModsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         
        }
        public void GetMessage(string message,byte[] color)
        {
            TextBlock chatTextBlock = new TextBlock();
            if(MainTabControl.SelectedIndex!=3)
            ChatTabItem.Foreground = new SolidColorBrush(Color.FromRgb(160,36,36));
            chatTextBlock.FontSize = 16;
            chatTextBlock.TextWrapping = TextWrapping.Wrap;
            chatTextBlock.Text += "\n" + message;
            Color customColor = Color.FromArgb(255, color[0],color[1],color[2]);
            chatTextBlock.Foreground=new SolidColorBrush(customColor);
            MessageStackPanelView.Children.Add(chatTextBlock);
            MessageTextBox.Text = "";
        }
     
        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
           ChatClient.SendMessage(MessageTextBox.Text);
            ScrollViewerChat.ScrollToEnd();
        }

        private void SendNameButton_Click(object sender, RoutedEventArgs e)
        {
            
            ChatClient.SetName(NameInChatTextBox.Text+":");
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.NameInChatStackPanel }, false);
            DeactiveOrActiveElements(new InactiveElement[] { InactiveElement.MessageStackPanel }, true);
        }
        public void RepostMod(string name)
        {
            if (!NameInChatStackPanel.IsEnabled)
                ChatClient.SendMessage(name);
            else
                System.Windows.MessageBox.Show("Сначала вы должны ввести ник в чате");
        }
        public void RepostButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModsTabItem.IsEnabled)
            {
                MainTabControl.SelectedIndex = 1;
                NameFindModTextBox.Text = (sender as System.Windows.Controls.Button).Content.ToString();
                FindModButton_Click(null, null);
            }
           
               
        }
            private void SelectedItemMyMods_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem comboBox = MyModsComboBox.SelectedItem as ComboBoxItem;
            if (comboBox != null)
            {
                PasswordTextBox.Text = comboBox.Tag.ToString();
                IdTextBox.Text = comboBox.Content.ToString();
            }
        }

        private void FindModButton_Click(object sender, RoutedEventArgs e)
        {
            Client.RunActionAsync(ClientActions.UpdateListMods, new object[] { NameFindModTextBox.Text,AuthorFindModTextBox.Text });
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void CoppyLinkButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText((sender as FrameworkElement).Tag.ToString());
        }

        private void ChatTabItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChatTabItem.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }
    }
}
