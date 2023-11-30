using DirectShowLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;

namespace MultiCam
{
    

    public class CaptureArguments
    {
        public CaptureArguments_data ac_data;

        public DateTime t;

        public MenuItem menuItem;
        public LayoutAnchorable layoutAnchorable;
        public VideoCapture videoCapture;
        public Capture_UC capture_UC;
        public Dictionary<string, Mat> images_to_save;

        MainWindow mainWindow;

        public DsDevice ds_device;
        public IAMCameraControl iamCameraControl;

        public Mat frameMat;
        public CancellationTokenSource cts;
        public string name;

        public CaptureArguments() { }

        public CaptureArguments(
            int deviceIndex,
            DsDevice ds_device,
            int position,
            CaptureArguments_data ac_data = null)
        {
            t = DateTime.Now;

            if (ac_data == null)
                this.ac_data = new CaptureArguments_data();
            else
                this.ac_data = ac_data;

            ac_data.deviceIndex = deviceIndex;
            ac_data.position = position;

            this.ds_device = ds_device;

            cts = new CancellationTokenSource();
            iamCameraControl = CameraSettings.DsDevise_to_IAMCameraControl(ds_device);
            name = $"{ds_device.Name} ({deviceIndex})";
        }

        internal void Update(WriteableBitmap writeableBitmap)
        {
            capture_UC.FrameImage = writeableBitmap;
        }

        internal void _LinkWithIHM(LayoutAnchorable layoutAnchorable, Capture_UC capture_UC, Dictionary<string, Mat> images_to_save, MainWindow mainWindow, MenuItem menuItem)
        {
            this.menuItem = menuItem;
            this.layoutAnchorable = layoutAnchorable;
            this.capture_UC = capture_UC;
            this.images_to_save = images_to_save;
            this.mainWindow = mainWindow;
        }

        internal MainWindow MainWindow()
        {
            return mainWindow;
        }
    }
}