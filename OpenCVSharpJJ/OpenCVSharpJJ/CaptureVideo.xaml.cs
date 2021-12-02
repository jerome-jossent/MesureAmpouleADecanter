using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OpenCVSharpJJ
{
    /// <summary>
    /// Logique d'interaction pour CaptureVideo.xaml
    /// </summary>
    public partial class CaptureVideo : System.Windows.Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        Mat frame = new Mat();
        Mat[] bgr;

        Thread thread;
        int indexDevice;
        VideoInInfo.Format format;
        Dictionary<string, VideoInInfo.Format> formats;
        VideoCapture capture;
        bool isRunning = false;
        bool first = true;
        System.Diagnostics.Stopwatch chrono = new System.Diagnostics.Stopwatch();

        OpenCvSharp.Window w;
        bool display_in_OpenCVSharpWindow = false;

        public System.Drawing.Bitmap _bitmap
        {
            get
            {
                if (bitmap == null)
                    return null;
                return bitmap;
            }
            set
            {
                if (bitmap != value)
                {
                    bitmap = value;
                    OnPropertyChanged("_bitmap");
                }
            }
        }
        System.Drawing.Bitmap bitmap = new Bitmap(200, 200);

        public CaptureVideo()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Camera_Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            isRunning = false;
            CaptureCameraStop();
        }


        #region CAPTURE DEVICE
        private void Camera_Init()
        {
            ListDevices();
            cbx_device.SelectedIndex = 1;
            cbx_deviceFormat.SelectedIndex = 0;
        }

        private void Button_ListDevices_Click(object sender, MouseButtonEventArgs e)
        {
            ListDevices();
        }

        private void ListDevices()
        {
            var devices = VideoInInfo.EnumerateVideoDevices_JJ();
            if (cbx_device != null)
                cbx_device.ItemsSource = devices.Select(d => d.Name).ToList();
        }

        private void Button_CaptureDevice_Click(object sender, MouseButtonEventArgs e)
        {
            Capture_Start();
        }

        void Capture_Start()
        {
            chrono.Start();
            isRunning = !isRunning;

            if (isRunning)
            {
                indexDevice = cbx_device.SelectedIndex;
                CaptureCamera(indexDevice);
                Button_CaptureDevicePlay.Visibility = Visibility.Collapsed;
                Button_CaptureDeviceStop.Visibility = Visibility.Visible;
            }
            else
            {
                CaptureCameraStop();
                Button_CaptureDevicePlay.Visibility = Visibility.Visible;
                Button_CaptureDeviceStop.Visibility = Visibility.Collapsed;
            }
        }

        private void Combobox_CaptureDevice_Change(object sender, SelectionChangedEventArgs e)
        {
            indexDevice = cbx_device.SelectedIndex;
            formats = VideoInInfo.EnumerateSupportedFormats_JJ(indexDevice);
            cbx_deviceFormat.ItemsSource = formats.OrderBy(f => f.Value.format).ThenByDescending(f => f.Value.w).Select(f => f.Key);
        }

        private void Combobox_CaptureDeviceFormat_Change(object sender, SelectionChangedEventArgs e)
        {
            format = formats[cbx_deviceFormat.SelectedValue as string];
        }

        #endregion

        #region CAMERA MANAGEMENT
        void CaptureCamera(int index)
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
                Thread.Sleep(100);
            }
            indexDevice = index;
            thread = new Thread(new ThreadStart(CaptureCameraCallback));
            thread.Start();
        }

        void CaptureCameraStop()
        {
            thread?.Abort();
            first = true;
            thread = null;
        }

        void CaptureCameraCallback()
        {
            int actualindexDevice = indexDevice;
            frame = new Mat();
            capture = new VideoCapture(indexDevice);
            capture.Open(indexDevice, VideoCaptureAPIs.DSHOW);

            if (capture.IsOpened())
            {
                while (isRunning)
                {
                    //Si changement de camera
                    if (indexDevice != actualindexDevice)
                    {
                        capture.Open(indexDevice, VideoCaptureAPIs.DSHOW);
                        actualindexDevice = indexDevice;
                    }

                    //si changement de format vidéo
                    if (format != null)
                    {
                        capture.Set(VideoCaptureProperties.FrameWidth, format.w);
                        capture.Set(VideoCaptureProperties.FrameHeight, format.h);
                        capture.Set(VideoCaptureProperties.Fps, format.fr);
                        capture.Set(VideoCaptureProperties.FourCC, OpenCvSharp.FourCC.FromString(format.format));
                        format = null;
                    }

                    if (first)
                    {
                        Display_Init();
                        first = false;
                    }

                    //captation de l'image
                    capture.Read(frame);

                    //Viewer debug
                    if (display_in_OpenCVSharpWindow)
                    {
                        if (frame.Empty())
                            Cv2.DestroyWindow(w.Name);
                        else
                        {
                            if (w == null)
                                w = new OpenCvSharp.Window();

                            w.ShowImage(frame);
                            Cv2.WaitKey(1);
                        }
                    }

                    if (!frame.Empty())
                    {
                        Application.Current.Dispatcher.BeginInvoke(
                            DispatcherPriority.Background,
                            new Action(() =>
                            {
                                _bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                            }));
                    }
                }
            }
            #endregion

            void Display_Init()
            {
                bgr = new Mat[] { new Mat(frame.Size(), MatType.CV_8UC1),
                                  new Mat(frame.Size(), MatType.CV_8UC1),
                                  new Mat(frame.Size(), MatType.CV_8UC1)};
            }

        }
    }
}
