using Advise;
using ImageProcessing;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using OpenCvSharp.Text;
using System.IO;
using System.Net;
using System.IO.Compression;
using Tesseract;

namespace OpenCVSharpJJ
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        
        public MainWindow()
        {
            InitializeComponent();

        }

        private void btn_global_Click(object sender, RoutedEventArgs e)
        {
            Global global = new Global();
            global.Show();
            this.Close();
        }

        private void btn_MesureAmpouleADecanter_Click(object sender, RoutedEventArgs e)
        {
            MesureAmpouleADecanter mesureAmpouleADecanter = new MesureAmpouleADecanter();
            mesureAmpouleADecanter.Show();
            this.Close();
        }
    }
}