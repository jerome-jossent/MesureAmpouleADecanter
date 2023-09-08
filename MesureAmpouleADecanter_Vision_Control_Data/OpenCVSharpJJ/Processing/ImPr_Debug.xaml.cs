using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace OpenCVSharpJJ.Processing
{
    /// <summary>
    /// Logique d'interaction pour ImPr_Debug.xaml
    /// </summary>
    public partial class ImPr_Debug : UserControl, INotifyPropertyChanged
    {
        public ImPr_Debug()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public string _debuginfo
        {
            get { return debuginfo; }
            set
            {
                if (debuginfo == value)
                    return;
                debuginfo = value;
                OnPropertyChanged("_debuginfo");
            }
        }
        string debuginfo;

        public SolidColorBrush _debugcolor
        {
            get { return debugcolor; }
            set
            {
                if (debugcolor == value)
                    return;
                debugcolor = value;
                OnPropertyChanged("_debugcolor");
            }
        }
        SolidColorBrush debugcolor;

        

    }
}
