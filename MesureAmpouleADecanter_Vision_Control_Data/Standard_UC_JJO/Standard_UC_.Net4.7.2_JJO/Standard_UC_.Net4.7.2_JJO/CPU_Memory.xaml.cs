using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Standard_UC_Net4_7_2_JJO
{
    public partial class CPU_Memory : UserControl, INotifyPropertyChanged
    {
        public int _waitPeriod_ms;

        int cpuUsageTotal;
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string CurrentCpuUsage { get => "CPU : " + CurrentCpuUsage_val + "%"; }
        public int CurrentCpuUsage_val { get => cpuUsageTotal; }
        public string AvailableRAM { get => "Memory : " + AvailableRAM_val + "MB"; }
        public int AvailableRAM_val { get => (int)System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1048576; }//1024*1024

        public CPU_Memory()
        {
            InitializeComponent();
            DataContext = this;
            _waitPeriod_ms = 1000;
            Thread thread = new Thread(() => Cpu_memory_usage());
            thread.Start();
        }

        async void Cpu_memory_usage()
        {
            while (true)
            {
                cpuUsageTotal = await GetCpuUsageForProcess();
                OnPropertyChanged("CurrentCpuUsage");
                OnPropertyChanged("AvailableRAM");
            }
        }

        async Task<int> GetCpuUsageForProcess()
        {
            timer.Restart();
            TimeSpan startCpuUsage = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(_waitPeriod_ms);
            timer.Stop();
            TimeSpan endCpuUsage = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;

            double cpuUsed_ms = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            int cpuUsageTotal = (int)(100 * cpuUsed_ms / (Environment.ProcessorCount * timer.ElapsedMilliseconds));

            return cpuUsageTotal;
        }
    }
}