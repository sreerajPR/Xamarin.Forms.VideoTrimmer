using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Media;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using VideoTrimmer.Services;
using Xamarin.Essentials;

namespace VideoTrimmerTest
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        bool isBusy;
        string inputFilePath;

        public MainPage()
        {
            InitializeComponent();

            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
        }

        async void PickVideo_Clicked(System.Object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (await CheckAndRequestPermissionAsync<Permissions.StorageRead>("Storage") && await CheckAndRequestPermissionAsync<Permissions.Photos>("Photos"))
            {
                if (!CrossMedia.Current.IsPickVideoSupported)
                {
                    await DisplayAlert("No Gallery Access", ":( No access to gallery available.", "OK");
                    return;
                }

                if (isBusy)
                    return;
                isBusy = true;

                var file = await CrossMedia.Current.PickVideoAsync();

                if (file == null)
                {
                    isBusy = false;
                    return;
                }

                Status.Text = "Video Picked. Enter Start time and Length required, and press Trim Button";

                inputFilePath = file.Path;

                isBusy = false;
            }
        }

        async void TrimVideo_Clicked(System.Object sender, System.EventArgs e)
        {
            int startTime, endTime;

            if(!int.TryParse(StartTime.Text,out startTime))
            {
                await DisplayAlert("", "Start Time is not in proper format", "OK");
                return;
            }
            if (!int.TryParse(Length.Text, out endTime))
            {
                await DisplayAlert("", "Length is not in proper format", "OK");
                return;
            }

            if(string.IsNullOrWhiteSpace(inputFilePath))
            {
                await DisplayAlert("", "Video for trimming is not selected", "OK");
                return;
            }

            string outputPath = DependencyService.Get<ITrimmedFilePathProvider>().GetOutputPath();

            TrimmingIndicatorView.IsVisible = true;

            try
            {
                if (await VideoTrimmerService.Instance.TrimAsync(startTime * 1000, endTime * 1000, inputFilePath, outputPath))
                {
                    await DisplayAlert("", "Video Trimmed Successfully", "OK");
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = Title,
                        File = new ShareFile(outputPath)
                    });
                }
                else
                {
                    await DisplayAlert("", "Video Trimming failed", "OK");
                }
            }
            finally
            {
                TrimmingIndicatorView.IsVisible = false;
            }
            
        }

        private async Task<bool> CheckAndRequestPermissionAsync<TPermission>(string permissionName) where TPermission : Permissions.BasePermission, new()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<TPermission>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<TPermission>();
                    //Best practice to always check that the key exists
                    if (status != PermissionStatus.Granted)
                    {
                        if (status == PermissionStatus.Denied)
                        {
                            await DisplayAlert("Permission Required", "Please enable "+ permissionName + " permission to carry on with this operation.", "OK");
                        }
                    }
                }

                if (status == PermissionStatus.Granted || status == PermissionStatus.Unknown)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
