using System;
using System.Threading;
using System.Diagnostics;
using UnityEngine;

namespace FFmpegOut.LiveStream
{
    public sealed class StreamFFmpegSession : FFmpegSession
    {
        private Thread ffmpegThread;
        private Process ffmpegProcess;
        private string ffmpegArguments;

        private const string UNITY_CAM_TEX_BYTE_FORMAT =
            "-pixel_format rgba -colorspace bt709 -f rawvideo -vcodec rawvideo";
        private const string FFMPEG_LOGLEVEL = "-loglevel warning";

        private StreamFFmpegSession(string arguments) : base(arguments)
        {
            this.ffmpegArguments = arguments;
        }

        public static StreamFFmpegSession Create(
            int width, int height, float frameRate,
            FFmpegPreset encodingPreset, StreamPreset streamPreset,
            string address)
        {
            string ffmpegArguments =
                $"{UNITY_CAM_TEX_BYTE_FORMAT} {FFMPEG_LOGLEVEL} -framerate {frameRate} -video_size {width}x{height} "
                + $"-re -i pipe:0 {encodingPreset.GetOptions()} "
                + $"{streamPreset.GetOptions()} {address}";
            UnityEngine.Debug.Log($"FFmpeg Arguments: {ffmpegArguments}");
            return new StreamFFmpegSession(ffmpegArguments);
        }

        public void Start()
        {
            if (ffmpegThread == null || !ffmpegThread.IsAlive)
            {
                ffmpegThread = new Thread(ExecuteFFmpeg);
                ffmpegThread.IsBackground = true;
                ffmpegThread.Start();
            }
        }

        private void ExecuteFFmpeg()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("ffmpeg", ffmpegArguments)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            ffmpegProcess = new Process { StartInfo = startInfo };
            ffmpegProcess.Start();

            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.BeginErrorReadLine();
            ffmpegProcess.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log("FFmpeg output: " + args.Data);
            ffmpegProcess.ErrorDataReceived += (sender, args) => UnityEngine.Debug.Log("FFmpeg error: " + args.Data);
        }

        public void Stop()
        {
            if (ffmpegProcess != null)
            {
                if (!ffmpegProcess.HasExited)
                {
                    ffmpegProcess.Kill();
                }
                ffmpegProcess.Dispose();
            }
            if (ffmpegThread != null)
            {
                if (ffmpegThread.IsAlive)
                {
                    ffmpegThread.Join();  // Wait for the thread to finish
                }
                ffmpegThread = null;
            }
        }
    }
}
