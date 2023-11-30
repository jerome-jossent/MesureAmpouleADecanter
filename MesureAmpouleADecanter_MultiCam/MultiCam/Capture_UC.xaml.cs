using OpenCvSharp.WpfExtensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using DirectShowLib;
using Xceed.Wpf.AvalonDock.Layout;
using System.Xml.Linq;

namespace MultiCam
{
    public partial class Capture_UC : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public IEnumerable<int> positionEnum
        {
            get { return Enumerable.Range(0, 10); }
            //get { return args.mainWindow.ARGS.Keys.ToArray(); }
        }

        public int position
        {
            get => captureParameters.ac_data.position;
            set
            {
                captureParameters.MainWindow().SwitchPositions(captureParameters.ac_data.position, value);
            }
        }

        public ImageSource FrameImage
        {
            get { return frameImage; }
            set
            {
                frameImage = value;
                OnPropertyChanged();
            }
        }
        ImageSource frameImage;


        public GridLength settings_gridcolumnwidth
        {
            get => _settings_gridcolumnwidth;
            set
            {
                _settings_gridcolumnwidth = value;
                OnPropertyChanged();
            }
        }
        GridLength _settings_gridcolumnwidth;

        public CaptureArguments captureParameters;

        public Capture_UC()
        {
            InitializeComponent();
            DataContext = this;
            img_settings.Visibility = Visibility.Hidden;
        }

        public void _Link(CaptureArguments captureParameters)
        {
            this.captureParameters = captureParameters;
            captureParameters.layoutAnchorable.Title = captureParameters.ac_data.position.ToString();

            captureParameters.layoutAnchorable.Closing += LayoutAnchorable_Closing;

            SetTitle();
            GetCameraParameters();
            new Thread(() => Thread_cap(captureParameters)).Start();
        }

        private void LayoutAnchorable_Closing(object? sender, CancelEventArgs e)
        {
            _Stop();
        }

        public void SetTitle()
        {
            captureParameters.layoutAnchorable.Title = captureParameters.name + " - position " + captureParameters.ac_data.position.ToString();
        }

        void GetCameraParameters()
        {
            SetSlider(sld_exposition, CameraControlProperty.Exposure);
            SetSlider(sld_focus, CameraControlProperty.Focus);
        }

        void SetSlider(Slider sld, CameraControlProperty cameraControlProperty)
        {
            CameraSettings.PropertyValues vals = CameraSettings.GetProperty(captureParameters.iamCameraControl, cameraControlProperty);
            sld.Minimum = vals.min;
            sld.Maximum = vals.max;
            sld.SmallChange = vals.step;
            sld.Value = vals.deflt;

            sld.ValueChanged += (sender, e) => CameraSettings.SetProperty(captureParameters.iamCameraControl, cameraControlProperty, (int)e.NewValue);
        }

