using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using WIA;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Diagnostics.Metrics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MesureAmpouleADecanter_Scanner
{
    public class Parametres : INotifyPropertyChanged
    {
        public enum WIA_Format { BMP, PNG, GIF, JPG, TIFF };
        public static Dictionary<WIA_Format, string> WIAFormats = new Dictionary<WIA_Format, string>() {
                {WIA_Format.BMP , "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}" },
                {WIA_Format.PNG , "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}" },
                {WIA_Format.GIF , "{B96B3CB0-0728-11D3-9D7B-0000F81EF32E}" },
                {WIA_Format.JPG , "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}" },
                {WIA_Format.TIFF , "{B96B3CB1-0728-11D3-9D7B-0000F81EF32E}" }
            };

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public Dictionary<int, Resolution_dpi> resolutions { get => _resolutions; }
        Dictionary<int, Resolution_dpi> _resolutions;


        public int dpi_H
        {
            get => _dpi_H; set
            {
                if (_dpi_H == value) return;
                _dpi_H = value;
                OnPropertyChanged();
                OnPropertyChanged("width_max");
                OnPropertyChanged("width_min");
                width = width_max;
            }
        }
        int _dpi_H;

        public int dpi_V
        {
            get => _dpi_V; set
            {
                if (_dpi_V == value) return;
                _dpi_V = value;
                OnPropertyChanged();
                OnPropertyChanged("height_max");
                OnPropertyChanged("height_min");
                height = height_max;
            }
        }
        int _dpi_V;

        public int top
        {
            get => _top; set
            {
                if (_top == value) return;
                _top = value;
                OnPropertyChanged();
            }
        }
        int _top;

        public int width
        {
            get => _width; set
            {
                if (_width == value) return;
                _width = value;
                OnPropertyChanged();
            }
        }
        int _width;

        public int left
        {
            get => _left; set
            {
                if (_left == value) return;
                _left = value;
                OnPropertyChanged();
            }
        }
        int _left;

        public int height
        {
            get => _height; set
            {
                if (_height == value) return;
                _height = value;
                OnPropertyChanged();
            }
        }
        int _height;

        public int brightness
        {
            get => _brightness; set
            {
                if (_brightness == value) return;
                _brightness = value;
                OnPropertyChanged();
            }
        }
        int _brightness;

        public int contrast
        {
            get => _contrast; set
            {
                if (_contrast == value) return;
                _contrast = value;
                OnPropertyChanged();
            }
        }
        int _contrast;

        public int colorMode
        {
            get => _colorMode; set
            {
                if (_colorMode == value) return;
                _colorMode = value;
                OnPropertyChanged();
            }
        }
        int _colorMode;

        public List<int> dpis
        {
            get => _dpis; set
            {
                if (_dpis == value) return;
                _dpis = value;
                OnPropertyChanged();
            }
        }
        List<int> _dpis;

        public int width_max
        {
            get
            {
                if (_resolutions == null) return 0;
                return _resolutions[dpi_H].width;
            }
        }

        public int height_max
        {
            get
            {
                if (_resolutions == null) return 0;
                return _resolutions[dpi_V].height;
            }
        }
        public int width_min
        {
            get
            {
                if (_resolutions == null) return 0;
                return _resolutions[dpi_H].width_min;
            }
        }

        public int height_min
        {
            get
            {
                if (_resolutions == null) return 0;
                return _resolutions[dpi_V].height_min;
            }
        }

        #region Constantes
        const string WIA_SCAN_COLOR_MODE = "6146";
        const string WIA_HORIZONTAL_SCAN_RESOLUTION_DPI = "6147";
        const string WIA_VERTICAL_SCAN_RESOLUTION_DPI = "6148";
        const string WIA_HORIZONTAL_SCAN_START_PIXEL = "6149";
        const string WIA_VERTICAL_SCAN_START_PIXEL = "6150";
        const string WIA_HORIZONTAL_SCAN_SIZE_PIXELS = "6151";
        const string WIA_VERTICAL_SCAN_SIZE_PIXELS = "6152";
        const string WIA_SCAN_BRIGHTNESS_PERCENTS = "6154";
        const string WIA_SCAN_CONTRAST_PERCENTS = "6155";
        #endregion

        public Parametres() { }

        public Parametres(IItem scannerItem)
        {
            foreach (object? item in scannerItem.Properties)
            {
                //"Item Name"
                //"Full Item Name"
                //"Rotation"
                //"Orientation"
                //"Compression"
                //"Pixels Per Line"
                //"Number of Lines"
                //"Filename extension"
            }
            Property PixelsPerLine = scannerItem.Properties["Pixels Per Line"];
            Property NumberofLines = scannerItem.Properties["Number of Lines"];

            //PORTRAIT	0 degrees.
            //LANDSCAPE   90 - degree counter - clockwise rotation, relative to the PORTRAIT orientation.
            //ROT180  180 - degree counter - clockwise rotation, relative to the PORTRAIT orientation.
            //ROT270  270 - degree counter - clockwise rotation, relative to the PORTRAIT orientation.

            //Property RotationProperty = scannerItem.Properties["Orientation"];
            //RotationProperty.set_Value(1);

            Property xResolutionProperty = scannerItem.Properties["Horizontal Resolution"];
            dpis = new List<int>();
            switch (xResolutionProperty.SubType)
            {
                case WiaSubType.ListSubType:
                    // Here you can access property SubTypeValues, which contains a list of allowed values for the resolution.
                    foreach (object? item in xResolutionProperty.SubTypeValues)
                        dpis.Add((int)item);
                    break;

                case WiaSubType.RangeSubType:
                    // Here you can access SubTypeMin and SubTypeMax properties, with the minimum and maximum allowed values.
                    dpis.Add((int)xResolutionProperty.SubTypeMin);
                    dpis.Add((int)xResolutionProperty.SubTypeDefault);
                    dpis.Add((int)xResolutionProperty.SubTypeMax);
                    break;

                case WiaSubType.UnspecifiedSubType:
                    // Here you can access SubTypeDefault property, which contains the default resolution value.
                    break;
            }

            dpi_H = (int)GetWIAProperty(scannerItem.Properties, WIA_HORIZONTAL_SCAN_RESOLUTION_DPI);
            dpi_V = (int)GetWIAProperty(scannerItem.Properties, WIA_VERTICAL_SCAN_RESOLUTION_DPI);

            left = (int)GetWIAProperty(scannerItem.Properties, WIA_HORIZONTAL_SCAN_START_PIXEL);
            top = (int)GetWIAProperty(scannerItem.Properties, WIA_VERTICAL_SCAN_START_PIXEL);

            width = (int)GetWIAProperty(scannerItem.Properties, WIA_HORIZONTAL_SCAN_SIZE_PIXELS);
            height = (int)GetWIAProperty(scannerItem.Properties, WIA_VERTICAL_SCAN_SIZE_PIXELS);

            brightness = (int)GetWIAProperty(scannerItem.Properties, WIA_SCAN_BRIGHTNESS_PERCENTS);
            contrast = (int)GetWIAProperty(scannerItem.Properties, WIA_SCAN_CONTRAST_PERCENTS);
            colorMode = (int)GetWIAProperty(scannerItem.Properties, WIA_SCAN_COLOR_MODE);

            _resolutions = new Dictionary<int, Resolution_dpi>();
            foreach (int dpi in dpis)
            {
                int w = width * dpi / dpi_H;
                int h = height * dpi / dpi_H;
                _resolutions.Add(dpi, new Resolution_dpi(dpi, w, h));
            }
        }

        public Parametres(Parametres from)
        {
            dpi_H = from.dpi_H;
            //dpi_V = from.dpi_V;
            left = from.left;
            top = from.top;
            width = from.width;
            height = from.height;
            brightness = from.brightness;
            contrast = from.contrast;
            colorMode = from.colorMode;
        }

        internal Parametres Copy()
        {
            return new Parametres(this);
        }

        static object GetWIAProperty(IProperties properties, object propName)
        {
            Property prop = properties.get_Item(ref propName);
            dynamic val = prop.get_Value();
            return val;
        }

        static void SetWIAProperty(IProperties properties, object propName, object propValue)
        {
            Property prop = properties.get_Item(ref propName);
            prop.set_Value(ref propValue);
        }

        public static void AdjustScannerSettings(IItem scannerItem, Parametres parametres)
        {
            SetWIAProperty(scannerItem.Properties, WIA_HORIZONTAL_SCAN_RESOLUTION_DPI, parametres.dpi_H);
            SetWIAProperty(scannerItem.Properties, WIA_VERTICAL_SCAN_RESOLUTION_DPI, parametres.dpi_V);

            SetWIAProperty(scannerItem.Properties, WIA_HORIZONTAL_SCAN_START_PIXEL, parametres.left);
            SetWIAProperty(scannerItem.Properties, WIA_VERTICAL_SCAN_START_PIXEL, parametres.top);

            SetWIAProperty(scannerItem.Properties, WIA_HORIZONTAL_SCAN_SIZE_PIXELS, parametres.width);
            SetWIAProperty(scannerItem.Properties, WIA_VERTICAL_SCAN_SIZE_PIXELS, parametres.height);

            SetWIAProperty(scannerItem.Properties, WIA_SCAN_BRIGHTNESS_PERCENTS, parametres.brightness);
            SetWIAProperty(scannerItem.Properties, WIA_SCAN_CONTRAST_PERCENTS, parametres.contrast);
            SetWIAProperty(scannerItem.Properties, WIA_SCAN_COLOR_MODE, parametres.colorMode);
        }
    }
}
