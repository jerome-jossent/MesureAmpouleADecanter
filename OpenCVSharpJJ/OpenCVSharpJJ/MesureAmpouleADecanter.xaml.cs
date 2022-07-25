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

namespace OpenCVSharpJJ
{
    public partial class MesureAmpouleADecanter : System.Windows.Window, INotifyPropertyChanged
    {
        #region BINDINGS IHM
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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

        public string _SavedImageFolder
        {
            get { return SavedImageFolder; }
            set
            {
                if (SavedImageFolder == value)
                    return;
                SavedImageFolder = value;
                OnPropertyChanged("_SavedImageFolder");
            }
        }
        string SavedImageFolder = @"C:\DATA\decantation";

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
        public bool? _SaveFrame
        {
            get { return SaveFrame; }
            set
            {
                if (SaveFrame == value)
                    return;
                SaveFrame = value;
                OnPropertyChanged("_SaveFrame");
            }
        }
        bool? SaveFrame = false;

        public int Threshold1
        {
            get { return threshold1; }
            set
            {
                if (threshold1 == value)
                    return;
                threshold1 = value;
                OnPropertyChanged("Threshold1");
                if (!captureVideoIsRunning)
                    ComputePicture(frame.mat);
            }
        }
        int threshold1 = 150;
        public int Threshold2
        {
            get { return threshold2; }
            set
            {
                if (threshold2 == value)
                    return;
                threshold2 = value;
                OnPropertyChanged("Threshold2");
                if (!captureVideoIsRunning)
                    ComputePicture(frame.mat);
            }
        }
        int threshold2 = 10;

        public int ResizeFactor
        {
            get { return resizeFactor; }
            set
            {
                if (resizeFactor == value)
                    return;
                resizeFactor = value;
                OnPropertyChanged("ResizeFactor");
                OnPropertyChanged("ResizeFactorString");
                if (!captureVideoIsRunning)
                    ComputePicture(frame.mat);
            }
        }
        int resizeFactor = 100;

