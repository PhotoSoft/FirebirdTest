using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdTest
{
    internal class FirebirdClient
    {
        private readonly int _rowsCount;
        private readonly string _databasePath;
        private readonly byte[] _imgData;

        public FirebirdClient(bool deleteDb)
        {
            _rowsCount = 1000;
            _databasePath = $"{Application.StartupPath}\\database.fdb";
            if (deleteDb)
            {
                File.Delete(_databasePath);
            }

            string imgPath = Application.StartupPath + "\\img.jpg";
            if (!File.Exists(imgPath))
            {
                throw new Exception("Copy img.jpg to folder with FirebirdTest.exe.");
            }

            _imgData = File.ReadAllBytes(imgPath);

            CheckIfDatabaseExists();
        }

        public void UpdateTest()
        {
            var sw = Stopwatch.StartNew();

            using (var dbConnection = new FbConnection(GetConnectionString()))
            {
                dbConnection.Open();

                using (var dbCmd = new FbCommand(null, dbConnection))
                {
                    dbCmd.CommandText = "UPDATE files SET blob_0=@blob_0 WHERE id=@id";

                    for (int i = 0; i < _rowsCount; i++)
                    {
                        dbCmd.Parameters.Clear();
                        dbCmd.Parameters.AddWithValue("@id", i);
                        dbCmd.Parameters.AddWithValue("@blob_0", _imgData);

                        dbCmd.ExecuteNonQuery();
                    }
                }

                dbConnection.Close();
            }

            sw.Stop();
            MessageBox.Show($"Update execution time: {sw.Elapsed.ToString(@"m\:ss")}", "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CheckIfDatabaseExists()
        {
            if (File.Exists(_databasePath)) return;

            CreateDatabase();
        }

        private void CreateDatabase()
        {
            using (var dbConnection = new FbConnection(GetConnectionString()))
            {
                FbConnection.CreateDatabase(dbConnection.ConnectionString);

                dbConnection.Open();

                using (var dbCmd = new FbCommand(null, dbConnection))
                {
                    string table = @"CREATE TABLE FILES
                    (
                      ID Integer,
                      BLOB_0 Blob sub_type 0 DEFAULT '',
                      PRIMARY KEY (ID)
                    )";

                    dbCmd.CommandText = table;
                    dbCmd.ExecuteNonQuery();

                    FillNewDatabase(dbCmd);
                }

                dbConnection.Close();
            }
        }

        private void FillNewDatabase(FbCommand dbCmd)
        {
            dbCmd.CommandText = "INSERT INTO files (id) VALUES (@id)";

            for (int i = 0; i < _rowsCount; i++)
            {
                dbCmd.Parameters.Clear();
                dbCmd.Parameters.AddWithValue("@id", i);

                dbCmd.ExecuteNonQuery();
            }
        }

        private string GetConnectionString()
        {
            var sb = new FbConnectionStringBuilder();
            sb.ServerType = FbServerType.Default;
            sb.Database = $"localhost:{_databasePath}";
            sb.UserID = "SYSDBA";
            sb.Password = "masterkey";
            sb.Pooling = true;

            return sb.ConnectionString;
        }
    }
}