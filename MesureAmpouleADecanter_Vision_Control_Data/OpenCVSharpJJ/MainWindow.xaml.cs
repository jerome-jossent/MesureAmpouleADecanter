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
           
            new MesureAmpouleADecanter().Show();
            //new CaptureVideo().Show();

            this.Close();
        }
    }
}