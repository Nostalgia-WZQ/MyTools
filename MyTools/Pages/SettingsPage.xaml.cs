using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.Core;
using MyTools.Classes;
using Windows.Storage;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using System.Text;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyTools.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            
        }
       

        private void VideoTranscodingCommandDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Name == "VideoTranscodingGPUCommandDefaultButton")
            {
                VideoTranscodingGPUCommandTextBox.Text = GlobalData.VideoTranscodingGPUCommandDefault;
            }
            else if (button.Name == "VideoTranscodingCPUCommandDefaultButton")
            {
                VideoTranscodingCPUCommandTextBox.Text = GlobalData.VideoTranscodingCPUCommandDefault;
            }
        }
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private void VideoTranscodingCommandTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            localSettings.Values["VideoTranscodingGPUCommand"] = VideoTranscodingGPUCommandTextBox.Text;
            localSettings.Values["VideoTranscodingCPUCommand"] = VideoTranscodingCPUCommandTextBox.Text;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (localSettings.Values.ContainsKey("VideoTranscodingGPUCommand") && localSettings.Values.ContainsKey("VideoTranscodingCPUCommand") && localSettings.Values.ContainsKey("ThemeSelection"))
            {
                VideoTranscodingGPUCommandTextBox.Text = localSettings.Values["VideoTranscodingGPUCommand"].ToString();
                VideoTranscodingCPUCommandTextBox.Text = localSettings.Values["VideoTranscodingCPUCommand"].ToString();
                if(localSettings.Values["ThemeSelection"].ToString()== "Light")
                {
                    LightThemeRadioButton.IsChecked = true;
                }
                else if(localSettings.Values["ThemeSelection"].ToString() == "Dark")
                {
                    DarkThemeRadioButton.IsChecked = true;
                }
                else
                {
                    DefaultThemeRadioButton.IsChecked = true;
                }
            }

            VersionTextBlock.Text = GetVersionDescription();

        }
        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            var theme = radioButton.Tag.ToString();

            if (MainWindow.current.Content is FrameworkElement rootElement)
            {
                switch (theme)
                {
                    case "Light":
                        rootElement.RequestedTheme = ElementTheme.Light;
                        localSettings.Values["ThemeSelection"] = "Light";
                        break;
                    case "Dark":
                        rootElement.RequestedTheme = ElementTheme.Dark;
                        localSettings.Values["ThemeSelection"] = "Dark";
                        break;
                    case "Default":
                        rootElement.RequestedTheme = ElementTheme.Default;
                        localSettings.Values["ThemeSelection"] = "Default";
                        break;
                }
            }

        }
        private static string GetVersionDescription()
        {
            Version version;
            var packageVersion = Package.Current.Id.Version;
            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            //version = Assembly.GetExecutingAssembly().GetName().Version!;
            return $"版本号：{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async void OpenDataFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Content.ToString() == "打开安装文件夹")
            {
                var folder = AppDomain.CurrentDomain.BaseDirectory;
                await Launcher.LaunchFolderPathAsync(folder);
            }
            else
            {
                var folder = ApplicationData.Current.LocalFolder;
                await Launcher.LaunchFolderAsync(folder);
            }
           
        }
    }
}
