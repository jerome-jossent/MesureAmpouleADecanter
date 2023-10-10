using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
using Communication_Série;
using System.IO.Ports;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using OpenCVSharpJJ.Processing;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using ScottPlot;
using System.Windows.Forms;
using System.Collections.Specialized;
using Xceed.Wpf.AvalonDock.Layout;
using System.Reflection;
using System.Security.Cryptography;
using SharpDX.MediaFoundation;
using ScottPlot.Renderable;
using ScottPlot.Plottable;
using OpenCvSharp.Flann;

namespace OpenCVSharpJJ
{
    public partial class MesureAmpouleADecanter : System.Windows.Window, INotifyPropertyChanged
    {
        #region CLASSES
        public class NamedMat
        {
            public Mat mat;
            public ImageType imageType;
            public NamedMat(ImageType imageType)
            {
                this.imageType = imageType;
                //  /!\     si erreur dll OpenCV : dans les propriétés du projet, décocher "Préférer 32 bits"
                mat = new Mat();
            }
        }

        public class NamedMats
        {
            public Dictionary<ImageType, NamedMat> MatNamesToMats = new Dictionary<ImageType, NamedMat>();
            internal NamedMat Get(ImageType imageType)
            {
                if (!MatNamesToMats.ContainsKey(imageType))
                    MatNamesToMats.Add(imageType, new NamedMat(imageType));
                return MatNamesToMats[imageType];
            }
        }

        public class PointJJ
        {
            public DateTime T { get; set; }
            public double t { get; set; }
            public string T_string
            {
                get
                {
                    string chaine = T.ToString("yyyy/MM/dd HH:mm:ss.fff");
                    return chaine;
                }
            }
            public float z_mm { get; set; }

            public int erreur_pixel { get; set; }

            public PointJJ(DateTime T, float z_mm, int erreur_pixel)
            {
                this.T = T;
                this.z_mm = z_mm;
                this.erreur_pixel = erreur_pixel;
                this.t = (T - MesureAmpouleADecanter.t0).TotalSeconds;
            }
            public PointJJ(float z_mm, int erreur_pixel)
            {
                T = DateTime.Now;
                this.z_mm = z_mm;
                this.erreur_pixel = erreur_pixel;
            }

            public override string ToString()
            {
                return t.ToString().Replace(",", ".") + "," +
                    z_mm + "," +
                    "," +
                    T_string + "," +
                    erreur_pixel;
            }
        }
        #endregion

        #region ENUMERATIONS
        public enum ImageType
        {
            none,
            original,
            resized,
            rotated,
            roi1, roi2,
            gray1, gray2,
            bw1, bw2,
            canny,
            bgr1, bgr2,
            debug1, debug2, debug3, debug4, debug5,
            graph1, graph2,
            frame_clone
        }

        enum Calque { all, red, green, blue }
        #endregion

        #region VARIABLES & cie
        #region BINDINGS IHM
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //title + fps
        public string _title
        {
            get { return title + _fps; }
            set
            {
                if (title == value)
                    return;
                title = value;
                OnPropertyChanged("_title");
            }
        }
        string title = "Mesure décantation";

        public string _fps
        {
            get { return fps; }
            set
            {
                if (fps == value)
                    return;
                fps = value;
                OnPropertyChanged("_title");
            }
        }
        string fps;


        //update bande_morte_pix quand             configuration.bande_morte_mm change





        //public int Threshold1
        //{
        //    get { return threshold1; }
        //    set
        //    {
        //        if (threshold1 == value)
        //            return;
        //        threshold1 = value;

        //        ROI_AutoAdjustH(value);

        //        OnPropertyChanged("Threshold1");
        //        if (!captureVideoIsRunning)
        //            ComputePicture(frame.mat);
        //    }
        //}
        //int threshold1 = 350;

        //public int Threshold2
        //{
        //    get { return threshold2; }
        //    set
        //    {
        //        if (threshold2 == value)
        //            return;
        //        threshold2 = value;
        //        OnPropertyChanged("Threshold2");
        //        if (!captureVideoIsRunning)
        //            ComputePicture(frame.mat);
        //    }
        //}
        //int threshold2 = 10;

        public int rotatedframe_width { get { return rotated.mat.Width; } }

        public int rotatedframe_height { get { return rotated.mat.Height; } }

        public int roi_x
        {
            get { return _roi_x; }
            set
            {
                if (_roi_x == value)
                    return;
                _roi_x = value;
                OnPropertyChanged();
                ROI_Change();
                if (!captureVideoIsRunning)
                    ComputePicture(frame_clone.mat);
            }
        }
        int _roi_x;

        public int roi_y
        {
            get { return _roi_y; }
            set
            {
                if (_roi_y == value)
                    return;
                _roi_y = value;
                OnPropertyChanged();
                ROI_Change();
                if (!captureVideoIsRunning)
                    ComputePicture(frame_clone.mat);
            }
        }
        int _roi_y;

        public int roi_xw
        {
            get { return _roi_xw; }
            set
            {
                if (_roi_xw == value)
                    return;
                _roi_xw = value;
                OnPropertyChanged();
                ROI_Change();
                if (!captureVideoIsRunning)
                    ComputePicture(frame_clone.mat);
            }
        }
        int _roi_xw;

        public int roi_yh
        {
            get { return _roi_yh; }
            set
            {
                if (_roi_yh == value)
                    return;
                _roi_yh = value;
                OnPropertyChanged();
                ROI_Change();
                if (!captureVideoIsRunning)
                    ComputePicture(frame_clone.mat);
            }
        }
        int _roi_yh;

        public int savedImageCount
        {
            get { return _savedImageCount; }
            set
            {
                if (_savedImageCount == value)
                    return;
                _savedImageCount = value;
                OnPropertyChanged();
            }
        }
        int _savedImageCount = 0;