        public string ResizeFactorString
        {
            get { return resizeFactor.ToString() + " %"; }
        }

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
                    OnPropertyChanged("_imageSource");
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
                    OnPropertyChanged("_imageCalque");
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
                    OnPropertyChanged("_image1");
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
                    OnPropertyChanged("_image2");
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
                    OnPropertyChanged("_image3");
                }
            }
        }
        System.Drawing.Bitmap image3;

        public ObservableCollection<string> ArduinoMessages
        {
            get
            {
                return arduinoMessages;
            }
            set
            {
                arduinoMessages = value;
                OnPropertyChanged("ArduinoMessages");
            }
        }

        public ObservableCollection<PointsJJ> _points { get => points; set => points = value; }
        ObservableCollection<PointsJJ> points = new ObservableCollection<PointsJJ>();
        public int _pointsMax { get; set; }

        ObservableCollection<string> arduinoMessages = new ObservableCollection<string>();

        #endregion

        #region ENUMERATIONS
        public enum ImageType
        {
            none,
            original,
            rotated,
            roi1, roi2,
            gray1, gray2,
            bw1, bw2,
            canny,
            bgr1, bgr2,
            debug1, debug2, debug3, debug4, debug5,
            graph1, graph2
        }

        enum Calque { all, red, green, blue }
        #endregion

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

        public class PointsJJ
        {
            public DateTime T { get; set; }
            public string t
            {
                get
                {
                    string chaine = T.ToString("G");
                    return chaine;
                }
            }
            public float v { get; set; }

            public PointsJJ(DateTime t, float v)
            {
                T = t;
                this.v = v;
            }
            public PointsJJ(float v)
            {
                T = DateTime.Now;
                this.v = v;
            }
        }
        #endregion

        #region PARAMETERS
        int bande_morte_pix = 5;
        float ratio_mm_pix = 0.2f;
        bool arduinoWaiting;

        RotateFlags? rotation;

        long T0;
        NamedMats NMs;
        NamedMat none = new NamedMat(ImageType.none);
        NamedMat frame = new NamedMat(ImageType.original);
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
        Mat[] bgr;

        Thread threadCaptureVideo;
        Thread threadCommandeArduino;
        int indexDevice;
        VideoInInfo.Format format;
        Dictionary<string, VideoInInfo.Format> formats;
        VideoCapture capture;
        bool captureVideoIsRunning = false;
        bool first = true;
        System.Diagnostics.Stopwatch chrono = new System.Diagnostics.Stopwatch();

        OpenCvSharp.Rect roi;

        Communication_Série.Communication_Série cs;
        string buffer;
        char[] split_car = new char[] { '\n' };
        float? camera_pos_mm;
        float? camera_pos_codeur;
        float camera_pos_max;

        bool cameraDisplacement_Running = false;

        bool camera_pos_low_switch, camera_pos_high_switch;

        OpenCvSharp.Window w;
        //bool display_in_OpenCVSharpWindow = false;
        #endregion

        #region CONSTANTES
        Scalar rouge = new Scalar(0, 0, 255);
        Scalar vert = new Scalar(0, 255, 0);
        Scalar bleu = new Scalar(255, 100, 0);
        Scalar blanc = new Scalar(255, 255, 255, 255);
        Scalar noir = new Scalar(0, 0, 0);
        #endregion

        #region WINDOW MANAGEMENT
        public MesureAmpouleADecanter()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Camera_Init();
            Arduino_Init();
            ImageProcessing_Init();
            CameraDisplacement_Init();

            //PRESETS
            cbx_rotation.SelectedIndex = 3;
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            CameraDisplacement_Stop();
            CaptureCamera_Stop();
            cs?.PortCom_OFF();
        }

        void gds_mouseenter(object sender, MouseEventArgs e)
        {
            dottedline.Visibility = Visibility.Visible;
        }

        void gds_mouseleave(object sender, MouseEventArgs e)
        {
            dottedline.Visibility = Visibility.Hidden;
        }
        #endregion

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
            Capture_Start();
        }

        void Capture_Start()
        {
            chrono.Start();
            captureVideoIsRunning = !captureVideoIsRunning;

            if (captureVideoIsRunning)
            {
                indexDevice = cbx_device.SelectedIndex;
                CaptureCamera(indexDevice);
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
            indexDevice = cbx_device.SelectedIndex;
            formats = VideoInInfo.EnumerateSupportedFormats_JJ(indexDevice);
            cbx_deviceFormat.ItemsSource = formats.OrderBy(f => f.Value.format).ThenByDescending(f => f.Value.w).Select(f => f.Key);
        }

        void Combobox_CaptureDeviceFormat_Change(object sender, SelectionChangedEventArgs e)
        {
            if (cbx_deviceFormat.SelectedValue != null)
                format = formats[cbx_deviceFormat.SelectedValue as string];
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
            indexDevice = index;
            threadCaptureVideo = new Thread(new ThreadStart(CaptureCameraCallback));
            threadCaptureVideo.Start();
        }

        void CaptureCamera_Stop()
        {
            captureVideoIsRunning = false;
            Thread.Sleep(100);
            threadCaptureVideo?.Abort();
            first = true;
            threadCaptureVideo = null;
        }

        void CaptureCameraCallback()
        {
            int actualindexDevice = indexDevice;
            frame.mat = new Mat();
            capture = new VideoCapture(indexDevice);
            capture.Open(indexDevice, VideoCaptureAPIs.DSHOW);

            if (capture.IsOpened())
            {
                while (captureVideoIsRunning)
                {
                    //si changement de camera
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
                        capture.Set(VideoCaptureProperties.FourCC, FourCC.FromString(format.format));
                        format = null;
                    }

                    if (first)
                    {
                        Display_Init();
                        MatNamesToMats_Reset();
                        roi = new OpenCvSharp.Rect();
                        first = false;
                    }

                    //captation de l'image
                    capture.Read(frame.mat);

                    //traitement de l'image
                    ComputePicture(frame.mat);

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
                }
                capture.Dispose();
            }
        }
        #endregion

        void ComputePicture(Mat image)
        {
            if (!image.Empty())
            {
                NamedMat imageToSave = FrameProcessing1(image);
            }

            UpdateDisplayImages();

            DisplayFPS();
        }


        DateTime t_last_save;
        TimeSpan t_vide_save = TimeSpan.FromSeconds(1);


        void Save(NamedMat image)
        {
            DateTime t = DateTime.Now;
            if (t.Subtract(t_last_save) > t_vide_save)
            {
                t_last_save = t;
                string filename = SavedImageFolder + "//" + DateTime.Now.ToString("yyyy-MM-dd HHmmss-fff") + ".jpg";
                image.mat.SaveImage(filename);
            }


        }

        #region IMAGE PROCESSINGS
        void ImageProcessing_Init()
        {
            cbx_rotation.Items.Add("Non");
            cbx_rotation.Items.Add(RotateFlags.Rotate90Clockwise.ToString());
            cbx_rotation.Items.Add(RotateFlags.Rotate180.ToString());
            cbx_rotation.Items.Add(RotateFlags.Rotate90Counterclockwise.ToString());

            MatNamesToMats_Reset();

            i1._UpdateCombobox(NMs);
            i2._UpdateCombobox(NMs);
            i3._UpdateCombobox(NMs);
            i4._UpdateCombobox(NMs);

            int i = 1;
            i1.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i1.matName = ImageType.graph1;
            i2.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i2.matName = ImageType.rotated;
            i3.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i3.matName = ImageType.roi1;
            i4.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i4.matName = ImageType.none;
        }

        void MatNamesToMats_Reset()
        {
            NMs = new NamedMats();
            NMs.MatNamesToMats.Add(ImageType.none, none);
            NMs.MatNamesToMats.Add(ImageType.original, frame);
            NMs.MatNamesToMats.Add(ImageType.rotated, rotated);
            NMs.MatNamesToMats.Add(ImageType.gray1, frameGray);
            NMs.MatNamesToMats.Add(ImageType.canny, cannymat);
            NMs.MatNamesToMats.Add(ImageType.bw1, bw1);
            NMs.MatNamesToMats.Add(ImageType.bw2, bw2);
            NMs.MatNamesToMats.Add(ImageType.bgr1, BGR);
            NMs.MatNamesToMats.Add(ImageType.roi1, ROI1);
            NMs.MatNamesToMats.Add(ImageType.roi2, ROI2);
            NMs.MatNamesToMats.Add(ImageType.debug1, debug1);
            NMs.MatNamesToMats.Add(ImageType.debug2, debug2);
            NMs.MatNamesToMats.Add(ImageType.debug3, debug3);
            NMs.MatNamesToMats.Add(ImageType.debug4, debug4);
            NMs.MatNamesToMats.Add(ImageType.graph1, graph1);
            NMs.MatNamesToMats.Add(ImageType.graph2, graph2);
        }

        void Button_CaptureDeviceROI_Click(object sender, RoutedEventArgs e)
        {
            string window_name = "Valid ROI with 'Enter' or 'Space', cancel with 'c'";

            if (frame.mat.Empty())
                return;

            rotated.mat = Resize(frame.mat);
            rotated.mat = Rotation(rotated.mat, rotation);

            OpenCvSharp.Rect newroi = Cv2.SelectROI(window_name, rotated.mat, true);
            roi = newroi;
            tbx_roi.Text = ROIToString();
            _title = roi.ToString();
            Cv2.DestroyWindow(window_name);
        }

        string ROIToString()
        {
            return roi.X + "|" + roi.Y + "|" + roi.Width + "|" + roi.Height + "|";
        }

        void Button_CaptureDeviceROI_Save_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.roi = ROIToString();
            Properties.Settings.Default.Save();
        }

        void Button_CaptureDeviceROI_Load_Click(object sender, RoutedEventArgs e)
        {
            string[] param = Properties.Settings.Default.roi.Split('|');
            int x = int.Parse(param[0]);
            int y = int.Parse(param[1]);
            int w = int.Parse(param[2]);
            int h = int.Parse(param[3]);

            roi = new OpenCvSharp.Rect(x, y, w, h);
            tbx_roi.Text = ROIToString();
        }

        private void Button_SetROI_Click(object sender, RoutedEventArgs e)
        {
            string roi_s = tbx_roi.Text;
            try
            {
                string[] param = roi_s.Split('|');
                int x = int.Parse(param[0]);
                int y = int.Parse(param[1]);
                int w = int.Parse(param[2]);
                int h = int.Parse(param[3]);

                roi = new OpenCvSharp.Rect(x, y, w, h);
            }
            catch (Exception ex)
            {

            }
        }

        void Button_CaptureDeviceROI_None_Click(object sender, RoutedEventArgs e)
        {
            roi = new OpenCvSharp.Rect(0, 0, 0, 0);
            tbx_roi.Text = ROIToString();
        }

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
        int nbrLignes;
        int nbrPixel_par_ligne;
        int nbrLignes_prec;
        int nbrPixel_par_ligne_prec;
        float[] moyennesX;
        float[] d1; //derivée première
        float[] d2; //derivée seconde

        NamedMat FrameProcessing1(Mat image)
        {
            //filtre gaussien
            //Savitzky-Golay filter     https://docs.scipy.org/doc/scipy/reference/generated/scipy.signal.savgol_filter.html

            rotated.mat = Resize(image);

            rotated.mat = Rotation(rotated.mat, rotation);

            ROI1.mat = ROI_NewMat(rotated.mat, roi);

            //gris
            frameGray.mat = RGBToGray(ROI1.mat, Calque.all);

            //seuillage
            //frameGray.mat = frameGray.mat.Threshold(Threshold1, 255, ThresholdTypes.Binary);
            //Cv2.Canny(frameGray.mat, frameGray.mat, 50, Threshold1);

            FrameProcessing_InitData(ROI1.mat);

            //trouve niveau :
            Mat<byte> mat3 = new Mat<byte>(frameGray.mat);
            MatIndexer<byte> indexer = mat3.GetIndexer();

            //somme par ligne
            for (int y = 0; y < nbrLignes; y++)
            {
                moyennesX[y] = 0;
                for (int x = 0; x < nbrPixel_par_ligne; x++)
                    moyennesX[y] += indexer[y, x];

                moyennesX[y] /= nbrPixel_par_ligne;
            }

            //derivée primaire (simplifié : car même pas /(x2-x1))
            for (int i = 1; i < nbrLignes - 2; i++)
                d1[i] = moyennesX[i + 1] - moyennesX[i - 1];

            //derivée seconde simplifié : car /(x2-x1) => /1
            //et recherche des maximum et minimum
            float d2_min = 0;
            float d2_max = 0;
            int d2_max_index = 0;
            for (int i = 2; i < nbrLignes - 4; i++)
            {
                d2[i] = d1[i + 1] - d1[i - 1];

                if (i == 2)
                {
                    d2_min = d2[i];
                    d2_max = d2[i];
                    d2_max_index = i;
                }
                else
                {
                    if (d2[i] < d2_min)
                        d2_min = d2[i];

                    if (d2[i] > d2_max)
                    {
                        d2_max = d2[i];
                        d2_max_index = i;
                    }
                }
            }

            //intersections à 0 entre le minmum et le maximum de la zone d'intérêt
            //détection de la zone d'intérêt
            int niveau_pixel;
            float n_val = d2[d2_max_index];
            if (n_val > 0)
            {
                int index_prec = d2_max_index;
                int index = index_prec - 1;
                while (d2[index] > 0)
                {
                    index_prec = index;
                    index = index_prec - 1;
                }
                niveau_pixel = index_prec;
            }
            else if (n_val < 0)
            {
                int index_prec = d2_max_index;
                int index = index_prec + 1;
                while (d2[index] < 0)
                {
                    index_prec = index;
                    index = index_prec + 1;
                }
                niveau_pixel = index_prec;
            }
            else
                niveau_pixel = d2_max_index;

            if (SaveFrame == true)
                Save(ROI1);

            //tracé du niveau en ligne hachée sur l'image
            int morceaux = 10;
            int dashed_line_A = 0;
            int dashed_line_Z = ROI1.mat.Width - 1;
            int dashed_line_total_length = dashed_line_Z - dashed_line_A;
            float dashed_line_length = dashed_line_total_length / (morceaux * 2 - 1);
            int epaisseur = (int)(0.4 * nbrLignes / 100);
            if (epaisseur < 1) epaisseur = 1;
            for (int i = 0; i < morceaux * 2 - 1; i += 2)
            {
                int x1 = (int)(dashed_line_length * i);
                int x2 = (int)(dashed_line_length * (i + 1));
                Cv2.Line(ROI1.mat, x1, niveau_pixel, x2, niveau_pixel, bleu, epaisseur);
                Cv2.Line(frameGray.mat, x1, niveau_pixel, x2, niveau_pixel, bleu, epaisseur);
            }

            Cv2.Line(graph1.mat, 0, niveau_pixel, graph1.mat.Width - 1, niveau_pixel, bleu, epaisseur);

            //centre caméra (mire+)
            int milieuhauteur = rotated.mat.Height / 2;
            int milieulargeur = rotated.mat.Width / 2;
            Cv2.Line(rotated.mat, milieulargeur - 50, milieuhauteur, milieulargeur + 50, milieuhauteur, rouge, 2);
            Cv2.Line(rotated.mat, milieulargeur, milieuhauteur - 50, milieulargeur, milieuhauteur + 50, rouge, 2);

            //graphique de l'image
            int largeur_graph = 300;
            graph1.mat = new Mat(nbrLignes, largeur_graph, type: MatType.CV_8UC3, noir);

            //série "moyennes des pixels par ligne"
            for (int i = 1; i < nbrLignes; i++)
            {
                int y1 = (int)moyennesX[i - 1];
                int y2 = (int)moyennesX[i];
                Cv2.Line(graph1.mat, y1, i - 1, y2, i, blanc, 1);
            }

            ////série "d1"
            //for (int i = 1; i < nbrLignes; i++)
            //{
            //    int y1 = (int)((d1[i - 1] - min) * (float)nbrPixel_par_ligne / (max - min));
            //    int y2 = (int)((d1[i] - min) * (float)nbrPixel_par_ligne / (max - min));
            //    Cv2.Line(graph1.mat, y1, i - 1, y2, i, vert, 1);
            //}

            //série "d2"
            for (int i = 1; i < nbrLignes; i++)
            {
                int y1 = (int)((d2[i - 1] - d2_min) * (float)largeur_graph / (d2_max - d2_min));
                int y2 = (int)((d2[i] - d2_min) * (float)largeur_graph / (d2_max - d2_min));
                Cv2.Line(graph1.mat, y1, i - 1, y2, i, rouge, 1);
            }

            int y0 = (int)(-d2_min * (float)largeur_graph / (d2_max - d2_min));
            Cv2.Line(graph1.mat, y0, 0, y0, nbrLignes - 1, vert, 1);

            int distancepixel = milieuhauteur - (niveau_pixel + roi.Y);

            NewDistancePixel(distancepixel);

            Cv2.PutText(rotated.mat,
                        distancepixel.ToString(),
                        new OpenCvSharp.Point(0, rotated.mat.Height),
                        HersheyFonts.HersheyTriplex,
                        5,
                        blanc,
                        thickness: 4);

            return ROI1;
        }

        void GraphTemporelle()
        {
            Scalar fond = noir;
            int hauteur_graph = ROI1.mat.Height;
            int largeur_graph = 300;
            Mat mat = new Mat(hauteur_graph, largeur_graph, type: MatType.CV_8UC3, fond);

            if (_points.Count < 300)
                return;

            while (_points.Count > 300)
                _points.RemoveAt(0);

            //tracé
            try
            {
                for (int i = _points.Count - 300; i < _points.Count; i++)
                {
                    int y1 = ROI1.mat.Height - (int)(_points[i - 1].v + ROI1.mat.Height / 2);
                    int y2 = ROI1.mat.Height - (int)(_points[i].v + ROI1.mat.Height / 2);
                    Cv2.Line(mat, i - 1 - (_points.Count - 300), y1, i - (_points.Count - 300), y2, blanc, 1);
                }
            }
            catch (Exception ex)
            {
                //
            }

            graph2.mat = mat;
        }

        void FrameProcessing2(Mat rotated)
        {
            if (roi.Width > 0)
                ROI1.mat = new Mat(rotated, roi);
            else
                ROI1.mat = rotated;

            frameGray.mat = RGBToGray(ROI1.mat, Calque.all);

            //NamedMat BW1 = NMs.Get(ImageType.bw1);

            ////BW1.mat = frameGray.mat.Threshold(Threshold1, 255, ThresholdTypes.Binary);
            //Cv2.InRange(frameGray.mat, new Scalar(Threshold1), new Scalar(Threshold2), BW1.mat);


            //Cv2.Canny(frameGray.mat, cannymat.mat, 50, 200);

            //bgr[0] = cannymat.mat;
            //bgr[1] = new Mat(bgr[0].Size(), MatType.CV_8UC1);
            //bgr[2] = bgr[1];

            //Cv2.Merge(bgr, BGR.mat);


            //Cv2.Canny(ROI1.mat, cannymat.mat, 95, 100);
            Cv2.Canny(ROI1.mat, cannymat.mat, Threshold1, Threshold2);

            //HoughLinesP
            //LineSegmentPoint[] segHoughP = Cv2.HoughLinesP(cannymat.mat, 1, Math.PI / 180, 100, 100, 10);
            LineSegmentPoint[] segHoughP = Cv2.HoughLinesP(cannymat.mat, 1, Math.PI / 2, 2, 30, 1);

            //debug1.mat = ROI1.mat.EmptyClone();

            bgr[0] = frameGray.mat;
            bgr[1] = bgr[0];
            bgr[2] = bgr[1];
            Cv2.Merge(bgr, BGR.mat);

            debug1.mat = BGR.mat.Clone();

            foreach (LineSegmentPoint s in segHoughP)
                debug1.mat.Line(s.P1, s.P2, Scalar.Red, 1, LineTypes.AntiAlias, 0);
        }
        #endregion

        #region COMMON IMAGE PROCESSING
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

        Mat Rotation(Mat frame, RotateFlags? rotation)
        {
            Mat _out = new Mat();
            switch (rotation)
            {
                case null:
                    return frame;
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

        Mat Resize(Mat frame)
        {
            if (ResizeFactor == 100)
                return frame;

            double facteur = (double)ResizeFactor / 100;
            Mat _out = new Mat();
            Cv2.Resize(frame, _out, new OpenCvSharp.Size(0, 0), facteur, facteur, InterpolationFlags.Cubic);

            return _out;
        }

        Mat ROI(Mat frame, OpenCvSharp.Rect roi)
        {
            Mat _out = new Mat();
            if (roi.Width > 0 &&
                    frame.Height - roi.Y > 0 &&
                    frame.Width - roi.X > 0 &&
                    frame.Height - (roi.Y + roi.Height) > 0 &&
                    frame.Width - (roi.X + roi.Width) > 0)
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

        #region DISPLAY IMAGE(S)
        void Display_Init()
        {
            bgr = new Mat[] { new Mat(frame.mat.Size(), MatType.CV_8UC1),
                              new Mat(frame.mat.Size(), MatType.CV_8UC1),
                              new Mat(frame.mat.Size(), MatType.CV_8UC1)};
        }

        void UpdateDisplayImages()
        {
            i1._Update(NMs.MatNamesToMats);
            i2._Update(NMs.MatNamesToMats);
            i3._Update(NMs.MatNamesToMats);
            i4._Update(NMs.MatNamesToMats);
        }

        void DisplayFPS()
        {
            long T = chrono.ElapsedMilliseconds;
            float f = 1000f / (T - T0);
            T0 = T;
            _fps = " [" + f.ToString("N1") + " fps]";
        }
        #endregion

        #region ARDUINO
        void Arduino_Init()
        {
            COMBaudsRefresh();
            cbx_bauds.Text = "9600";
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
            if (cs == null)
            {
                cs = new Communication_Série.Communication_Série(cbx_COM.Text, cbx_bauds.Text, DataReceived);
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
                    SLD_camera_position((float)camera_pos_mm);
                }
                if (txt.Contains("D max = "))
                {
                    val_txt = txt.Replace("D max = ", "");
                    val_txt = val_txt.Replace("mm", "");
                    val_txt = val_txt.Replace(".", ",");
                    camera_pos_max = float.Parse(val_txt);
                    SLD_camera_position_max(camera_pos_max);
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
                MessageBox.Show(txt + "\n\n" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void SLD_camera_position(float value)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    if (value > sld_camera_position_mm.Maximum)
                        sld_camera_position_mm.Maximum = value;
                    sld_camera_position_mm.Value = value;
                }));
        }
        void SLD_camera_position_max(float value)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    sld_camera_position_mm.Maximum = value;
                }));
        }

        void AddTextInLBX(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ////last is up
                    //ArduinoMessages.Insert(0, message);
                    //while (ArduinoMessages.Count > 100)
                    //    ArduinoMessages.RemoveAt(ArduinoMessages.Count - 1);

                    //last is down
                    ArduinoMessages.Add(message);
                    while (ArduinoMessages.Count > 100)
                        ArduinoMessages.RemoveAt(0);
                    lbx_arduino_received.ScrollIntoView(lbx_arduino_received.Items[lbx_arduino_received.Items.Count - 1]);
                }));

            OnPropertyChanged("ArduinoMessages");
        }

        void tbx_txt_to_arduino_KeyDown(object sender, KeyEventArgs e)
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

                cs?.Envoyer(txt);
            }
            else
            {
                AddTextInLBX("!! ARDUINO DISCONNECTED !!");
            }
        }

        void Button_Clear_Click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ArduinoMessages.Clear();
                }));
            OnPropertyChanged("ArduinoMessages");
        }

        bool etalonnage_fin_haut;
        void Button_Etalonnage_Click(object sender, RoutedEventArgs e)
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
        void Button_GetPosition_Click(object sender, RoutedEventArgs e)
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

        void Button_DriveByVision_Click(object sender, RoutedEventArgs e)
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
        }

        List<int> ds_pix = new List<int>();
        int ds_pix_nbr = 9;
        bool newTarget = false;
        int distancepixel;

        DateTime t_last;
        TimeSpan t_vide = TimeSpan.FromSeconds(0.1);


        void NewDistancePixel(int d_pix)
        {
            ds_pix.Add(d_pix);
            if (ds_pix.Count > ds_pix_nbr)
                ds_pix.RemoveAt(0);
            else
                return;

            int v = Mediane(ds_pix.ToArray());

            if (Math.Abs(v) > bande_morte_pix)
            {
                distancepixel = v;
                newTarget = true;
            }
            else
            {
                DateTime t = DateTime.Now;
                if (t.Subtract(t_last) > t_vide)
                {
                    if (camera_pos_mm != null)
                    {
                        t_last = t;
                        Application.Current.Dispatcher.Invoke(() => NewPoint(t));
                    }
                }
            }


            //POUR TESTER GRAPH
            camera_pos_mm = v;
            DateTime ttest = DateTime.Now;
            if (ttest.Subtract(t_last) > t_vide)
            {
                if (camera_pos_mm != null)
                {
                    t_last = ttest;
                    Application.Current.Dispatcher.Invoke(() => NewPoint(ttest));
                }
            }
        }

        void NewPoint(DateTime t)
        {
            _points.Add(new PointsJJ(t, (float)camera_pos_mm));
            GraphTemporelle();
        }

        int Mediane(int[] valeurs)
        {
            List<int> listToSort = valeurs.ToList();
            listToSort.Sort();
            return listToSort[listToSort.Count / 2];
        }

        void CameraDisplacement()
        {
            int d_mm;
            float? camera_pos_precedent;
            cameraDisplacement_Running = true;
            int distancepixel_target;
            while (cameraDisplacement_Running)
            {
                if (newTarget)
                {
                    newTarget = false;
                    arduinoWaiting = false;
                    camera_pos_precedent = camera_pos_mm;
                    distancepixel_target = distancepixel;
                    d_mm = (int)(distancepixel * ratio_mm_pix);

                    d_mm /= 2;//par petit pas

                    if (distancepixel > 0)
                    {
                        //on monte
                        SendToArduino("u" + d_mm);
                    }
                    else
                    {
                        //on descend
                        SendToArduino("d" + d_mm);
                    }

                    //temps d'action
                    while (!arduinoWaiting)
                        Thread.Sleep(10);

                    //adjust ratio
                    if (camera_pos_precedent != null)
                    {
                        int delta_pixel = (distancepixel - distancepixel_target);
                        if (delta_pixel != 0)
                            ratio_mm_pix = (float)Math.Abs((float)camera_pos_precedent - (float)camera_pos_mm) / delta_pixel;

                        //ratio_mm_pix = 1;
                        //ratio_mm_pix /= 2;
                    }
                }
                Thread.Sleep(10);
            }
        }
        #endregion

        void Button_CaptureDeviceSCAN_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(Scan);
            t.Start();
        }

        void Scan()
        {
            camera_pos_high_switch = false;
            camera_pos_low_switch = false;

            camera_pos_mm = 0;
            //va à l'extrémité haute
            SendToArduino("H");
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
                    Thread.Sleep(200);
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

        #region IMAGE FILE(S)
        void Button_PickFiles_Click(object sender, MouseButtonEventArgs e)
        {
            lbx_files.Items.Clear();
            SelectFiles();
        }

        void Button_PickFilesAdd_Click(object sender, MouseButtonEventArgs e)
        {
            SelectFiles();
        }

        void SelectFiles()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
                foreach (var item in openFileDialog.FileNames)
                    lbx_files.Items.Add(item);
            if (lbx_files.Items.Count > 0)
                lbx_files.SelectedIndex = 0;
        }

        void Button_Files_Clear(object sender, MouseButtonEventArgs e)
        {
            lbx_files.Items.Clear();
        }

        void cbx_rotation_Changed(object sender, SelectionChangedEventArgs e)
        {
            switch (cbx_rotation.SelectedItem.ToString())
            {
                case "Non":
                    rotation = null;
                    break;
                case "Rotate90Clockwise":
                    rotation = RotateFlags.Rotate90Clockwise;
                    break;
                case "Rotate180":
                    rotation = RotateFlags.Rotate180;
                    break;
                case "Rotate90Counterclockwise":
                    rotation = RotateFlags.Rotate90Counterclockwise;
                    break;
            }
        }

        void lbx_files_Change(object sender, SelectionChangedEventArgs e)
        {
            if (first)
            {
                Display_Init();
                MatNamesToMats_Reset();
                first = false;
            }
            if (lbx_files.SelectedValue == null)
                return;

            frame.mat = new Mat(lbx_files.SelectedValue.ToString());

            ComputePicture(frame.mat);
        }

        private void Button_SaveData_ToDisk_Click(object sender, MouseButtonEventArgs e)
        {

        }

        #endregion

        #region Graphique
        void AddPoint(DateTime t, float valeur)
        {
            //LiveChartsCore.Series<DateTime, float>
            //var s = chart.Series.First();//.Series[0];
            //s.Points.Add(new Point(t, valeur));
        }
        #endregion
    }
}