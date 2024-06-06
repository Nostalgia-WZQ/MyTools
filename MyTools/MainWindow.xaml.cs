using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using MyTools.Class;
using MyTools.Classes;
using MyTools.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyTools
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            AppWindow.Closing += AppWindow_Closing;
            current = this;
            ExtendsContentIntoTitleBar = true;
            ContentFrame.Navigated += On_Navigated;
        }

        public static MainWindow current;
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

            if (args.IsSettingsSelected == true)
            {
                NavView_Navigate(typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                Type navPageType = Type.GetType(args.SelectedItemContainer.Tag.ToString());
                NavView_Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
            }

        }
        private void NavView_Navigate(Type navPageType, NavigationTransitionInfo transitionInfo)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null && !Type.Equals(preNavPageType, navPageType))
            {
                ContentFrame.Navigate(navPageType, null, transitionInfo);
            }
        }
        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
                NavView.Header = "设置";
            }
            else if (ContentFrame.SourcePageType != null)
            {
                // Select the nav view item that corresponds to the page being navigated to.
                NavView.SelectedItem = NavView.MenuItems
                            .OfType<NavigationViewItem>()
                            .First(i => i.Tag.Equals(ContentFrame.SourcePageType.FullName.ToString()));

                NavView.Header =
                    ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();

            }
        }
        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }
        private bool TryGoBack()
        {
            if (!ContentFrame.CanGoBack)
                return false;
            // Don't go back if the nav pane is overlayed.
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                 NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;
            ContentFrame.GoBack();
            return true;
        }

        private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            args.Cancel = true;
            string processesToClose = "";
            int processCount = 0;
            // 获取GlobalData中托管的进程列表
            List<Process> ManagedProcesses = GlobalData.ManagedProcesses;
            // 收集即将关闭的进程信息
            foreach (var managedProcess in ManagedProcesses)
            {
                if (!managedProcess.HasExited)
                {
                    processesToClose += $"- {managedProcess.ProcessName} ({managedProcess.Id})\n";
                    processCount++;
                }
            }
            if (processCount > 0)
            {
                ContentDialogResult result = await ShowMessages.ShowDialog(rootPanel.XamlRoot, "警告", $"您确定要关闭应用程序吗？以下{processCount}个进程也将被终止：\n{processesToClose}",true);

                if (result == ContentDialogResult.Primary)
                {
                    // 在关闭前再次弹窗显示即将关闭的进程
                    await ShowMessages.ShowDialog(rootPanel.XamlRoot, "警告", $"即将关闭以下进程：\n{processesToClose}",false);

                    foreach (var managedProcess in ManagedProcesses)
                    {
                        if (!managedProcess.HasExited)
                        {
                            managedProcess.CloseMainWindow();
                            if (!managedProcess.WaitForExit(1000))
                            {
                                managedProcess.Kill();
                            }
                        }
                    }
                    ManagedProcesses.Clear(); // 清空列表，防止资源泄漏
                    Application.Current.Exit();
                }
            }
            else
            {
                Application.Current.Exit();// 如果没有关联进程，直接关闭应用程序

            }


        }
       








    }
}
