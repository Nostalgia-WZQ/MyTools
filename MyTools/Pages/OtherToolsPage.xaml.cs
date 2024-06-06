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
using MyTools.Class;
using MyTools.Classes;
using System.Diagnostics;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyTools.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OtherToolsPage : Page
    {
        public OtherToolsPage()
        {
            this.InitializeComponent();
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (RefreshDNScacheRadioButton.IsChecked == true)
            {
                await RunProcess("ipconfig", "/flushdns");

            }
            else if (OpenWSASettingsRadioButton.IsChecked == true)
            {
                await RunProcess("cmd", $"/c start wsa://com.android.settings");
            }
        }
        private async Task RunProcess(string ProcessName, string ProcessCommand)
        {
            ConfirmButton.IsEnabled = false;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ProcessName,
                Arguments = ProcessCommand,
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            //��������
            Process process = new Process();
            process.StartInfo = startInfo;
            
            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                await ShowMessages.ShowDialog(this.XamlRoot, "����", $"������һ�����󣡴�����Ϣ��{ex.Message}", false);
                ConfirmButton.IsEnabled = true;
                return;
            }
            GlobalData.ManagedProcesses.Add(process);
            await process.WaitForExitAsync();// �ȴ��������
            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                await ShowMessages.ShowDialog(this.XamlRoot, "ʧ�ܣ�", $"����{ProcessName}ִ��ʧ�ܣ��˳����룺{exitCode}", false);
            }
            else
            {
                await ShowMessages.ShowDialog(this.XamlRoot, "�ɹ���", $"����{ProcessName}ִ�гɹ���", false);
            }
            ConfirmButton.IsEnabled = true;
        }


    }
}
