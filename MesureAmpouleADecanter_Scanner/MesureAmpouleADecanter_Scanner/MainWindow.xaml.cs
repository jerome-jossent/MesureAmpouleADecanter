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

using WIA;
using System.IO;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel;
using Xceed.Wpf.Toolkit;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace MesureAmpouleADecanter_Scanner
{
    //https://ourcodeworld.com/articles/read/382/creating-a-scanning-application-in-winforms-with-csharp
    //https://github.com/MicrosoftDocs/win32/blob/docs/desktop-src/wia/-wia-wiaitempropscanneritem.md

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        ImageFile image = null;

        public Parametres parametres
        {
            get => _parametres;
            set
            {
                _parametres = value;
                OnPropertyChanged();
            }
        }
        Parametres _parametres = new Parametres();

        public ObservableCollection<Scanner> scannerList
        {
            get => _scannerList;
            set
            {
                _scannerList = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<Scanner> _scannerList = new ObservableCollection<Scanner>();

        public Scanner scanner
        {
            get => _scanner;
            set
            {
                if (_scanner == value) return;
                _scanner = value;
                OnPropertyChanged();
                GetParameters(scanner);
            }
        }
        Scanner _scanner;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetScanners();
        }

        void GetScanners()
        {
            Dictionary<int, Scanner> scanners = GetScannersList();
            scannerList = new ObservableCollection<Scanner>();
            foreach (Scanner scanner in scanners.Values)
                scannerList.Add(scanner);

            if (scanners.Count >= 1)            
                scanner = scanners.First().Value;            
        }

        void GetParameters(Scanner scanner)
        {
            if (scanner.scanner == null)
                return;

            Device device = scanner.scanner.Connect();

            // Select the scanner
            Item scannerItem = device.Items[1];
            parametres = new Parametres(scannerItem);
        }

        void Scan()
        {
            try
            {
                DeviceInfo firstDevice = scanner.scanner;

                if (firstDevice == null)
                    return;

                DateTime t0 = DateTime.Now;
                image = Scan(firstDevice, parametres);
                DateTime t1 = DateTime.Now;

                _lbl.Content = image.Width + "x" + image.Height + " [" + (t1 - t0).TotalSeconds.ToString("0.00") + "s]";
            }
            catch (COMException ex) { DisplayError(ex); }

            if (image == null)
                return;

            DisplayImage(image);
        }

        void DisplayError(COMException ex)
        {
            uint errorCode = (uint)ex.ErrorCode;
            string erreur = "";
            if (errorCode == 0x80210006) erreur = "The scanner is busy or isn't ready";
            else if (errorCode == 0x80210064) erreur = "The scanning process has been cancelled.";
            else if (errorCode == 0x8021000C) erreur = "There is an incorrect setting on the WIA device.";
            else if (errorCode == 0x80210005) erreur = "The device is offline. Make sure the device is powered on and connected to the PC.";
            else if (errorCode == 0x80210001) erreur = "An unknown error has occurred with the WIA device.";
            if (erreur != "")
                System.Windows.MessageBox.Show(erreur, "COMException", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                System.Windows.MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        Dictionary<int, Scanner> GetScannersList()
        {
            Dictionary<int, Scanner> scanners = new Dictionary<int, Scanner>();

            DeviceManager deviceManager = new DeviceManager();

            //commence à partir de 1 !
            for (int i = 1; i <= deviceManager.DeviceInfos.Count; i++)
            {
                if (deviceManager.DeviceInfos[i].Type != WiaDeviceType.ScannerDeviceType)
                    continue;
                DeviceInfo scanner = deviceManager.DeviceInfos[i];
                Scanner s = new Scanner(i, scanner);
                scanners.Add(i, s);
            }

            return scanners;
        }

        ImageFile Scan(DeviceInfo firstDevice, Parametres parametres)
        {
            // Connect to the first available scanner
            Device device = firstDevice.Connect();

            // Select the scanner
            Item scannerItem = device.Items[1];

            // Applique les paramètres
            Parametres.AdjustScannerSettings(scannerItem, parametres);

            // SCAN : Retrieve an image in JPEG format and store it into a variable
            ImageFile imageFile = (ImageFile)scannerItem.Transfer(Parametres.WIAFormats[Parametres.WIA_Format.BMP]);

            return imageFile;
        }

        void DisplayImage(ImageFile image)
        {
            var imageBytes = (byte[])image.FileData.get_BinaryData();
            var ms = new MemoryStream(imageBytes);

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = ms;
            imageSource.EndInit();
            _img.Source = imageSource;
        }

        void Save_Click(object sender, RoutedEventArgs e)
        {
            Save(image);
        }

        void Save(ImageFile image)
        {
            // Save the image in some path with filename
            string path = @"scan.jpeg";

            if (File.Exists(path))
                File.Delete(path);

            // Save image !
            //            image.SaveFile(path);

            Save((BitmapImage)_img.Source, path);
        }

        void Save(BitmapImage image, string filePath)
        {
            BitmapEncoder encoder = new JpegBitmapEncoder();// new  PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            string argument = "/select, \"" + filePath + "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        void Scan_Click(object sender, RoutedEventArgs e)
        {
            Scan();
        }

        void ResolutionMax(object sender, RoutedEventArgs e)
        {
            parametres.width = parametres.width_max;
            parametres.height = parametres.height_max;
        }

        void ScanForever_Click(object sender, RoutedEventArgs e)
        {
            //T0
            //16 lignes
            //TFin
            //moyenner pixel et temps
            //recommencer
        }
    }
}