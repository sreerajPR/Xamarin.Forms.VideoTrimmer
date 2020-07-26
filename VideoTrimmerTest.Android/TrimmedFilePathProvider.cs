using System;
using System.IO;
using VideoTrimmerTest.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(TrimmedFilePathProvider))]
namespace VideoTrimmerTest.Droid
{
    public class TrimmedFilePathProvider: ITrimmedFilePathProvider
    {
        public string GetOutputPath()
        {
            string rmConnectDirectoryPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "VideoTrimmerTest");
            Java.IO.File rmConnectDirectoryFile = new Java.IO.File(rmConnectDirectoryPath);
            rmConnectDirectoryFile.Mkdirs();
            string fileName = DateTime.UtcNow.ToString("yyyyMMddhhmmss") + ".mp4";
            return Path.Combine(rmConnectDirectoryPath, fileName);
        }
    }
}
