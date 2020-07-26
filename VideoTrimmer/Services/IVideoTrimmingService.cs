using System;
using System.Threading.Tasks;

namespace VideoTrimmer.Services
{
    internal interface IVideoTrimmingService
    {
        Task<bool> TrimAsync(int startMS, int lengthMS, string inputPath, string outputPath);
    }
}
