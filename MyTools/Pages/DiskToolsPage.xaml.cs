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


        private async void SelectVirtualDiskButton_Click(object sender, RoutedEventArgs e)//选择虚拟磁盘文件
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
                //GetMediaInformation();//获取媒体信息
            }
        }
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        //加载已保存的虚拟磁盘路径和获取盘符
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //加载已保存的虚拟磁盘路径       
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
            //获取盘符
            var availableDrives = GetAvailableDrives();
            DriveSelectorComboBox.ItemsSource = availableDrives;
        }

        //记住虚拟磁盘路径
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
                    drives.Add(drive.RootDirectory.FullName.TrimEnd('\\')); // 去掉末尾反斜杠
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
                         ? "请选择有效的虚拟磁盘文件路径！"
                         : "指定的文件不存在，请检查路径是否正确！";
                    //XamlRoot xamlRoot = this.XamlRoot;
                    await ShowMessages.ShowDialog(this.XamlRoot, "警告！", $"{errorMessage}", false);

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
                    await ShowMessages.ShowDialog(this.XamlRoot, "警告！", "请先选择要上Bitlocker锁的盘符！", false);
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
            // 启动进程
            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                await ShowMessages.ShowDialog(this.XamlRoot, "错误！", $"发生了一个错误！错误信息：{ex.Message}", false);
                ConfirmButton.IsEnabled = true;
                return;
            }

            GlobalData.ManagedProcesses.Add(process);
            // 开始异步读取输出数据
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();// 等待进程完成
            process.OutputDataReceived -= outputHandler;
            process.ErrorDataReceived -= outputHandler;
            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {               
                await ShowMessages.ShowDialog(this.XamlRoot, "失败！", $"进程{ProcessName}执行失败，退出代码：{exitCode}\n退出信息：\n{OutputInformation}", false);        
            }
            else
            {
                await ShowMessages.ShowDialog(this.XamlRoot, "成功！", $"进程{ProcessName}执行成功！\n退出信息：\n{OutputInformation}", false);
            }

            ConfirmButton.IsEnabled = true;
        }

        // 定义UAC取消提示的错误代码常量
        private const int ERROR_CANCELLED = 1223; // 用户取消了UAC提示

        // 检查当前应用程序是否以管理员权限运行
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
                // 已有管理员权限，直接执行相关操作
                elevatedAction();
            }
            else
            {
                // 无管理员权限，请求用户以管理员权限重新启动
                ContentDialogResult result = await ShowMessages.ShowDialog(this.XamlRoot, "需管理员权限", "所选功能需要管理员权限。\n\n退出应用程序？", true);
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

        // 重启应用程序并请求管理员权限的方法
        private async void RestartAsAdmin()
        {
            // 获取当前进程的主模块（即应用程序的可执行文件路径）
            string currentExePath = Process.GetCurrentProcess().MainModule.FileName;

            // 构建启动新进程的信息，要求以管理员权限运行
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = currentExePath,
                UseShellExecute = true,
                Verb = "runas" // 设置“runas”动词以请求管理员权限
            };

            try
            {
                // 启动新进程
                using Process process = Process.Start(startInfo);

            }
            catch (Win32Exception ex)
            {
                // 处理UAC提示被用户取消的情况
                if (ex.NativeErrorCode == ERROR_CANCELLED)
                {
                    await ShowMessages.ShowDialog(this.XamlRoot, "错误！", "操作已被取消。请以管理员身份运行应用程序。", false);
                    return;
                }
                else
                {
                    // 其他异常情况的处理
                    await ShowMessages.ShowDialog(this.XamlRoot, "错误！", $"尝试重启应用程序时发生错误：\n{ex.Message}", false);
                    return;
                }
            }
            // 关闭当前实例
            Application.Current.Exit();

        }











    }
}
