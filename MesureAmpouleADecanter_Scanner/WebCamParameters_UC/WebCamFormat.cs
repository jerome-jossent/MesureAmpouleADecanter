using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebCamParameters_UC
{
    public class WebCamFormat
    {
        public class Format
        {
            public int w { get; set; }
            public int h { get; set; }
            public int fr { get; set; }
            public string format { get; set; }

            [Newtonsoft.Json.JsonIgnore]
            [JsonIgnore]
            public string Name { get => "(" + format + ") " + w + "*" + h + " [" + fr + "fps]"; }

            [Newtonsoft.Json.JsonIgnore]
            [JsonIgnore]
            public int _pixels_par_sec { get => w * h * fr; }

            public Format() { }

            public Format(VideoInfoHeader? videoInfo)
            {
                w = videoInfo.BmiHeader.Width;
                h = videoInfo.BmiHeader.Height;
                fr = (int)(10000000 / videoInfo.AvgTimePerFrame);
                format = GetVideoFormat(videoInfo.BmiHeader.Compression);
            }

            public Format(int width, int height, int frameRate, string format)
            {
                w = width;
                h = height;
                fr = frameRate;
                this.format = format;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public static List<IBaseFilter> Get_Webcams()
        {
            List<IBaseFilter> webcams = new List<IBaseFilter>();
            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            foreach (var device in devices)
            {
                var guid = typeof(IBaseFilter).GUID;
                device.Mon.BindToObject(null, null, ref guid, out object filter);
                if (filter is IBaseFilter baseFilter)
                    webcams.Add(baseFilter);
            }
            return webcams;
        }

        public static List<Format> Get_Formats(int deviceIndex)
        {
            return Get_Formats(GetIBaseFilter(deviceIndex));
        }
        public static List<Format> Get_Formats(DsDevice webcam)
        {
            return Get_Formats(GetIBaseFilter(webcam));
        }
        public static List<Format> Get_Formats(IBaseFilter webcamFilter)
        {
            var formats = new List<Format>();
            var pinEnum = webcamFilter.EnumPins(out IEnumPins enumPins);
            if (pinEnum != 0 || enumPins == null)
                return formats;

            var pins = new IPin[1];
            while (enumPins.Next(1, pins, IntPtr.Zero) == 0)
            {
                var pin = pins[0];
                pin.QueryPinInfo(out PinInfo pinInfo);
                if (pinInfo.dir == PinDirection.Output)
                {
                    if (pin is IAMStreamConfig streamConfig)
                    {
                        streamConfig.GetNumberOfCapabilities(out int count, out int size);
                        var caps = new VideoStreamConfigCaps();
                        for (int i = 0; i < count; i++)
                        {
                            var ptr = Marshal.AllocCoTaskMem(size);
                            streamConfig.GetStreamCaps(i, out AMMediaType mediaType, ptr);

                            if (mediaType.formatType == FormatType.VideoInfo)
                            {
                                VideoInfoHeader? videoInfo = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
                                Format f = new Format(videoInfo);
                                formats.Add(f);
                            }
                            Marshal.FreeCoTaskMem(ptr);
                            DsUtils.FreeAMMediaType(mediaType);
                        }
                    }
                }
            }
            return formats;
        }
        
        public static Format GetFormat_MaxPerf(int deviceIndex)
        {
            return GetFormat_MaxPerf(GetIBaseFilter(deviceIndex));
        }
        public static Format GetFormat_MaxPerf(DsDevice webcam)
        {
            return GetFormat_MaxPerf(GetIBaseFilter(webcam));
        }
        public static Format GetFormat_MaxPerf(IBaseFilter webcamFilter)
        {
            List<Format> formats = Get_Formats(webcamFilter);
            //best score ?
            var f = formats.MaxBy(x => x._pixels_par_sec);
            return f;
        }

        public static IBaseFilter GetIBaseFilter(DsDevice webcam)
        {
            var guid = typeof(IBaseFilter).GUID;
            webcam.Mon.BindToObject(null, null, ref guid, out object filter);
            if (filter is IBaseFilter baseFilter)
                return baseFilter;
            else
                return null;
        }
        public static IBaseFilter GetIBaseFilter(int deviceIndex)
        {
            List<IBaseFilter> webcams = Get_Webcams();
            for (int i = 0; i < webcams.Count; i++)
                if (i == deviceIndex)
                    return (webcams[i]);
            return null;
        }

        public static string GetVideoFormat(int compression)
        {
            // https://docs.microsoft.com/en-us/windows/desktop/medfound/video-subtype-guids
            byte[] format_buffer = BitConverter.GetBytes(compression);
            string fourCC = Encoding.Default.GetString(format_buffer);
            return fourCC;
        }

    }
}
