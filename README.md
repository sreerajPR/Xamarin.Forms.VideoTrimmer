# Xamarin.Forms.VideoTrimmer
Xamarin.Forms Library that targets Android and iOS, to trim videos

Nuget package: https://www.nuget.org/packages/Xamarin.Forms.VideoTrimmer

# Setup

Install Nuget Package in Xamarin.Android, Xamarin.iOS and Xamarin.Forms projects.

# API Usage

Invoke `VideoTrimmerService.Instance.TrimAsync(int startMS, int lengthMS, string inputPath, string outputPath)` from Xamarin.Forms project.

## Example

```C#
//inputFilePath is the file path of input video
//startMS and lengthMS to be provided in milli seconds
if (await VideoTrimmerService.Instance.TrimAsync(startTime * 1000, lengthInSeconds * 1000, inputFilePath, outputPath))
{
    //if the TrimAsync method returns true, trimmed video will be present at "outputPath" location
    await DisplayAlert("", "Video Trimmed Successfully", "OK");
}
else
{
    await DisplayAlert("", "Video Trimming failed", "OK");
}

```
