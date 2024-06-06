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
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;
using MyTools.Class;
using MyTools.Classes;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyTools.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DirectoryToolsPage : Page
    {
        public DirectoryToolsPage()
        {
            this.InitializeComponent();
            FileDragDropService fileDragDropService = new FileDragDropService();
            fileDragDropService.SetupDragDrop(SelectedFolderPathTextBox, false);
        }


        private void DirectoryToolsRadioButton_Checked(object sender, RoutedEventArgs e)
        {

            SelectedFolderPathTextBox.IsEnabled = true;
            SelectFolderButton.IsEnabled = true;
            if ((sender as RadioButton)?.Name == "ExportDirectoryRadioButton")
            {
                OutputDirectoryPathTextBox.IsEnabled = true;
                OutputDirectoryButton.IsEnabled = true;
            }
            else
            {
                OutputDirectoryPathTextBox.IsEnabled = false;
                OutputDirectoryButton.IsEnabled = false;
            }
        }
        private async void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {

            // Clear previous returned file name, if it exists, between iterations of this scenario
            //SelectedFolderPathTextBox.Text = "";

            // Create a folder picker
            FolderPicker openPicker = new Windows.Storage.Pickers.FolderPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = App.m_window;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {

                SelectedFolderPathTextBox.Text = folder.Path;
            }


        }
        private async void OutputDirectoryButton_Click(object sender, RoutedEventArgs e)//选择文本文件
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            //OutputDirectoryPathTextBox.Text = "";

            // Create a file picker
            FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = App.m_window;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            // Set options for your file picker
            savePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("文本文件", new List<string>() { ".txt" });

            savePicker.SuggestedFileName = "1.txt";

            // Open the picker for the user to pick a file
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                OutputDirectoryPathTextBox.Text = file.Path;
            }
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedFolderPathTextBox.Text) || !Directory.Exists(SelectedFolderPathTextBox.Text))
            {
                string errorMessage = string.IsNullOrWhiteSpace(SelectedFolderPathTextBox.Text)
                     ? "请选择有效的文件夹路径！"
                     : "指定的文件夹不存在，请检查路径是否正确！";
                await ShowMessages.ShowDialog(this.XamlRoot, "警告！", $"{errorMessage}", false);
                return;
            }
            if (ViewDirectoryRadioButton.IsChecked == true)
            {
                await RunProcess("cmd", $"/c tree \"{SelectedFolderPathTextBox.Text}\" /f");

            }
            else if (ExportDirectoryRadioButton.IsChecked == true)
            {
                await RunProcess("cmd", $"/c tree \"{SelectedFolderPathTextBox.Text}\" /f >\"{OutputDirectoryPathTextBox.Text}\"");
            }

        }

        private async Task RunProcess(string ProcessName, string ProcessCommand)
        {
            ConfirmButton.IsEnabled = false;
            OutputTextBox.Text = "";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ProcessName,
                Arguments = ProcessCommand,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            //启动进程
            Process process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            // 在UI线程上更新文本框内容 
            DataReceivedEventHandler outputHandler = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data) && args.Data != "N/A")
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        //实时输出运行状态
                        OutputTextBox.Text += args.Data + Environment.NewLine;
                    });
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
                await ShowMessages.ShowDialog(this.XamlRoot, "失败！", $"进程{ProcessName}执行失败，退出代码：{exitCode}", false);
            }
            else
            {
                await ShowMessages.ShowDialog(this.XamlRoot, "成功！", $"进程{ProcessName}执行成功！", false);
            }
            ConfirmButton.IsEnabled = true;
        }













    }
}
