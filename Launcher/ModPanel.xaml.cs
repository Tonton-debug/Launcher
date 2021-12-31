using System;
using System.Collections.Generic;
using System.IO;
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

namespace Launcher
{
    /// <summary>
    /// Логика взаимодействия для ModPanel.xaml
    /// </summary>
    public partial class ModPanel : UserControl
    {
        private static string _nameChosenModPanel=""; 
        private bool _myMode;
        private Mod mod;
        private MainWindow _mainWindow;
        public static void SetNameChosenModPanel(string text)
        {
            _nameChosenModPanel = text;
        }
        public ModPanel(string author,string name,string description,int version,bool isMyMode,MainWindow get,bool hasUpdate=false)
        {
            InitializeComponent();
            AuthorTextBlock.Text = author;
            ModNameTextBlock.Text = name;
            DescriptionTextBlock.Text = description;
            VersionTextBlock.Text = version.ToString();
            _mainWindow = get;
           
             _myMode = isMyMode;
            if (!isMyMode)
            {
                DownloadModButton.Visibility = Visibility.Visible;
                MyModStackPanel.Visibility = Visibility.Collapsed;
             
            }         
            else
            {
                mod = _mainWindow.MyProgramSettings.GetMod(name, false);
                DownloadModButton.Visibility = Visibility.Collapsed;
                MyModStackPanel.Visibility = Visibility.Visible;
                
                if (_nameChosenModPanel != "" && _nameChosenModPanel != name)
                    ChoseModButton.IsEnabled = false;
                else if (_nameChosenModPanel != null && _nameChosenModPanel == name)
                {
                    ChoseModButton.IsEnabled = true; ChoseModButton.Content = "Отменить";
                }
                if (hasUpdate)
                    UpdateModButton.IsEnabled = true;
            }
           
        }

        private void DownloadModButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChoseModButton.Content.ToString() == "Отменить")
            {
                _nameChosenModPanel = "";
                mod.ChoseMod = false;
                ReplaceDLL(false);
            }
            _mainWindow.Client.RunActionAsync(ClientActions.InstallMod, new object[] { ModNameTextBlock.Text, AuthorTextBlock.Text, DescriptionTextBlock.Text,VersionTextBlock.Text,_myMode });
            DownloadModButton.IsEnabled = false;
        }

        private void DeleteModButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChoseModButton.Content.ToString() == "Отменить")
            {
                _nameChosenModPanel = "";
                mod.ChoseMod = false;
                ReplaceDLL(false);
            }
            
            _mainWindow.MyProgramSettings.RemoveMod(ModNameTextBlock.Text, false);
            _mainWindow.Client.RunActionAsync(ClientActions.UpdateListMods, null);
            File.Delete(Directory.GetCurrentDirectory() + @"\Mods\" + ModNameTextBlock.Text + ".dll");    
        }
        private void ReplaceDLL(bool isMod)
        {
            try
            {
                    File.Delete(Directory.GetCurrentDirectory() + @"\online test\online game_Data\Managed\Assembly-CSharp.dll");
                    File.Copy(isMod?Directory.GetCurrentDirectory() + @"\mods\" + ModNameTextBlock.Text+".dll": Directory.GetCurrentDirectory() + @"\mainDLL.dll", Directory.GetCurrentDirectory() + @"\online test\online game_Data\Managed\Assembly-CSharp.dll");
            }
            catch (Exception e)
            {
                _mainWindow.AddToLog("Ошибка\n" + e.Message);
                _nameChosenModPanel = "";
                mod.ChoseMod = false;
            }
        }
        private void ChoseModButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChoseModButton.Content.ToString()== "Выбрать")
            {
                _nameChosenModPanel = ModNameTextBlock.Text;
                ReplaceDLL(true);
                mod.ChoseMod = true;
            }         
            else if(ChoseModButton.Content.ToString() == "Отменить")
            {
                _nameChosenModPanel = "";
                ReplaceDLL(false);
                mod.ChoseMod = false;
            }         
                
            _mainWindow.Client.RunActionAsync(ClientActions.UpdateListMods, null);
        }

        private void RepostModButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.RepostMod("mod-"+ModNameTextBlock.Text);
        }
    }
}
