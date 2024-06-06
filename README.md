# 概述
该应用主要通过调用ffmpeg、CMD、powershell等进程进行相关操作，所以其具备的功能通过命令行和批处理程序也可实现，但本人喜欢鼠标点点点，所以学着做了这款应用。非专业人士，应用优化、代码规范等后续慢慢来。
## 媒体工具
需ffmpeg（含ffprobe），请到 https://ffmpeg.org 下载并配置到环境变量，具体方法自行搜索。  
视频转码命令可到设置页更改。
![媒体工具](Images/Screenshots/媒体工具.png)
## 磁盘工具
需以管理员权限运行该应用，挂载虚拟磁盘功能需开启Hyper-V功能。
![磁盘工具](Images/Screenshots/磁盘工具.png)
## 计算哈希值
![计算哈希值](Images/Screenshots/计算哈希值.png)
## 查看导出目录树
![查看导出目录树](Images/Screenshots/查看导出目录树.png)
## 其他工具
![其他工具](Images/Screenshots/其他工具.png)
## 设置界面
![设置](Images/Screenshots/设置.png)
# 安装
## 系统要求
需Windows 10 Build 17763 及以上，支持 ARM64/x86/x64。
## 安装步骤
1.进入[Release](https://github.com/Nostalgia-WZQ/MyTools/releases)页面，下载压缩包并解压。  
2.打开系统设置，搜索开发者选项，打开“开发人员模式”。滚动到页面底部，展开“PowerShell”，开启“更改执行策略...”选项。  
3.应用包解压后，右键单击文件夹中的“install.ps1”脚本，选择“使用 PowerShell运行”。  
4.安装完成后可关闭第二步打开的开关，后续更新或重新安装直接双击后缀为.msix的文件即可。
# 免责声明
该应用部分功能会对电脑内的文件进行操作，为了确保数据安全，请务必在操作前做好原始文件的备份。在操作过程中若发生文件被覆盖、删除或丢失等意外情况，本软件不承担相关责任。请谨慎操作，保障个人数据安全。
