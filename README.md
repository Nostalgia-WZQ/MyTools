<p align="center">
<img src="Images/Logo/MyTools.png" width="100px"/>
</p>
<div align="center">

# MyTools
[![GitHub Repo stars](https://img.shields.io/github/stars/Nostalgia-WZQ/MyTools?style=flat)](https://github.com/Nostalgia-WZQ/MyTools/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/Nostalgia-WZQ/MyTools)](https://github.com/Nostalgia-WZQ/MyTools/forks)

[![GitHub License](https://img.shields.io/github/license/Nostalgia-WZQ/MyTools)](https://github.com/Nostalgia-WZQ/MyTools/blob/master/LICENSE.txt)
[![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/Nostalgia-WZQ/MyTools)](https://github.com/Nostalgia-WZQ/MyTools/issues?q=is%3Aopen+is%3Aissue)
[![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-closed/Nostalgia-WZQ/MyTools)](https://github.com/Nostalgia-WZQ/MyTools/issues?q=is%3Aissue+is%3Aclosed)

[![GitHub Release](https://img.shields.io/github/v/release/Nostalgia-WZQ/MyTools)](https://github.com/Nostalgia-WZQ/MyTools/releases)
[![GitHub Release Date](https://img.shields.io/github/release-date/Nostalgia-WZQ/MyTools)](https://github.com/Nostalgia-WZQ/MyTools/releases/latest)
[![GitHub Downloads (all assets, latest release)](https://img.shields.io/github/downloads/Nostalgia-WZQ/MyTools/latest/total)](https://github.com/Nostalgia-WZQ/MyTools/releases/latest)
[![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/Nostalgia-WZQ/MyTools/total)](https://github.com/Nostalgia-WZQ/MyTools/releases) 
</div>

# 目录
- [MyTools](#MyTools)
- [概述](#概述)
	- [媒体工具](#媒体工具)
	- [磁盘工具](#磁盘工具)
	- [计算哈希值](#计算哈希值)
	- [查看导出目录树](#查看导出目录树)
	- [其他工具](#其他工具)
	- [设置界面](#设置界面)
- [安装](#安装)
	- [系统要求](#系统要求)
	- [安装](#安装)
		- [方法一（微软应用商店安装）](#方法一微软应用商店安装)
		- [方法二（手动安装）](#方法二手动安装)
- [免责声明](#免责声明)
- [Star数量统计](#Star数量统计)

# 概述
该应用主要通过调用FFmpeg、CMD、PowerShell等进程进行相关操作，所以其具备的功能通过命令行和批处理程序也可实现，但本人喜欢鼠标点点点，所以学着做了这款应用。非专业人士，应用优化、代码规范等后续慢慢来。   
早前版本的简单演示视频：【B站】https://b23.tv/gDqRK84
## 媒体工具
需ffmpeg（含ffprobe），请到 https://ffmpeg.org 下载并配置到环境变量，具体方法自行搜索。  
预设GPU转码命令：高压缩中质量，速度快，必须是支持hevc硬编码的英伟达显卡（如RTX20,30,40）。     
预设CPU转码命令：高压缩高质量，速度慢。    
视频转码命令可到设置页更改。    
![媒体工具](Images/Screenshots/媒体工具.png)
## 磁盘工具
此页功能需以管理员权限运行该应用，挂载虚拟磁盘功能需开启Hyper-V功能。
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
需Windows 10 Build 17763 及以上，支持 x64。
## 安装
### 方法一（微软应用商店安装）

<a href="https://apps.microsoft.com/detail/9p8kkslj76g3?referrer=appbadge&mode=direct">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

### 方法二（手动安装）
进入[Release](https://github.com/Nostalgia-WZQ/MyTools/releases)页面，下载.msix文件双击安装。
# 免责声明
该应用部分功能会对电脑内的文件进行操作，为了确保数据安全，请务必在操作前做好原始文件的备份。在操作过程中若发生文件被覆盖、删除或丢失等意外情况，本软件不承担相关责任。请谨慎操作，保障个人数据安全。
# Star数量统计
[![Stargazers over time](https://starchart.cc/Nostalgia-WZQ/MyTools.svg?variant=adaptive)](https://starchart.cc/Nostalgia-WZQ/MyTools)