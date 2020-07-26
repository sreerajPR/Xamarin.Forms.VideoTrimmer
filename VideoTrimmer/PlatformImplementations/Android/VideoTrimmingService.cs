﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Media;
using Java.Lang;
using Java.Nio;
using VideoTrimmer.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(VideoTrimmer.PlatformImplementations.Android.VideoTrimmingService))]
namespace VideoTrimmer.PlatformImplementations.Android
{
    internal class VideoTrimmingService: IVideoTrimmingService
    {
        public Task<bool> Trim(int startMS, int lengthMS, string inputPath, string outputPath)
        {
            return Task.Run<bool>(() =>
            {
                try
                {
                    bool didOperationSucceed = false;
                    MediaExtractor extractor = new MediaExtractor();
                    extractor.SetDataSource(inputPath);
                    int trackCount = extractor.TrackCount;
                    // Set up MediaMuxer for the destination.
                    MediaMuxer muxer;
                    muxer = new MediaMuxer(outputPath, MuxerOutputType.Mpeg4);
                    // Set up the tracks and retrieve the max buffer size for selected
                    // tracks.
                    Dictionary<int, int> indexDict = new Dictionary<int, int>(trackCount);
                    int bufferSize = -1;
                    for (int i = 0; i < trackCount; i++)
                    {
                        MediaFormat format = extractor.GetTrackFormat(i);
                        string mime = format.GetString(MediaFormat.KeyMime);
                        bool selectCurrentTrack = false;
                        if (mime.StartsWith("audio/"))
                        {
                            selectCurrentTrack = true;
                        }
                        else if (mime.StartsWith("video/"))
                        {
                            selectCurrentTrack = true;
                        }
                        if (selectCurrentTrack)
                        {
                            extractor.SelectTrack(i);
                            int dstIndex = muxer.AddTrack(format);
                            indexDict.Add(i, dstIndex);
                            if (format.ContainsKey(MediaFormat.KeyMaxInputSize))
                            {
                                int newSize = format.GetInteger(MediaFormat.KeyMaxInputSize);
                                bufferSize = newSize > bufferSize ? newSize : bufferSize;
                            }
                        }
                    }
                    if (bufferSize < 0)
                    {
                        bufferSize = 1337; //TODO: I don't know what to put here tbh, it will most likely be above 0 at this point anyways :)
                    }
                    // Set up the orientation and starting time for extractor.
                    MediaMetadataRetriever retrieverSrc = new MediaMetadataRetriever();
                    retrieverSrc.SetDataSource(inputPath);
                    string degreesString = retrieverSrc.ExtractMetadata(MetadataKey.VideoRotation);
                    if (degreesString != null)
                    {
                        int degrees = int.Parse(degreesString);
                        if (degrees >= 0)
                        {
                            muxer.SetOrientationHint(degrees);
                        }
                    }
                    if (startMS > 0)
                    {
                        extractor.SeekTo(startMS * 1000, MediaExtractorSeekTo.ClosestSync);
                    }
                    // Copy the samples from MediaExtractor to MediaMuxer. We will loop
                    // for copying each sample and stop when we get to the end of the source
                    // file or exceed the end time of the trimming.
                    int offset = 0;
                    int trackIndex = -1;
                    ByteBuffer dstBuf = ByteBuffer.Allocate(bufferSize);
                    MediaCodec.BufferInfo bufferInfo = new MediaCodec.BufferInfo();
                    try
                    {
                        muxer.Start();
                        while (true)
                        {
                            bufferInfo.Offset = offset;
                            bufferInfo.Size = extractor.ReadSampleData(dstBuf, offset);
                            if (bufferInfo.Size < 0)
                            {
                                bufferInfo.Size = 0;
                                break;
                            }
                            else
                            {
                                bufferInfo.PresentationTimeUs = extractor.SampleTime;
                                if (lengthMS > 0 && bufferInfo.PresentationTimeUs > (lengthMS * 1000))
                                {
                                    Console.WriteLine("The current sample is over the trim end time.");
                                    break;
                                }
                                else
                                {
                                    bufferInfo.Flags = ConvertMediaExtractorSampleFlagsToMediaCodecBufferFlags(extractor.SampleFlags);
                                    trackIndex = extractor.SampleTrackIndex;
                                    muxer.WriteSampleData(indexDict[trackIndex], dstBuf, bufferInfo);
                                    extractor.Advance();
                                }
                            }
                        }
                        muxer.Stop();
                        didOperationSucceed = true;
                        //deleting the old file
                        //JFile file = new JFile(srcPath);
                        //file.Delete();
                    }
                    catch (IllegalStateException e)
                    {
                        // Swallow the exception due to malformed source.
                        Console.WriteLine("The source video file is malformed");
                    }
                    finally
                    {
                        muxer.Release();
                    }
                    return didOperationSucceed;
                }
                catch
                {
                    return false;
                }
                
            });
            // Set up MediaExtractor to read from the source.

        }

        private MediaCodecBufferFlags ConvertMediaExtractorSampleFlagsToMediaCodecBufferFlags(MediaExtractorSampleFlags mediaExtractorSampleFlag)
        {
            switch (mediaExtractorSampleFlag)
            {
                case MediaExtractorSampleFlags.None:
                    return MediaCodecBufferFlags.None;
                case MediaExtractorSampleFlags.Encrypted:
                    return MediaCodecBufferFlags.KeyFrame;
                case MediaExtractorSampleFlags.Sync:
                    return MediaCodecBufferFlags.SyncFrame;
                default:
                    throw new NotImplementedException("ConvertMediaExtractorSampleFlagsToMediaCodecBufferFlags");
            }
        }

    }
}