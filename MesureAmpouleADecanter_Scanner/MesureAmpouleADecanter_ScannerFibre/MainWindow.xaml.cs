using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using MessageBox = System.Windows.MessageBox;
using Microsoft.Win32;

using Xceed.Wpf.Toolkit;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xceed.Wpf.AvalonDock.Layout;

using DirectShowLib;

using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using Rect = OpenCvSharp.Rect;

using MesureAmpouleADecanter_ScannerFibre.Properties;
using static WebCamParameters_UC.WebCamFormat;


namespace MesureAmpouleADecanter_ScannerFibre
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        #region Parameters Binding
        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public ImageSource _image
        {
            get => image;
            set
            {
                image = value;
                OnPropertyChanged();
            }
        }
        ImageSource image;

        public ImageSource _image1
        {
            get => image1;
            set
            {
                image1 = value;
                OnPropertyChanged();
            }
        }
        ImageSource image1;

        public ImageSource _image3
        {
            get => image3;
            set
            {
                image3 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(_scans_height));
            }
        }
        ImageSource image3;


        public string _file
        {
            get => file;
            set
            {
                file = value;
                OnPropertyChanged();
                Settings.Default.file = file;
                Settings.Default.Save();
                //if (MessageBox.Show("Réinitialiser les réglages de crop/roi ?", "Reset Crop ?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                //    ResetROI();
                ProcessFile(_file);
            }
        }
        string file = Settings.Default.file;

        public string _title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }
        string title = "";

        public int _tabindex
        {
            get => tabindex;
            set
            {
                tabindex = value;
                OnPropertyChanged();
            }
        }
        int tabindex;

        public bool _play
        {
            get => play;
            set
            {
                play = value;
                OnPropertyChanged();
            }
        }
        bool play = false;

        public bool _HoughCircle_Detection
        {
            get => HoughCircle_Detection;
            set
            {
                HoughCircle_Detection = value;
                OnPropertyChanged();
            }
        }
        bool HoughCircle_Detection = false;

        public bool _scan_save
        {
            get => scan_save;
            set
            {
                scan_save = value;
                OnPropertyChanged();
            }
        }
        bool scan_save = true;

        public int _videoTotalFrames
        {
            get
            {
                if (capVideo != null)
                    return capVideo.FrameCount;
                return 1;
            }
        }

        public int _videoPositionFrame
        {
            get
            {
                if (capVideo != null)
                    return capVideo.PosFrames;
                return 1;
            }
            set
            {
                if (capVideo != null)
                    newWantedPosFrames = value;
            }
        }
        int? newWantedPosFrames;

        //#region ROI Parameters
        //bool roi_change;
        //public double _roi_left
        //{
        //    get => roi_left;
        //    set
        //    {
        //        roi_left = value;
        //        OnPropertyChanged();
        //        Settings.Default.roi_left = roi_left;
        //        Settings.Default.Save();
        //        roi_change = true;
        //    }
        //}
        //double roi_left = Settings.Default.roi_left;

        //public double _roi_right
        //{
        //    get => roi_right;
        //    set
        //    {
        //        roi_right = value;
        //        OnPropertyChanged();
        //        Settings.Default.roi_right = roi_right;
        //        Settings.Default.Save();
        //        roi_change = true;
        //    }
        //}
        //double roi_right = Settings.Default.roi_right;

        //public double _roi_width_maximum
        //{
        //    get => roi_width_maximum;
        //    set
        //    {
        //        roi_width_maximum = value;
        //        OnPropertyChanged();
        //        roi_change = true;
        //    }
        //}
        //double roi_width_maximum;


        //public double _roi_top
        //{
        //    get => roi_top;
        //    set
        //    {
        //        roi_top = value;
        //        OnPropertyChanged();
        //        Settings.Default.roi_top = roi_top;
        //        Settings.Default.Save();
        //        roi_change = true;
        //    }
        //}
        //double roi_top = Settings.Default.roi_top;

        //public double _roi_bottom
        //{
        //    get => roi_bottom;
        //    set
        //    {
        //        roi_bottom = value;
        //        OnPropertyChanged();
        //        Settings.Default.roi_bottom = roi_bottom;
        //        Settings.Default.Save();
        //        roi_change = true;
        //    }
        //}
        //double roi_bottom = Settings.Default.roi_bottom;

        //public double _roi_height_maximum
        //{
        //    get => roi_height_maximum;
        //    set
        //    {
        //        roi_height_maximum = value;
        //        OnPropertyChanged();
        //        roi_change = true;
        //    }
        //}
        //double roi_height_maximum;
        //#endregion

        #region HoughCircle Parameters
        bool houghCircle_change;
        public double _houghcircle_dp
        {
            get => houghcircle_dp;
            set
            {
                houghcircle_dp = value;
                OnPropertyChanged();
                Settings.Default.houghcircle_dp = houghcircle_dp;
                Settings.Default.Save();
                houghCircle_change = true;
            }
        }
        double houghcircle_dp = Settings.Default.houghcircle_dp;

        public double _houghcircle_param1
        {
            get => houghcircle_param1;
            set
            {
                houghcircle_param1 = value;
                Settings.Default.houghcircle_param1 = houghcircle_param1;
                Settings.Default.Save();
                OnPropertyChanged();
                houghCircle_change = true;
            }
        }
        double houghcircle_param1 = Settings.Default.houghcircle_param1;

        public double _houghcircle_param2
        {
            get => houghcircle_param2;
            set
            {
                houghcircle_param2 = value;
                Settings.Default.houghcircle_param2 = houghcircle_param2;
                Settings.Default.Save();
                OnPropertyChanged();
                houghCircle_change = true;
            }
        }
        double houghcircle_param2 = Settings.Default.houghcircle_param2;


        public int _houghcircle_radius_min
        {
            get => houghcircle_radius_min;
            set
            {
                houghcircle_radius_min = value;
                Settings.Default.houghcircle_radius_min = houghcircle_radius_min;
                Settings.Default.Save();
                OnPropertyChanged();
                houghCircle_change = true;
            }
        }
        int houghcircle_radius_min = Settings.Default.houghcircle_radius_min;

        public int _houghcircle_radius_max
        {
            get => houghcircle_radius_max;
            set
            {
                houghcircle_radius_max = value;
                Settings.Default.houghcircle_radius_max = houghcircle_radius_max;
                Settings.Default.Save();
                OnPropertyChanged();
                houghCircle_change = true;
            }
        }
        int houghcircle_radius_max = Settings.Default.houghcircle_radius_max;
        #endregion

        public int _rayon
        {
            get => rayon;
            set
            {
                rayon = value;
                Settings.Default.rayon = rayon;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        int rayon = Settings.Default.rayon;
        #endregion

        public Config config = new Config();

        public Format _webcam_format
        {
            get => config.webcam_format; set
            {
                if (value == null)
                    value = config.webcam_format;
                else
                    config.webcam_format = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Scan_UC> _scans
        {
            get => scans; set
            {
                scans = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<Scan_UC> scans = new ObservableCollection<Scan_UC>();

        public int _scans_height { get => _sensors.Count * 3; }

        public bool _scan_focus_last
        {
            get => scan_focus_last; set
            {
                scan_focus_last = value;
                OnPropertyChanged();
            }
        }
        bool scan_focus_last = true;

        public bool _sensors_dsiplay
        {
            get => sensors_display; set
            {
                sensors_display = value;
                OnPropertyChanged();
            }
        }
        bool sensors_display = true;

        public bool _store_scans
        {
            get => store_scans; set
            {
                store_scans = value;
                OnPropertyChanged();
            }
        }
        bool store_scans = true;

        public ObservableCollection<Format> _webcam_formats
        {
            get => webcam_formats; set
            {
                webcam_formats = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<Format> webcam_formats;

        public ObservableCollection<ROI_UC> _rois
        {
            get => rois; set
            {
                rois = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<ROI_UC> rois = new ObservableCollection<ROI_UC>();

        public ObservableCollection<Sensor> _sensors
        {
            get => sensors; set
            {
                sensors = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<Sensor> sensors = new ObservableCollection<Sensor>();

        public ObservableCollection<Sensor_UC> _sensors_uc
        {
            get => sensors_uc; set
            {
                sensors_uc = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<Sensor_UC> sensors_uc = new ObservableCollection<Sensor_UC>();

        Sensor nearestSensor;

        Scan_UC scan_mat_uc;

        #region Parameters local

        List<DsDevice> webcams;
        int webcam_index;
        DsDevice webcam;
        WebCamParameters_UC.WebCamConfig webCam_Config;

        Mat frame;
        Mat ROI_mat;
        Mat cercles_mat = new Mat();
        Mat scan_mat;
        int scan_mat_width = 20;
        int scan_mat_column = 0;
        DateTime? scan_mat_T0 = null;
        DateTime? experience_T0 = DateTime.Now;

        List<Cercle> cercles;
        bool circle_Recompute;
        VideoCapture capVideo;
        int sleepTime;
        Thread videoThread;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Scalar CirclesReset_color = Scalar.Black;

        SensorMap sensorMap;
        bool sensors_to_sorted = false;
        #endregion

        #region Window
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            //WebCamParameters_UC._Manager.ShowDialog();
            //WebCamParameters_UC._Manager.Set_WebCamConfig(@"D:\DATA\decantation\c922 Pro Stream Webcam.wcc");
            //WebCamParameters_UC._Manager.Set_WebCamConfig(@"D:\DATA\decantation\c922 Pro Stream Webcam_default.wcc");
            //WebCamParameters_UC._Manager.Set_WebCamParameter("c922 Pro Stream Webcam", VideoProcAmpProperty.WhiteBalance, 1, false);

        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            INITS();
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            StopAll();
        }


        #endregion

        void INITS()
        {
            CameraListRefresh_Click(null, null);

            cercles = new List<Cercle>();

            //load cam, best format
            _cbx_webcam.SelectedIndex = 0;
        }

        #region Image & Mat Operations
        Mat ToGray(Mat src)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        //Mat ROI(Mat src)
        //{
        //    int left = (int)_roi_left;// 20;
        //    int right = (int)(_roi_width_maximum - _roi_right);//150;
        //    int top = (int)(_roi_height_maximum - _roi_top);//1720;
        //    int bottom = (int)(_roi_bottom);//100;

        //    //Dispatcher.BeginInvoke(() =>
        //    //{
        //    //    //Title = left + " ↔ " + right + "\t" + top + " ↨ " + bottom;
        //    //    //Title = _houghcircle_dp.ToString("0.00") + "\t" + _houghcircle_param1.ToString("0.00") + "\t" + _houghcircle_param2.ToString("0.00");
        //    //});

        //    return ROI(src, left, top, right, bottom);
        //}

        Mat ROI(Mat src, int left, int top, int right, int bottom)
        {
            int w = src.Width - left - right;
            int h = src.Height - top - bottom;
            OpenCvSharp.Rect roi_rect = new OpenCvSharp.Rect(left, top, w, h);

            Mat roi_frame = new Mat(src, roi_rect);
            return roi_frame;
        }

        Mat Resize(Mat src, float zoom)
        {
            Mat res = new Mat();
            int nh = (int)(src.Height * zoom);
            int nw = (int)(src.Width * zoom);
            Cv2.Resize(src, res, new OpenCvSharp.Size(nh, nw), interpolation: InterpolationFlags.Cubic);
            return res;
        }

        BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        #endregion

        void StopAll()
        {
            cancellationTokenSource.Cancel();
        }

        //void ResetROI()
        //{
        //    _roi_left = 0;
        //    _roi_top = 0;
        //    _roi_right = 0;
        //    _roi_bottom = 0;
        //}

        void ProcessFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return;

            VideoStop();

            capVideo = new OpenCvSharp.VideoCapture(filePath);
            sleepTime = (int)Math.Round(1000 / capVideo.Fps);
            OnPropertyChanged(nameof(_videoTotalFrames));

            CancellationToken token = cancellationTokenSource.Token;
            //Task.Factory.StartNew(() => PlayVideo(token), token);
        }

        void VideoStop()
        {
            if (_play)
            {
                _play = false;
                Thread.Sleep(1000);
            }
            capVideo?.Dispose();
        }

        //void CirclesReset()
        //{
        //    int left = (int)_roi_left;// 20;
        //    int right = (int)(_roi_width_maximum - _roi_right);//150;
        //    int top = (int)(_roi_height_maximum - _roi_top);//1720;
        //    int bottom = (int)(_roi_bottom);//100;

        //    int w = capVideo.FrameWidth - left - right;
        //    int h = capVideo.FrameHeight - top - bottom;

        //    cercles = new List<Cercle>();
        //    _sensors = new ObservableCollection<Sensor>();
        //    _sensors_uc = new ObservableCollection<Sensor_UC>();
        //}


        void Camera(int webcam_index)
        {
            config.webcam_name = webcam.Name;
            //config.webcam_format = _webcam_format;

            VideoStop();
            capVideo = new OpenCvSharp.VideoCapture();
            capVideo.Open(webcam_index, VideoCaptureAPIs.DSHOW);

            CancellationToken token_cameralive = cancellationTokenSource.Token;
            Task.Factory.StartNew(() => ThreadVideoLive(token_cameralive), token_cameralive);
        }

        void Show(Mat mat)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _image = mat.ToWriteableBitmap();
                }
                catch (Exception ex) { }
            }));
        }

        void ThreadVideoLive(CancellationToken cancellationToken)
        {
            //bool loop = true;
            frame = new Mat();
            //CirclesReset();
            int nframe = 0;

            //while (!capVideo.IsDisposed && loop)
            //{
            //CirclesReset();
            while (!capVideo.IsDisposed &&
                !cancellationToken.IsCancellationRequested)
            {
                //read next frame
                capVideo.Read(frame);

                _title = nframe++.ToString();

                //si echec alors on saute la frame
                if (frame.Empty())
                {
                    _title += " frame is Empty !";
                    continue;
                }

                //display
                Show(frame);

                UpdateROIS(frame);

                //si pause & pas fermeture de la fenêtre
                while (!_play &&
                       !cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(10);

                }
            }
            //}
        }


        //void PlayVideo(CancellationToken cancellationToken)
        //{
        //    int framestart = 1190;
        //    capVideo.PosFrames = framestart;

        //    _roi_width_maximum = capVideo.FrameWidth;
        //    _roi_height_maximum = capVideo.FrameHeight;

        //    if (_roi_right == 0)
        //        _roi_right = _roi_width_maximum;
        //    if (_roi_top == 0)
        //        _roi_top = _roi_height_maximum;

        //    bool loop = true;

        //    frame = new Mat();

        //    while (!capVideo.IsDisposed && loop)
        //    {
        //        //init
        //        capVideo.PosFrames = 0;
        //        CirclesReset();

        //        while (!capVideo.IsDisposed &&
        //            !cancellationToken.IsCancellationRequested)
        //        {

        //            //si goto frame number
        //            if (newWantedPosFrames != null)
        //            {
        //                capVideo.Set(VideoCaptureProperties.PosFrames, (int)newWantedPosFrames - 1);
        //                newWantedPosFrames = null;
        //            }

        //            //read next frame
        //            capVideo.Read(frame);
        //            OnPropertyChanged("_videoPositionFrame");

        //            //si arrivé à la fin
        //            if (frame.Empty())
        //                break;

        //            Thread.Sleep(sleepTime);

        //            if (roi_change || houghCircle_change)
        //                CirclesReset();

        //            ProcessFrame(frame);

        //            ScanConstructFromSensors();

        //            //si pause & pas fermeture de la fenêtre
        //            while (!_play &&
        //                   !cancellationToken.IsCancellationRequested)
        //            {
        //                Thread.Sleep(10);

        //                if (newWantedPosFrames != null)
        //                {
        //                    capVideo.Set(VideoCaptureProperties.PosFrames, (int)newWantedPosFrames - 1);
        //                    newWantedPosFrames = null;
        //                    capVideo.Read(frame);
        //                    ProcessFrame(frame);
        //                }

        //                if (roi_change || houghCircle_change || circle_Recompute)
        //                {
        //                    circle_Recompute = false;
        //                    CirclesReset();
        //                    ProcessFrame(frame);
        //                }
        //            }
        //        }
        //    }
        //}

        //void ProcessFrame(Mat frame)
        //{
        //    Mat frame_copy = frame.Clone();
        //    roi_change = false;
        //    houghCircle_change = false;
        //    try
        //    {
        //        ROI_mat = ROI(frame_copy);
        //        Mat G = ToGray(ROI_mat);

        //        CircleSegment[] circleSegments = Cv2.HoughCircles(G,
        //            method: HoughModes.Gradient,
        //            dp: _houghcircle_dp, //1.9 4k vertical
        //            minDist: 1,    //1
        //            param1: _houghcircle_param1,   //100
        //            param2: _houghcircle_param2,   //37
        //            minRadius: _houghcircle_radius_min,  //0
        //            maxRadius: _houghcircle_radius_max); //20 

        //        //SensorsAdd(circleSegments, ROI_mat.Width, ROI_mat.Height);

        //        //SensorsUpdate(G.Width, G.Height);

        //        //dessine les cercles sur la frame
        //        for (int i = 0; i < circleSegments.Length; i++)
        //            Cv2.Circle(ROI_mat, (OpenCvSharp.Point)circleSegments[i].Center, (int)circleSegments[i].Radius, new Scalar(0, 0, 255), 1);

        //        Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            try
        //            {
        //                _image = Convert(ROI_mat.ToBitmap());
        //            }
        //            catch (Exception ex) { }
        //        }));
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        void SensorsAdd(CircleSegment[] newCircleSegments, Rect roi)
        {
            //pour chaque nouveau cercle
            for (int i = 0; i < newCircleSegments.Length; i++)
            {
                //rapporter à l'image d'origine !
                newCircleSegments[i].Center += roi.TopLeft;

                Point2f center = newCircleSegments[i].Center;
                try
                {
                    int x = (int)center.X;
                    int y = (int)center.Y;

                    //est-ce que le point de mesure existe déjà ?
                    int j = 0;
                    bool found = false;

                    for (j = 0; j < cercles.Count; j++)
                    {
                        double d = Point2f.Distance(center, cercles[j].circleSegment.Center);
                        if (d < _rayon)
                        {
                            found = true;
                            break;
                        }
                    }
                    Cercle? c;
                    if (found) // oui
                    {
                        c = cercles[j];

                        //moyenne position centre
                        float X_moy = (center.X + c.nbr_fois_centre_repere * c.center_abs.X) / (c.nbr_fois_centre_repere + 1);
                        float Y_moy = (center.Y + c.nbr_fois_centre_repere * c.center_abs.Y) / (c.nbr_fois_centre_repere + 1);
                        c.center_abs = new Point2f(X_moy, Y_moy);

                        c.nbr_fois_centre_repere++;
                    }
                    else // non
                    {
                        //new circle
                        c = new Cercle(newCircleSegments[i], cercles.Count, roi);
                        cercles.Add(c);
                    }
                    c.actif = true;
                }
                catch (Exception ex)
                {

                }
            }
        }


        void SensorsRemove(Sensor s)
        {
            int i = 0;
            try
            {
                s.uc._spinner_index.Spin -= Sensor_UC_spinner_index_Spin;
                i = 1;
                s.uc._btn_Delete.MouseDown -= Sensor_UC_DeleteMe;
                i = 2;
                _sensors_uc.Remove(s.uc);
                i = 3;
                cercles.Remove(s.cercle);
                i = 4;
                _sensors.Remove(s);
                i = 5;
            }
            catch (Exception ex)
            {
                ex = ex;

            }
        }

        Mat SensorsUpdate(int w, int h, ROI_UC roi)
        {
            Mat cercles_mat = new Mat(new OpenCvSharp.Size(w, h), MatType.CV_8UC3, CirclesReset_color);

            double size = (h + w) / 2;
            _rayon = (int)(size * 80 / 2000);
            double zoom = ((double)_rayon) / 30;
            int offsetY = (int)(32 * size / 2000);
            int offsetX = (int)(32 * size / 2000);
            int thickness = (int)(6 * size / 2000);
            if (thickness < 1) thickness = 1;

            //Dessine tous les cercles
            for (int i = 0; i < cercles.Count; i++)
            {
                try
                {
                    Cercle c = cercles[i];
                    //concerne cet ROI ?
                    if (c.roi_Left != roi._roi.Left || c.roi_Top != roi._roi.Top)
                        continue;

                    if (c.actif)
                        Cv2.Circle(cercles_mat, (OpenCvSharp.Point)c.center, _rayon + 2, Scalar.White, -1);

                    //dessin du disque
                    Cv2.Circle(cercles_mat, (OpenCvSharp.Point)c.center, _rayon, c.couleur, -1);

                    //écrit le numéro
                    OpenCvSharp.Point xy = new OpenCvSharp.Point(c.center.X, c.center.Y);

                    //positionne le numéro par rapport au centre du cercle détecté
                    string text;
                    if (c.sensor != null && c.sensor.numero != null)
                        text = ((int)c.sensor.numero).ToString();
                    else
                        text = c.numero.ToString();

                    xy.Y += offsetY;
                    xy.X += -offsetX * text.Length;
                    Cv2.PutText(cercles_mat, text, xy,
                        HersheyFonts.HersheySimplex,
                        zoom, Cercle.couleurTexte, thickness: thickness);

                    if (i >= _sensors.Count)
                    {
                        Sensor s = new Sensor(c);
                        _sensors.Add(s);

                        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Sensor_UC sensor_uc = new Sensor_UC();
                            sensor_uc._Link(s);
                            sensor_uc._spinner_index.Spin += Sensor_UC_spinner_index_Spin;
                            sensor_uc._btn_Delete.MouseDown += Sensor_UC_DeleteMe;
                            _sensors_uc.Add(sensor_uc);
                        });
                    }

                    //reset actif pour la prochaine frame
                    c.actif = false;

                }
                catch (Exception ex)
                {
                    ex = ex;
                }
            }
            OnPropertyChanged("_sensors");

            return cercles_mat;
        }

        void Sensor_UC_DeleteMe(object sender, MouseButtonEventArgs e)
        {
            var im = sender as System.Windows.Controls.Image;
            var sp = im.Parent as StackPanel;
            var uc = sp.Parent as Sensor_UC;
            Sensor s = uc._s;
            SensorsRemove(s);
        }


        void Sensor_UC_spinner_index_Spin(object? sender, Xceed.Wpf.Toolkit.SpinEventArgs e)
        {
            ButtonSpinner spinner = (ButtonSpinner)sender;
            StackPanel sp = spinner.Parent as StackPanel;
            Sensor_UC suc = sp.Parent as Sensor_UC;

            if (suc._s.numero == null)
            {
                //TODO
                //suc._s.numero == senso
                return;
            }

            int valeurcherchee;
            if (e.Direction == SpinDirection.Increase) // monte
            {
                //si premier, annuler
                if (suc._s.numero == 0) return;
                //celui du dessus je l'augmente
                valeurcherchee = (int)suc._s.numero - 1;
                foreach (Sensor_UC suc_other in sensors_uc)
                    if (suc_other._s.numero == valeurcherchee)
                        suc_other._s.SetNumero((int)suc._s.numero);

                suc._s.SetNumero(valeurcherchee);
            }
            else //descend
            {
                //si dernier, annuler
                if (suc._s.numero == sensors_uc.Count - 1) return;
                valeurcherchee = (int)suc._s.numero + 1;
                foreach (Sensor_UC suc_other in sensors_uc)
                    if (suc_other._s.numero == valeurcherchee)
                        suc_other._s.SetNumero((int)suc._s.numero);

                suc._s.SetNumero(valeurcherchee);
            }

            Sensor_Sort();
        }


        void ScanConstructFromSensors()
        {
            if (scan_mat == null || scan_mat.Height != _sensors.Count)
                scan_mat = new Mat(_sensors.Count, scan_mat_width, MatType.CV_8UC3);

            if (scan_mat_T0 == null)
                scan_mat_T0 = DateTime.Now;

            //reset colonne
            Cv2.Line(scan_mat, new OpenCvSharp.Point(scan_mat_column, 0), new OpenCvSharp.Point(scan_mat_column, scan_mat.Height - 1), Scalar.Black, 1);
            //ligne de scan
            if (scan_mat_column < scan_mat.Width - 2)
                Cv2.Line(scan_mat, new OpenCvSharp.Point(scan_mat_column + 1, 0), new OpenCvSharp.Point(scan_mat_column + 1, scan_mat.Height - 1), Scalar.GreenYellow, 1);

            foreach (Sensor s in _sensors)
            {
                Scalar pixel = scan_mat.At<Scalar>();
                if (s.numero != null)
                    scan_mat.At<Vec3b>((int)s.numero, scan_mat_column) = new Vec3b(s.pixelValue.Item0, s.pixelValue.Item1, s.pixelValue.Item2);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _image3 = scan_mat.ToBitmapSource();
                }
                catch (Exception ex) { }
            }));

            scan_mat_column++;
        }

        void ScanPart_Save()
        {
            if (scan_mat_column > scan_mat.Width - 1)
            {
                DateTime scan_mat_Tfin = DateTime.Now;
                DateTime t0 = (DateTime)scan_mat_T0;
                TimeSpan duree = scan_mat_Tfin.Subtract(t0);
                if (_scan_save)
                {
                    string folder = "_SCANS\\";
                    Directory.CreateDirectory(folder);
                    string name = t0.ToString("yyyy-MM-dd hh-mm-ss.fff")
                        + " (" + duree.TotalSeconds.ToString("F2") + ")";
                    //sauvegarde image
                    scan_mat.SaveImage(folder + name + ".jpg");
                }
                TimeSpan deltatT = t0 - (DateTime)experience_T0;
                Mat mat_to_save = scan_mat.Clone();

                if (_store_scans)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        Scan_UC s = new(mat_to_save, t0, duree);
                        s._img.StretchDirection = StretchDirection.UpOnly;
                        s._img.Stretch = Stretch.UniformToFill;
                        s.Height = _scans_height;

                        _scans.Add(s);

                        //follow last
                        if (scan_focus_last)
                            _lbx_scans.ScrollIntoView(s);
                    });
                }

                //reset
                scan_mat_T0 = null;
                scan_mat_column = 0;
            }
        }

        void SelectVideoFile()
        {
            OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Video";
            dialog.DefaultExt = ".mp4";
            dialog.Filter = "Videos (.mp4)|*.mp4";
            if (File.Exists(_file))
                dialog.DefaultDirectory = System.IO.Path.GetDirectoryName(_file);

            if (dialog.ShowDialog() == true)
                _file = dialog.FileName;
        }

        #region IHM Click

        void btn_pause_Click(object sender, MouseButtonEventArgs e)
        {
            _play = false;
        }

        void btn_play_Click(object sender, MouseButtonEventArgs e)
        {
            _play = true;
        }


        #endregion



        void CameraListRefresh_Click(object sender, RoutedEventArgs e)
        {
            //_sp_webcam_list.Children.Clear();
            _cbx_webcam.Items.Clear();

            webcams = WebCamParameters_UC._Manager.Get_WebCams();
            int index = 0;
            foreach (DsDevice webcam in webcams)
            {
                MenuItem mi = new MenuItem();
                mi.Header = webcam.Name;
                mi.Tag = index;
                mi.Click += WebCam_Selected;
                //_sp_webcam_list.Children.Add(mi);

                TextBlock tb = new TextBlock();
                tb.Text = webcam.Name;
                tb.Tag = index;
                _cbx_webcam.Items.Add(tb);

                index++;
            }
        }

        void WebCam_Selected(object sender, RoutedEventArgs e)
        {
            int webcam_index = (int)(sender as MenuItem).Tag;
            webcam = webcams[webcam_index];
            Camera(webcam_index);
        }

        void SortSensors_Up2Down_Click(object sender, MouseButtonEventArgs e)
        {
            SortSensors_Up2Down();
        }

        void SortSensors_Up2Down()
        {
            foreach (Sensor s in _sensors)
                s.Reset();
            Sensor.ResetSensorsOrder();
            sensors_to_sorted = true;
        }

        //void SensorMap_Click(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Right)
        //    {
        //        var image = sender as System.Windows.Controls.Image;
        //        System.Windows.Point p = e.GetPosition(image);

        //        float x = (float)(p.X / image.ActualWidth * cercles_mat.Width);
        //        float y = (float)(p.Y / image.ActualHeight * cercles_mat.Height);
        //        Point2f pointClick = new Point2f(x, y);

        //        Cercle cercleSelectionné = null;
        //        //quel cercle est concerné ?
        //        foreach (Cercle cercle in cercles)
        //        {
        //            if (Point2f.Distance(pointClick, cercle.circleSegment.Center) < rayon)
        //            {
        //                //trouvé !
        //                cercleSelectionné = cercle;
        //                break;
        //            }
        //        }
        //        if (cercleSelectionné == null) return;

        //        cercleSelectionné.sensor.uc._Selected();
        //    }
        //}

        OpenCvSharp.Rect? SelectROI()
        {
            string window_name = "Valid ROI with 'Enter' or 'Space', Cancel with 'c'";
            OpenCvSharp.Rect? newroi = Cv2.SelectROI(window_name, frame.Clone(), true);
            if (((OpenCvSharp.Rect)newroi).Width == 0 || ((OpenCvSharp.Rect)newroi).Height == 0)
                newroi = null;
            Cv2.DestroyWindow(window_name);
            return newroi;
        }

        void _cbx_webcam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            webcam_index = (int)(e.AddedItems[0] as TextBlock).Tag;
            webcam = webcams[webcam_index];

            config.webcam_name = webcam.Name;
            CameraSelected();
        }

        void CameraSelected()
        {
            _webcam_formats = new ObservableCollection<Format>(WebCamParameters_UC._Manager.Get_WebCam_Formats(webcam));
        }

        void _cbx_webcam_format_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objet = e.AddedItems[0];
            _webcam_format = objet as Format;
            CameraFormatSelected();
        }

        void CameraFormatSelected()
        {
            Camera(webcam_index);
            Set_Format();
            _play = true;
        }

        void FormatForce()
        {
            if (capVideo == null)
                Camera(webcam_index);
            int tentative = 0;
            Mat im = new Mat();
            do
            {
                Set_Format();
                while (im.Empty())
                {
                    capVideo.Read(im);
                    tentative++;
                }
            } while (im.Width != _webcam_format.w && tentative < 100);
            if (tentative >= 100)
            {
                MessageBox.Show("FormatForce Echec");
            }
        }

        void Set_Format()
        {
            capVideo.Set(VideoCaptureProperties.FourCC, FourCC.FromString(_webcam_format.format));
            capVideo.Set(VideoCaptureProperties.FrameWidth, _webcam_format.w);
            capVideo.Set(VideoCaptureProperties.FrameHeight, _webcam_format.h);
            capVideo.Set(VideoCaptureProperties.Fps, _webcam_format.fr);
        }

        void WebCamSettings_Show_Click(object sender, MouseButtonEventArgs e)
        {
            var newcfg = WebCamParameters_UC._Manager.ShowDialog();
            if (newcfg != null)
            {
                webCam_Config = newcfg;
            }
        }

        void DefineROI_Click(object sender, MouseButtonEventArgs e)
        {
            Rect? roi = SelectROI();
            if (roi != null)
                ROI_New((Rect)roi);
        }

        void ROI_New(Rect roi)
        {
            ROI_UC roi_uc = new ROI_UC(roi);
            if (!config.rois.Contains(roi))
                config.rois.Add(roi);

            roi_uc._img.MouseDown += _roi_sensor_Click;
            roi_uc._img_sensormap.MouseDown += _sensormap_Click;
            _rois.Add(roi_uc);

            //ajout dans Avalondock
            AddNewAvalonView(roi_uc, roi_uc._name);

            TextBlock tb = new TextBlock();
            tb.Text = roi_uc._name;
            tb.Tag = roi_uc;
            tb.MouseDown += ROI_selected;
            _lbx_rois.Items.Add(tb);

        }

        void ROI_selected(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var tb = sender as TextBlock;
                ROI_UC roi_uc = tb.Tag as ROI_UC;

                roi_uc._img.MouseDown -= _roi_sensor_Click;
                roi_uc._img_sensormap.MouseDown -= _sensormap_Click;

                config.rois.Remove(roi_uc._roi);
                _rois.Remove(roi_uc);
                _lbx_rois.Items.Remove(sender);
            }
        }

        void _sensormap_Click(object sender, MouseButtonEventArgs e)
        {
            var image = sender as System.Windows.Controls.Image;
            var grid = image.Parent as Grid;
            var uc = grid.Parent as ROI_UC;

            System.Windows.Point p = e.GetPosition(image);

            float x = (float)(p.X / image.ActualWidth * uc._roi.Width);
            float y = (float)(p.Y / image.ActualHeight * uc._roi.Height);
            Point2f pointClick = new Point2f(x, y);

            Cercle cercleSelected = null;
            //quel cercle est concerné ?
            foreach (Cercle c in cercles)
            {
                //concerne cet ROI ?
                if (c.roi_Left != uc._roi.Left || c.roi_Top != uc._roi.Top)
                    continue;

                if (Point2f.Distance(pointClick, c.center) < rayon)
                {
                    //trouvé !
                    cercleSelected = c;
                    break;
                }
            }
            if (cercleSelected == null) return;

            if (e.ChangedButton == MouseButton.Right)
            {
                //supprimer
                cercles.Remove(cercleSelected);
                if (cercleSelected.sensor != null)
                {
                    Sensor_UC sensor_uc = cercleSelected.sensor.uc;
                    //lv_sensors.Items.Remove(sensor_uc);
                    _sensors_uc.Remove(sensor_uc);
                }
            }
            else
                cercleSelected.sensor.uc._Selected();
        }

        void _roi_sensor_Click(object sender, MouseButtonEventArgs e)
        {
            //quel sensor est le plus proche ?
            var image = sender as System.Windows.Controls.Image;
            var grid = image.Parent as Grid;
            var uc = grid.Parent as ROI_UC;

            System.Windows.Point p = e.GetPosition(image);

            float x = (float)(p.X / image.ActualWidth * uc._roi.Width) + uc._roi.Left;
            float y = (float)(p.Y / image.ActualHeight * uc._roi.Height) + uc._roi.Top;
            Point2f pointClick = new Point2f(x, y);

            nearestSensor = null;
            double distance_min = double.MaxValue;
            //quel sensor est concerné ?
            foreach (Sensor s in sensors)
            {
                double distance = Point2f.Distance(pointClick, new Point2f(s.x, s.y));
                if (distance < distance_min)
                {
                    distance_min = distance;
                    nearestSensor = s;
                }
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                lv_sensors.ScrollIntoView(nearestSensor.uc);
                lv_sensors.SelectedItem = nearestSensor.uc;
                lv_sensors.Focus();
                nearestSensor.uc._Selected();
            }
        }

        void _roi_sensor_KeyPress(object sender, KeyEventArgs e)
        {
            if (nearestSensor != null)
            {
                if (e.Key == Key.Q)
                    nearestSensor.x -= 1;
                if (e.Key == Key.D)
                    nearestSensor.x += 1;
                if (e.Key == Key.Z)
                    nearestSensor.y -= 1;
                if (e.Key == Key.S)
                    nearestSensor.y += 1;
            }
        }

        void UpdateROIS(Mat frame)
        {
            foreach (ROI_UC roi in _rois)
            {
                try
                {
                    Mat roi_frame = new Mat(frame, roi._roi);
                    roi_frame = roi_frame.Clone();

                    Mat G = ToGray(roi_frame);

                    if (_HoughCircle_Detection)
                    {
                        CircleSegment[] circleSegments = Cv2.HoughCircles(G,
                            method: HoughModes.Gradient,
                            dp: _houghcircle_dp, //1.9 4k vertical
                            minDist: 1,    //1
                            param1: _houghcircle_param1,   //100
                            param2: _houghcircle_param2,   //37
                            minRadius: _houghcircle_radius_min,  //0
                            maxRadius: _houghcircle_radius_max); //20 

                        SensorsAdd(circleSegments, roi._roi);

                        //dessine les cercles sur la frame
                        for (int i = 0; i < circleSegments.Length; i++)
                            Cv2.Circle(roi_frame, (OpenCvSharp.Point)circleSegments[i].Center - roi._roi.TopLeft, (int)circleSegments[i].Radius, new Scalar(0, 0, 255), 1);

                        roi._circlesCount = circleSegments.Length;
                    }

                    //if (roi._circlesCount > 0)
                    //                    {
                    Mat sensormap = SensorsUpdate(G.Width, G.Height, roi);
                    //                    }

                    SensorsMeasure();

                    if (sensors_display)
                        foreach (Sensor sensor in _sensors)
                            Sensors_display(roi_frame, roi, sensor);

                    roi.Show(roi_frame);


                    if (_sensors.Count > 0)
                    {
                        ScanConstructFromSensors();

                        ScanPart_Save();

                        //Trouver le front
                        //Tracer I = f(H)
                        _graph1._Update(_sensors);

                    }

                    roi.Show_sensormap(sensormap);
                }
                catch (Exception ex)
                {
                    ex = ex;
                }
            }
        }

        void Sensors_display(Mat frame, ROI_UC roi_uc, Sensor sensor)
        {
            Cv2.Circle(frame, sensor.x - roi_uc._roi.Left, sensor.y - roi_uc._roi.Top, 2, new Scalar(255, 0, 255), 1);
        }

        void SensorsMeasure()
        {
            bool all_sensors_are_initialized = true;
            foreach (Sensor sensor in _sensors)
            {
                Vec3b pixelValue;

                //mesure
                //v0 = juste centre
                //pixelValue = frame.At<Vec3b>(sensor.y, sensor.x);

                //v1 = carré
                //                int range = 9;
                //                int nbr_val = 0;
                //                float[] mesure = new float[3];
                //                for (int x = -range; x < range + 1; x++)
                //                    for (int y = -range; y < range + 1; y++)
                //                    {
                //                        Vec3b pixelValueRange = frame.At<Vec3b>(c.y + y, c.x + x);
                //                        mesure[0] += pixelValueRange.Item0;
                //                        mesure[1] += pixelValueRange.Item1;
                //                        mesure[2] += pixelValueRange.Item2;
                //                        nbr_val++;
                //                    }

                ////                Cv2.his



                //                range++;
                //                //cadre jaune de la zone de mesure
                //                OpenCvSharp.Point A = new OpenCvSharp.Point(c.x-range, c.y-range);
                //                OpenCvSharp.Point B = new OpenCvSharp.Point(c.x+range, c.y-range);
                //                OpenCvSharp.Point C = new OpenCvSharp.Point(c.x+range, c.y+range);
                //                OpenCvSharp.Point D = new OpenCvSharp.Point(c.x-range, c.y+range);

                //                Cv2.Line(ROI_mat, A, B, Scalar.Black);
                //                Cv2.Line(ROI_mat, B, C, Scalar.Black);
                //                Cv2.Line(ROI_mat, C, D, Scalar.Black);
                //                Cv2.Line(ROI_mat, D, A, Scalar.Black);


                //                pixelValue.Item0 = (byte)(mesure[0] / nbr_val);
                //                pixelValue.Item1 = (byte)(mesure[1] / nbr_val);
                //                pixelValue.Item2 = (byte)(mesure[2] / nbr_val);

                //=> pas non plus très représentatif...

                //v2 = max carré

                int range = 9;
                float[] pixel = new float[3];
                float mesure = 0;
                float mesure_max = 0;
                int x_max = sensor.x, y_max = sensor.y;

                for (int x = -range; x < range + 1; x++)
                    for (int y = -range; y < range + 1; y++)
                    {
                        Vec3b pixelValueRange = frame.At<Vec3b>(sensor.y + y, sensor.x + x);
                        mesure = pixelValueRange.Item0 + pixelValueRange.Item1 + pixelValueRange.Item2;
                        if (mesure > mesure_max)
                        {
                            mesure_max = mesure;
                            x_max = sensor.x + x;
                            y_max = sensor.y + y;
                        }
                    }

                pixelValue = frame.At<Vec3b>(y_max, x_max);

                sensor.SetMeasure(pixelValue);

                if (sensor.numero == null)
                    all_sensors_are_initialized = false;
            }

            //sort ?
            if (sensors_to_sorted && all_sensors_are_initialized)
            {
                Sensor_Sort();
            }
        }

        void Sensor_Sort()
        {
            var sorted = (_sensors_uc.OrderBy(uc => uc._s.numero)).ToList();
            _sensors_uc = new ObservableCollection<Sensor_UC>(sorted);
        }

        void HoughCircle_Switch_Click(object sender, MouseButtonEventArgs e)
        {
            _HoughCircle_Detection = !_HoughCircle_Detection;
            OnPropertyChanged(nameof(_rois));
        }

        void Sensors_Load_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "Config";
            dialog.DefaultExt = ".cfg";
            dialog.Filter = "Config (.cfg)|*.cfg";
            if (File.Exists(_file))
                dialog.DefaultDirectory = System.IO.Path.GetDirectoryName(_file);

            if (dialog.ShowDialog() == true)
            {
                string jsonString = File.ReadAllText(dialog.FileName);
                config = Config.FromJSON(jsonString);
                //caméra présente ?
                bool camerapresente = false;
                for (int i = 0; i < _cbx_webcam.Items.Count; i++)
                {
                    TextBlock tb = _cbx_webcam.Items[i] as TextBlock;
                    if (tb.Text == config.webcam_name)// webcam.Name)
                    {
                        camerapresente = true;
                        //this.webcam = webcam;
                        webcam_index = i;// (int)tb.Tag;
                        webcam = webcams[i];
                    }
                }

                if (!camerapresente)
                {
                    MessageBox.Show("Caméra demandée non présente :\n" + config.webcam_name);
                    return;
                }

                CameraSelected();

                FormatForce();

                OnPropertyChanged(nameof(_webcam_format));

                //appliqués les réglages caméra
                WebCamParameters_UC._Manager.Set_WebCamConfig(config.webcam_name, config.webcam_parameters);

                //ROIs
                _rois.Clear();
                _lbx_rois.Items.Clear();
                foreach (Rect roi in config.rois)
                    ROI_New(roi);

                //Sensors
                _sensors_uc = new ObservableCollection<Sensor_UC>();
                _sensors = new ObservableCollection<Sensor>(config.sensors);
                foreach (Sensor sensor in _sensors)
                {
                    sensor.Load();
                    Sensor_UC sensor_uc = new Sensor_UC();
                    sensor_uc._spinner_index.Spin += Sensor_UC_spinner_index_Spin;
                    sensor_uc._btn_Delete.MouseDown += Sensor_UC_DeleteMe;
                    sensor_uc._Link(sensor);
                    _sensors_uc.Add(sensor_uc);
                }
                Sensor_Sort();

                List<float> Hs = (sensors.Select(x => x.hauteur_mm)).ToList();

                //Camera(webcam_index);
                _play = true;
            }
        }

        void Sensors_Save_Click(object sender, MouseButtonEventArgs e)
        {
            //quelle caméra ?

            //quel format ?

            //quels réglages de caméra ?
            config.webcam_parameters = WebCamParameters_UC._Formulaire._GetWebCamSettings(config.webcam_name);
            //quelles ROI ?

            //quelles sont les coordonnées et taille des sensors ?
            foreach (Sensor sensor in sensors)
                sensor.Save();

            config.sensors = sensors.ToList();

            //save all
            string jsonString = config.ToJSON();

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "Config";
            dialog.DefaultExt = ".cfg";
            dialog.Filter = "Config (.cfg)|*.cfg";

            //au même endroit que la vidéo
            if (File.Exists(_file))
                dialog.DefaultDirectory = System.IO.Path.GetDirectoryName(_file);

            if (dialog.ShowDialog() == true)
                File.WriteAllText(dialog.FileName, jsonString);
        }

        void Scan_save_Switch_Click(object sender, MouseButtonEventArgs e)
        {
            _scan_save = !_scan_save;
        }

        void ScansReset_Click(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _scans.Clear();
            });
            experience_T0 = DateTime.Now;
        }

        void Scan_focus_last_Switch_Click(object sender, MouseButtonEventArgs e)
        {
            _scan_focus_last = !_scan_focus_last;
        }

        void Scan_Save_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                FileInfo file = Save_Scan(directory);
                //open file explorer
                Process.Start("explorer.exe", "/select, \"" + file.FullName + "\"");
            }
            catch (Exception ex)
            {

            }
        }

        FileInfo Save_Scan(string directory)
        {
            Mat[] images = _scans.Select(x => x._mat).ToArray();
            Mat mat = new Mat();
            Cv2.HConcat(images, mat);

            TimeSpan duree = _scans.Last()._t - _scans.First()._t;
            string filepath = directory + _scans.First()._t.ToString("yyyy-MM-dd HH¤mm") + " [" + duree.TotalSeconds.ToString("f1") + "s].jpg";
            filepath = filepath.Replace('¤', 'h');
            mat.SaveImage(filepath);
            FileInfo fileInfo = new FileInfo(filepath);
            return fileInfo;
        }

        void Sensors_dsiplay_Switch_Click(object sender, MouseButtonEventArgs e)
        {
            _sensors_dsiplay = !_sensors_dsiplay;
        }

        void Store_scans_Switch_Click(object sender, MouseButtonEventArgs e)
        {
            _store_scans = !_store_scans;
        }




        #region AVALONDOCK
        void LayoutSaveButton_Click(object sender, RoutedEventArgs e)
        {
            XmlLayoutSerializer layoutSerializer = new XmlLayoutSerializer(_DManager);
            using (var writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "docks.txt"))
            {
                layoutSerializer.Serialize(writer);
            }
        }

        void LayoutLoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XmlLayoutSerializer layoutSerializer = new XmlLayoutSerializer(_DManager);
                using (var reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "docks.txt"))
                {
                    layoutSerializer.Deserialize(reader);
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {

            }
            catch (Exception ex)
            {
                throw;
            }

            //views = new List<LayoutAnchorable>() {
            //    layoutAnchorable_algorythn_parameters,
            //    layoutAnchorable_arduino_config,
            //    layoutAnchorable_arduino_read,
            //    layoutAnchorable_augmented_display,
            //    layoutAnchorable_camera_config,
            //    layoutAnchorable_data_points,
            //    layoutAnchorable_graph1,
            //    layoutAnchorable_graph2,
            //    layoutAnchorable_image_process,
            //    layoutAnchorable_system_paremeters,
            //    layoutAnchorable_view1,
            //    layoutAnchorable_view2,
            //    layoutAnchorable_view3,
            //    layoutAnchorable_view4
            //};
        }

        void AddNewAvalonView(Control vue, string nom)
        {
            LayoutAnchorable anchorable = new LayoutAnchorable
            {
                Title = nom,
                Content = vue,
                CanClose = false,
            };

            // Création d'un nouveau LayoutAnchorablePane si aucun n'existe
            LayoutAnchorablePane newPane = new LayoutAnchorablePane();
            newPane.Children.Add(anchorable);
            _DManager.Layout.RootPanel.Children.Add(newPane);

            // Afficher l'élément
            anchorable.IsActive = true;
            anchorable.IsSelected = true;
        }

    }

    #endregion
}
