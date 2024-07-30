using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MyTools.Classes;
using MyTools.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyTools
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            InitializeAppSettings();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
            if (MainWindow.current.Content is FrameworkElement rootElement)
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                switch (localSettings.Values["ThemeSelection"].ToString())
                {
                    case "Light":
                        rootElement.RequestedTheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        rootElement.RequestedTheme = ElementTheme.Dark;
                        break;
                    case "Default":
                        rootElement.RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
        }

        //private Window m_window;
        public static Window m_window;
        private void InitializeAppSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (!localSettings.Values.ContainsKey("VideoTranscodingGPUCommand"))
            {
                localSettings.Values["VideoTranscodingGPUCommand"] = GlobalData.VideoTranscodingGPUCommandDefault;
            }
            if (!localSettings.Values.ContainsKey("VideoTranscodingCPUCommand"))
            {
                localSettings.Values["VideoTranscodingCPUCommand"] = GlobalData.VideoTranscodingCPUCommandDefault;
            }
            if (!localSettings.Values.ContainsKey("VideoAddSubtitlesCommand"))
            {
                localSettings.Values["VideoAddSubtitlesCommand"] = GlobalData.VideoAddSubtitlesCommandDefault;
            }
            if (!localSettings.Values.ContainsKey("MergeAudioAndVideoCommand"))
            {
                localSettings.Values["MergeAudioAndVideoCommand"] = GlobalData.MergeAudioAndVideoCommandDefault;
            }
            if (!localSettings.Values.ContainsKey("ThemeSelection"))
            {
                localSettings.Values["ThemeSelection"] = "Default";
            }           
        }





    }
}
