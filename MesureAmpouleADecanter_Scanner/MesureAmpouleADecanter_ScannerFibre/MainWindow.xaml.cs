using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MesureAmpouleADecanter_ScannerFibre.Images;
using MesureAmpouleADecanter_ScannerFibre.Properties;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using static System.Net.Mime.MediaTypeNames;

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

        public ObservableCollection<Sensor> _sensors { get; set; } = new ObservableCollection<Sensor>();

        #region Parameters local
        Mat frame;
        Mat ROI_mat;
        Mat cercles_mat = new Mat();
        Mat scan_mat = new Mat(100, 100, MatType.CV_8UC3);
        int scan_mat_column = 0;

        List<Cercle> cercles;
        bool circle_Recompute;
        VideoCapture capVideo;
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
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayCrop(false);
            DisplayHoughCircle(false);
            ProcessFile(file);
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

            Dispatcher.BeginInvoke(() =>
            {
                //Title = left + " ↔ " + right + "\t" + top + " ↨ " + bottom;
                Title = _houghcircle_dp.ToString("0.00") + "\t" + _houghcircle_param1.ToString("0.00") + "\t" + _houghcircle_param2.ToString("0.00");
            });

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

            capVideo = new VideoCapture(filePath);
            sleepTime = (int)Math.Round(1000 / capVideo.Fps);
            OnPropertyChanged("_videoTotalFrames");

            CancellationToken token = cancellationTokenSource.Token;
            Task.Factory.StartNew(() => PlayVideo(token), token);
        }

        void VideoStop()
        {
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

            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                lv_sensors.Items.Clear();
            });
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
            while (loop)
            {
                //init
                capVideo.PosFrames = 0;
                CirclesReset();

                while (!capVideo.IsDisposed &&
                    !cancellationToken.IsCancellationRequested)
                {
                    frame = new Mat();

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



                SensorsAdd(circleSegments, ROI_mat.Width, ROI_mat.Height);

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

        void SensorsAdd(CircleSegment[] newCircleSegments, int width, int height)
        {
            if (cercles_mat.Empty())
                cercles_mat = new Mat(height, width, MatType.CV_8UC3);

            //pour chaque nouveau cercle
            for (int i = 0; i < newCircleSegments.Length; i++)
            {
                try
                {
                    Point2f center = newCircleSegments[i].Center;
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

            //2000
            int offsetY = (int)(36 * size / 2000); //5 pour 415 ; 30 pour 2000
            int offsetX = (int)(35 * size / 2000); //5 pour 415 ; 28 pour 2000
            int thickness = (int)(6 * size / 2000);//1 pour 415 ;  6 pour 2000
            if (thickness < 1) thickness = 1;

            // return;

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
                string text = c.numero.ToString();
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
                scan_mat.At<Vec3b>(s.numero, scan_mat_column) = new Vec3b(s.pixelValue.Item0, s.pixelValue.Item1, s.pixelValue.Item2);
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

        #region IHM Click
        void OpenVideoFile_Click(object sender, MouseButtonEventArgs e)
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

        void btn_pause_Click(object sender, MouseButtonEventArgs e)
        {
            _play = true;
            PlayPauseButtonUpdate();
        }

        void btn_play_Click(object sender, MouseButtonEventArgs e)
        {
            _play = false;
            PlayPauseButtonUpdate();
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
                PlayPauseButtonUpdate();
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

        void PlayPauseButtonUpdate()
        {
            btn_pause.Visibility = (_play) ? Visibility.Hidden : Visibility.Visible;
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
    }
}