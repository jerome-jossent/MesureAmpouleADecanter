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

using System.ComponentModel;

namespace OpenCVSharpJJ.Processing
{
    public partial class ImPr_ListBoxItem : UserControl, INotifyPropertyChanged
    {
        ImPr imPr;

        #region BINDINGS IHM
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string _titre
        {
            get
            {
                return imPr.imPrType.ToString();
            }
        }

        public string _info
        {
            get
            {
                return l_info;
            }
            set
            {
                if (l_info == value)
                    return;
                l_info = value;
                OnPropertyChanged("_info");
            }
        }
        string l_info;
        #endregion

        public ImPr_ListBoxItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void ImPr_Link(ImPr imPr)
        {
            this.imPr = imPr;
        }
    }
}
