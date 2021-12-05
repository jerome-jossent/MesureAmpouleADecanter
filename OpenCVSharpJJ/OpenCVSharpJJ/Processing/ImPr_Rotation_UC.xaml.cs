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
    public partial class ImPr_Rotation_UC : UserControl, INotifyPropertyChanged
    {
        private ImPr_Rotation imPr_Rotation;

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
                if (imPr_Rotation != null)
                    return imPr_Rotation._actived;
                else
                    return false;
            }
            set
            {
                if (imPr_Rotation._actived == value)
                    return;
                imPr_Rotation._actived = value;
                OnPropertyChanged("_actived");
            }
        }

        public RotateFlags _rotationType
        {
            get
            {
                if (imPr_Rotation != null)
                    return imPr_Rotation.rotationType;
                else
                    return RotateFlags.Rotate180;
            }
            set
            {
                if (imPr_Rotation.rotationType == value)
                    return;
                imPr_Rotation.rotationType = value;
                OnPropertyChanged("_rotationType");
                imPr_Rotation.Update_string();
            }
        }
        #endregion

        public ImPr_Rotation_UC()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Link(ImPr_Rotation imPr_Rotation) { this.imPr_Rotation = imPr_Rotation; }

    }
}
