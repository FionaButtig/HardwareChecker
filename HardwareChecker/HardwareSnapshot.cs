using System;

namespace HardwareInfoApp
{
    public class HardwareSnapshot
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string ComputerName { get; set; }

        public string UserName { get; set; }

        public DateTime ScanDate { get; set; }

        public string CPU { get; set; }

        public string GPU { get; set; }

        public string RAM { get; set; }

        public string Disk { get; set; }

        public string Motherboard { get; set; }
    }
}