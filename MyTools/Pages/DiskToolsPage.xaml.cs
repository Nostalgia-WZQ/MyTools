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
using Windows.Storage.Pickers;
using Windows.Storage;
using MyTools.Class;
using Windows.ApplicationModel.DataTransfer;
using MyTools.Classes;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.Principal;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyTools.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DiskToolsPage : Page
    {
        public DiskToolsPage()
        {
            this.InitializeComponent();
            FileDragDropService fileDragDropService = new FileDragDropService();
            fileDragDropService.SetupDragDrop(SelectedVirtualDiskPathTextBox, false);
        }

        private void DiskToolsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton)?.Name == "MountVirtualDiskRadioButton")
            {
                RememberPathCheckBox.IsEnabled = true;
                SelectedVirtualDiskPathTextBox.IsEnabled = true;
                SelectVirtualDiskButton.IsEnabled = true;
            }
            else
            {
                RememberPathCheckBox.IsEnabled = false;
                SelectedVirtualDiskPathTextBox.IsEnabled = false;
                SelectVirtualDiskButton.IsEnabled = false;
            }
            if ((sender as RadioButton)?.Name == "BitLockerLockRadioButton")
            {
                DriveSelectorComboBox.IsEnabled = true;
            }
            else
            {
                DriveSelectorComboBox.IsEnabled = false;
            }
        }


        private async void SelectVirtualDiskButton_Click(object sender, RoutedEventArgs e)//ѡ����������ļ�
        {
            //SelectedVirtualDiskPathTextBox.Text = "";
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            // See the sample code below for how to make the window accessible from the App class.
            var window = App.m_window;
            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.FileTypeFilter.Add(".vhd");
            openPicker.FileTypeFilter.Add(".vhdx");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                SelectedVirtualDiskPathTextBox.Text = file.Path;
                //GetMediaInformation();//��ȡý����Ϣ
            }
        }
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        //�����ѱ�����������·���ͻ�ȡ�̷�
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //�����ѱ�����������·��       
            if (localSettings.Values.ContainsKey("VirtualDiskPath") && localSettings.Values.ContainsKey("RememberPathCheckBoxIsChecked"))
            {
                if (localSettings.Values["RememberPathCheckBoxIsChecked"] is bool IsCheckedValue)
                {
                    if (IsCheckedValue)
                    {
                        SelectedVirtualDiskPathTextBox.Text = localSettings.Values["VirtualDiskPath"].ToString();
                        RememberPathCheckBox.IsChecked = true;
                    }
                    else
                    {
                        SelectedVirtualDiskPathTextBox.Text = "";
                        RememberPathCheckBox.IsChecked = false;
                    }
                }

            }
            //��ȡ�̷�
            var availableDrives = GetAvailableDrives();
            DriveSelectorComboBox.ItemsSource = availableDrives;
        }

        //��ס�������·��
        private void RememberPathCheckBox_Checked(object sender, RoutedEventArgs e)
        {

            localSettings.Values["VirtualDiskPath"] = SelectedVirtualDiskPathTextBox.Text;
            localSettings.Values["RememberPathCheckBoxIsChecked"] = true;
        }

        private void RememberPathCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            localSettings.Values["VirtualDiskPath"] = "";
            localSettings.Values["RememberPathCheckBoxIsChecked"] = false;
        }
        private void SelectedVirtualDiskPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RememberPathCheckBox.IsChecked == true)
            {
                localSettings.Values["VirtualDiskPath"] = SelectedVirtualDiskPathTextBox.Text;
            }
        }
        private List<string> GetAvailableDrives()
        {
            var drives = new List<string>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && (drive.DriveType == DriveType.Removable || drive.DriveType == DriveType.Fixed))
                {
                    //drives.Add(drive.RootDirectory.FullName);                   
                    drives.Add(drive.RootDirectory.FullName.TrimEnd('\\')); // ȥ��ĩβ��б��
                }
            }
            return drives;
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {

            if (MountVirtualDiskRadioButton.IsChecked == true)
            {
                if (string.IsNullOrEmpty(SelectedVirtualDiskPathTextBox.Text) || !File.Exists(SelectedVirtualDiskPathTextBox.Text))
                {
                    string errorMessage = string.IsNullOrWhiteSpace(SelectedVirtualDiskPathTextBox.Text)
                         ? "��ѡ����Ч����������ļ�·����"
                         : "ָ�����ļ������ڣ�����·���Ƿ���ȷ��";
                    //XamlRoot xamlRoot = this.XamlRoot;
                    await ShowMessages.ShowDialog(this.XamlRoot, "���棡", $"{errorMessage}", false);

                    return;
                }
                await ExecuteWithElevatedPermissions(async () =>
                {
                    await RunProcess("powershell", $"Mount-VHD -Path '{SelectedVirtualDiskPathTextBox.Text}' ");
                });

            }
            else if (BitLockerLockRadioButton.IsChecked == true)
            {
                if ((DriveSelectorComboBox.SelectedIndex == -1))
                {
                    await ShowMessages.ShowDialog(this.XamlRoot, "���棡", "����ѡ��Ҫ��Bitlocker�����̷���", false);
                    return;
                }
                else
                {
                    await ExecuteWithElevatedPermissions(async () =>
                    {
                        string selectedDrive = DriveSelectorComboBox.SelectedItem.ToString();
                    await RunProcess("manage-bde", $"-lock {selectedDrive}");
                    });
                }
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
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            Process process = new Process();
            process.StartInfo = startInfo;
            string OutputInformation="";
            DataReceivedEventHandler outputHandler = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data) && args.Data != "N/A")
                {
                    OutputInformation += args.Data + Environment.NewLine;
    
                }
            };
            process.OutputDataReceived += outputHandler;
            process.ErrorDataReceived += outputHandler;
            // ��������
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
            // ��ʼ�첽��ȡ�������
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();// �ȴ��������
            process.OutputDataReceived -= outputHandler;
            process.ErrorDataReceived -= outputHandler;
            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {               
                await ShowMessages.ShowDialog(this.XamlRoot, "ʧ�ܣ�", $"����{ProcessName}ִ��ʧ�ܣ��˳����룺{exitCode}\n�˳���Ϣ��\n{OutputInformation}", false);        
            }
            else
            {
                await ShowMessages.ShowDialog(this.XamlRoot, "�ɹ���", $"����{ProcessName}ִ�гɹ���\n�˳���Ϣ��\n{OutputInformation}", false);
            }

            ConfirmButton.IsEnabled = true;
        }

        // ����UACȡ����ʾ�Ĵ�����볣��
        private const int ERROR_CANCELLED = 1223; // �û�ȡ����UAC��ʾ

        // ��鵱ǰӦ�ó����Ƿ��Թ���ԱȨ������
        private bool IsRunningWithElevatedPermissions()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private async Task ExecuteWithElevatedPermissions(Action elevatedAction)
        {
            if (IsRunningWithElevatedPermissions())
            {
                // ���й���ԱȨ�ޣ�ֱ��ִ����ز���
                elevatedAction();
            }
            else
            {
                // �޹���ԱȨ�ޣ������û��Թ���ԱȨ����������
                ContentDialogResult result = await ShowMessages.ShowDialog(this.XamlRoot, "�����ԱȨ��", "��ѡ������Ҫ����ԱȨ�ޡ�\n\n�˳�Ӧ�ó���", true);
                if (result == ContentDialogResult.Primary)
                {

                    //RestartAsAdmin();
                    Application.Current.Exit();
                }
                else
                {
                    return;
                }                              
            }
        }

        // ����Ӧ�ó����������ԱȨ�޵ķ���
        private async void RestartAsAdmin()
        {
            // ��ȡ��ǰ���̵���ģ�飨��Ӧ�ó���Ŀ�ִ���ļ�·����
            string currentExePath = Process.GetCurrentProcess().MainModule.FileName;

            // ���������½��̵���Ϣ��Ҫ���Թ���ԱȨ������
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = currentExePath,
                UseShellExecute = true,
                Verb = "runas" // ���á�runas���������������ԱȨ��
            };

            try
            {
                // �����½���
                using Process process = Process.Start(startInfo);

            }
            catch (Win32Exception ex)
            {
                // ����UAC��ʾ���û�ȡ�������
                if (ex.NativeErrorCode == ERROR_CANCELLED)
                {
                    await ShowMessages.ShowDialog(this.XamlRoot, "����", "�����ѱ�ȡ�������Թ���Ա�������Ӧ�ó���", false);
                    return;
                }
                else
                {
                    // �����쳣����Ĵ���
                    await ShowMessages.ShowDialog(this.XamlRoot, "����", $"��������Ӧ�ó���ʱ��������\n{ex.Message}", false);
                    return;
                }
            }
            // �رյ�ǰʵ��
            Application.Current.Exit();

        }











    }
}
