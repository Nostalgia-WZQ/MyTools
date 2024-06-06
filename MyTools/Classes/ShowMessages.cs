using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.AppNotifications;

namespace MyTools.Class
{
    internal class ShowMessages
    {
        public static async Task<ContentDialogResult> ShowDialog(XamlRoot xamlRoot, string DialogTitle, string DialogContent,bool ShowCloseButton)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = xamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = DialogTitle;
            dialog.PrimaryButtonText = "确定";
            //dialog.SecondaryButtonText = "Don't Save";
            if (ShowCloseButton)
            {
                dialog.CloseButtonText = "取消";
            }            
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = DialogContent;

            var result = await dialog.ShowAsync();
            return result;
        }

        public static bool SendNotificationToast(string title, string message)
        {
            var xmlPayload = new string($@"
        <toast>    
            <visual>    
                <binding template=""ToastGeneric"">    
                    <text>{title}</text>
                    <text>{message}</text>    
                </binding>
            </visual>  
        </toast>");

            var toast = new AppNotification(xmlPayload);
            AppNotificationManager.Default.Show(toast);
            return toast.Id != 0;
        }


    }
}
