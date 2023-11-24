using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media.Imaging;
using Xceed.Wpf.AvalonDock.Layout;

namespace MultiCam
{
    public class CaptureArguments
    {
        public DateTime t;

        public LayoutAnchorable layoutAnchorable;
        public VideoCapture videoCapture;
        public int index;

        public int position;

        public Capture_UC capture_UC;
        public CancellationTokenSource cts;

        public Dictionary<string, Mat> images_to_save;
        public MainWindow mainWindow;
        public DirectShowLib.DsDevice ds_device;
        public DirectShowLib.IAMCameraControl iamCameraControl;

        public Mat frameMat;
        public Rect roi;

        public CaptureArguments(
            LayoutAnchorable layoutAnchorable,
            VideoCapture videoCapture, 
            int index, 
            DirectShowLib.DsDevice ds_device, 
            int position, 
            Capture_UC capture_UC,
            CancellationTokenSource cts,
            Dictionary<string, Mat> images_to_save, MainWindow mainWindow)
        {
            t = DateTime.Now;
            this.layoutAnchorable = layoutAnchorable;
            this.videoCapture = videoCapture;
            this.index = index;
            this.position = position;
            this.capture_UC = capture_UC;
            this.cts = cts;
            this.images_to_save = images_to_save;
            this.mainWindow = mainWindow;
            this.ds_device = ds_device;

            iamCameraControl = CameraSettings.DsDevise_to_IAMCameraControl(ds_device);
        }

        internal void Update(WriteableBitmap writeableBitmap)
        {
            capture_UC.FrameImage = writeableBitmap;
        }
    }
}