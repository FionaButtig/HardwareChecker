using System.Windows;

namespace HardwareInfoApp
{
    public partial class EditHardwareWindow : Window
    {
        public HardwareSnapshot Snapshot { get; private set; }

        public EditHardwareWindow(HardwareSnapshot snapshot)
        {
            InitializeComponent();

            Snapshot = snapshot;

            CpuBox.Text = snapshot.CPU;
            GpuBox.Text = snapshot.GPU;
            RamBox.Text = snapshot.RAM;
            DiskBox.Text = snapshot.Disk;
            MotherboardBox.Text = snapshot.Motherboard;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Snapshot.CPU = CpuBox.Text;
            Snapshot.GPU = GpuBox.Text;
            Snapshot.RAM = RamBox.Text;
            Snapshot.Disk = DiskBox.Text;
            Snapshot.Motherboard = MotherboardBox.Text;

            DialogResult = true;
            Close();
        }
    }
}