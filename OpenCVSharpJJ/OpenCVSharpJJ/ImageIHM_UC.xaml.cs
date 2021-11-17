using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
using System.Windows.Threading;
using static OpenCVSharpJJ.MesureAmpouleADecanter;

namespace OpenCVSharpJJ
{
    public partial class ImageIHM_UC : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //variable
        public ImageType matName
        {
            get
            {
                return _matName;
            }
            set
            {
                if (_matName != value)
                {
                    _matName = value;
                    OnPropertyChanged("matName");
                }
                if (cbx_wpf.Text != _matName.ToString())
                    cbx_wpf.Text = _matName.ToString();
            }
        }
        ImageType _matName;

        public Mat mat;

        public ImageIHM_UC()
        {
            InitializeComponent();
            DataContext = this;
        }

        public System.Drawing.Bitmap _bitmap
        {
            get
            {
                if (bitmap == null)
                    return null;
                return bitmap;
            }
            set
            {
                if (bitmap != value)
                {
                    bitmap = value;
                    OnPropertyChanged("_bitmap");
                }
            }
        }
        System.Drawing.Bitmap bitmap = new Bitmap(200, 200);

        private void Image_Enter(object sender, MouseEventArgs e)
        {
            cbx_wpf.Visibility = Visibility.Visible;
        }

        private void Image_Leave(object sender, MouseEventArgs e)
        {
            cbx_wpf.Visibility = Visibility.Collapsed;
        }

        private void ImageCBX_Enter(object sender, MouseEventArgs e)
        {
            cbx_wpf.Visibility = Visibility.Visible;
        }

        private void ImageCBX_Leave(object sender, MouseEventArgs e)
        {
            cbx_wpf.Visibility = Visibility.Collapsed;
        }

        private void ImageCBX_SelectionChange(object sender, SelectionChangedEventArgs e)
        {
            matName = (ImageType)e.AddedItems[0];
        }

        internal void _UpdateCombobox(NamedMats NMs)
        {
            cbx_wpf.Items.Clear();
            foreach (var item in NMs.MatNamesToMats)
            {
                cbx_wpf.Items.Add(item.Key);
            }
        }

        internal void _Update(Dictionary<ImageType, NamedMat> matNamesToMats)
        {
            if (matNamesToMats.ContainsKey(matName))
            {
                Mat frame = matNamesToMats[matName].mat;
                if (frame != null && !frame.Empty())
                {
                    try
                    {
                        _bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);

                        Application.Current.Dispatcher.BeginInvoke(
                            DispatcherPriority.Background,
                            new Action(() =>
                            {
                                image_wpf.MaxHeight = frame.Height;
                                image_wpf.MaxWidth = frame.Width;
                            }));
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "")
                        {
                            Console.WriteLine("Erreur inconnue");
                            return;
                        }
                        throw;
                    }
                }
            }
        }
    }
}