using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace HardwareInfoApp
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase(dbPath);
        }

        private void InitializeDatabase(string dbPath)
        {
            if (!File.Exists(dbPath))
                SQLiteConnection.CreateFile(dbPath);

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Snapshots (
                        Id          TEXT PRIMARY KEY,
                        DisplayName TEXT NOT NULL,
                        ComputerName TEXT,
                        UserName    TEXT,
                        ScanDate    TEXT NOT NULL,
                        CPU         TEXT,
                        GPU         TEXT,
                        RAM         TEXT,
                        Disk        TEXT,
                        Motherboard TEXT
                    );";
                using (var cmd = new SQLiteCommand(sql, conn))
                    cmd.ExecuteNonQuery();
            }
        }

        public void SaveSnapshot(HardwareSnapshot snap)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT OR REPLACE INTO Snapshots
                        (Id, DisplayName, ComputerName, UserName, ScanDate, CPU, GPU, RAM, Disk, Motherboard)
                    VALUES
                        (@Id, @DisplayName, @ComputerName, @UserName, @ScanDate, @CPU, @GPU, @RAM, @Disk, @Motherboard);";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", snap.Id);
                    cmd.Parameters.AddWithValue("@DisplayName", snap.DisplayName);
                    cmd.Parameters.AddWithValue("@ComputerName", snap.ComputerName);
                    cmd.Parameters.AddWithValue("@UserName", snap.UserName);
                    cmd.Parameters.AddWithValue("@ScanDate", snap.ScanDate.ToString("O"));
                    cmd.Parameters.AddWithValue("@CPU", snap.CPU);
                    cmd.Parameters.AddWithValue("@GPU", snap.GPU);
                    cmd.Parameters.AddWithValue("@RAM", snap.RAM);
                    cmd.Parameters.AddWithValue("@Disk", snap.Disk);
                    cmd.Parameters.AddWithValue("@Motherboard", snap.Motherboard);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteSnapshot(string id)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Snapshots WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<HardwareSnapshot> LoadAllSnapshots()
        {
            var list = new List<HardwareSnapshot>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Snapshots ORDER BY ScanDate DESC;";
                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new HardwareSnapshot
                        {
                            Id = reader["Id"].ToString(),
                            DisplayName = reader["DisplayName"].ToString(),
                            ComputerName = reader["ComputerName"].ToString(),
                            UserName = reader["UserName"].ToString(),
                            ScanDate = DateTime.Parse(reader["ScanDate"].ToString()),
                            CPU = reader["CPU"].ToString(),
                            GPU = reader["GPU"].ToString(),
                            RAM = reader["RAM"].ToString(),
                            Disk = reader["Disk"].ToString(),
                            Motherboard = reader["Motherboard"].ToString()
                        });
                    }
                }
            }

            return list;
        }
    }
}
