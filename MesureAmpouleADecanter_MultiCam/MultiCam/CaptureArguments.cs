using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media.Imaging;

namespace MultiCam
{
    public class CaptureArguments
    {
        public int index;
        public VideoCapture videoCapture;
        public DateTime t;

        public enum Position { Haut, Milieu, Bas }
        public Position position;

        public Capture_UC capture_UC;
        public CancellationTokenSource cts;

        public Dictionary<string, Mat> images_to_save;
        public MainWindow mainWindow;
        public DirectShowLib.DsDevice ds_device;

        public CaptureArguments(
            VideoCapture videoCapture, 
            int index, 
            DirectShowLib.DsDevice ds_device, 
            Position position, 
            Capture_UC capture_UC,
            CancellationTokenSource cts,
            Dictionary<string, Mat> images_to_save, MainWindow mainWindow)
        {
            t = DateTime.Now;
            this.videoCapture = videoCapture;
            this.index = index;
            this.position = position;
            this.capture_UC = capture_UC;
            this.cts = cts;
            this.images_to_save = images_to_save;
            this.mainWindow = mainWindow;
            this.ds_device = ds_device;
        }

        internal void Update(WriteableBitmap writeableBitmap)
        {
            capture_UC.FrameImage = writeableBitmap;
        }
    }
}