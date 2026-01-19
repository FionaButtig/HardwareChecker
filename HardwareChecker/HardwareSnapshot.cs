using System;

namespace HardwareInfoApp
{
    public class HardwareSnapshot
    {
        public string Id { get; set; }              // unique file ID
        public string DisplayName { get; set; }     // editable name

        public string ComputerName { get; set; }
        public string UserName { get; set; }
        public DateTime ScanDate { get; set; }

        public string CPU { get; set; }
        public string GPU { get; set; }
        public string RAM { get; set; }
        public string Disk { get; set; }
        public string Motherboard { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
