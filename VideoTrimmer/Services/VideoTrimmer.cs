using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VideoTrimmer.Services
{
    public class VideoTrimmerService
    {
        IVideoTrimmingService videoTrimmingService;

        private static VideoTrimmerService instance;

        public static VideoTrimmerService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new VideoTrimmerService();
                }

                return instance;
            }
            
        }

        private VideoTrimmerService()
        {
            videoTrimmingService = DependencyService.Get<IVideoTrimmingService>();
        }

        public async Task<bool> TrimAsync(int startMS, int lengthMS, string inputPath, string outputPath)
        {
            //Todo: All validations should be done and proper exceptions should be raised here.
            return await videoTrimmingService.TrimAsync(startMS, lengthMS, inputPath, outputPath);
        }
    }
}
