using System;
using System.Linq;
using System.Threading.Tasks;
using AVFoundation;
using CoreMedia;
using Foundation;
using UIKit;
using VideoTrimmer.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(VideoTrimmer.PlatformImplementations.iOS.VideoTrimmingService))]
namespace VideoTrimmer.PlatformImplementations.iOS
{
    internal class VideoTrimmingService : IVideoTrimmingService
    {
        public async Task<bool> TrimAsync(int startMS, int lengthMS, string inputPath, string outputPath)
        {
            bool didOperationSucceed = false;
            try
            {
                NSUrl inputFileUrl = new NSUrl(inputPath, false);
                AVAsset videoAsset = AVAsset.FromUrl(inputFileUrl);

                var compatiblePresets = AVAssetExportSession.ExportPresetsCompatibleWithAsset(videoAsset).ToList();
                var preset = "";

                if (compatiblePresets.Contains("AVAssetExportPresetLowQuality"))
                {
                    preset = "AVAssetExportPresetLowQuality";
                }
                else
                {
                    preset = compatiblePresets.FirstOrDefault();
                }

                AVAssetExportSession exportSession = new AVAssetExportSession(videoAsset, preset);
                exportSession.OutputUrl = NSUrl.FromFilename(outputPath);
                exportSession.OutputFileType = AVFileType.QuickTimeMovie;

                CMTime start = CMTime.FromSeconds(startMS / 1000, videoAsset.Duration.TimeScale);
                CMTime duration = CMTime.FromSeconds(lengthMS / 1000, videoAsset.Duration.TimeScale);
                CMTimeRange range = new CMTimeRange();
                range.Start = start;
                range.Duration = duration;
                exportSession.TimeRange = range;

                exportSession.OutputFileType = AVFileType.QuickTimeMovie;
                await exportSession.ExportTaskAsync();

                if (exportSession.Status != AVAssetExportSessionStatus.Completed)
                {
                    Console.WriteLine("Log starts...");
                    Console.WriteLine(exportSession.Status.ToString());
                    Console.WriteLine(exportSession.Error);
                }

                didOperationSucceed = true;
            }
            catch (Exception es)
            {

            }
            return didOperationSucceed;
        }
    }
}
