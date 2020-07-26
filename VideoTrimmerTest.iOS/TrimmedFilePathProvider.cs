using System;
using System.IO;
using Foundation;
using UIKit;
using VideoTrimmerTest.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(TrimmedFilePathProvider))]
namespace VideoTrimmerTest.iOS
{
    public class TrimmedFilePathProvider: ITrimmedFilePathProvider
    {
        public string GetOutputPath()
        {
            int SystemVersion = Convert.ToInt16(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
            string filename = DateTime.Now.ToString("yyyyMMddhhmmss") + ".mov";
            string outputPath;
            if (SystemVersion >= 8)
            {
                var documents = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
                outputPath = Path.Combine(documents, filename);
            }
            else
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // iOS 7 and earlier
                outputPath = Path.Combine(documents, filename);
            }

            return outputPath;
        }
    }
}