        public System.Drawing.Bitmap _imageSource
        {
            get
            {
                if (imageSource == null)
                    return null;
                return imageSource;
            }
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    OnPropertyChanged();
                }
            }
        }
        System.Drawing.Bitmap imageSource;

        public System.Drawing.Bitmap _imageCalque
        {
            get
            {
                if (imageCalque == null)
                    return null;
                return imageCalque;
            }
            set
            {
                if (imageCalque != value)
                {
                    imageCalque = value;
                    OnPropertyChanged();
                }
            }
        }
        System.Drawing.Bitmap imageCalque;

        public System.Drawing.Bitmap _image1
        {
            get
            {
                if (image1 == null)
                    return null;
                return image1;
            }
            set
            {
                if (image1 != value)
                {
                    image1 = value;
                    OnPropertyChanged();
                }
            }
        }
        System.Drawing.Bitmap image1;

        public System.Drawing.Bitmap _image2
        {
            get
            {
                if (image2 == null)
                    return null;
                return image2;
            }
            set
            {
                if (image2 != value)
                {
                    image2 = value;
                    OnPropertyChanged();
                }
            }
        }
        System.Drawing.Bitmap image2;

        public System.Drawing.Bitmap _image3
        {
            get
            {
                if (image3 == null)
                    return null;
                return image3;
            }
            set
            {
                if (image3 != value)
                {
                    image3 = value;
                    OnPropertyChanged();
                }
            }
        }
        System.Drawing.Bitmap image3;

        public ObservableCollection<string> arduinoMessages
        {
            get
            {
                return _arduinoMessages;
            }
            set
            {
                _arduinoMessages = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<string> _arduinoMessages = new ObservableCollection<string>();

        public ObservableCollection<PointJJ> _points { get => points; set => points = value; }
        ObservableCollection<PointJJ> points = new ObservableCollection<PointJJ>();

        #endregion

        #region VARIABLES GLOBALES

        List<LayoutAnchorable> views;

        string data_filename;
        public static DateTime t0;
        public int _pointsMax { get; set; }
        int nbrLignes;
        int nbrPixel_par_ligne;
        int nbrLignes_prec;
        int nbrPixel_par_ligne_prec;
        float[] moyennesX;
        float[] d1; //derivée première
        float[] d2; //derivée seconde

        List<int> ds_pix = new List<int>();
        int points_par_z_max_ds_pix_nbr = 9;
        bool newTarget = false;

        public int distancepixel
        {
            get { return _distancepixel; }
            set
            {
                if (_distancepixel == value)
                    return;
                _distancepixel = value;
                OnPropertyChanged();
            }
        }
        int _distancepixel = -234;

        public float deplacement_mm_commande
        {
            get { return _deplacement_mm_commande; }
            set
            {
                if (_deplacement_mm_commande == value)
                    return;
                _deplacement_mm_commande = value;
                OnPropertyChanged();
            }
        }
        float _deplacement_mm_commande = 123;

        DateTime t_last_save;
        TimeSpan t_vide_save = TimeSpan.FromSeconds(1);

        DateTime t_last;
        TimeSpan t_vide = TimeSpan.FromSeconds(0.1);
        PointJJ lastPoint;

        bool firstFileToSave = true;
        string savedImagesPath;


        bool etalonnage_fin_haut;
        #endregion

        #region PARAMETERS
        bool configurationLoading;
        public Configuration configuration
        {
            get { return _configuration; }
            set
            {
                if (_configuration == value)
                    return;
                _configuration = value;
                OnPropertyChanged();
            }
        }
        Configuration _configuration = new Configuration();

        bool arduinoWaiting;

        long T0;
        NamedMats NMs;
        NamedMat none = new NamedMat(ImageType.none);
        NamedMat frame = new NamedMat(ImageType.original);
        NamedMat frame_clone = new NamedMat(ImageType.original);
        NamedMat resized = new NamedMat(ImageType.resized);
        NamedMat rotated = new NamedMat(ImageType.rotated);
        NamedMat frameGray = new NamedMat(ImageType.gray1);
        NamedMat cannymat = new NamedMat(ImageType.canny);
        NamedMat BGR = new NamedMat(ImageType.bgr1);
        NamedMat ROI1 = new NamedMat(ImageType.roi1);
        NamedMat ROI2 = new NamedMat(ImageType.roi2);
        NamedMat debug1 = new NamedMat(ImageType.debug1);
        NamedMat debug2 = new NamedMat(ImageType.debug2);
        NamedMat debug3 = new NamedMat(ImageType.debug3);
        NamedMat debug4 = new NamedMat(ImageType.debug4);
        NamedMat bw1 = new NamedMat(ImageType.bw1);
        NamedMat bw2 = new NamedMat(ImageType.bw2);
        NamedMat graph1 = new NamedMat(ImageType.graph1);
        NamedMat graph2 = new NamedMat(ImageType.graph2);
        //Mat[] bgr;

        OpenCvSharp.Rect roi
        {
            get { return _roi; }
            set
            {
                if (_roi == value)
                    return;
                _roi = value;
                configuration.roi = new System.Windows.Rect(roi.X, roi.Y, roi.Width, roi.Height);
                OnPropertyChanged();
                ROI_Change();
            }
        }
        OpenCvSharp.Rect _roi;

        Thread threadCaptureVideo;
        Thread threadAlgo;
        Thread threadCommandeArduino;

        Dictionary<string, VideoInInfo.Format> formats;
        VideoCapture capture;
        bool captureVideoIsRunning = false;
        bool captureCamera_first = true;
        VideoInInfo.Format camera_encodage_resolution_precedent = null;
        System.Diagnostics.Stopwatch mesureFPS = new System.Diagnostics.Stopwatch();

        bool algoIsRunning = false;
        bool algoStandBy = false;
        object takeFrame = new object();

        Communication_Série.Communication_Série cs;
        string buffer;
        char[] split_car = new char[] { '\n' };
        float? camera_pos_mm;
        float? camera_pos_codeur;
        float camera_pos_max;

        bool cameraDisplacement_Running = false;

        bool camera_pos_low_switch;
        bool camera_pos_high_switch;

        OpenCvSharp.Window dislpay_debug;
        //OpenCvSharp.Window w;
        //bool display_in_OpenCVSharpWindow = false;
        #endregion

        #region COULEURS (OpenCV)
        Scalar rouge = new Scalar(0, 0, 255);
        Scalar magenta = new Scalar(255, 0, 150);
        Scalar vert = new Scalar(0, 255, 0);
        Scalar turquoise = new Scalar(255, 255, 0);
        Scalar bleu = new Scalar(255, 100, 0);
        Scalar blanc = new Scalar(255, 255, 255, 255);
        Scalar gris = new Scalar(128, 128, 128);
        Scalar noir = new Scalar(0, 0, 0);
        #endregion
        #endregion

        #region WINDOW MANAGEMENT
        public MesureAmpouleADecanter()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            t0 = DateTime.Now;

            Camera_Init();
            Arduino_Init();
            ImageProcessing_Init();
            CameraDisplacement_Init();

            _pointsMax = 1000;
            DateTime T = DateTime.Now;

            LayoutLoadButton_Click(null, null);

            //focus FORCé sur le dernier message Arduino
            ((System.Collections.Specialized.INotifyCollectionChanged)lbx_arduino_received.Items).CollectionChanged +=
       lbx_arduino_received_CollectionChanged;
            //((System.Collections.Specialized.INotifyCollectionChanged)lbx_arduino_received.ItemsSource).CollectionChanged += (s, e2) =>
            //{
            //    if (e2.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            //        lbx_arduino_received.SelectedItem = lbx_arduino_received.Items[lbx_arduino_received.Items.Count - 1];
            //    //lbx_arduino_received.ScrollIntoView(lbx_arduino_received.Items[lbx_arduino_received.Items.Count - 1]);
            //};

            //focus FORCé sur la dernière donnée enregistrée
            ((System.Collections.Specialized.INotifyCollectionChanged)items_data.ItemsSource).CollectionChanged += (s, e2) =>
            {
                if (e2.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    scv_data.ScrollToBottom();//.ScrollIntoView(items_data.Items[items_data.Items.Count - 1]);
            };

            this.WindowState = WindowState.Maximized;

            Debug_graph_INIT();
        }

        private void lbx_arduino_received_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lbx_arduino_received.SelectedIndex = lbx_arduino_received.Items.Count - 1;
            lbx_arduino_received.ScrollIntoView(lbx_arduino_received.SelectedItem);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            CameraDisplacement_Stop();
            Algo_Stop();
            CaptureCamera_Stop();
            cs?.PortCom_OFF();
        }

        #region AVALONDOCK
        void LayoutSaveButton_Click(object sender, RoutedEventArgs e)
        {
            XmlLayoutSerializer layoutSerializer = new XmlLayoutSerializer(DManager);
            using (var writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "docks.txt"))
            {
                layoutSerializer.Serialize(writer);
            }
        }

        void LayoutLoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XmlLayoutSerializer layoutSerializer = new XmlLayoutSerializer(DManager);
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

            views = new List<LayoutAnchorable>() {
                layoutAnchorable_algorythn_parameters,
                layoutAnchorable_arduino_config,
                layoutAnchorable_arduino_read,
                layoutAnchorable_augmented_display,
                layoutAnchorable_camera_config,
                layoutAnchorable_data_points,
                layoutAnchorable_graph1,
                layoutAnchorable_graph2,
                layoutAnchorable_image_process,
                layoutAnchorable_system_paremeters,
                layoutAnchorable_view1,
                layoutAnchorable_view2,
                layoutAnchorable_view3,
                layoutAnchorable_view4
            };
        }

        private void LayoutShowButton_Click(object sender, RoutedEventArgs e)
        {
            //foreach (var child in Avalon_Views.Children)
            //{
            //    if (child == null) continue;
            //    LayoutAnchorable la = (LayoutAnchorable)child;
            //    if (la.IsHidden)
            //        la.Show();
            //}

            //foreach (LayoutAnchorable la in views)
            //{
            //    if (la!= null)
            //    {
            //        try
            //        {
            //            la.Float();

            //        }
            //        catch (Exception ex)
            //        {

            //        }

            //        //Xceed.Wpf.AvalonDock.Layout.LayoutAnchorableFloatingWindow lfw = new Xceed.Wpf.AvalonDock.Layout.LayoutAnchorableFloatingWindow();
            //        //Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePane lap3 = new Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePane(la);
            //        //lap3.Parent = lfw;
            //        //lfw.RootPanel = new Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePaneGroup(lap3);
            //        //lfw.Parent = DManager.Layout;
            //        //DManager.Layout.FloatingWindows.Add(lfw);
            //    }
            //    //bool dansihm = false;
            //    //foreach (LayoutContent lc in DManager.Layout.Descendents().OfType<LayoutContent>())
            //    //{
            //    //    LayoutAnchorable la2 = lc as LayoutAnchorable;
            //    //    dansihm = (la2.Title == la.Title);
            //    //    if (dansihm) break;

            //    //}
            //    continue;
            //    //if (!dansihm)
            //    //{
            //    //    //la.Hide();
            //    //    //la.Show();
            //    //    la.AddToLayout(DManager, AnchorableShowStrategy.Most);
            //    //    //la.Dock();
            //    //    //la.Float();

            //    //    break;
            //    //    //ajout à l'IHM
            //    //    Xceed.Wpf.AvalonDock.Layout.LayoutAnchorableFloatingWindow lfw = new Xceed.Wpf.AvalonDock.Layout.LayoutAnchorableFloatingWindow();
            //    //    Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePane lap3 = new Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePane(la);
            //    //    lap3.Parent = lfw;
            //    //    lfw.RootPanel = new Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePaneGroup(lap3);
            //    //    lfw.Parent = DManager.Layout;
            //    //    DManager.Layout.FloatingWindows.Add(lfw);
            //    //}
            //}
        }
        #endregion
        #endregion

        #region CONFIGURATION
        void Configuration_Save_Click(object sender, RoutedEventArgs e)
        {
            configuration.Save();
        }

        void Configuration_Load_Click(object sender, RoutedEventArgs e)
        {
            configurationLoading = true;
            configuration = Configuration.Load();

            try
            {
                cbx_COM.Text = configuration.deplacement_portCOM;
                cbx_bauds.Text = configuration.deplacement_bauds;

                cbx_device.SelectedIndex = configuration.camera_index;
                cbx_deviceFormat.Text = configuration.camera_encodage_resolution?.Name;

                LoadROI();
            }
            catch (Exception)
            {
                throw;
            }
            configurationLoading = false;
        }

        private void Configuration_Load_Start_Click(object sender, RoutedEventArgs e)
        {
            Configuration_Load_Click(null, null);
            Algo_Start();
            Capture_Start();
            Arduino_Connexion();
        }
        #endregion

        #region CAMERA, ALGO & IMAGE
        #region CAPTURE DEVICE
        void Camera_Init()
        {
            ListDevices();
            Capture_Button_Update();
        }

        void Button_ListDevices_Click(object sender, MouseButtonEventArgs e)
        {
            ListDevices();
        }

        void ListDevices()
        {
            var devices = VideoInInfo.EnumerateVideoDevices_JJ();
            if (cbx_device != null)
                cbx_device.ItemsSource = devices.Select(d => d.Name).ToList();
        }

        void Button_CaptureDevice_Click(object sender, MouseButtonEventArgs e)
        {
            Algo_Start();
            Capture_Start();
        }

        void Capture_Start()
        {
            mesureFPS.Start();
            captureVideoIsRunning = !captureVideoIsRunning;

            if (captureVideoIsRunning)
            {
                configuration.camera_index = cbx_device.SelectedIndex;
                CaptureCamera(configuration.camera_index);
            }
            else
            {
                CaptureCamera_Stop();
            }
            Capture_Button_Update();
        }

        void Capture_Button_Update()
        {
            if (captureVideoIsRunning)
            {
                Button_CaptureDevicePlay.Visibility = Visibility.Collapsed;
                Button_CaptureDeviceStop.Visibility = Visibility.Visible;
            }
            else
            {
                Button_CaptureDevicePlay.Visibility = Visibility.Visible;
                Button_CaptureDeviceStop.Visibility = Visibility.Collapsed;
            }
        }

        void Combobox_CaptureDevice_Change(object sender, SelectionChangedEventArgs e)
        {
            configuration.camera_index = cbx_device.SelectedIndex;
            formats = VideoInInfo.EnumerateSupportedFormats_JJ(configuration.camera_index);
            cbx_deviceFormat.ItemsSource = formats.OrderBy(f => f.Value.format).ThenByDescending(f => f.Value.w).Select(f => f.Key);
        }

        void Combobox_CaptureDeviceFormat_Change(object sender, SelectionChangedEventArgs e)
        {
            if (configurationLoading) return;

            if (cbx_deviceFormat.SelectedValue != null)
                configuration.camera_encodage_resolution = formats[cbx_deviceFormat.SelectedValue as string];
        }
        #endregion

        #region CAMERA MANAGEMENT
        void CaptureCamera(int index)
        {
            if (threadCaptureVideo != null && threadCaptureVideo.IsAlive)
            {
                CaptureCamera_Stop();
                Thread.Sleep(100);
            }
            configuration.camera_index = index;
            threadCaptureVideo = new Thread(new ThreadStart(CaptureCamera_WithOpenCV));
            threadCaptureVideo.Start();
        }

        void CaptureCamera_Stop()
        {
            captureVideoIsRunning = false;
            Thread.Sleep(100);
            threadCaptureVideo?.Abort();
            captureCamera_first = true;
            threadCaptureVideo = null;
        }

        //CAPTURE VIDEO (OPENCV)
        void CaptureCamera_WithOpenCV()
        {
            int actualindexDevice = configuration.camera_index;
            frame.mat = new Mat();
            capture = new VideoCapture(configuration.camera_index);
            capture.Open(configuration.camera_index, VideoCaptureAPIs.DSHOW);

            if (capture.IsOpened())
            {
                while (captureVideoIsRunning)
                {
                    //si changement de camera
                    if (configuration.camera_index != actualindexDevice)
                    {
                        capture.Open(configuration.camera_index, VideoCaptureAPIs.DSHOW);
                        actualindexDevice = configuration.camera_index;
                    }

                    //si changement de format vidéo
                    if (configuration.camera_encodage_resolution != camera_encodage_resolution_precedent)
                    {
                        capture.Set(VideoCaptureProperties.FrameWidth, configuration.camera_encodage_resolution.w);
                        capture.Set(VideoCaptureProperties.FrameHeight, configuration.camera_encodage_resolution.h);
                        capture.Set(VideoCaptureProperties.Fps, configuration.camera_encodage_resolution.fr);
                        capture.Set(VideoCaptureProperties.FourCC, FourCC.FromString(configuration.camera_encodage_resolution.format));
                        camera_encodage_resolution_precedent = configuration.camera_encodage_resolution;
                    }

                    if (captureCamera_first)
                    {
                        //Display_Init();
                        MatNamesToMats_Reset();
                        roi = new OpenCvSharp.Rect();
                        captureCamera_first = false;

                        for (int i = 0; i < 3; i++)
                            capture.Read(frame.mat);
                    }

                    //captation de l'image
                    capture.Read(frame.mat);

                    OnPropertyChanged("rotatedframe_width");
                    OnPropertyChanged("rotatedframe_height");

                    //traitement de l'image
                    //ComputePicture(frame.mat);
                    if (algoStandBy)
                        frame_clone.mat = frame.mat.Clone();

                    ////viewer debug
                    //if (display_in_OpenCVSharpWindow)
                    //{
                    //    if (w != null && frame.mat.Empty())
                    //        Cv2.DestroyWindow(w.Name);
                    //    else
                    //    {
                    //        if (!frame.mat.Empty())
                    //        {
                    //            if (w == null)
                    //                w = new OpenCvSharp.Window();
                    //            w.ShowImage(frame.mat);
                    //            Cv2.WaitKey(1);
                    //        }
                    //    }
                    //}

                    System.GC.Collect();
                }
                capture.Dispose();
            }
        }
        #endregion

        void Algo_Start()
        {
            if (threadAlgo != null && threadAlgo.IsAlive)
            {
                Algo_Stop();
                Thread.Sleep(100);
            }
            threadAlgo = new Thread(new ThreadStart(Algo));
            threadAlgo.Start();
        }

        void Algo_Stop()
        {
            algoIsRunning = false;
            Thread.Sleep(100);
            threadAlgo?.Abort();
            threadAlgo = null;
        }

        void Algo()
        {
            algoStandBy = true;
            algoIsRunning = true;
            while (algoIsRunning)
            {
                lock (takeFrame)
                {
                    if (frame_clone.mat != null && !frame_clone.mat.Empty())
                    {
                        algoStandBy = false;
                        ComputePicture(frame_clone.mat);
                        frame_clone.mat = null;
                        algoStandBy = true;
                    }
                    else
                        Thread.Sleep(10);
                }
            }
        }


        #region IMAGE PROCESSINGS
        void ImageProcessing_Init()
        {
            MatNamesToMats_Reset();

            i1._UpdateCombobox(NMs);
            i2._UpdateCombobox(NMs);
            i3._UpdateCombobox(NMs);
            i4._UpdateCombobox(NMs);

            int i = 1;
            i1.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i1.matName = ImageType.graph1;
            i2.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i2.matName = ImageType.roi1;
            i3.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i3.matName = ImageType.rotated;
            i4.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i4.matName = ImageType.none;
        }

        void MatNamesToMats_Reset()
        {
            NMs = new NamedMats();
            NMs.MatNamesToMats.Add(ImageType.none, none);
            NMs.MatNamesToMats.Add(ImageType.original, frame);
            NMs.MatNamesToMats.Add(ImageType.resized, resized);
            NMs.MatNamesToMats.Add(ImageType.rotated, rotated);
            NMs.MatNamesToMats.Add(ImageType.roi1, ROI1);
            NMs.MatNamesToMats.Add(ImageType.roi2, ROI2);
            NMs.MatNamesToMats.Add(ImageType.gray1, frameGray);
            NMs.MatNamesToMats.Add(ImageType.canny, cannymat);
            NMs.MatNamesToMats.Add(ImageType.bw1, bw1);
            NMs.MatNamesToMats.Add(ImageType.bw2, bw2);
            NMs.MatNamesToMats.Add(ImageType.bgr1, BGR);
            NMs.MatNamesToMats.Add(ImageType.debug1, debug1);
            NMs.MatNamesToMats.Add(ImageType.debug2, debug2);
            NMs.MatNamesToMats.Add(ImageType.debug3, debug3);
            NMs.MatNamesToMats.Add(ImageType.debug4, debug4);
            NMs.MatNamesToMats.Add(ImageType.graph1, graph1);
            NMs.MatNamesToMats.Add(ImageType.graph2, graph2);
            NMs.MatNamesToMats.Add(ImageType.frame_clone, frame_clone);
        }

        #region ROI
        void Button_CaptureDeviceROI_Click(object sender, RoutedEventArgs e)
        {
            string window_name = "Valid ROI with 'Enter' or 'Space', cancel with 'c'";

            if (frame_clone.mat.Empty())
                return;

            resized.mat = Resize(frame_clone.mat);
            rotated.mat = Rotation(resized.mat, configuration.rotation_angle);

            OpenCvSharp.Rect newroi;
            newroi = Cv2.SelectROI(window_name, rotated.mat, true);
            //newroi = new OpenCvSharp.Rect(884,81,66,884);

            roi = newroi;

            tbx_roi.Text = ROIToString();
            _title = roi.ToString();
            Cv2.DestroyWindow(window_name);
        }

        void ROI_Change()
        {
            int x, y, w, h;
            y = roi_y;
            h = roi_yh - y;

            x = roi_x;
            w = roi_xw - x;
            roi = new OpenCvSharp.Rect(x, y, w, h);
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                tbx_roi.Text = ROIToString();
            }));
        }

        string ROIToString()
        {
            return roi.X + "|" + roi.Y + "|" + roi.Width + "|" + roi.Height + "|";
        }

        void LoadROI()
        {
            int x = (int)configuration.roi.X;
            int y = (int)configuration.roi.Y;
            int w = (int)configuration.roi.Width;
            int h = (int)configuration.roi.Height;

            roi_x = 0;
            roi_xw = 0;
            roi_y = 0;
            roi_yh = 0;

            roi_xw = x + w;
            roi_x = x;
            roi_yh = y + h;
            roi_y = y;
        }

        void Button_SetROI_Click(object sender, RoutedEventArgs e)
        {
            string roi_s = tbx_roi.Text;
            try
            {
                string[] param = roi_s.Split('|');
                int x = int.Parse(param[0]);
                int y = int.Parse(param[1]);
                int w = int.Parse(param[2]);
                int h = int.Parse(param[3]);

                roi_x = 0;
                roi_xw = 0;
                roi_y = 0;
                roi_yh = 0;

                roi_xw = x + w;
                roi_x = x;
                roi_yh = y + h;
                roi_y = y;
            }
            catch (Exception ex)
            {

            }
        }

        void Button_CaptureDeviceROI_None_Click(object sender, RoutedEventArgs e)
        {
            roi_x = 0;
            roi_xw = rotatedframe_width;
            roi_y = 0;
            roi_yh = rotatedframe_height;
        }

        void ROI_AutoAdjustH(int hauteur_camera_mm)     // 2022/07/30 Dmax = 334.58mm
        {
            //float d_cam_objet_mm = 200;
            //double fov = 85 * Math.PI / 180;
            //float alpha = (float)fov / 2;

            ////int hauteur_camera_mm = 237; //exemple (position live)
            //int hauteur_camera_mm_max = 490; //exemple (calibration)
            //int hauteur_camera_mm_min = 150; //exemple (calibration)

            ////int keepX = roi.X;
            ////int keepW = roi.Width;

            ////en fonction de la hauteur on va retirer de la ROI les zones extrêmes
            //float d_cam_bordimage_mm = d_cam_objet_mm * (float)Math.Tan(alpha);

            //if (hauteur_camera_mm + d_cam_bordimage_mm > hauteur_camera_mm_max)
            //{
            //    //on doit rogner le haut : Y
            //    float d_mm = hauteur_camera_mm + d_cam_bordimage_mm - hauteur_camera_mm_max;
            //    int d_pix = (int)(d_mm / (float)_ratio_mm_pix);

            //    roi.Y = d_pix;
            //}
            //else
            //{
            //    //on rogne pas
            //    roi.Y = 0;
            //}

            //if (hauteur_camera_mm - d_cam_bordimage_mm < hauteur_camera_mm_min)
            //{
            //    //on doit rogner le bas : Height
            //    float d_mm = hauteur_camera_mm_min - (hauteur_camera_mm - d_cam_bordimage_mm);
            //    int d_pix = (int)(d_mm / (float)_ratio_mm_pix);

            //    roi.Height = rotated.mat.Height - d_pix;
            //}
            //else
            //{
            //    //on rogne pas
            //    roi.Height = rotated.mat.Height;
            //}
        }
        #endregion

        void FrameProcessing_InitData(Mat mat)
        {
            nbrLignes = mat.Height;
            nbrPixel_par_ligne = mat.Width;

            if (nbrLignes_prec != nbrLignes ||
                nbrPixel_par_ligne != nbrPixel_par_ligne_prec)
            {
                moyennesX = new float[nbrLignes];
                //moyennesX = new float[nbrLignes];
                d1 = new float[nbrLignes];
                d2 = new float[nbrLignes];

                nbrPixel_par_ligne_prec = nbrPixel_par_ligne;
                nbrLignes_prec = nbrLignes;
            }
        }

        NamedMat FrameProcessing1(Mat image)
        {
            //filtre gaussien
            //Savitzky-Golay filter     https://docs.scipy.org/doc/scipy/reference/generated/scipy.signal.savgol_filter.html

            DateTime t0 = DateTime.Now;

            resized.mat = Resize(image);

            rotated.mat = Rotation(resized.mat, configuration.rotation_angle);

            ROI1.mat = ROI_NewMat(rotated.mat, roi);

            //Display_debug(ROI1.mat);

            //gris
            frameGray.mat = RGBToGray(ROI1.mat, Calque.all);

            DateTime t1 = DateTime.Now;
            double d_preproc = (t1 - t0).TotalMilliseconds;

            //seuillage
            //frameGray.mat = frameGray.mat.Threshold(Threshold1, 255, ThresholdTypes.Binary);
            //Cv2.Canny(frameGray.mat, frameGray.mat, 50, Threshold1);

            FrameProcessing_InitData(ROI1.mat);

            //trouve niveau :
            int niveau_pixel;

            #region moyenne pixel par ligne
            Mat<byte> mat3 = new Mat<byte>(frameGray.mat);
            MatIndexer<byte> indexer = mat3.GetIndexer();
            float valmin = 0;
            float valmax = 0;
            for (int y = 0; y < nbrLignes; y++)
            {
                moyennesX[y] = 0;
                for (int x = 0; x < nbrPixel_par_ligne; x++)
                    moyennesX[y] += indexer[y, x];

                moyennesX[y] /= nbrPixel_par_ligne;

                if (y == 0)
                {
                    valmin = moyennesX[y];
                    valmax = moyennesX[y];
                }
                if (moyennesX[y] > valmax) valmax = moyennesX[y];
                if (moyennesX[y] < valmin) valmin = moyennesX[y];
            }
            #endregion

            #region // filtre gaussien TODO ?
            //https://stackoverflow.com/questions/59263100/how-to-easily-apply-a-gauss-filter-to-a-list-array-of-doubles
            #endregion

            #region // inverser les valeurs
            //if (configuration.inverser == true)
            //{
            //    for (int y = 0; y < moyennesX.Length; y++)
            //        moyennesX[y] = valmax - moyennesX[y];
            //}
            #endregion

            #region recherche dépassements de seuil
            float x_perte = (float)configuration.seuil_perte_intensite;
            float x_range = valmax - valmin;
            float seuil = (1 - x_perte) * x_range + valmin;

            ////on recherche (à partir de l'intensité max) ()en partant du haut la PREMIERE ligne pour laquelle on perd X% d'intensité
            //int niveau_pixel_debut = -1;
            //for (int y = 0; y < moyennesX.Length; y++)
            //    if (moyennesX[y] < seuil)
            //    {
            //        niveau_pixel_debut = y;
            //        break;
            //    }

            //on recherche (à partir de l'intensité max) ()en partant du bas la PREMIERE ligne pour laquelle on atteint X% d'intensité
            int niveau_pixel_fin = -1;

            //on recherche en partant du bas
            if (configuration.inverser == false)
                for (int y = moyennesX.Length - 1; y >= 0; y--)
                    if (moyennesX[y] > seuil)
                    {
                        niveau_pixel_fin = y;
                        break;
                    }

            //on recherche en partant du haut
            if (configuration.inverser == true)
            {
                //seuil = valmax - seuil;
                for (int y = 0; y < moyennesX.Length; y++)
                    if (moyennesX[y] < seuil)
                    {
                        niveau_pixel_fin = y;
                        break;
                    }
            }
            #endregion
            // moyenne entre niveau_pixel_debut et niveau_pixel_fin ???????????????
            //niveau_pixel = niveau_pixel_debut;
            niveau_pixel = niveau_pixel_fin;

            #region // filtre médian
            //int fenetre = 200; // nbr valeurs
            //float[] medianesX = Medianes(moyennesX, fenetre);
            //moyennesX = medianesX;
            #endregion

            #region // derivée primaire (simplifié : car même pas /(x2-x1))
            //float d1_min = 0;
            //float d1_max = 0;
            //int d1_max_index = 0;
            //for (int i = 1; i < nbrLignes - 2; i++)
            //{
            //    d1[i] = moyennesX[i + 1] - moyennesX[i - 1];

            //    if (i == 1)
            //    {
            //        d1_min = d1[i];
            //        d1_max = d1[i];
            //        d1_max_index = i;
            //    }
            //    else
            //    {
            //        if (d1[i] < d1_min)
            //            d1_min = d1[i];

            //        if (d1[i] > d1_max)
            //        {
            //            d1_max = d1[i];
            //            d1_max_index = i;
            //        }
            //    }
            //}
            #endregion

            #region // derivée seconde simplifié : car /(x2-x1) => /1
            ////et recherche des maximum et minimum
            //float d2_min = 0;
            //float d2_max = 0;
            //int d2_max_index = 0;
            //for (int i = 2; i < nbrLignes - 4; i++)
            //{
            //    d2[i] = d1[i + 1] - d1[i - 1];

            //    if (i == 2)
            //    {
            //        d2_min = d2[i];
            //        d2_max = d2[i];
            //        d2_max_index = i;
            //    }
            //    else
            //    {
            //        if (d2[i] < d2_min)
            //            d2_min = d2[i];

            //        if (d2[i] > d2_max)
            //        {
            //            d2_max = d2[i];
            //            d2_max_index = i;
            //        }
            //    }
            //}
            #endregion

            #region // détection de la zone d'intérêt
            ////intersections à 0 entre le minimum et le maximum de la zone d'intérêt
            //int index_prec;
            //int index;
            //float n_val;
            //string ds = "d1";

            //if (ds == "d1")
            //{
            //    n_val = d1[d1_max_index];
            //    if (n_val < 0)
            //    {
            //        index_prec = d1_max_index;
            //        index = index_prec - 1;
            //        while (d1[index] > 0)
            //        {
            //            index_prec = index;
            //            index = index_prec - 1;
            //        }
            //        niveau_pixel = index_prec;
            //    }
            //    else if (n_val > 0)
            //    {
            //        index_prec = d1_max_index;
            //        index = index_prec + 1;
            //        while (d1[index] < 0)
            //        {
            //            index_prec = index;
            //            index = index_prec + 1;
            //        }
            //        niveau_pixel = index_prec;
            //    }
            //    else
            //        niveau_pixel = d1_max_index;
            //}
            //else
            //{
            //    n_val = d2[d2_max_index];
            //    if (n_val > 0)
            //    {
            //        index_prec = d2_max_index;
            //        index = index_prec - 1;
            //        while (d2[index] > 0)
            //        {
            //            index_prec = index;
            //            index = index_prec - 1;
            //        }
            //        niveau_pixel = index_prec;
            //    }
            //    else if (n_val < 0)
            //    {
            //        index_prec = d2_max_index;
            //        index = index_prec + 1;
            //        while (d2[index] < 0)
            //        {
            //            index_prec = index;
            //            index = index_prec + 1;
            //        }
            //        niveau_pixel = index_prec;
            //    }
            //    else
            //        niveau_pixel = d2_max_index;
            //}
            #endregion

            #region SAVE
            if (configuration.saveFrame == true)
            {
                Save(rotated);
                Save(ROI1);
            }
            #endregion

            #region TRACE niveau en ligne hachée sur l'image
            int morceaux = 10;
            int dashed_line_A = 0;
            int dashed_line_Z = ROI1.mat.Width - 1;
            int dashed_line_total_length = dashed_line_Z - dashed_line_A;
            float dashed_line_length = (float)dashed_line_total_length / (morceaux * 2 - 1);
            int epaisseur = (int)(0.4 * nbrLignes / 100);
            if (epaisseur < 1) epaisseur = 1;
            for (int i = 0; i < morceaux * 2 - 1; i += 2)
            {
                int x1 = (int)(dashed_line_length * i);
                int x2 = (int)(dashed_line_length * (i + 1));
                Cv2.Line(ROI1.mat, x1, niveau_pixel, x2, niveau_pixel, bleu, epaisseur);
                Cv2.Line(frameGray.mat, x1, niveau_pixel, x2, niveau_pixel, bleu, epaisseur);
            }
            #endregion

            // Cv2.Line(graph1.mat, 0, niveau_pixel, graph1.mat.Width - 1, niveau_pixel, bleu, epaisseur);

            #region GRAPH INIT de l'image
            int largeur_graph = 255;
            graph1.mat = new Mat(nbrLignes, largeur_graph, type: MatType.CV_8UC3, noir);
            #endregion

            #region // GRAPH BLEU CLAIR
            //int y0 = (int)(-d1_min * (float)largeur_graph / (d1_max - d1_min));
            //Cv2.Line(graph1.mat, y0, 0, y0, nbrLignes - 1, turquoise, 2);

            //int y00 = (int)(-d2_min * (float)largeur_graph / (d2_max - d2_min));
            //Cv2.Line(graph1.mat, y00, 0, y00, nbrLignes - 1, magenta, 2);
            #endregion

            #region // GRAPH ROUGE série "d2"
            //for (int i = 1; i < nbrLignes; i++)
            //{
            //    int y1 = (int)((d2[i - 1] - d2_min) * (float)largeur_graph / (d2_max - d2_min));
            //    int y2 = (int)((d2[i] - d2_min) * (float)largeur_graph / (d2_max - d2_min));
            //    Cv2.Line(graph1.mat, y1, i - 1, y2, i, rouge, 1);
            //}
            #endregion

            #region // GRAPH BLEU série "d1"
            //for (int i = 1; i < nbrLignes; i++)
            //{
            //    int y1 = (int)((d1[i - 1] - d1_min) * (float)largeur_graph / (d1_max - d1_min));
            //    int y2 = (int)((d1[i] - d1_min) * (float)largeur_graph / (d1_max - d1_min));
            //    Cv2.Line(graph1.mat, y1, i - 1, y2, i, bleu, 1);
            //}
            #endregion

            #region GRAPH BLANC série "moyennes des pixels par ligne"
            for (int i = 1; i < nbrLignes; i++)
            {
                int y1 = (int)moyennesX[i - 1];
                int y2 = (int)moyennesX[i];
                Cv2.Line(graph1.mat, y1, i - 1, y2, i, blanc, 2);
            }
            //trace le seuil d'intensité
            Cv2.Line(graph1.mat, (int)seuil, 0, (int)seuil, graph1.mat.Rows, Scalar.Yellow, 4);
            #endregion

            #region // GRAPH GRIS série "médianes des pixels par ligne"
            //for (int i = 1; i < nbrLignes; i++)
            //{
            //    int y1 = (int)medianesX[i - 1];
            //    int y2 = (int)medianesX[i];
            //    Cv2.Line(graph1.mat, y1, i - 1, y2, i, gris, 1);
            //}
            #endregion

            #region dessine des lignes de repères "Grids"
            if (configuration.displayGrids == true)
            {
                //horizontales
                int n_h = 10;
                for (int i = 0; i < n_h; i++)
                {
                    int y_ = (i + 1) * rotated.mat.Rows / n_h;
                    Cv2.Line(rotated.mat, 0, y_, rotated.mat.Cols - 1, y_, blanc, 3);
                }
                //verticales
                int n_v = 10;
                for (int i = 0; i < n_v; i++)
                {
                    int x_ = (i + 1) * rotated.mat.Cols / n_v;
                    Cv2.Line(rotated.mat, x_, 0, x_, rotated.mat.Rows - 1, blanc, 3);
                }
            }
            #endregion

            #region dessine roi sur rotated
            if (configuration.displayROI == true)
                Cv2.Rectangle(rotated.mat, roi, bleu, 3);
            #endregion

            #region TRACE centre caméra (mire+)
            int milieuhauteur = rotated.mat.Height / 2;
            int milieulargeur = rotated.mat.Width / 2;
            Cv2.Line(rotated.mat, milieulargeur - 50, milieuhauteur, milieulargeur + 50, milieuhauteur, rouge, 2);
            Cv2.Line(rotated.mat, milieulargeur, milieuhauteur - 50, milieulargeur, milieuhauteur + 50, rouge, 2);
            #endregion

            #region dessine des lignes de repères
            if (configuration.displayGrids == true)
            {
                //horizontales
                int n_h = 10;
                for (int i = 0; i < n_h; i++)
                {
                    int y_ = (i + 1) * rotated.mat.Rows / n_h;
                    Cv2.Line(rotated.mat, 0, y_, rotated.mat.Cols - 1, y_, blanc, 3);
                }
                //verticales
                int n_v = 10;
                for (int i = 0; i < n_v; i++)
                {
                    int x_ = (i + 1) * rotated.mat.Cols / n_v;
                    Cv2.Line(rotated.mat, x_, 0, x_, rotated.mat.Rows - 1, blanc, 3);
                }
            }
            #endregion

            #region dessine la bande morte
            if (configuration.deadBand == true)
            {
                Cv2.Line(ROI1.mat, 0, milieuhauteur - roi.Y + configuration.bande_morte_pix, rotated.mat.Cols - 1, milieuhauteur - roi.Y + configuration.bande_morte_pix, turquoise, 1);
                Cv2.Line(ROI1.mat, 0, milieuhauteur - roi.Y - configuration.bande_morte_pix, rotated.mat.Cols - 1, milieuhauteur - roi.Y - configuration.bande_morte_pix, turquoise, 1);
            }
            #endregion

            #region dessine centre sur roi
            if (configuration.displayCenter == true)
                Cv2.Line(ROI1.mat, 0, milieuhauteur - roi.Y, ROI1.mat.Cols - 1, milieuhauteur - roi.Y, rouge, 1);
            #endregion

            int distancepixel_2 = milieuhauteur - (niveau_pixel + roi.Y);

            NewDistancePixel(distancepixel_2);

            //Cv2.PutText(rotated.mat,
            //            distancepixel_2.ToString(),
            //            new OpenCvSharp.Point(0, rotated.mat.Height),
            //            HersheyFonts.HersheyTriplex,
            //            5,
            //            blanc,
            //            thickness: 4);

            return ROI1;
        }

        #endregion

        #region (COMMON) IMAGE PROCESSING
        Mat RGBToGray(Mat input, Calque calque)
        {
            Mat gris = new Mat();
            Mat[] layers = null;
            switch (calque)
            {
                case Calque.all:
                    Cv2.CvtColor(input, gris, ColorConversionCodes.RGB2GRAY);
                    break;
                case Calque.red:
                    layers = input.Split();
                    gris = layers[2];
                    break;
                case Calque.green:
                    layers = input.Split();
                    gris = layers[1];
                    break;
                case Calque.blue:
                    layers = input.Split();
                    gris = layers[0];
                    break;
            }
            return gris;
        }

        #region Rotate
        //avec ajout de noir (plutôt qu'image tronquée
        Mat Rotation(Mat src, float angle)
        {
            if (angle == 0) return Rotation(src, null);
            if (angle == 90) return Rotation(src, RotateFlags.Rotate90Clockwise);
            if (angle == 180) return Rotation(src, RotateFlags.Rotate180);
            if (angle == 270) return Rotation(src, RotateFlags.Rotate90Counterclockwise);

            Point2f center = new Point2f((src.Width - 1) / 2f, (src.Height - 1) / 2f);
            Mat rotationMat = Cv2.GetRotationMatrix2D(center, angle, 1);

            Size2f imagesize = new Size2f(src.Width, src.Height);
            Point2f c = new Point2f();
            OpenCvSharp.Rect boundingRect = new RotatedRect(c, imagesize, angle).BoundingRect();
            rotationMat.Set(0, 2, rotationMat.Get<double>(0, 2) + (boundingRect.Width / 2f) - (src.Width / 2f));
            rotationMat.Set(1, 2, rotationMat.Get<double>(1, 2) + (boundingRect.Height / 2f) - (src.Height / 2f));

            Mat rotatedSrc = new Mat();
            Cv2.WarpAffine(src, rotatedSrc, rotationMat, boundingRect.Size);
            return rotatedSrc;
        }

        Mat Rotation(Mat frame, RotateFlags? rotation)
        {
            Mat _out = new Mat();
            switch (rotation)
            {
                case null:
                    _out = frame.Clone();
                    break;
                case RotateFlags.Rotate90Clockwise:
                    Cv2.Rotate(frame, _out, RotateFlags.Rotate90Clockwise);
                    break;
                case RotateFlags.Rotate90Counterclockwise:
                    Cv2.Rotate(frame, _out, RotateFlags.Rotate90Counterclockwise);
                    break;
                case RotateFlags.Rotate180:
                    Cv2.Rotate(frame, _out, RotateFlags.Rotate180);
                    break;
            }
            return _out;
        }

        private void Image_Turn90_Left_Click(object sender, MouseButtonEventArgs e)
        {
            configuration.rotation_angle -= 90;
        }

        private void Image_Turn90_Right_Click(object sender, MouseButtonEventArgs e)
        {
            configuration.rotation_angle += 90;
        }

        #endregion

        #region Resize
        Mat Resize(Mat frame)
        {
            return Resize(frame, configuration.resize_factor);
        }

        Mat Resize(Mat frame, double ResizeFactor)
        {
            if (ResizeFactor == 1)
                return frame;


            Mat _out = new Mat();
            //erreur
            if (ResizeFactor <= 0)
                ResizeFactor = 10f / frame.Width;

            Cv2.Resize(frame, _out, new OpenCvSharp.Size(0, 0), ResizeFactor, ResizeFactor, InterpolationFlags.Cubic);
            return _out;
        }
        #endregion

        #region ROI : Region Of Interest
        Mat ROI(Mat frame, OpenCvSharp.Rect roi)
        {
            Mat _out;
            if (
                roi.Width > 0 &&
                roi.Height > 0 &&

                frame.Height - roi.Y > 0 &&
                frame.Width - roi.X > 0 &&

                frame.Height - (roi.Y + roi.Height) >= 0 &&
                frame.Width - (roi.X + roi.Width) >= 0
                )
                _out = new Mat(frame, roi);
            else
                _out = frame;
            return _out;
        }

        Mat ROI_NewMat(Mat frame, OpenCvSharp.Rect roi)
        {
            return ROI(frame, roi).Clone();
        }
        #endregion

        #endregion

        #region IMAGE DISPLAY & SAVE
        //void Display_Init()
        //{
        //    bgr = new Mat[] { new Mat(frame.mat.Size(), MatType.CV_8UC1),
        //                      new Mat(frame.mat.Size(), MatType.CV_8UC1),
        //                      new Mat(frame.mat.Size(), MatType.CV_8UC1)};
        //}

        void UpdateDisplayImages()
        {
            i1._Update(NMs.MatNamesToMats);
            i2._Update(NMs.MatNamesToMats);
            i3._Update(NMs.MatNamesToMats);
            i4._Update(NMs.MatNamesToMats);
        }

        void DisplayFPS()
        {
            long T = mesureFPS.ElapsedMilliseconds;
            float f = 1000f / (T - T0);
            T0 = T;
            _fps = " [" + f.ToString("N1") + " fps]";
        }

        void ComputePicture(Mat image)
        {
            if (!image.Empty())
            {
                NamedMat imageToSave = FrameProcessing1(image);
            }

            UpdateDisplayImages();

            DisplayFPS();
        }

        void Save(NamedMat image)
        {
            DateTime t = DateTime.Now;
            if (t.Subtract(t_last_save) > t_vide_save)
            {
                t_last_save = t;
                string filename;

                if (firstFileToSave)
                {
                    savedImageCount = 0;

                    savedImagesPath = configuration.savedImageFolder + "\\";
                    if (configuration.addDirectory == true)
                        savedImagesPath += DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss") + "\\";

                    //create folder
                    Directory.CreateDirectory(savedImagesPath);

                    firstFileToSave = false;
                }

                if (camera_pos_mm == null)
                    filename = savedImagesPath + image.imageType + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss_fff") + " _ " + ".jpg";
                else
                    filename = savedImagesPath + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss_fff") + " _ " + (int)camera_pos_mm + ".jpg";
                bool res = image.mat.SaveImage(filename);

                savedImageCount++;
            }
        }

        void Display_debug(Mat mat)
        {
            if (dislpay_debug == null)
                dislpay_debug = new OpenCvSharp.Window("dislpay_debug");

            if (mat.Empty())
                return;

            dislpay_debug.ShowImage(mat);

            Cv2.WaitKey(0);
        }

        #endregion
        #endregion

        #region DEPLACEMENT
        #region ARDUINO
        void Arduino_Init()
        {
            COMBaudsRefresh();
            cbx_bauds.Text = "115200";
        }

        void Button_COMRefresh_Click(object sender, MouseButtonEventArgs e)
        {
            COMBaudsRefresh();
        }

        void COMBaudsRefresh()
        {
            Communication_Série.Communication_Série.PortCom_Fill(cbx_COM);
            Communication_Série.Communication_Série.Bauds_Fill(cbx_bauds);
        }

        void Button_Connexion_Click(object sender, MouseButtonEventArgs e)
        {
            Arduino_Connexion();
        }

        void Arduino_Connexion()
        {
            if (cs == null)
            {
                //cs = new Communication_Série.Communication_Série(cbx_COM.Text, cbx_bauds.Text, DataReceived);
                cs = new Communication_Série.Communication_Série(configuration.deplacement_portCOM,
                   configuration.deplacement_bauds,
                   //cbx_COM.Text, cbx_bauds.Text, 
                   DataReceived);
                if (cs.PortCom_ON())
                {
                    Button_DeviceConnect.Visibility = Visibility.Visible;
                    Button_DeviceDisconnect.Visibility = Visibility.Collapsed;
                }
                else
                {
                    AddTextInLBX(cs.messageErreur);
                }
            }
            else
            {
                cs.PortCom_OFF();
                cs = null;
                Button_DeviceConnect.Visibility = Visibility.Collapsed;
                Button_DeviceDisconnect.Visibility = Visibility.Visible;
            }
        }

        void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            sp.Encoding = Encoding.UTF8;
            string indata = sp.ReadExisting();
            indata = indata.Replace("\r", "");
            buffer += indata;

            string[] lignes = buffer.Split(split_car);

            for (int i = 0; i < lignes.Length - 1; i++)
            {
                ArduinoInterpretMessage(lignes[i]);
                AddTextInLBX(lignes[i]);
            }
            buffer = lignes[lignes.Length - 1];
        }

        void ArduinoInterpretMessage(string txt)
        {
            string val_txt;
            try
            {
                if (txt.Contains("Position = "))
                {
                    val_txt = txt.Replace("Position = ", "");
                    val_txt = val_txt.Replace("mm", "");
                    val_txt = val_txt.Replace(".", ",");
                    camera_pos_mm = float.Parse(val_txt);
                    Debug_Newcamera_pos_mm((float)camera_pos_mm);
                }
                if (txt.Contains("D max = "))
                {
                    val_txt = txt.Replace("D max = ", "");
                    val_txt = val_txt.Replace("mm", "");
                    val_txt = val_txt.Replace(".", ",");
                    camera_pos_max = float.Parse(val_txt);
                }

                if (txt.Contains("Up max reached"))
                {
                    camera_pos_high_switch = true;
                }

                if (txt.Contains("Down min reached"))
                {
                    camera_pos_low_switch = true;
                }

                //codeur
                if (txt.Contains("Coder = "))
                {
                    val_txt = txt.Replace("Coder = ", "");
                    camera_pos_codeur = float.Parse(val_txt);
                }

                //waiting
                if (txt == "Waiting")
                {
                    arduinoWaiting = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(txt + "\n\n" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_SendConfigToArduino_Click(object sender, MouseButtonEventArgs e)
        {
            //envoi l'ensemble des paramètres de configuration du système vers Arduino
            SendToArduino("B" + configuration.bar_mm_by_turn.ToString().Replace(',', '.'));
            Thread.Sleep(100);
            SendToArduino("C" + configuration.coder_imp_by_turn.ToString());
            Thread.Sleep(100);
            SendToArduino("M" + configuration.motor_steps_by_turn.ToString());
            Thread.Sleep(100);
            SendToArduino("T" + configuration.motor_step_duration.ToString());
            Thread.Sleep(100);
            SendToArduino("t" + configuration.motor_step_pause.ToString());
            Thread.Sleep(100);
            SendToArduinoInfo(null, null);
        }

        void AddTextInLBX(string message)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    //last is down
                    arduinoMessages.Add(message);
                    while (arduinoMessages.Count > 100)
                        arduinoMessages.RemoveAt(0);
                    OnPropertyChanged("arduinoMessages");
                }));
        }

        void tbx_txt_to_arduino_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendToArduino();
        }
        void SendToArduino(object sender, MouseButtonEventArgs e)
        {
            SendToArduino();
        }
        void SendToArduino()
        {
            string txt = tbx_txt_to_arduino.Text;
            SendToArduino(txt);
            tbx_txt_to_arduino.Text = "";
        }
        void SendToArduino(string txt)
        {
            if (cs != null)
            {
                if (!txt.Contains('\n'))
                    txt += '\n';

                if (txt.Length > 12)
                    System.Windows.MessageBox.Show("ATTENTION DEPASSEMENT DE CAPACITE DE 12 CARACTERES MAX ATTENDU!");

                cs?.Envoyer(txt);
            }
            else
            {
                AddTextInLBX("!! ARDUINO DISCONNECTED !!");
            }
        }
        void SendToArduinoInfo(object sender, MouseButtonEventArgs e)
        {
            SendToArduino("i");
        }
        void Button_Clear_Click(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    arduinoMessages.Clear();
                }));
        }

        private void Button_Etalonnage_Click(object sender, MouseButtonEventArgs e)
        {
            if (etalonnage_fin_haut)
                SendToArduino("c1");
            else
                SendToArduino("c0");
            etalonnage_fin_haut = !etalonnage_fin_haut;
        }

        void Button_UP_Click(object sender, MouseButtonEventArgs e)
        {
            SendToArduino("u");
        }

        void Button_DOWN_Click(object sender, MouseButtonEventArgs e)
        {
            SendToArduino("d");
        }
        void Button_UP_Max_Click(object sender, MouseButtonEventArgs e)
        {
            SendToArduino("U");
        }

        void Button_DOWN_Min_Max_Click(object sender, MouseButtonEventArgs e)
        {
            SendToArduino("D");
        }
        private void Button_GetPosition_Click(object sender, MouseButtonEventArgs e)
        {
            camera_pos_mm = null;
            SendToArduino("p");
        }

        void Button_ARRETURGENCE_Click(object sender, RoutedEventArgs e)
        {
            //arrêt Drive by vision
            if (cameraDisplacement_Running)
                CameraDisplacement_Start();

            SendToArduino("e");
        }
        #endregion

        #region ARDUINO - MANAGE DEPLACEMENT
        void CameraDisplacement_Init()
        {
            CameraDisplacement_Button_Update();
        }

        private void Button_DriveByVision_Click(object sender, MouseButtonEventArgs e)
        {
            CameraDisplacement_Start();
        }

        void CameraDisplacement_Start()
        {
            cameraDisplacement_Running = !cameraDisplacement_Running;

            if (cameraDisplacement_Running)
            {
                threadCommandeArduino = new Thread(CameraDisplacement);
                threadCommandeArduino.Start();
            }
            else
            {
                CameraDisplacement_Stop();
            }
            CameraDisplacement_Button_Update();
        }

        void CameraDisplacement_Button_Update()
        {
            if (cameraDisplacement_Running)
            {
                Button_DriveByVisionPlay.Visibility = Visibility.Collapsed;
                Button_DriveByVisionStop.Visibility = Visibility.Visible;
            }
            else
            {
                Button_DriveByVisionPlay.Visibility = Visibility.Visible;
                Button_DriveByVisionStop.Visibility = Visibility.Collapsed;
            }
        }

        void CameraDisplacement_Stop()
        {
            cameraDisplacement_Running = false;
            Thread.Sleep(100);
            threadCommandeArduino?.Abort();
            threadCommandeArduino = null;

            camera_pos_low_switch = false;
            camera_pos_high_switch = false;
        }

        void NewDistancePixel(int d_pix)
        {
            ds_pix.Add(d_pix);
            if (ds_pix.Count > points_par_z_max_ds_pix_nbr)
                ds_pix.RemoveAt(0);
            else
                return;

            //distancepixel : aussi dans CameraDisplacement()
            distancepixel = Mediane(ds_pix.ToArray());

            distancepixel = d_pix;

            Debug_Newdistancepixel(distancepixel);

            //if (Math.Abs(distancepixel) > configuration.bande_morte_pix)
            //test : on n'est pas dans la bande morte
            if ((distancepixel > 0 && distancepixel > configuration.bande_morte_pix) ||
                (distancepixel < 0 && distancepixel < -configuration.bande_morte_pix))
            {
                //pas assez proche, si on peut, rapprochons-nous
                if (arduinoWaiting)
                    newTarget = true;
            }
            else
            {
                //on est proche :
                newTarget = false;
                DateTime t = DateTime.Now;

                //on peut sauvegarder ce point !?
                if (t - t_last > t_vide)
                {
                    if (camera_pos_mm != null)
                    {
                        t_last = t;
                        System.Windows.Application.Current.Dispatcher.Invoke(() => NewPoint(t, d_pix));
                    }
                }
            }
        }

        float? vitesseinstantannee_mm_par_sec;

        void NewPoint(DateTime t, int ecart_pix)
        {
            PointJJ newPoint = new PointJJ(t, (float)camera_pos_mm, ecart_pix);

            if (lastPoint == null)
            {
                //t0 = t;
                newPoint.t = 0;
                lastPoint = newPoint;

                //création du fichier de sauvegarde de l'expérience
                string ligne = "t(s),z(mm),,date,erreur(pix)\n\r";
                data_filename = AppDomain.CurrentDomain.BaseDirectory + "kynch " + t0.ToString("yyyy_MM_dd HH_mm_ss") + ".csv";
                System.IO.File.AppendAllText(data_filename, ligne);
            }

            if (newPoint.z_mm == lastPoint.z_mm)
            {
                if (Math.Abs(newPoint.erreur_pixel) < Math.Abs(lastPoint.erreur_pixel))
                {
                    //le point est meilleur : on écrase l'ancien et on met le nouveau
                    lastPoint = newPoint;

                    //mesure vitesse instantanée

                    if (_points.Count > 2)
                    {
                        var Z = _points[_points.Count - 1];
                        var Y = _points[_points.Count - 2];
                        vitesseinstantannee_mm_par_sec = (float)((Z.z_mm - Y.z_mm) / (Z.t - Y.t));
                    }
                    else
                        vitesseinstantannee_mm_par_sec = null;
                }
            }
            else
            {
                //sauvegarde/écriture dans fichier data du point précédent
                string[] ligne = new string[1] { lastPoint.ToString() };
                System.IO.File.AppendAllLines(data_filename, ligne);
                _points.Add(lastPoint);

                plot._Add(lastPoint);

                lastPoint = newPoint;
            }
        }

        void CameraDisplacement()
        {
            //distancepixel : valeur significative de déplacement. Issu d'un test de bande morte, filtrée et écrite.
            //newTarget     : booléen mis à 1 au moment où distancepixel est nouvellement calculé
            bool first_commande = true;

            int delta_pix;
            int delta_pix_precedent = 0;

            float deplacement_mm_commande_precedent = 0;
            int limitateur_occurence = 0;

            while (cameraDisplacement_Running)
            {
                //en attente d'ordre de déplacement & pas d'arrêt (continue)
                if (!newTarget)
                {
                    Thread.Sleep(10);
                    continue;
                }

                delta_pix = distancepixel; // variable setté dans NewDistancePixel()
                bool monte = delta_pix > 0;

                if (first_commande)
                {
                    first_commande = false;
                    deplacement_mm_commande = 3; // arbitrairement, on prend 3 mm pour le premier déplacement
                }
                else
                {
                    // calcul auto du ratio (si la cible correspond toujours au même objet observé !)
                    //_ratio_mm_pix = (float)deplacement_mm_commande_precedent / (delta_pix_precedent - delta_pix);
                    deplacement_mm_commande = (float)((double)delta_pix / configuration.ratio_pix_par_mm);

                    if (vitesseinstantannee_mm_par_sec != null)
                        deplacement_mm_commande += (float)Math.Round((float)vitesseinstantannee_mm_par_sec * 1, 2); // arbitrairement 1 seconde de temps de réaction


                    if (deplacement_mm_commande > 0 && deplacement_mm_commande_precedent < 0 ||
                       deplacement_mm_commande < 0 && deplacement_mm_commande_precedent > 0
                        )
                    {
                        deplacement_mm_commande /= 3;
                        Console.WriteLine("limitateur " + limitateur_occurence++);
                    }

                }

                Debug_Newdeplacement_mm_commande(deplacement_mm_commande);

                if (deplacement_mm_commande == 0 || camera_pos_low_switch || camera_pos_high_switch)
                {

                }
                else
                {
                    //TODO PARAMETRISER CLAMP
                    //deplacement_mm_commande = Clamp(deplacement_mm_commande, -5, 5); //par petit pas (maxi +/-1 mm)

                    string commandeArduino = "";
                    float val = deplacement_mm_commande;
                    if (monte)
                        commandeArduino = "u";
                    else
                    {
                        commandeArduino = "d";
                        val = -val;
                    }
                    string val_txt = val.ToString().Replace(',', '.');
                    SendToArduino(commandeArduino + val_txt);

                    //temps d'action : attente que la commande soit terminée (Arduino dit "Waiting")
                    arduinoWaiting = false;
                    while (!arduinoWaiting)
                        Thread.Sleep(TimeSpan.FromTicks(1));
                }

                deplacement_mm_commande_precedent = deplacement_mm_commande;
                delta_pix_precedent = delta_pix;
            }
        }
        #endregion
        #endregion

        #region DATA Points

        void Button_OpenDataFile_Click(object sender, MouseButtonEventArgs e)
        {
            if (System.IO.File.Exists(data_filename))
                System.Diagnostics.Process.Start(data_filename);
        }

        void Button_DATA_Clear_Click(object sender, MouseButtonEventArgs e)
        {
            _points.Clear();
            lastPoint = null;
            if (System.IO.File.Exists(data_filename))
                System.IO.File.Delete(data_filename);

            plot._Clear();
        }

        private void btn_savedImageFolder_SelectFolder_click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            //default folder
            if (System.IO.Directory.Exists(configuration.savedImageFolder))
                dialog.SelectedPath = configuration.savedImageFolder;

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                configuration.savedImageFolder = dialog.SelectedPath;
        }

        private void btn_OpenDataFolder_click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(savedImagesPath);
        }

        #endregion

        #region Debug / graph
        ScottPlot.Plottable.DataLogger debug_log_01;
        ScottPlot.Plottable.DataLogger debug_log_02;
        ScottPlot.Plottable.DataLogger debug_log_03;
        ScottPlot.Plottable.DataLogger debug_log_04;

        bool debug_record;
        private void Debug_graph_INIT()
        {
            debug_plot.Plot.Style(new ScottPlot.Styles.Gray1());


            //debug_log_01 = debug_plot.Plot.AddDataLogger();
            //debug_log_01.MarkerSize = 5;
            //debug_log_01.Color = System.Drawing.Color.Magenta;
            //debug_plot.Plot.YAxis.Color(debug_log_01.Color);
            //debug_log_01.YAxisIndex = 0; //1 à droite

            debug_plot.Plot.YAxis.IsVisible = false;

            debug_log_02 = debug_plot.Plot.AddDataLogger();
            debug_log_02.MarkerSize = 5;
            debug_log_02.Color = System.Drawing.Color.DodgerBlue;
            // Create another axis to the left and give it an index
            var secondYAxis = debug_plot.Plot.AddAxis(Edge.Left, axisIndex: 2, "distancepixel", color: debug_log_02.Color);
            debug_log_02.YAxisIndex = 2;

            debug_log_03 = debug_plot.Plot.AddDataLogger();
            debug_log_03.MarkerSize = 5;
            debug_log_03.Color = System.Drawing.Color.Yellow;
            var thirdYAxis = debug_plot.Plot.AddAxis(Edge.Left, axisIndex: 3, "deplacement_mm_commande", color: debug_log_03.Color);
            debug_log_03.YAxisIndex = 3;

            debug_log_04 = debug_plot.Plot.AddDataLogger();
            debug_log_04.MarkerSize = 5;
            debug_log_04.Color = System.Drawing.Color.Lime;
            var fourthYAxis = debug_plot.Plot.AddAxis(Edge.Left, axisIndex: 4, "camera_pos_mm", color: debug_log_04.Color);
            debug_log_04.YAxisIndex = 4;
        }

        private void Button_Debug_Clear_Click(object sender, MouseButtonEventArgs e)
        {
            debug_log_01?.Clear();
            debug_log_02?.Clear();
            debug_log_03?.Clear();
            debug_log_04?.Clear();
            debug_plot.Plot.AxisAuto();
            debug_plot.Refresh();
        }
        private void Button_Debug_Pause_Click(object sender, MouseButtonEventArgs e) { debug_record = false; }
        private void Button_Debug_Play_Click(object sender, MouseButtonEventArgs e) { debug_record = true; }

        void Debug_Newdistancepixel(float val)
        {
            Debug_New(debug_log_02, val);
        }
        void Debug_Newdeplacement_mm_commande(float val)
        {
            Debug_New(debug_log_03, val);
        }
        void Debug_Newcamera_pos_mm(float val)
        {
            Debug_New(debug_log_04, val);
        }

        void Debug_New(DataLogger dataLogger, float val)
        {
            if (!debug_record)
                return;

            debug_record = false;

            double x = (DateTime.Now - t0).TotalSeconds;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dataLogger.Add(x, val);
                    debug_plot.Plot.AxisAuto();
                    debug_plot.Refresh();
                }
                catch (Exception)
                {

                }
            }), priority: DispatcherPriority.Background);
        }
        #endregion

        #region (COMMON) MATHS
        static float[] Medianes(float[] valeurs, int fenetre)
        {
            float[] medianes = new float[valeurs.Length];

            for (int i = 0; i < medianes.Length; i++)
            {
                int index_a = i - fenetre;
                if (index_a < 0) index_a = 0;
                int index_z = i + fenetre;
                if (index_z > valeurs.Length - 1) index_z = valeurs.Length - 1;

                List<float> valeurs_fentre = valeurs.Skip(index_a).Take(index_z - index_a).OrderBy(n => n).Select(s => s).ToList<float>();
                medianes[i] = valeurs_fentre[valeurs_fentre.Count / 2];
            }

            return medianes;
        }

        static int Mediane(int[] valeurs)
        {
            List<int> listToSort = valeurs.ToList();
            listToSort.Sort();
            return listToSort[listToSort.Count / 2];
        }

        static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        #endregion

        #region SCAN
        void Scan()
        {
            camera_pos_high_switch = false;
            camera_pos_low_switch = false;

            camera_pos_mm = 0;
            //va à l'extrémité haute
            SendToArduino("U");
            while (!camera_pos_high_switch)
            {
                Thread.Sleep(100);
            }

            //va à l'extrémité basse
            int H = 4;
            OpenCvSharp.Size s = rotated.mat.Size();
            OpenCvSharp.Point p = new OpenCvSharp.Point(0, s.Height / 2 - H / 2);
            OpenCvSharp.Rect rect = new OpenCvSharp.Rect(p, new OpenCvSharp.Size(s.Width, H));

            NamedMat d = NMs.Get(ImageType.debug1);
            d.mat = new Mat();
            while (!camera_pos_low_switch)
            {
                //descend de 1mm
                float camera_pos_prec = (float)camera_pos_mm;
                float delta_mm = 1;
                while (camera_pos_prec - camera_pos_mm < delta_mm)// && !camera_pos_low_switch)
                {
                    SendToArduino("d1");
                    Thread.Sleep(400);
                }


                //améliorer la neteté
                //TODO : prendre plusieurs fois la ROI et en faire la médiane
                List<Mat> mediane = new List<Mat>();









                NamedMat roi2 = NMs.Get(ImageType.roi2);
                roi2.mat = new Mat(rotated.mat, rect);

                if (d.mat.Size().Width == 0)
                    Cv2.VConcat(new List<Mat> { roi2.mat }, d.mat);
                else
                    Cv2.VConcat(new List<Mat> { d.mat, roi2.mat }, d.mat);
            }
        }

        private void mit_scan_start_Click(object sender, RoutedEventArgs e)
        {
            mit_scan_start.IsEnabled = false;
            mit_scan_stop.IsEnabled = true;

            //envoyer un message à l'arduio pour dire "mode SCAN = ON"




            Thread t = new Thread(Scan);
            t.Start();
        }

        private void mit_scan_stop_Click(object sender, RoutedEventArgs e)
        {
            mit_scan_start.IsEnabled = true;
            mit_scan_stop.IsEnabled = false;

            //envoyer un message à l'arduio pour dire "mode SCAN = OFF"
        }

        void simul_data_for_graph_start_Click(object sender, RoutedEventArgs e)
        {
            plot._Simulator_Start();
        }

        void simul_data_for_graph_stop_Click(object sender, RoutedEventArgs e)
        {
            plot._Simulator_Stop();
        }
        #endregion
    }
}