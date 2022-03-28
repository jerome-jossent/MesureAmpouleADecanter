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

        public int Threshold1
        {
            get { return threshold1; }
            set
            {
                if (threshold1 == value)
                    return;
                threshold1 = value;
                OnPropertyChanged("Threshold1");
                if (!isRunning)
                    ComputePicture();
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
                if (!isRunning)
                    ComputePicture();
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
                if (!isRunning)
                    ComputePicture();
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
        ObservableCollection<string> arduinoMessages = new ObservableCollection<string>();

        public bool CameraDisplacement_Running
        {
            get
            {
                return cameraDisplacement_Running;
            }
            set
            {
                cameraDisplacement_Running = value;
                OnPropertyChanged("CameraDisplacement_Running");
            }
        }
        bool cameraDisplacement_Running = false;
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
            debug1, debug2, debug3, debug4, debug5
        }
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
        #endregion

        #region PARAMETERS
        int bande_morte_pix = 5;
        float ratio_mm_pix = 0.2f;
        int distancepixel;
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
        Mat[] bgr;

        Thread thread;
        Thread threadCommandeArduino;
        int indexDevice;
        VideoInInfo.Format format;
        Dictionary<string, VideoInInfo.Format> formats;
        VideoCapture capture;
        bool isRunning = false;
        bool first = true;
        System.Diagnostics.Stopwatch chrono = new System.Diagnostics.Stopwatch();

        OpenCvSharp.Rect roi;

        Communication_Série.Communication_Série cs;
        string buffer;
        char[] split_car = new char[] { '\n' };
        float? camera_pos;
        float? camera_pos_codeur;
        float? camera_pos_codeur_prec;
        float camera_pos_max;

        bool camera_pos_low_switch, camera_pos_high_switch;

        OpenCvSharp.Window w;
        bool display_in_OpenCVSharpWindow = false;
        #endregion

        #region CONSTANTES
        Scalar rouge = new Scalar(0, 0, 255);
        Scalar vert = new Scalar(0, 255, 0);
        Scalar blanc = new Scalar(255, 255, 255);
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

            //cbx_device.SelectedIndex = 1;
            //cbx_deviceFormat.SelectedIndex = 0;
            //Capture_Start();
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            CameraDisplacement_Stop();

            isRunning = false;
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
            isRunning = !isRunning;

            if (isRunning)
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
            if (isRunning)
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
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
                Thread.Sleep(100);
            }
            indexDevice = index;
            thread = new Thread(new ThreadStart(CaptureCameraCallback));
            thread.Start();
        }

        void CaptureCamera_Stop()
        {
            thread?.Abort();
            first = true;
            thread = null;
        }

        void CaptureCameraCallback()
        {
            int actualindexDevice = indexDevice;
            frame.mat = new Mat();
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
                        MatNamesToMats_Reset();
                        roi = new OpenCvSharp.Rect();
                        first = false;
                    }

                    //captation de l'image
                    capture.Read(frame.mat);

                    //Viewer debug
                    if (display_in_OpenCVSharpWindow)
                    {
                        if (w != null && frame.mat.Empty())
                            Cv2.DestroyWindow(w.Name);
                        else
                        {
                            if (!frame.mat.Empty())
                            {
                                if (w == null)
                                    w = new OpenCvSharp.Window();
                                w.ShowImage(frame.mat);
                                Cv2.WaitKey(1);
                            }
                        }
                    }

                    ComputePicture();
                }
            }
        }
        #endregion

        void ComputePicture()
        {
            if (!frame.mat.Empty())
                FrameProcessing1();

            UpdateDisplayImages();

            DisplayFPS();
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
            i1.matName = ImageType.rotated;
            i2.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i2.matName = ImageType.roi1;
            i3.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i3.matName = ImageType.gray1;
            i4.matName = NMs.MatNamesToMats.ElementAt(i++).Key;
            i4.matName = ImageType.bw1;
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
        }

        void Button_CaptureDeviceROI_Click(object sender, RoutedEventArgs e)
        {
            string window_name = "Valid ROI with 'Enter' or 'Space', cancel with 'c'";

            if (frame.mat.Empty())
                return;

            rotated.mat = Resize(frame.mat);
            rotated.mat = Rotation(rotated.mat);

            OpenCvSharp.Rect newroi = Cv2.SelectROI(window_name, rotated.mat, true);
            //            if (newroi.Width > 0)
            roi = newroi;
            _title = roi.ToString();
            Cv2.DestroyWindow(window_name);
        }

        void Button_CaptureDeviceROI_Save_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.roi = roi.X + "|" + roi.Y + "|" + roi.Width + "|" + roi.Height + "|";
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
        }

        Mat Rotation(Mat frame)
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

        Mat Rotation(Mat frame, RotateFlags rotation)
        {
            Mat _out = new Mat();
            switch (rotation)
            {
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

            double facteur = (double)resizeFactor / 100;
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

        void FrameProcessing1()
        {
            //filtre gaussien
            //Savitzky-Golay filter     https://docs.scipy.org/doc/scipy/reference/generated/scipy.signal.savgol_filter.html

            rotated.mat = Resize(frame.mat);

            rotated.mat = Rotation(rotated.mat);

            ROI1.mat = ROI_NewMat(rotated.mat, roi);

            //gris
            frameGray.mat = RGBToGray(ROI1.mat, Calque.all);

            //seuillage
            //frameGray.mat = frameGray.mat.Threshold(Threshold1, 255, ThresholdTypes.Binary);
            //Cv2.Canny(frameGray.mat, frameGray.mat, 50, Threshold1);





            //OpenCvSharp.XImgProc.FastLineDetector fld = OpenCvSharp.XImgProc.FastLineDetector.Create();

            //Vec4f[] lines = fld.Detect(frameGray.mat);

            //List<Vec4f> linesH = new List<Vec4f>();
            //foreach (Vec4f item in lines)
            //{
            //    var angle = Math.Atan2(item.Item3 - item.Item1, item.Item2 - item.Item0) * 180/3.14;
            //    if (angle > -10 && angle < 10)
            //        linesH.Add(item);
            //}
            //fld.DrawSegments(frameGray.mat, linesH.ToArray());





            //trouve niveau :
            //var mat3 = new Mat<byte>(bw1.mat);
            var mat3 = new Mat<byte>(frameGray.mat);
            var indexer = mat3.GetIndexer();

            //index des lignes
            int[] Y = new int[frameGray.mat.Height];
            for (int i = 0; i < Y.Length; i++)
                Y[i] = i;

            //somme par ligne
            int[] sommeSurX = new int[frameGray.mat.Height];
            for (int y = 0; y < frameGray.mat.Height; y++)
                for (int x = 0; x < frameGray.mat.Width; x++)
                    sommeSurX[y] += indexer[y, x];

            //moyenne mobile
            // moyenne sur 5 valeurs
            int[] moyennesX = new int[sommeSurX.Length];
            for (int i = 2; i < sommeSurX.Length - 2; i++)
            {
                moyennesX[i] = sommeSurX[i - 2] + sommeSurX[i - 1] + sommeSurX[i] + sommeSurX[i + 1] + sommeSurX[i + 2];
                moyennesX[i] /= 5;
            }

            //derivée primaire (simplifié : car même pas /(x2-x1)
            //et recherche des maximum et minimum
            int[] d1 = new int[sommeSurX.Length];
            int max = 0, max_index = 0;
            int min = 0;
            for (int i = 3; i < sommeSurX.Length - 3; i++)
            {
                d1[i] = moyennesX[i + 1] - moyennesX[i - 1];
            }

            //derivée seconde (simplifié : car même pas /(x2-x1)
            int[] d2 = new int[sommeSurX.Length];
            for (int i = 4; i < sommeSurX.Length - 4; i++)
            {
                d2[i] = d1[i + 1] - d1[i - 1];
            }

            for (int i = 3; i < sommeSurX.Length - 3; i++)
            {
                if (i == 3)
                {
                    max = d2[i];
                    max_index = i;

                    min = d2[i];
                }
                else
                {
                    if (d2[i] > max)
                    {
                        max = d2[i];
                        max_index = i;
                    }
                    if (d2[i] < min)
                    {
                        min = d2[i];
                    }
                }
            }

            //intersections à 0 entre le minmum et le maximum de la zone d'intérêt
            //détection de la zone d'intérêt
            int niveau_pixel;
            int n_val = d2[max_index];
            if (n_val > 0)
            {
                int index_prec = max_index;
                int index = index_prec - 1;
                while ((d2[index]) > 0)
                {
                    index_prec = index;
                    index = index_prec - 1;
                }
                //2 lignes de pixels trouvées
                niveau_pixel = index_prec;
            }
            else if (n_val < 0)
            {
                int index_prec = max_index;
                int index = index_prec + 1;
                while ((d2[index]) < 0)
                {
                    index_prec = index;
                    index = index_prec + 1;
                }
                //2 lignes de pixels trouvées
                niveau_pixel = index_prec;
            }
            else
            {
                niveau_pixel = max_index;
            }

            bw1.mat = new Mat(moyennesX.Length, frameGray.mat.Height, type: MatType.CV_8UC3, noir);

            for (int i = 1; i < moyennesX.Length; i++)
            {
                int y1 = moyennesX[i - 1] / frameGray.mat.Width;
                int y2 = moyennesX[i] / frameGray.mat.Width;
                Cv2.Line(bw1.mat, y1, i - 1, y2, i, blanc, 1);
            }

            for (int i = 1; i < moyennesX.Length; i++)
            {
                int y1 = (int)((d2[i - 1] - min) * (float)frameGray.mat.Width / (max - min));
                int y2 = (int)((d2[i] - min) * (float)frameGray.mat.Width / (max - min));
                Cv2.Line(bw1.mat, y1, i - 1, y2, i, rouge, 1);
            }

            int y0 = (int)(-min * (float)frameGray.mat.Width / (max - min));
            Cv2.Line(bw1.mat, y0, 0, y0, moyennesX.Length - 1, vert, 1);


            //Cv2.ImShow("visu", bw1.mat);
            //Cv2.WaitKey(1);

            //string list = "";
            //foreach (var item in moyennesX)
            //{
            //    list += item + "\n";
            //}

            //centre caméra
            int milieuhauteur = rotated.mat.Height / 2;
            int milieulargeur = rotated.mat.Width / 2;
            Cv2.Line(rotated.mat, milieulargeur - 50, milieuhauteur, milieulargeur + 50, milieuhauteur, rouge, 2);
            Cv2.Line(rotated.mat, milieulargeur, milieuhauteur - 50, milieulargeur, milieuhauteur + 50, rouge, 2);

            //niveau
            Cv2.Line(ROI1.mat, 0, niveau_pixel, ROI1.mat.Width - 1, niveau_pixel, vert, 1);

            distancepixel = milieuhauteur - (niveau_pixel + roi.Y);

            Cv2.PutText(rotated.mat, distancepixel.ToString(), new OpenCvSharp.Point(0, rotated.mat.Height), HersheyFonts.HersheyTriplex, 5, blanc, thickness: 4);
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

        #region COMMON IMAGE PROCESSING

        enum Calque { all, red, green, blue }
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
        #endregion
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
            string nom = "USB Serial Port (COM4)";
            if (cbx_COM.Items.Contains(nom))
                cbx_COM.Text = nom;
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
                cs = new Communication_Série.Communication_Série(cbx_COM.Text, cbx_bauds.Text, datareceived);
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

        void datareceived(object sender, SerialDataReceivedEventArgs e)
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
            try
            {
                string val_txt;
                if (txt == "CNC Shield Initialized")
                    return;

                if (txt.Contains("Position = "))
                {
                    //Serial.print("Position = ");
                    //Serial.print(d);
                    //Serial.println("mm");
                    val_txt = txt.Replace("Position = ", "");
                    val_txt = val_txt.Replace("mm", "");
                    val_txt = val_txt.Replace(".", ",");
                    camera_pos = float.Parse(val_txt);
                    SLD_camera_position((float)camera_pos);
                }
                if (txt.Contains("D max = "))
                {
                    //Serial.print("D max = ");
                    //Serial.print(d_abs_mm);
                    //Serial.println("mm");
                    val_txt = txt.Replace("D max = ", "");
                    val_txt = val_txt.Replace("mm", "");
                    val_txt = val_txt.Replace(".", ",");
                    camera_pos_max = float.Parse(val_txt);
                    SLD_camera_position_max(camera_pos_max);
                }

                if (txt.Contains("Y max atteint"))
                {
                    camera_pos_high_switch = true;
                }

                if (txt.Contains("Y min atteint"))
                {
                    camera_pos_low_switch = true;
                }

                //codeur
                if (txt.Contains("Codeur = "))
                {
                    camera_pos_codeur_prec = camera_pos_codeur;
                    val_txt = txt.Replace("Codeur = ", "");
                    camera_pos_codeur = float.Parse(val_txt);
                }

                //waiting
                if (txt == "waiting")
                {
                    arduinoWaiting = true;
                }
            }
            catch (Exception ex)
            {
                throw;
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

        void Button_Etalonnage_Click(object sender, RoutedEventArgs e)
        {
            SendToArduino("e");
        }

        void Button_UP_Click(object sender, MouseButtonEventArgs e)
        {
            SendToArduino("h");
        }

        void Button_DOWN_Click(object sender, MouseButtonEventArgs e)
        {
            SendToArduino("b");
        }
        void Button_GetPosition_Click(object sender, RoutedEventArgs e)
        {
            camera_pos = null;
            SendToArduino("p");
        }

        void Button_ARRETURGENCE_Click(object sender, RoutedEventArgs e)
        {
            SendToArduino("a");
        }
        #endregion

        #region ARDUINO - MANAGE DEPLACEMENT

        void CameraDisplacement_Init()
        {
            threadCommandeArduino = new Thread(CameraDisplacement);
            threadCommandeArduino.Start();
        }
        void CameraDisplacement_Stop()
        {

            threadCommandeArduino?.Abort();
            threadCommandeArduino = null;
        }

        //thread qui suit la valeur "distancepixel"
        void CameraDisplacement()
        {
            int d_pix;
            int d_mm;
            int distancepixel_precedent = 0;
            while (true)
            {
                if (CameraDisplacement_Running == true)
                {
                    d_pix = Math.Abs(distancepixel);
                    if (d_pix < bande_morte_pix)
                    {
                        //on est ok
                    }
                    else
                    {
                        d_mm = (int)(d_pix * ratio_mm_pix);
                        arduinoWaiting = false;
                        distancepixel_precedent = distancepixel;
                        if (distancepixel > 0)
                        {
                            //on monte
                            SendToArduino("h" + d_mm);
                        }
                        else
                        {
                            // on descend
                            SendToArduino("b" + d_mm);
                        }
                    }

                    //temps d'action
                    while (!arduinoWaiting)
                        Thread.Sleep(10);

                    //adjust ratio
                    ratio_mm_pix = (float)Math.Abs(distancepixel_precedent - distancepixel) / d_pix;
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

            ////monte de 1mm
            //SendToArduino("h1");
            camera_pos = 0;
            //va à l'extrémité haute
            SendToArduino("H");
            while (!camera_pos_high_switch)
            {
                Thread.Sleep(100);
            }

            ////va à l'extrémité basse
            //SendToArduino("B");
            int H = 4;
            OpenCvSharp.Size s = rotated.mat.Size();
            OpenCvSharp.Point p = new OpenCvSharp.Point(0, s.Height / 2 - H / 2);
            OpenCvSharp.Rect rect = new OpenCvSharp.Rect(p, new OpenCvSharp.Size(s.Width, H));

            NamedMat d = NMs.Get(ImageType.debug1);
            d.mat = new Mat();
            while (!camera_pos_low_switch)
            {
                //descend de 1mm
                float camera_pos_prec = (float)camera_pos;
                float delta_mm = 1;
                while (camera_pos_prec - camera_pos < delta_mm)// && !camera_pos_low_switch)
                {
                    SendToArduino("b1");
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

            ComputePicture();
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