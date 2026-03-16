using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualBasic;

namespace HardwareInfoApp
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _db;

        public MainWindow()
        {
            InitializeComponent();

            string dbFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "HardwareLibrary");

            Directory.CreateDirectory(dbFolder);

            string dbPath = Path.Combine(dbFolder, "hardware.db");
            _db = new DatabaseService(dbPath);

            LoadLibrary();
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            ScanAndDisplayCurrentPC();
        }

        private void SnapshotList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SnapshotList.SelectedItem is HardwareSnapshot snap)
                DisplaySnapshot(snap);
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var snap = SnapshotList.SelectedItem as HardwareSnapshot;
            if (snap == null) return;

            string input = Interaction.InputBox(
                "Neuen Namen eingeben:",
                "Hardware Eintrag umbenennen",
                snap.DisplayName);

            if (string.IsNullOrWhiteSpace(input))
                return;

            snap.DisplayName = input;
            _db.SaveSnapshot(snap);
            LoadLibrary();
        }

        private void EditHardwareButton_Click(object sender, RoutedEventArgs e)
        {
            var snap = SnapshotList.SelectedItem as HardwareSnapshot;
            if (snap == null) return;

            var window = new EditHardwareWindow(snap);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                _db.SaveSnapshot(snap);
                DisplaySnapshot(snap);
                LoadLibrary();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var snap = SnapshotList.SelectedItem as HardwareSnapshot;
            if (snap == null) return;

            var result = MessageBox.Show(
                "Diesen Hardware Eintrag Löschen?",
                "Löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            _db.DeleteSnapshot(snap.Id);
            InfoPanel.Children.Clear();
            LoadLibrary();
        }

        private void ScanAndDisplayCurrentPC()
        {
            string name = Interaction.InputBox(
                "Bitte einen Namen für diesen Scan eingeben:",
                "Scan benennen",
                Environment.MachineName);

            if (string.IsNullOrWhiteSpace(name))
                name = Environment.MachineName + " (" + DateTime.Now.ToString("g") + ")";

            var snapshot = CreateSnapshot();
            snapshot.DisplayName = name;

            _db.SaveSnapshot(snapshot);
            LoadLibrary();
            DisplaySnapshot(snapshot);
        }

        private void LoadLibrary()
        {
            SnapshotList.ItemsSource = _db.LoadAllSnapshots();
        }

        private HardwareSnapshot CreateSnapshot()
        {
            return new HardwareSnapshot
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = Environment.MachineName + " (" + DateTime.Now.ToString("g") + ")",
                ComputerName = Environment.MachineName,
                UserName = Environment.UserName,
                ScanDate = DateTime.Now,
                CPU = ReadCPUInfo(),
                GPU = ReadGPUInfo(),
                RAM = ReadRAMInfo(),
                Disk = ReadDiskInfo(),
                Motherboard = ReadMotherboardInfo()
            };
        }

        private void DisplaySnapshot(HardwareSnapshot snap)
        {
            InfoPanel.Children.Clear();

            AddSection("Computer",
                snap.DisplayName +
                "\nUser: " + snap.UserName +
                "\nScan: " + snap.ScanDate.ToString("g"));

            AddSection("CPU", snap.CPU);
            AddSection("GPU", snap.GPU);
            AddSection("RAM", snap.RAM);
            AddSection("Disk", snap.Disk);
            AddSection("Motherboard", snap.Motherboard);
        }

        private void AddSection(string title, string content)
        {
            InfoPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DeepSkyBlue,
                Margin = new Thickness(0, 10, 0, 5)
            });

            InfoPanel.Children.Add(new TextBlock
            {
                Text = content,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            });

            InfoPanel.Children.Add(new Separator
            {
                Margin = new Thickness(0, 5, 0, 10)
            });
        }

        private string ReadCPUInfo()
        {
            var sb = new StringBuilder();
            using (var searcher = new ManagementObjectSearcher("select * from Win32_Processor"))
                foreach (var obj in searcher.Get())
                {
                    sb.AppendLine("Name: " + obj["Name"]);
                    sb.AppendLine("Cores: " + obj["NumberOfCores"]);
                    sb.AppendLine("Logical Processors: " + obj["NumberOfLogicalProcessors"]);
                    sb.AppendLine("Max Clock: " + obj["MaxClockSpeed"] + " MHz");
                }
            return sb.ToString();
        }

        private string ReadGPUInfo()
        {
            var sb = new StringBuilder();
            using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
                foreach (var obj in searcher.Get())
                {
                    double ramGB = Math.Round(
                        Convert.ToDouble(obj["AdapterRAM"]) / (1024 * 1024 * 1024), 2);
                    sb.AppendLine("Name: " + obj["Name"]);
                    sb.AppendLine("Driver: " + obj["DriverVersion"]);
                    sb.AppendLine("VRAM: " + ramGB + " GB");
                }
            return sb.ToString();
        }

        private string ReadRAMInfo()
        {
            using (var searcher = new ManagementObjectSearcher("select * from Win32_ComputerSystem"))
                foreach (var obj in searcher.Get())
                {
                    double total = Convert.ToDouble(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                    return "Installed RAM: " + Math.Round(total, 2) + " GB";
                }
            return "Unknown";
        }

        private string ReadDiskInfo()
        {
            var sb = new StringBuilder();
            using (var searcher = new ManagementObjectSearcher("select * from Win32_DiskDrive"))
                foreach (var obj in searcher.Get())
                {
                    double size = Math.Round(Convert.ToDouble(obj["Size"]) / (1024 * 1024 * 1024), 2);
                    sb.AppendLine("Model: " + obj["Model"]);
                    sb.AppendLine("Interface: " + obj["InterfaceType"]);
                    sb.AppendLine("Size: " + size + " GB");
                    sb.AppendLine();
                }
            return sb.ToString();
        }

        private string ReadMotherboardInfo()
        {
            var sb = new StringBuilder();
            using (var searcher = new ManagementObjectSearcher("select * from Win32_BaseBoard"))
                foreach (var obj in searcher.Get())
                {
                    sb.AppendLine("Manufacturer: " + obj["Manufacturer"]);
                    sb.AppendLine("Product: " + obj["Product"]);
                    sb.AppendLine("Serial: " + obj["SerialNumber"]);
                }
            return sb.ToString();
        }
    }
}
