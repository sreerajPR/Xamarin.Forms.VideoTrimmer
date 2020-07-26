using System;
using System.Threading.Tasks;

namespace VideoTrimmer.Services
{
    internal interface IVideoTrimmingService
    {
        Task<bool> Trim(int startMS, int lengthMS, string inputPath, string outputPath);
    }
}
