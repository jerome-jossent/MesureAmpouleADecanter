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

namespace Communication_Série
{
    public partial class UI_TextBox : UserControl
    {
        public UI_TextBox()
        {
            InitializeComponent();
        }

        public void _displaytext(string texte)
        {
            Dispatcher.Invoke(() => {
                _tbx.AppendText(texte);
                _tbx.ScrollToEnd();
            });
        }
    }
}
