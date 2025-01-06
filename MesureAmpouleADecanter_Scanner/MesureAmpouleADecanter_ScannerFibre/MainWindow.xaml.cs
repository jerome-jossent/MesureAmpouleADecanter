using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DirectShowLib;
using MesureAmpouleADecanter_ScannerFibre.Properties;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using static System.Net.Mime.MediaTypeNames;
using static WebCamParameters_UC.WebCamFormat;
using Rect = OpenCvSharp.Rect;

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
                if (MessageBox.Show("Réinitialiser les réglages de crop/roi ?", "Reset Crop ?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    ResetROI();
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
                _title = tabindex.ToString();
            }
        }
        int tabindex = 2;

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

        #region ROI Parameters
        bool roi_change;
        public double _roi_left
        {
            get => roi_left;
            set
            {
                roi_left = value;
                OnPropertyChanged();
                Settings.Default.roi_left = roi_left;
                Settings.Default.Save();
                roi_change = true;
            }
        }
        double roi_left = Settings.Default.roi_left;

        public double _roi_right
        {
            get => roi_right;
            set
            {
                roi_right = value;
                OnPropertyChanged();
                Settings.Default.roi_right = roi_right;
                Settings.Default.Save();
                roi_change = true;
            }
        }
        double roi_right = Settings.Default.roi_right;

        public double _roi_width_maximum
        {
            get => roi_width_maximum;
            set
            {
                roi_width_maximum = value;
                OnPropertyChanged();
                roi_change = true;
            }
        }
        double roi_width_maximum;


        public double _roi_top
        {
            get => roi_top;
            set
            {
                roi_top = value;
                OnPropertyChanged();
                Settings.Default.roi_top = roi_top;
                Settings.Default.Save();
                roi_change = true;
            }
        }
        double roi_top = Settings.Default.roi_top;

        public double _roi_bottom
        {
            get => roi_bottom;
            set
            {
                roi_bottom = value;
                OnPropertyChanged();
                Settings.Default.roi_bottom = roi_bottom;
                Settings.Default.Save();
                roi_change = true;
            }
        }
        double roi_bottom = Settings.Default.roi_bottom;

        public double _roi_height_maximum
        {
            get => roi_height_maximum;
            set
            {
                roi_height_maximum = value;
                OnPropertyChanged();
                roi_change = true;
            }
        }
        double roi_height_maximum;
        #endregion

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
                {
                    value = config.webcam_format;
                    //if (config.webcam_format != null)
                    //{
                    //    OnPropertyChanged();
                    //    CameraFormatSelected();
                    //}
                }
                else
                    config.webcam_format = value;

                //CameraFormatSelected();
                OnPropertyChanged();
            }
        }

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


        #region Parameters local

        List<DsDevice> webcams;
        int webcam_index;
        DsDevice webcam;
        //WebCamParameters_UC.WebCamFormat.Format webcam_format;
        WebCamParameters_UC.WebCamConfig webCam_Config;

        Mat frame;
        Mat ROI_mat;
        Mat cercles_mat = new Mat();
        Mat scan_mat = new Mat(100, 200, MatType.CV_8UC3, Scalar.Magenta);
        int scan_mat_column = 0;

        List<Cercle> cercles;
        bool circle_Recompute;
        OpenCvSharp.VideoCapture capVideo;
        int sleepTime;
        Thread videoThread;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Scalar CirclesReset_color = Scalar.Black;

        SensorMap sensorMap;
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
            DisplayCrop(false);
            DisplayHoughCircle(false);

            //ProcessFile(file);
            CameraListRefresh_Click(null, null);

            //load cam, best format
            _cbx_webcam.SelectedIndex = 0;
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            StopAll();
        }

        void DisplayCrop(bool status)
        {
            if (status)
            {
                grid_column_crop.Width = new GridLength(1, GridUnitType.Auto);
                grid_row_crop.Height = new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                grid_column_crop.Width = new GridLength(0);
                grid_row_crop.Height = new GridLength(0);
            }
        }

        void DisplayHoughCircle(bool status)
        {
            if (status)
                grid_row_houghcircle.Height = new GridLength(1, GridUnitType.Auto);
            else
                grid_row_houghcircle.Height = new GridLength(0);
        }

        #endregion

        #region Image & Mat Operations
        Mat ToGray(Mat src)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        Mat ROI(Mat src)
        {
            int left = (int)_roi_left;// 20;
            int right = (int)(_roi_width_maximum - _roi_right);//150;
            int top = (int)(_roi_height_maximum - _roi_top);//1720;
            int bottom = (int)(_roi_bottom);//100;

            //Dispatcher.BeginInvoke(() =>
            //{
            //    //Title = left + " ↔ " + right + "\t" + top + " ↨ " + bottom;
            //    //Title = _houghcircle_dp.ToString("0.00") + "\t" + _houghcircle_param1.ToString("0.00") + "\t" + _houghcircle_param2.ToString("0.00");
            //});

            return ROI(src, left, top, right, bottom);
        }

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

        void ResetROI()
        {
            _roi_left = 0;
            _roi_top = 0;
            _roi_right = 0;
            _roi_bottom = 0;
        }

        void ProcessFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return;

            VideoStop();

            capVideo = new OpenCvSharp.VideoCapture(filePath);
            sleepTime = (int)Math.Round(1000 / capVideo.Fps);
            OnPropertyChanged("_videoTotalFrames");

            CancellationToken token = cancellationTokenSource.Token;
            Task.Factory.StartNew(() => PlayVideo(token), token);
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

        void CirclesReset()
        {
            int left = (int)_roi_left;// 20;
            int right = (int)(_roi_width_maximum - _roi_right);//150;
            int top = (int)(_roi_height_maximum - _roi_top);//1720;
            int bottom = (int)(_roi_bottom);//100;

            int w = capVideo.FrameWidth - left - right;
            int h = capVideo.FrameHeight - top - bottom;

            cercles = new List<Cercle>();
            _sensors = new ObservableCollection<Sensor>();

            Dispatcher.BeginInvoke(() =>
            {
                lv_sensors.Items.Clear();
            });
        }


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
            bool loop = true;
            frame = new Mat();
            CirclesReset();

            while (!capVideo.IsDisposed && loop)
            {
                //CirclesReset();
                while (!capVideo.IsDisposed &&
                    !cancellationToken.IsCancellationRequested)
                {
                    //read next frame
                    capVideo.Read(frame);

                    //si arrivé à la fin
                    if (frame.Empty())
                        continue;

                    //display
                    Show(frame);

                    UpdateROIS(frame);

                    //if (roi_change || houghCircle_change)
                    //    CirclesReset();

                    //ProcessFrame(frame);

                    //ScanConstructFromSensors();

                    //Thread.Sleep(sleepTime);

                    //si pause & pas fermeture de la fenêtre
                    while (!_play &&
                           !cancellationToken.IsCancellationRequested)
                    {
                        Thread.Sleep(10);

                        //    if (newWantedPosFrames != null)
                        //    {
                        //        capVideo.Set(VideoCaptureProperties.PosFrames, (int)newWantedPosFrames - 1);
                        //        newWantedPosFrames = null;
                        //        capVideo.Read(frame);
                        //        ProcessFrame(frame);
                        //    }

                        //    if (roi_change || houghCircle_change || circle_Recompute)
                        //    {
                        //        circle_Recompute = false;
                        //        CirclesReset();
                        //        ProcessFrame(frame);
                        //    }
                    }
                }
            }
        }




        void PlayVideo(CancellationToken cancellationToken)
        {
            int framestart = 1190;
            capVideo.PosFrames = framestart;

            _roi_width_maximum = capVideo.FrameWidth;
            _roi_height_maximum = capVideo.FrameHeight;

            if (_roi_right == 0)
                _roi_right = _roi_width_maximum;
            if (_roi_top == 0)
                _roi_top = _roi_height_maximum;

            bool loop = true;

            frame = new Mat();

            while (!capVideo.IsDisposed && loop)
            {
                //init
                capVideo.PosFrames = 0;
                CirclesReset();

                while (!capVideo.IsDisposed &&
                    !cancellationToken.IsCancellationRequested)
                {

                    //si goto frame number
                    if (newWantedPosFrames != null)
                    {
                        capVideo.Set(VideoCaptureProperties.PosFrames, (int)newWantedPosFrames - 1);
                        newWantedPosFrames = null;
                    }

                    //read next frame
                    capVideo.Read(frame);
                    OnPropertyChanged("_videoPositionFrame");

                    //si arrivé à la fin
                    if (frame.Empty())
                        break;

                    Thread.Sleep(sleepTime);

                    if (roi_change || houghCircle_change)
                        CirclesReset();

                    ProcessFrame(frame);

                    ScanConstructFromSensors();

                    //si pause & pas fermeture de la fenêtre
                    while (!_play &&
                           !cancellationToken.IsCancellationRequested)
                    {
                        Thread.Sleep(10);

                        if (newWantedPosFrames != null)
                        {
                            capVideo.Set(VideoCaptureProperties.PosFrames, (int)newWantedPosFrames - 1);
                            newWantedPosFrames = null;
                            capVideo.Read(frame);
                            ProcessFrame(frame);
                        }

                        if (roi_change || houghCircle_change || circle_Recompute)
                        {
                            circle_Recompute = false;
                            CirclesReset();
                            ProcessFrame(frame);
                        }
                    }
                }
            }
        }

        void ProcessFrame(Mat frame)
        {
            Mat frame_copy = frame.Clone();
            roi_change = false;
            houghCircle_change = false;
            try
            {
                ROI_mat = ROI(frame_copy);
                Mat G = ToGray(ROI_mat);

                CircleSegment[] circleSegments = Cv2.HoughCircles(G,
                    method: HoughModes.Gradient,
                    dp: _houghcircle_dp, //1.9 4k vertical
                    minDist: 1,    //1
                    param1: _houghcircle_param1,   //100
                    param2: _houghcircle_param2,   //37
                    minRadius: _houghcircle_radius_min,  //0
                    maxRadius: _houghcircle_radius_max); //20 

                //SensorsAdd(circleSegments, ROI_mat.Width, ROI_mat.Height);

                SensorsUpdate(G.Width, G.Height);

                //dessine les cercles sur la frame
                for (int i = 0; i < circleSegments.Length; i++)
                    Cv2.Circle(ROI_mat, (OpenCvSharp.Point)circleSegments[i].Center, (int)circleSegments[i].Radius, new Scalar(0, 0, 255), 1);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        _image = Convert(ROI_mat.ToBitmap());
                    }
                    catch (Exception ex) { }
                }));
            }
            catch (Exception ex)
            {

            }
        }

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
                        float X_moy = (center.X + c.nbr_fois_centre_repere * c.circleSegment.Center.X) / (c.nbr_fois_centre_repere + 1);
                        float Y_moy = (center.Y + c.nbr_fois_centre_repere * c.circleSegment.Center.Y) / (c.nbr_fois_centre_repere + 1);
                        c.circleSegment.Center = new Point2f(X_moy, Y_moy);
                        c.nbr_fois_centre_repere++;
                    }
                    else // non
                    {
                        //new circle
                        c = new Cercle(newCircleSegments[i], cercles.Count);
                        cercles.Add(c);
                    }
                    c.actif = true;
                }
                catch (Exception ex)
                {

                }
            }
        }

        void SensorsUpdate(int w, int h)
        {

            cercles_mat = new Mat(new OpenCvSharp.Size(w, h), MatType.CV_8UC3, CirclesReset_color);

            double size = (h + w) / 2;
            _rayon = (int)(size * 80 / 2000);
            double zoom = ((double)_rayon) / 22;// 0.5;//0.5 quand r=15 ; 3 quand r=50

            int offsetY = (int)(36 * size / 2000); //5 pour 415 ; 30 pour 2000
            int offsetX = (int)(35 * size / 2000); //5 pour 415 ; 28 pour 2000
            int thickness = (int)(6 * size / 2000);//1 pour 415 ;  6 pour 2000
            if (thickness < 1) thickness = 1;

            //Dessine tous les cercles
            for (int i = 0; i < cercles.Count; i++)
            {
                Cercle c = cercles[i];

                if (c.actif)
                    Cv2.Circle(cercles_mat, (OpenCvSharp.Point)c.circleSegment.Center, _rayon + 2, Scalar.White, -1);
                //dessin du disque
                Cv2.Circle(cercles_mat, (OpenCvSharp.Point)c.circleSegment.Center, _rayon, c.couleur, -1);

                //écrit le numéro
                OpenCvSharp.Point xy = new OpenCvSharp.Point(c.circleSegment.Center.X, c.circleSegment.Center.Y);

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
                        Sensor_UC suc = new Sensor_UC();
                        suc._Link(s);
                        lv_sensors.Items.Add(suc);
                    });
                }

                //mesure
                //v0 = juste centre
                Vec3b pixelValue = ROI_mat.At<Vec3b>(c.y, c.x);

                //                //v1 = carré
                //                int range = 9;
                //                int nbr_val = 0;
                //                float[] mesure = new float[3];
                //                for (int x = -range; x < range + 1; x++)
                //                    for (int y = -range; y < range + 1; y++)
                //                    {
                //                        Vec3b pixelValueRange = ROI_mat.At<Vec3b>(c.y + y, c.x + x);
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




                c.sensor.SetColor(pixelValue);

                //reset actif pour la prochaine frame
                c.actif = false;
            }
            OnPropertyChanged("_sensors");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _image1 = Convert(cercles_mat.ToBitmap());
                }
                catch (Exception ex) { }
            }));
        }

        void ScanConstructFromSensors()
        {
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
                    _image3 = Convert(scan_mat.ToBitmap());
                }
                catch (Exception ex) { }
            }));

            scan_mat_column++;
            if (scan_mat_column > scan_mat.Width - 1)
                scan_mat_column = 0;
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
        void OpenVideoFile_Click(object sender, MouseButtonEventArgs e)
        {
            SelectVideoFile();
        }

        void btn_pause_Click(object sender, MouseButtonEventArgs e)
        {
            _play = false;
        }

        void btn_play_Click(object sender, MouseButtonEventArgs e)
        {
            _play = true;
        }

        void Crop_Click(object sender, MouseButtonEventArgs e)
        {
            if (grid_column_crop.Width == new GridLength(1, GridUnitType.Auto))
                DisplayCrop(false);
            else
                DisplayCrop(true);
        }

        void HoughCircle_Click(object sender, MouseButtonEventArgs e)
        {
            if (grid_row_houghcircle.Height == new GridLength(1, GridUnitType.Auto))
                DisplayHoughCircle(false);
            else
                DisplayHoughCircle(true);
        }

        void CirclesReset_Click(object sender, MouseButtonEventArgs e)
        {
            circle_Recompute = true;
            if (_play)
                CirclesReset();
        }

        void Image_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _play = !_play;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                CirclesReset();
                ProcessFrame(frame);
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {

            }
        }


        void SensorMap_Load_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "SensorMap";
            dialog.DefaultExt = ".sm";
            dialog.Filter = "SensorMap (.sm)|*.sm";
            if (File.Exists(_file))
                dialog.DefaultDirectory = System.IO.Path.GetDirectoryName(_file);

            if (dialog.ShowDialog() == true)
            {
                string jsonString = File.ReadAllText(dialog.FileName);
                SensorMap sm = SensorMap.FromJSON(jsonString);

                //set ROI
                roi_left = sm.roi_left_up_right_bottom.left;
                roi_top = sm.roi_left_up_right_bottom.top;
                roi_right = sm.roi_left_up_right_bottom.right;
                roi_bottom = sm.roi_left_up_right_bottom.bottom;

                //load SensorMap
                sensorMap = sm;
            }
        }

        void SensorMap_Save_Click(object sender, MouseButtonEventArgs e)
        {
            SensorMap sm = new SensorMap();
            sm.DefineROI((int)roi_left, (int)roi_top, (int)roi_right, (int)roi_bottom);
            foreach (Cercle c in cercles)
            {
                //Sensor s = new Sensor(c);
                sm.AddSensor(c.sensor);
            }
            string jsonString = sm.ToJSON();

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "SensorMap";
            dialog.DefaultExt = ".sm";
            dialog.Filter = "SensorMap (.sm)|*.sm";

            //au même endroit que la vidéo
            if (File.Exists(_file))
                dialog.DefaultDirectory = System.IO.Path.GetDirectoryName(_file);

            if (dialog.ShowDialog() == true)
                File.WriteAllText(dialog.FileName, jsonString);
        }
        #endregion

        private void VideoFileSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectVideoFile();
        }


        private void CameraListRefresh_Click(object sender, RoutedEventArgs e)
        {
            _sp_webcam_list.Children.Clear();
            _cbx_webcam.Items.Clear();

            webcams = WebCamParameters_UC._Manager.Get_WebCams();
            int index = 0;
            foreach (DsDevice webcam in webcams)
            {
                MenuItem mi = new MenuItem();
                mi.Header = webcam.Name;
                mi.Tag = index;
                mi.Click += WebCam_Selected;
                _sp_webcam_list.Children.Add(mi);

                TextBlock tb = new TextBlock();
                tb.Text = webcam.Name;
                tb.Tag = index;
                _cbx_webcam.Items.Add(tb);

                index++;
            }
        }

        private void WebCam_Selected(object sender, RoutedEventArgs e)
        {
            int webcam_index = (int)(sender as MenuItem).Tag;
            webcam = webcams[webcam_index];
            Camera(webcam_index);
        }

        private void SortSensors_Up2Down_Click(object sender, RoutedEventArgs e)
        {
            SortSensors_Up2Down();
        }

        private void SortSensors_Up2Down_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _play = !_play;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                SortSensors_Up2Down();
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {

            }
        }

        void SortSensors_Up2Down()
        {
            foreach (Sensor s in _sensors)
            {
                s.uc.index = (-s.numero).ToString();
                s.numero = null;
            }
            Sensor.ResetSensorsOrder();
        }

        private void SensorMap_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                var image = sender as System.Windows.Controls.Image;
                System.Windows.Point p = e.GetPosition(image);

                float x = (float)(p.X / image.ActualWidth * cercles_mat.Width);
                float y = (float)(p.Y / image.ActualHeight * cercles_mat.Height);
                Point2f pointClick = new Point2f(x, y);

                Cercle cercleSelectionné = null;
                //quel cercle est concerné ?
                foreach (Cercle cercle in cercles)
                {
                    if (Point2f.Distance(pointClick, cercle.circleSegment.Center) < rayon)
                    {
                        //trouvé !
                        cercleSelectionné = cercle;
                        break;
                    }
                }
                if (cercleSelectionné == null) return;

                cercleSelectionné.sensor.uc._Selected(true);
            }
        }










        OpenCvSharp.Rect? SelectROI()
        {
            string window_name = "Valid ROI with 'Enter' or 'Space', Cancel with 'c'";
            OpenCvSharp.Rect? newroi = Cv2.SelectROI(window_name, frame, true);
            if (((OpenCvSharp.Rect)newroi).Width == 0 || ((OpenCvSharp.Rect)newroi).Height == 0)
                newroi = null;
            Cv2.DestroyWindow(window_name);
            return newroi;
        }


        private void _cbx_webcam_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            _webcam_format = e.AddedItems[0] as Format;
            CameraFormatSelected();
        }

        private void CameraFormatSelected()
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
                    capVideo.Read(im);
                tentative++;
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
            Rect? roi;
            try
            {
                roi = SelectROI();
            }
            catch (Exception ex)
            {
                throw;
            }

            if (roi == null) return;

            try
            {
                ROI_New((Rect)roi);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        void ROI_New(Rect roi)
        {
            ROI_UC roi_uc = new ROI_UC(roi);
            if (!config.rois.Contains(roi))
                config.rois.Add(roi);

            _rois.Add(roi_uc);
            TextBlock tb = new TextBlock();
            tb.Text = roi_uc._name;
            tb.Tag = roi_uc;
            tb.MouseDown += roi_selected;
            _lbx_rois.Items.Add(tb);
        }

        void roi_selected(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var tb = sender as TextBlock;
                ROI_UC roi_uc = tb.Tag as ROI_UC;
                config.rois.Remove(roi_uc._roi);
                _rois.Remove(roi_uc);
                _lbx_rois.Items.Remove(sender);
            }
        }

        void UpdateROIS(Mat frame)
        {
            foreach (ROI_UC roi in _rois)
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

                //SensorsUpdate(G.Width, G.Height);

                roi.Show(roi_frame);
            }

            _title = cercles.Count.ToString();
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
                    if (tb.Text == webcam.Name)
                    {
                        camerapresente = true;
                        this.webcam = webcam;
                        webcam_index = (int)tb.Tag;
                    }
                }

                if (!camerapresente)
                {
                    MessageBox.Show("Caméra demandée non présente :\n" + config.webcam_name);
                    return;
                }

                CameraSelected();

                FormatForce();

                //appliqués les réglages caméra
                WebCamParameters_UC._Manager.Set_WebCamConfig(config.webcam_name, config.webcam_parameters);

                //ROIs
                foreach (Rect roi in config.rois)
                    ROI_New(roi);

                //quelles sont les coordonnées et taille des sensors ?


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
    }
}