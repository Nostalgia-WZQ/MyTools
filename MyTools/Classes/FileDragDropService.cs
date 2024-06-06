using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace MyTools.Classes
{
    internal class FileDragDropService
    {
        private bool ProcessMultipleFiles = false; // 默认设置为false，即只处理单个文件
        public void SetupDragDrop(TextBox textBox,bool processMultipleFiles)
        {
            textBox.AllowDrop = true;
            textBox.DragOver += SelectedFilePathTextBox_DragOver;
            textBox.Drop += SelectedFilePathTextBox_Drop;
            ProcessMultipleFiles=processMultipleFiles;
        }
        private void SelectedFilePathTextBox_DragOver(object sender, DragEventArgs e)
        {
            // 如果拖放的数据包含文件，则允许复制效果
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;

            }
            e.Handled = true;
        }

        private async void SelectedFilePathTextBox_Drop(object sender, DragEventArgs e)
        {
            // 获取触发事件的文本框实例
            TextBox textBox = sender as TextBox;
            // 获取拖放的数据包
            var dataPackageView = e.DataView;
            // 检查数据包中是否包含文件
            if (dataPackageView.Contains(StandardDataFormats.StorageItems))
            {
                // 获取文件列表
                var items = await dataPackageView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    if (ProcessMultipleFiles)
                    {
                        // 处理所有文件
                        foreach (var item in items)
                        {
                            if (item != null)
                            {
                                textBox.Text += item.Path + ";";
                            }
                        }
                    }
                    else
                    {
                        // 只处理第一个文件
                        var item = items[0];
                        if (item != null)
                        {
                            // 获取文件的路径并更新TextBlock
                            textBox.Text = item.Path;
                        }
                    }
                   
                }
            }
            e.Handled = true;
        }
    }
}
