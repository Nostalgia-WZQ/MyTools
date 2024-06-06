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
using Microsoft.UI;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using MyTools.Class;
using MyTools.Classes;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyTools.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HashToolsPage : Page
    {
        public HashToolsPage()
        {
            this.InitializeComponent();
            FileDragDropService fileDragDropService = new FileDragDropService();
            fileDragDropService.SetupDragDrop(SelectedFiletoCalculateHashPath, false);
            InitializeTimer();
            
        }
        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            timer.Tick += Timer_Tick;
            isCounting = false;
        }
        private DispatcherTimer timer; // 用于定期更新UI的计时器
        private TimeSpan elapsedTime; // 记录总的经过时间
        private bool isCounting; // 标记是否正在计时
        private void Timer_Tick(object sender, object e)
        {
            // 更新UI，显示当前计时
            elapsedTime += timer.Interval;
            string timeStr = string.Format("{0:D2}:{1:D2}:{2:D2}", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
            RunTimeTextBlock.Text = $"运行时间: {timeStr}";
        }

        private void HashToolsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SelectedFiletoCalculateHashButton.IsEnabled = true;
            SelectedFiletoCalculateHashPath.IsEnabled = true;
        }

        private async void SelectedFiletoCalculateHashButton_Click(object sender, RoutedEventArgs e)
        {
            //SelectedFiletoCalculateHashPath.Text = "";
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
            openPicker.FileTypeFilter.Add("*");
            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                SelectedFiletoCalculateHashPath.Text = file.Path;
                //GetMediaInformation();//获取媒体信息
            }
        }
        Dictionary<string, string> HashValues = new Dictionary<string, string>();
        private async Task CalculateHashesAsync(string filePath)
        {
            ConfirmButton.IsEnabled = false;
            if (!isCounting)
            {
                // 开始计时
                isCounting = true;
                elapsedTime = TimeSpan.Zero;
                timer.Start();
            }
            long fileSize = new FileInfo(filePath).Length;
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                Tuple<string, string, string, string> hashes = await Task.Run(() => CalculateHashes(fileStream, fileSize));
             
                HashValues.Clear();
                HashValues.Add("MD5", hashes.Item1);
                HashValues.Add("SHA1", hashes.Item2);
                HashValues.Add("SHA256", hashes.Item3);
                HashValues.Add("SHA512", hashes.Item4);


                OutputTextBox.Text = $"MD5:       {hashes.Item1}\n\nSHA1:      {hashes.Item2}\n\nSHA256:  {hashes.Item3}\n\nSHA512:  {hashes.Item4}";
            }
            
            ConfirmButton.IsEnabled = true;
            UpdateProgressBar(100);
            // 进程已退出，停止计时
            isCounting = false;
            timer.Stop();
            await ShowMessages.ShowDialog(this.XamlRoot, "成功！", "计算完成！", false);
        }
        private const int UpdateIntervalMs = 50; // 每50毫秒更新一次进度条
        private DateTime lastUpdateTime = DateTime.UtcNow;
        private Tuple<string, string, string, string> CalculateHashes(FileStream fileStream, long fileSize)
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            long totalBytesRead = 0;

            using (MD5 md5 = MD5.Create())
            using (SHA1 sha1 = SHA1.Create())
            using (SHA256 sha256 = SHA256.Create())
            using (SHA512 sha512 = SHA512.Create())
            {
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalBytesRead += bytesRead;

                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha512.TransformBlock(buffer, 0, bytesRead, null, 0);
                    // 在循环内部判断是否需要更新进度
                    if ((DateTime.UtcNow - lastUpdateTime).TotalMilliseconds >= UpdateIntervalMs)
                    {
                        lastUpdateTime = DateTime.UtcNow;
                        double progressPercentage = Math.Min(100, totalBytesRead / (double)fileSize * 100);
                        UpdateProgressBar(progressPercentage);
                    }
                }

                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha512.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return Tuple.Create(
                    BitConverter.ToString(md5.Hash).Replace("-", ""),
                    BitConverter.ToString(sha1.Hash).Replace("-", ""),
                    BitConverter.ToString(sha256.Hash).Replace("-", ""),
                    BitConverter.ToString(sha512.Hash).Replace("-", "")
                );
            }
        }
        private void UpdateProgressBar(double progressPercentage)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                progressBar.Value = Math.Min(100, Math.Max(0, progressPercentage));
                string processvalue = progressBar.Value.ToString("N2");
                ProgressValueTextBlock.Text = $"进度值：{processvalue}%";
               
            });
        }

        private void SelectedFiletoCalculateHashPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {            
            OutputTextBox.Text="";
            HashValues.Clear();
            HashComparisonResultTextBlock.Text = "↑↑↑运行完成后选择并填入待比较的哈希值↑↑↑";
            HashComparisonResultTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            progressBar.Value = 0;
            ProgressValueTextBlock.Text = "进度值：0.00%";
        }


        private void HashComparison()
        {
            var selectedHashType = SelectHash.SelectedItem as ComboBoxItem;
            string hashValue = selectedHashType.Content.ToString();

            if (HashValues.TryGetValue(hashValue, out var selectedHashValue))
            {

                if (OriginalHashValueTextBox.Text.ToUpper() == selectedHashValue)
                {
                    HashComparisonResultTextBlock.Text = "😄匹配成功！😄";
                    HashComparisonResultTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    HashComparisonResultTextBlock.Text = "😞匹配失败！😞";
                    HashComparisonResultTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                }
            }

        }
        private void OriginalHashValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HashComparison();
        }
        private void HashComparisonButton_Click(object sender, RoutedEventArgs e)
        {
            HashComparison();
        }
        private void SelectHashComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HashComparison();
        }
        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedFiletoCalculateHashPath.Text) || !File.Exists(SelectedFiletoCalculateHashPath.Text))
            {
                string errorMessage = string.IsNullOrWhiteSpace(SelectedFiletoCalculateHashPath.Text)
                     ? "请选择有效的文件路径！"
                     : "指定的文件不存在，请检查路径是否正确！";
                await ShowMessages.ShowDialog(this.XamlRoot, "错误！", $"{errorMessage}", false);
                return;
            }
            try
            {
                await CalculateHashesAsync(SelectedFiletoCalculateHashPath.Text);
            }
            catch (Exception ex)
            {
                await ShowMessages.ShowDialog(this.XamlRoot,"错误！", $"计算时发生错误: {ex.Message}",false);
                ConfirmButton.IsEnabled = true;
                return;
            }

        }



    }
}
