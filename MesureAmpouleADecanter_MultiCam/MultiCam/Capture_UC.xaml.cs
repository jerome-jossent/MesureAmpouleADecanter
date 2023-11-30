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
            get { return args.mainWindow.ARGS.Keys.ToArray(); }
        }

        public int position
        {
            get => args.position;
            set
            {
                args.mainWindow.SwitchCamera(args.position, value);
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

        public CaptureArguments args;

        public Capture_UC()
        {
            InitializeComponent();
            DataContext = this;
            img_settings.Visibility = Visibility.Hidden;
        }

        public void _Link(CaptureArguments args)
        {
            this.args = args;
            args.layoutAnchorable.Title = args.position.ToString();

            args.layoutAnchorable.Closing += LayoutAnchorable_Closing;

            SetTitleAndInfo();
            GetCameraParameters();
            new Thread(() => Thread_cap(args)).Start();
        }

        private void LayoutAnchorable_Closing(object? sender, CancelEventArgs e)
        {
            _Stop();
        }

        public void SetTitleAndInfo()
        {
            args.layoutAnchorable.Title = "Caméra " + args.position.ToString();
        }

        void GetCameraParameters()
        {
            SetSlider(sld_exposition, CameraControlProperty.Exposure);
            SetSlider(sld_focus, CameraControlProperty.Focus);
        }

        void SetSlider(Slider sld, CameraControlProperty cameraControlProperty)
        {
            CameraSettings.PropertyValues vals = CameraSettings.GetProperty(args.iamCameraControl, cameraControlProperty);
            sld.Minimum = vals.min;
            sld.Maximum = vals.max;
            sld.SmallChange = vals.step;
            sld.Value = vals.deflt;

            sld.ValueChanged += (sender, e) => CameraSettings.SetProperty(args.iamCameraControl, cameraControlProperty, (int)e.NewValue);
        }

        void Thread_cap(CaptureArguments args)
        {
            args.videoCapture = new VideoCapture();

            VideoCapture cap = args.videoCapture;

            cap.Open(args.index, VideoCaptureAPIs.DSHOW);
            if (!cap.IsOpened())
            {
                return;
            }

            int fps = 30;

            cap.Set(VideoCaptureProperties.FourCC, FourCC.FromString("MJPG"));
            cap.Set(VideoCaptureProperties.FrameWidth, 1920);
            cap.Set(VideoCaptureProperties.FrameHeight, 1080);
            cap.Set(VideoCaptureProperties.Fps, fps);
            cap.Set(VideoCaptureProperties.FourCC, FourCC.FromString("MJPG"));

            while (!args.cts.IsCancellationRequested)
            {
                if (cap.IsDisposed) break;
                args.frameMat = cap.RetrieveMat();

                if (args.frameMat.Empty())
                {
                    //attente d'1 frame
                    Thread.Sleep(1000 / fps);
                    continue;
                }

                Mat frameMat = args.frameMat;
                Mat m = frameMat.Clone();

                //GC.Collect();

                //To save
                DateTime t = DateTime.Now;
                if (args.t < t && m != null)
                {
                    args.images_to_save.Add(args.mainWindow.f + args.position.ToString() + DateTime.Now.ToString("hh_mm_ss.fff") + ".jpg", m);
                    args.t = t + (TimeSpan)args.mainWindow._timeBetweenFrameToSave;
                }

                //ligne au centre
                if (args.mainWindow.epaisseur > 0)
                    Cv2.Line(frameMat, 0, frameMat.Rows / 2, frameMat.Cols, frameMat.Rows / 2, args.mainWindow.rouge, args.mainWindow.epaisseur);

                //ROI
                if (args.roi.Width > 0 && args.roi.Height > 0)
                {
                    Mat frame_roi = new Mat(frameMat, args.roi);
                    frame_roi.CopyTo(frameMat);
                }

                Display(frameMat);
            }
            cap.Dispose();
        }
        public void _Stop()
        {
            args.cts.Cancel();
            Thread.Sleep(100);
            args.videoCapture?.Dispose();

            args.mainWindow.ARGS.Remove(args.index);
            GC.Collect();
        }

        void Display(Mat frameMat)
        {
            if (frameMat.Empty()) return;
            Dispatcher.Invoke(() => { args.Update(frameMat.ToWriteableBitmap()); });
        }

        void Settings_Click(object sender, MouseButtonEventArgs e)
        {
            settings_gridcolumnwidth = (settings_gridcolumnwidth == GridLength.Auto) ? new GridLength(0, GridUnitType.Pixel) : GridLength.Auto;
        }

        void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            img_settings.Visibility = Visibility.Visible;
        }

        void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            img_settings.Visibility = Visibility.Hidden;
        }

        void CameraSettings_Click(object sender, RoutedEventArgs e)
        {
            CameraSettings.ShowSettingsUI(args.ds_device);
        }

        void Set_ROI_Click(object sender, RoutedEventArgs e)
        {
            string name = "Set ROI : Space to validate, Escape to cancel";
            Mat cap = args.videoCapture.RetrieveMat();

            if (args.mainWindow.epaisseur > 0)
                Cv2.Line(cap, 0, cap.Rows / 2, cap.Cols, cap.Rows / 2, args.mainWindow.rouge, args.mainWindow.epaisseur);

            args.roi = Cv2.SelectROI(name, cap);
            Cv2.DestroyWindow(name);
        }

    }
}
