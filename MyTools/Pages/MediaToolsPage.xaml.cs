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
using Windows.Media.Capture;
using Windows.Media;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MyTools.Class;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using MyTools.Classes;
using Windows.Storage.Provider;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.AppNotifications;




// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyTools.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MediaToolsPage : Page
    {
        public MediaToolsPage()
        {
            this.InitializeComponent();
            FileDragDropService fileDragDropService = new FileDragDropService();
            fileDragDropService.SetupDragDrop(SelectedMediaPathTextBox,true);
            fileDragDropService.SetupDragDrop(SelectedSubtitlesPathTextBox, false);
            InitializeTimer();
        }
        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // ÿ�����һ��
            timer.Tick += Timer_Tick;
            isCounting = false;
        }
        private DispatcherTimer timer; // ���ڶ��ڸ���UI�ļ�ʱ��
        private TimeSpan elapsedTime; // ��¼�ܵľ���ʱ��
        private bool isCounting; // ����Ƿ����ڼ�ʱ
        private void Timer_Tick(object sender, object e)
        {
            // ����UI����ʾ��ǰ��ʱ
            elapsedTime += timer.Interval;
            string timeStr = string.Format("{0:D2}:{1:D2}:{2:D2}", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
            RunTimeTextBlock.Text = $"����ʱ��: {timeStr}";
        }

        private async void MediaToolsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SelectedMediaPathTextBox.IsEnabled = true;
            SelectMediaButton.IsEnabled = true;

            OutputMediaPathTextBox.IsEnabled = true;
            OutputMediaButton.IsEnabled = true;
            if (SelectedMediaPathTextBox != null && SelectedMediaPathTextBox.Text != string.Empty)
            {
                GetMediaInformation();//��ȡý����Ϣ
            }

            if ((sender as RadioButton)?.Name == "MediaCaptureRadioButton")
            {

                MediaBeginHourNumberBox.IsEnabled = true;
                MediaBeginMinuteNumberBox.IsEnabled = true;
                MediaBeginSecondNumberBox.IsEnabled = true;
                MediaBeginMillisecondsNumberBox.IsEnabled = true;
                EnableFinishTimeCheckBox.IsEnabled = true;
                if (SelectedMediaPathText.Count > 1)
                {
                    await ShowMessages.ShowDialog(this.XamlRoot, "���棡", $"�˹���ѡ�����ѡ��һ���ļ���", false);
                    SelectedMediaPathTextBox.Text = SelectedMediaPathText[0];
                    return;
                }
            }
            else
            {
                MediaBeginHourNumberBox.IsEnabled = false;
                MediaBeginMinuteNumberBox.IsEnabled = false;
                MediaBeginSecondNumberBox.IsEnabled = false;
                MediaBeginMillisecondsNumberBox.IsEnabled = false;

                MediaFinishHourNumberBox.IsEnabled = false;
                MediaFinishMinuteNumberBox.IsEnabled = false;
                MediaFinishSecondNumberBox.IsEnabled = false;
                MediaFinishMillisecondsNumberBox.IsEnabled = false;

                EnableFinishTimeCheckBox.IsEnabled = false;

            }
            if ((sender as RadioButton)?.Name == "VideoAddSubtitlesRadioButton")
            {
                SelectSubtitlesButton.IsEnabled = true;
                SelectedSubtitlesPathTextBox.IsEnabled = true;
                if (SelectedMediaPathText.Count > 1)
                {
                    await ShowMessages.ShowDialog(this.XamlRoot, "���棡", $"�˹���ѡ�����ѡ��һ���ļ���", false);
                    SelectedMediaPathTextBox.Text = SelectedMediaPathText[0];
                    return;
                }
            }
            else
            {
                SelectSubtitlesButton.IsEnabled = false;
                SelectedSubtitlesPathTextBox.IsEnabled = false;
            }
            if ((sender as RadioButton)?.Name == "FormatConversionRadioButton")
            {
                SelectFormatConversionComboBox.IsEnabled = true;
            }
            else
            {
                SelectFormatConversionComboBox.IsEnabled = false;
            }
        }
        private void RadioButtonIsEnabled(bool IsEnabled)
        {
            VideoTranscodingGPURadioButton.IsEnabled = IsEnabled;
            VideoTranscodingCPURadioButton.IsEnabled = IsEnabled;
            VideoAddSubtitlesRadioButton.IsEnabled = IsEnabled;
            MediaCaptureRadioButton.IsEnabled = IsEnabled;
            ExtractingAudioTracksRadioButton.IsEnabled = IsEnabled;
            FormatConversionRadioButton.IsEnabled = IsEnabled;

        }


        private void SelectFormatConversionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedMediaPathTextBox != null && SelectedMediaPathTextBox.Text != string.Empty)
            {
                GetMediaInformation();//��ȡý����Ϣ
            }

        }

        private async void EnableFinishTimeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MediaFinishHourNumberBox.IsEnabled = true;
            MediaFinishMinuteNumberBox.IsEnabled = true;
            MediaFinishSecondNumberBox.IsEnabled = true;
            MediaFinishMillisecondsNumberBox.IsEnabled = true;
            //��ȡ��Ƶʱ��
            string ffprobeCommand = $" -i \"{SelectedMediaPathTextBox.Text}\" -show_entries format=duration -v quiet -of csv=\"p=0\"";
            int exitCode = await RunProcess("ffprobe", ffprobeCommand);
            if (exitCode != 0)
            {
                await ShowMessages.ShowDialog(this.XamlRoot, "����", "�޷���ȡý��ʱ������ȷ��ѡ���ļ�Ϊý���ļ�", false);
                return;
            }
        }

        private void EnableFinishTimeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MediaFinishHourNumberBox.IsEnabled = false;
            MediaFinishMinuteNumberBox.IsEnabled = false;
            MediaFinishSecondNumberBox.IsEnabled = false;
            MediaFinishMillisecondsNumberBox.IsEnabled = false;
        }

        List<string> SelectedMediaPathText = new List<string>();
        private async void SelectMediaButton_Click(object sender, RoutedEventArgs e)
        {
            //SelectedMediaPathTextBox.Text = "";
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
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".mkv");
            openPicker.FileTypeFilter.Add(".avi");
            openPicker.FileTypeFilter.Add(".rmvb");
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a file           
            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                //SelectedMediaPathText.Clear();

                StringBuilder output = new StringBuilder();
                foreach (StorageFile file in files)
                {

                    output.Append(file.Path + ";");
                }
                SelectedMediaPathTextBox.Text += output.ToString();
            }

        }
        private async void SelectedMediaPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SelectedMediaPathText.Clear();
            string inputText = SelectedMediaPathTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(inputText))
            {
                string[] items = inputText.Split(';');
                foreach (string item in items)
                {
                    string trimmedItem = item.Trim();
                    if (!string.IsNullOrEmpty(trimmedItem))
                    {
                        SelectedMediaPathText.Add(trimmedItem);
                    }
                }
            }
            if (SelectedMediaPathText.Count > 1)
            {
                if (VideoAddSubtitlesRadioButton.IsChecked == true || MediaCaptureRadioButton.IsChecked == true)
                {
                    await ShowMessages.ShowDialog(this.XamlRoot, "���棡", $"�˹���ѡ�����ѡ��һ���ļ���", false);
                    SelectedMediaPathTextBox.Text = SelectedMediaPathText[0];
                    return;
                }
                else
                {
                    OutputMediaButton.IsEnabled = false;
                    OutputMediaPathTextBox.IsReadOnly = true;
                }
            }
            else
            {
                OutputMediaButton.IsEnabled = true;
                OutputMediaPathTextBox.IsReadOnly = false;
            }
            GetMediaInformation();//��ȡý����Ϣ
            if (EnableFinishTimeCheckBox.IsChecked == true)
            {
                //��ȡ��Ƶʱ��
                string ffprobeCommand = $" -i \"{SelectedMediaPathTextBox.Text}\" -show_entries format=duration -v quiet -of csv=\"p=0\"";
                int exitCode = await RunProcess("ffprobe", ffprobeCommand);
                if (exitCode != 0)
                {
                    await ShowMessages.ShowDialog(this.XamlRoot, "����", "�޷���ȡý��ʱ������ȷ��ѡ���ļ�Ϊý���ļ�", false);
                    return;
                }
            }
        }
        private void OutputMediaPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OutputMediaPathText.Clear();
            string outputText = OutputMediaPathTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(outputText))
            {
                string[] items = outputText.Split(';');
                foreach (string item in items)
                {
                    string trimmedItem = item.Trim();
                    if (!string.IsNullOrEmpty(trimmedItem))
                    {
                        OutputMediaPathText.Add(trimmedItem);
                    }
                }
            }


        }
        string MediaFolderPath;
        string MediaName;
        string MediaExtension;
        double MediaDuration;
        List<string> OutputMediaPathText = new List<string>();
        //��ȡý����Ϣ������ѡ���������·��
        private void GetMediaInformation()
        {
            //string ffprobeCommand = $" -i \"{SelectedMediaPath.Text}\" -show_entries format=duration -v quiet -of csv=\"p=0\"";
            //RunProcess("ffprobe", ffprobeCommand);
            //GetMediaDuration(SelectedMediaPath.Text);
            //OutputMediaPathText.Clear();
            OutputMediaPathTextBox.Text = string.Empty;
            StringBuilder output = new StringBuilder();
            foreach (string file in SelectedMediaPathText)
            {
                MediaOutputPathExtraction(file);
                string path = "";
                if (VideoTranscodingGPURadioButton.IsChecked == true)
                {
                    path = $"{MediaFolderPath}\\{MediaName}_gpu{MediaExtension}";
                }
                else if (VideoTranscodingCPURadioButton.IsChecked == true)
                {
                    path = $"{MediaFolderPath}\\{MediaName}_cpu{MediaExtension}";
                }
                else if (VideoAddSubtitlesRadioButton.IsChecked == true)
                {
                    path = $"{MediaFolderPath}\\{MediaName}_s.mkv";
                }
                else if (MediaCaptureRadioButton.IsChecked == true)
                {
                    path = $"{MediaFolderPath}\\{MediaName}_c{MediaExtension}";
                }
                else if (ExtractingAudioTracksRadioButton.IsChecked == true)
                {
                    path = $"{MediaFolderPath}\\{MediaName}.mp3";
                }
                else if (FormatConversionRadioButton.IsChecked == true)
                {
                    var SelectFormatType = SelectFormatConversionComboBox.SelectedItem as ComboBoxItem;
                    string FormatValue = SelectFormatType.Content.ToString();
                    path = $"{MediaFolderPath}\\{MediaName}_f{FormatValue}";
                }
                output.Append(path + ";");

            }
            OutputMediaPathTextBox.Text = output.ToString();
        }
        private void MediaOutputPathExtraction(string MediaFilePath)
        {

            MediaFolderPath = System.IO.Path.GetDirectoryName(MediaFilePath);// ��ȡĿ¼·���� MediaFolderPath ����
            string tempFileNameWithExtension = System.IO.Path.GetFileName(MediaFilePath);// ��ȡ�ļ�����������չ��������ʱ����
            MediaName = System.IO.Path.GetFileNameWithoutExtension(tempFileNameWithExtension); // ʹ�� Path.GetFileNameWithoutExtension ������ȡ�ļ����� MediaName ����
            MediaExtension = System.IO.Path.GetExtension(MediaFilePath);// ʹ�� Path.GetExtension ������ȡ��չ���� MediaExtension ����
        }
        private async void SelectSubtitlesButton_Click(object sender, RoutedEventArgs e)
        {
            //SelectedSubtitlesPathTextBox.Text = "";
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
            openPicker.FileTypeFilter.Add(".ass");
            openPicker.FileTypeFilter.Add(".srt");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                SelectedSubtitlesPathTextBox.Text = file.Path;
            }
        }
        private async void OutputMediaButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            //OutputMediaPathTextBox.Text = "";

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
            savePicker.FileTypeChoices.Add("ý���ļ�", new List<string>() { ".mkv", ".mp4", ".mp3" });

            savePicker.SuggestedFileName = "��Ƶ.mkv";

            // Open the picker for the user to pick a file
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                OutputMediaPathTextBox.Text = file.Path;
            }

        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {

            string ffmpegCommand = "";
            for (int i = 0; i < SelectedMediaPathText.Count; i++)
            {
                if (string.IsNullOrEmpty(SelectedMediaPathText[i]) || !File.Exists(SelectedMediaPathText[i]))
                {
                    string errorMessage = string.IsNullOrWhiteSpace(SelectedMediaPathText[i])
                         ? "��ѡ����Ч��ý���ļ�·����"
                         : "ָ�����ļ������ڣ�����·���Ƿ���ȷ��";
                    //XamlRoot xamlRoot = this.XamlRoot;
                    await ShowMessages.ShowDialog(this.XamlRoot, "���棡", $"{errorMessage}", false);

                    return;
                }
                if (EnableFinishTimeCheckBox.IsChecked == false)
                {
                    //��ȡ��Ƶʱ��
                    string ffprobeCommand = $" -i \"{SelectedMediaPathText[i]}\" -show_entries format=duration -v quiet -of csv=\"p=0\"";
                    int exitCode = await RunProcess("ffprobe", ffprobeCommand);
                    if (exitCode != 0)
                    {
                        await ShowMessages.ShowDialog(this.XamlRoot, "����", "�޷���ȡý��ʱ������ȷ��ѡ���ļ�Ϊý���ļ�", false);
                        return;
                    }
                }

                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

                if (VideoTranscodingGPURadioButton.IsChecked == true)
                {
                    if (localSettings.Values.ContainsKey("VideoTranscodingGPUCommand"))
                    {
                        ffmpegCommand = $" -i \"{SelectedMediaPathText[i]}\" {localSettings.Values["VideoTranscodingGPUCommand"].ToString()} -y \"{OutputMediaPathText[i]}\"";
                    }

                    //ffmpegCommand = $" -i \"{SelectedMediaPath.Text}\" -c:v libx264 -c:a copy -y \"{OutputMediaPath.Text}\"";
                    //ffmpegCommand = $" -i \"{SelectedMediaPathTextBox.Text}\" -c:a copy -c:v hevc_nvenc -pix_fmt yuv420p -profile:v main10 -cq 35 -bf 4 -b_ref_mode 2 -rc-lookahead 40 -preset p7 -g 300 -y \"{OutputMediaPathTextBox.Text}\"";


                }
                else if (VideoTranscodingCPURadioButton.IsChecked == true)
                {
                    if (localSettings.Values.ContainsKey("VideoTranscodingCPUCommand"))
                    {
                        ffmpegCommand = $" -i \"{SelectedMediaPathText[i]}\" {localSettings.Values["VideoTranscodingCPUCommand"].ToString()} -y \"{OutputMediaPathText[i]}\"";

                    }
                    //ffmpegCommand = $" -i \"{SelectedMediaPathTextBox.Text}\" -c:v libsvtav1 -crf 42 -bf 4 -preset 5 -g 240 -pix_fmt yuv420p10le -svtav1-params tune=0 -c:a copy -y \"{OutputMediaPathTextBox.Text}\"";

                }
                else if (VideoAddSubtitlesRadioButton.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(SelectedSubtitlesPathTextBox.Text) || !File.Exists(SelectedSubtitlesPathTextBox.Text))
                    {
                        string errorMessage = string.IsNullOrWhiteSpace(SelectedSubtitlesPathTextBox.Text)
                         ? "��ѡ����Ч����Ļ�ļ�·����"
                         : "ָ�����ļ������ڣ�����·���Ƿ���ȷ��";
                        await ShowMessages.ShowDialog(this.XamlRoot, "���棡", $"{errorMessage}", false);
                        return;
                    }
                    ffmpegCommand = $" -i \"{SelectedMediaPathText[i]}\" -i \"{SelectedSubtitlesPathTextBox.Text}\" -map 0 -map 1 -c copy -disposition:s:0 default -y \"{OutputMediaPathText[i]}\"";

                }
                else if (MediaCaptureRadioButton.IsChecked == true)
                {
                    double BeginSeconds = double.Parse(MediaBeginHourNumberBox.Text) * 3600 + double.Parse(MediaBeginMinuteNumberBox.Text) * 60 + double.Parse(MediaBeginSecondNumberBox.Text) + double.Parse(MediaBeginMillisecondsNumberBox.Text) / 1000;
                    double FinishSeconds = double.Parse(MediaFinishHourNumberBox.Text) * 3600 + double.Parse(MediaFinishMinuteNumberBox.Text) * 60 + double.Parse(MediaFinishSecondNumberBox.Text) + double.Parse(MediaFinishMillisecondsNumberBox.Text) / 1000;

                    if (BeginSeconds >= FinishSeconds)
                    {
                        await ShowMessages.ShowDialog(this.XamlRoot, "���棡", "��ʼʱ�䳬������ʱ�䣬���������룡", false);
                        return;
                    }
                    if (FinishSeconds > MediaDuration)
                    {
                        await ShowMessages.ShowDialog(this.XamlRoot, "���棡", "�趨ʱ�䳬����Ƶ��ʱ�������������룡", false);
                        return;
                    }
                    if (EnableFinishTimeCheckBox.IsChecked == false)
                    {
                        ffmpegCommand = $" -i \"{SelectedMediaPathText[i]}\" -ss {MediaBeginHourNumberBox.Text}:{MediaBeginMinuteNumberBox.Text}:{MediaBeginSecondNumberBox.Text}.{MediaBeginMillisecondsNumberBox.Text} -c copy -y \"{OutputMediaPathText[i]}\"";
                    }
                    else
                    {
                        ffmpegCommand = $" -i \"{SelectedMediaPathText[i]}\" -ss {MediaBeginHourNumberBox.Text}:{MediaBeginMinuteNumberBox.Text}:{MediaBeginSecondNumberBox.Text}.{MediaBeginMillisecondsNumberBox.Text} -to {MediaFinishHourNumberBox.Text}:{MediaFinishMinuteNumberBox.Text}:{MediaFinishSecondNumberBox.Text}.{MediaFinishMillisecondsNumberBox.Text} -c copy -y \"{OutputMediaPathText[i]}\"";
                    }
                }
                else if (ExtractingAudioTracksRadioButton.IsChecked == true)
                {
                    ffmpegCommand = $" -i \"{SelectedMediaPathText[i]}\" -vn -acodec libmp3lame -q:a 0 -y \"{OutputMediaPathText[i]}\"";

                }
                else if (FormatConversionRadioButton.IsChecked == true)
                {
                    ffmpegCommand = $" -i \"{SelectedMediaPathText[i]}\" -map 0 -c copy -y \"{OutputMediaPathText[i]}\"";

                }
                if (File.Exists(OutputMediaPathText[i]))
                {
                    ContentDialogResult result = await ShowMessages.ShowDialog(this.XamlRoot, "�Ƿ񸲸ǣ�", "�ļ��Ѵ��ڣ��Ƿ񸲸ǣ�", true);
                    if (result == ContentDialogResult.Primary)
                    {
                        CurrentProcessingTextBlock.Text = $"��ǰ����{i + 1}/{SelectedMediaPathText.Count}";
                        int exitCode = await RunProcess("ffmpeg", ffmpegCommand);
                        if (exitCode != 0)
                        {
                            await ShowMessages.ShowDialog(this.XamlRoot, "ʧ�ܣ�", $"����ִ��ʧ�ܣ��˳����룺{exitCode}", false);
                        }
                        else
                        {
                            UpdateProgressBar(100);
                            ShowMessages.SendNotificationToast("����ִ�гɹ���", $"\"{SelectedMediaPathText[i]}\"�������ļ��ѱ��浽��\"{OutputMediaPathText[i]}\"��");
                            //TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    CurrentProcessingTextBlock.Text = $"��ǰ����{i + 1}/{SelectedMediaPathText.Count}";
                    int exitCode = await RunProcess("ffmpeg", ffmpegCommand);
                    if (exitCode != 0)
                    {
                        await ShowMessages.ShowDialog(this.XamlRoot, "ʧ�ܣ�", $"����ִ��ʧ�ܣ��˳����룺{exitCode}", false);
                    }
                    else
                    {
                        UpdateProgressBar(100);
                        ShowMessages.SendNotificationToast("����ִ�гɹ���", $"\"{SelectedMediaPathText[i]}\"�������ļ��ѱ��浽��\"{OutputMediaPathText[i]}\"��");
                    }
                }
            }

           



        }

        


        //��������
        private async Task<int> RunProcess(string ProcessName, string ProcessCommand)
        {
            ConfirmButton.IsEnabled = false;
            RadioButtonIsEnabled(false);
            OutputTextBox.Text = "";
            progressBar.Value = 0;
            ProgressValueTextBlock.Text = "����ֵ��0.00%";
            if (!isCounting && ProcessName == "ffmpeg")
            {
                // ��ʼ��ʱ
                isCounting = true;
                elapsedTime = TimeSpan.Zero;
                timer.Start();
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ProcessName,
                Arguments = ProcessCommand,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8//���ffmpeg�����Ϣ��������           
            };
            //��������
            Process process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            // ��UI�߳��ϸ����ı������� 
            DataReceivedEventHandler outputHandler = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data) && args.Data != "N/A")
                {

                    DispatcherQueue.TryEnqueue(() =>
                    {

                        if (ProcessName == "ffprobe")
                        {
                            MediaDuration = double.Parse(args.Data);
                            UpdateTimes(MediaDuration);//��ȡý��ʱ��
                        }
                        else
                        {
                            //ʵʱ�������״̬
                            OutputTextBox.Text += args.Data + Environment.NewLine;
                            TextBoxScrollToEnd(OutputTextBox);
                            
                        }

                    });
                    if (ProcessName == "ffmpeg")
                    {
                        UpdateProgressFromffmpegOutput(args.Data);//���½�����
                    }
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
                if ((ex.Message.Contains("ffmpeg") && ex.Message.Contains("ϵͳ�Ҳ���ָ�����ļ�")) || (ex.Message.Contains("ffprobe") && ex.Message.Contains("ϵͳ�Ҳ���ָ�����ļ�")))
                {
                    await ShowMessages.ShowDialog(this.XamlRoot, "����", $"������һ�����󣡴�����Ϣ��{ex.Message}\n\n�뵽\"https://ffmpeg.org\"����ffmpeg.exe����ffprobe.exe�������õ�����������", false);
                }
                else
                {
                    await ShowMessages.ShowDialog(this.XamlRoot, "����", $"������һ�����󣡴�����Ϣ��{ex.Message}", false);
                }
                ConfirmButton.IsEnabled = true;
                RadioButtonIsEnabled(true);
                // �������˳���ֹͣ��ʱ
                isCounting = false;
                timer.Stop();
                return 1;
            }

            GlobalData.ManagedProcesses.Add(process);
            // ��ʼ�첽��ȡ�������
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();// �ȴ��������

            process.OutputDataReceived -= outputHandler;
            process.ErrorDataReceived -= outputHandler;
            int exitCode = process.ExitCode;


            ConfirmButton.IsEnabled = true;
            RadioButtonIsEnabled(true);
            // �������˳���ֹͣ��ʱ
            isCounting = false;
            timer.Stop();
            return exitCode;
        }

        private void UpdateTimes(double durationSeconds)
        {
            int Hours = (int)Math.Floor(durationSeconds / 3600);
            int Minutes = (int)Math.Floor((durationSeconds % 3600) / 60);
            int Seconds = (int)Math.Floor((durationSeconds % 60));
            int Milliseconds = (int)((durationSeconds % 1) * 1000);
            MediaFinishHourNumberBox.Text = Hours.ToString("00");
            MediaFinishMinuteNumberBox.Text = Minutes.ToString("00");
            MediaFinishSecondNumberBox.Text = Seconds.ToString("00");
            MediaFinishMillisecondsNumberBox.Text = Milliseconds.ToString("000");
        }

        private void UpdateProgressFromffmpegOutput(string outputLine)
        {
            const string timePattern = @"time=(\d{2}:\d{2}:\d{2}\.\d{2})";//ffmpeg�������Ϊ��λ
            Match match = Regex.Match(outputLine, timePattern);
            if (!match.Success) return;

            string rawTimeStr = match.Groups[1].Value;
            double currentTimeSeconds = ParseTimeToSeconds(rawTimeStr);

            double progressPercentage = currentTimeSeconds / MediaDuration * 100;
            UpdateProgressBar(progressPercentage);
        }

        private double ParseTimeToSeconds(string rawTimeStr)
        {
            string[] timeParts = rawTimeStr.Split(':');
            int hours = int.Parse(timeParts[0]);
            int minutes = int.Parse(timeParts[1]);
            double seconds = double.Parse(timeParts[2].Substring(0, 2)) + double.Parse(timeParts[2].Substring(3)) / 100; // ���Ǻ��벿��

            return hours * 3600 + minutes * 60 + seconds;
        }

        //���½�����
        private void UpdateProgressBar(double progressPercentage)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                progressBar.Value = Math.Min(100, Math.Max(0, progressPercentage));
                string processvalue = progressBar.Value.ToString("N2");
                ProgressValueTextBlock.Text = $"����ֵ��{processvalue}%";
            });
        }

        private void TextBoxScrollToEnd(TextBox textBox)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(textBox, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                break;
            }
        }


    }
}
