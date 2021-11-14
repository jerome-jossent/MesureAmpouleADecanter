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

            MesureAmpouleADecanter mesureAmpouleADecanter = new MesureAmpouleADecanter();
            mesureAmpouleADecanter.Show();
            this.Close();
        }
    }
}