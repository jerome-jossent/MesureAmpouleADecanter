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

        public enum Position { Haut, Milieu, Bas}
        public Position position;

        public Capture_UC capture_UC;
        public CancellationTokenSource cts;

        public Dictionary<string, Mat> images;
        public MainWindow mainWindow;

        public CaptureArguments(VideoCapture videoCapture, int index, Position position, Capture_UC capture_UC,
            CancellationTokenSource cts, System.Collections.Generic.Dictionary<string, Mat> images, MainWindow mainWindow)
        {
            t = DateTime.Now;
            this.videoCapture = videoCapture;
            this.index = index;
            this.position = position;
            this.capture_UC = capture_UC;
            this.cts = cts;
            this.images = images;
            this.mainWindow = mainWindow;
        }

        internal void Update(WriteableBitmap writeableBitmap)
        {
            capture_UC.FrameImage = writeableBitmap;
        }
    }
}