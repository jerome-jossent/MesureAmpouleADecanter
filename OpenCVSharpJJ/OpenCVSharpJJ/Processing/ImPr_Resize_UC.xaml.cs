using System;
using System.Collections.Generic;
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

using OpenCvSharp;
using System.ComponentModel;

namespace OpenCVSharpJJ.Processing
{
    public partial class ImPr_Resize_UC : UserControl, INotifyPropertyChanged
    {
        private ImPr_Resize imPr_Resize;

        #region BINDINGS IHM
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool _actived
        {
            get
            {
                if (imPr_Resize != null)
                    return imPr_Resize._actived;
                else
                    return false;
            }
            set
            {
                if (imPr_Resize._actived == value)
                    return;
                imPr_Resize._actived = value;
                OnPropertyChanged("_actived");
            }
        }

        public int _width
        {
            get
            {
                if (imPr_Resize != null)
                    return imPr_Resize.size.Width;
                else
                    return 0;
            }
            set
            {
                if (imPr_Resize.size.Width == value)
                    return;
                imPr_Resize.size = new OpenCvSharp.Size(value, imPr_Resize.size.Height);
                OnPropertyChanged("_width");
                imPr_Resize.Update_string();
            }
        }
        public int _height
        {
            get
            {
                if (imPr_Resize != null)
                    return imPr_Resize.size.Height;
                else
                    return 0;
            }
            set
            {
                if (imPr_Resize.size.Height == value)
                    return;
                imPr_Resize.size = new OpenCvSharp.Size(imPr_Resize.size.Width, value);
                OnPropertyChanged("_height");
                imPr_Resize.Update_string();
            }
        }

        public InterpolationFlags _interpolationType
        {
            get
            {
                return imPr_Resize.interpolationType;
            }
            set
            {
                if (imPr_Resize.interpolationType == value)
                    return;
                imPr_Resize.interpolationType = value;
                OnPropertyChanged("_interpolationType");
                imPr_Resize.Update_string();
            }
        }
        #endregion

        public ImPr_Resize_UC()
        {
            InitializeComponent();
            DataContext = this;
        }
        public void Link(ImPr_Resize imPr_Resize) { this.imPr_Resize = imPr_Resize; }

    }
}
