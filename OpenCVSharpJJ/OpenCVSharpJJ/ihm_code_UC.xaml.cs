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

namespace OpenCVSharpJJ
{
    /// <summary>
    /// Logique d'interaction pour ihm_code_UC.xaml
    /// </summary>
    public partial class ihm_code_UC : UserControl
    {
        double _score;
        double _seuil;
        public ihm_code_UC()
        {
            InitializeComponent();
            _SetLed(false);
        }

        public void _SetSeuil(double val)
        {
            _seuil = val;
        }

        public void _SetLed(bool val)
        {
            _led.Fill = (val) ? Brushes.Green : Brushes.Red;
        }
        public void _SetScore(double val)
        {
            _score = val;
            Application.Current.Dispatcher.Invoke(()=>{
                __score.Content = _score.ToString("P1");
                _progressbar.Value = _score;
                _SetLed(_score >= _seuil);
            });
        }
    }
}