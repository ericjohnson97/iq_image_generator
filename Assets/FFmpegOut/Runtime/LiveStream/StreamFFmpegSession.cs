using System;
using UnityEngine;

namespace FFmpegOut.LiveStream
{
    /// <summary>
    /// Represents a session for streaming video from Unity to an RTP endpoint using FFmpeg.
    /// </summary>
    public sealed class StreamFFmpegSession : FFmpegSession
    {
        // Defines the format and codec for the raw video data from Unity.
        private const string UNITY_CAM_TEX_BYTE_FORMAT =
            "-pixel_format rgba -colorspace bt709 -f rawvideo -vcodec rawvideo";
        // private const string UNITY_CAM_TEX_BYTE_FORMAT =
        //     "-pixel_format rgba";

        // Sets the FFmpeg logging level to warning.
        private const string FFMPEG_LOGLEVEL = "-loglevel warning";

        // Private constructor to enforce the use of the static Create method.
        private StreamFFmpegSession(string arguments) : base(arguments) { }

        /// <summary>
        /// Creates a new FFmpeg session for streaming.
        /// </summary>
        /// <param name="width">The width of the video.</param>
        /// <param name="height">The height of the video.</param>
        /// <param name="frameRate">The frame rate of the video.</param>
        /// <param name="encodingPreset">The encoding preset to use.</param>
        /// <param name="streamPreset">The streaming preset to use.</param>
        /// <param name="address">The address to stream to.</param>
        /// <returns>A new instance of StreamFFmpegSession.</returns>
        public static StreamFFmpegSession Create(
            int width, int height, float frameRate,
            FFmpegPreset encodingPreset, StreamPreset streamPreset,
            string address)
        {
            // Constructs the FFmpeg command-line arguments for streaming.
            string ffmpegArguments =
                $"{UNITY_CAM_TEX_BYTE_FORMAT} {FFMPEG_LOGLEVEL} -framerate {frameRate} -video_size {width}x{height} "
                + $"-re -i pipe:0 {encodingPreset.GetOptions()} "
                + $"{streamPreset.GetOptions()} {address}";

            // Logs the constructed FFmpeg arguments for debugging purposes.
            Debug.Log($"FFmpeg Arguments: {ffmpegArguments}");

            // Returns a new FFmpeg session with the constructed arguments.
            return new StreamFFmpegSession(ffmpegArguments);
        }
    }
}
