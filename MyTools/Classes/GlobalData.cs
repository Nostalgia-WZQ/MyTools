using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTools.Classes
{
    internal class GlobalData
    {
        public static List<Process> ManagedProcesses { get; set; } = new List<Process>();//收集开启的进程
        public static string VideoTranscodingGPUCommandDefault= $"-c:a copy -c:v hevc_nvenc -pix_fmt yuv420p -profile:v main10 -cq 35 -bf 4 -b_ref_mode 2 -rc-lookahead 40 -preset p7 -g 300";
        public static string VideoTranscodingCPUCommandDefault = $"-c:a copy -c:v libsvtav1 -crf 42 -bf 4 -preset 5 -g 240 -pix_fmt yuv420p10le -svtav1-params tune=0";
        public static string VideoAddSubtitlesCommandDefault = $"-map 0 -map 1 -c copy -disposition:s:0 default";
        public static string MergeAudioAndVideoCommandDefault = $"-map 0:v:0 -map 1:a:0 -c copy";
    }
}