        void Thread_cap(CaptureArguments ca)
        {
            ca.videoCapture = new VideoCapture();

            VideoCapture cap = ca.videoCapture;

            int tentative = 0;
            while (tentative < 10)
            {
                tentative++;

                if (cap == null)
                    return;

                if (cap.IsOpened())
                    break;

                cap.Open(ca.ac_data.deviceIndex, VideoCaptureAPIs.DSHOW);
                Thread.Sleep(100);
            }

            if (!cap.IsOpened())
            {
                MessageBox.Show("Impossible to open this device :\n" + ca.name,
                    "Open Device Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _Stop();
                return;
            }

            int fps = 30;
            cap.Set(VideoCaptureProperties.FourCC, FourCC.FromString("MJPG"));
            cap.Set(VideoCaptureProperties.FrameWidth, 1920);
            cap.Set(VideoCaptureProperties.FrameHeight, 1080);
            cap.Set(VideoCaptureProperties.Fps, fps);
            cap.Set(VideoCaptureProperties.FourCC, FourCC.FromString("MJPG"));


            while (!ca.cts.IsCancellationRequested)
            {
                if (cap.IsDisposed) break;
                ca.frameMat = cap.RetrieveMat();

                if (ca.frameMat.Empty())
                {
                    //attente d'1 frame
                    Thread.Sleep(1000 / fps);
                    continue;
                }

                Mat m = ca.frameMat.Clone();

                //To save
                DateTime t = DateTime.Now;
                if (ca.t < t)
                {
                    ca.images_to_save.Add(ca.MainWindow().f + ca.ac_data.position.ToString() + DateTime.Now.ToString("hh_mm_ss.fff") + ".jpg", m);
                    ca.t = t + (TimeSpan)ca.MainWindow()._timeBetweenFrameToSave;
                }

                //ligne au centre
                if (ca.MainWindow().epaisseur > 0)
                    Cv2.Line(ca.frameMat, 0, ca.frameMat.Rows / 2, ca.frameMat.Cols, ca.frameMat.Rows / 2, ca.MainWindow().rouge, ca.MainWindow().epaisseur);

                //ROI
                if (ca.ac_data.roi.Width > 0 && ca.ac_data.roi.Height > 0)
                {
                    Mat frame_roi = new Mat(ca.frameMat, ca.ac_data.roi);
                    frame_roi.CopyTo(ca.frameMat);
                }


                

                Display(frameMat);
            }
            cap.Dispose();
        }


        List<string> GetAvailableCodecs(VideoCapture cap)
        {
            List<string> codecs_to_test = new List<string>() { "DIVX", "XVID", "MJPG", "X264", "WMV1", "WMV2", "FMP4", "mp4v", "avc1", "I420", "IYUV", "mpg1", "H264" };
            List<string> available_codecs = new List<string>();
            foreach (string codec in codecs_to_test)
                if (TestFourCC(cap, codec)) available_codecs.Add(codec);
            return available_codecs;
        }

        bool TestFourCC(VideoCapture cap, string codec)
        {
            try
            {
                cap.Set(VideoCaptureProperties.FourCC, FourCC.FromString(codec));
                Mat mat = new Mat();
                int nbr_test = 10;
                while (nbr_test-- > 0)
                {
                    mat = cap.RetrieveMat();
                    Thread.Sleep(100);
                }
                return !mat.Empty();
            }
            catch (Exception)
            {

                return false;
            }
        }

        public void _Stop()
        {
            captureParameters.cts.Cancel();
            Thread.Sleep(100);
            captureParameters.videoCapture?.Dispose();

            captureParameters.MainWindow().capturesParameters.Remove(captureParameters.ac_data.deviceIndex);
            GC.Collect();

            //captureParameters.SetMenuItem(true);
        }

        void Display(Mat frameMat)
        {
            if (frameMat.Empty()) return;
            Dispatcher.Invoke(() => { captureParameters.Update(frameMat.ToWriteableBitmap()); });
        }

        void Settings_Click(object sender, MouseButtonEventArgs e)
        {
            settings_gridcolumnwidth = (settings_gridcolumnwidth == GridLength.Auto) ? new GridLength(0, GridUnitType.Pixel) : GridLength.Auto;
        }

        void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            //img_settings.Visibility = Visibility.Visible;
        }

        void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //img_settings.Visibility = Visibility.Hidden;
        }

        void CameraSettings_Click(object sender, RoutedEventArgs e)
        {
            CameraSettings.ShowSettingsUI(captureParameters.ds_device);
        }

        void Set_ROI_Click(object sender, RoutedEventArgs e)
        {
            string name = "Set ROI : 'Space' to validate, 'Escape' to cancel";
            Mat cap = captureParameters.videoCapture.RetrieveMat();

            if (captureParameters.MainWindow().epaisseur > 0)
                Cv2.Line(cap, 0, cap.Rows / 2, cap.Cols, cap.Rows / 2, captureParameters.MainWindow().rouge, captureParameters.MainWindow().epaisseur);

            captureParameters.ac_data.roi = Cv2.SelectROI(name, cap);
            Cv2.DestroyWindow(name);
        }

    }
}
