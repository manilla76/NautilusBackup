using System;
using System.Data.SqlClient;

namespace ThermoDpSQLReader.Model
{
    public class DataService : IDataService
    {
        private string pileName = string.Empty;

        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem("Welcome to MVVM Light");
            callback(item, null);
        }

        public string ReadSQL(string pileToFind)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                string insString = @"SELECT Top 1 * FROM [Nautilus].[dbo].[ProductTable] WHERE name LIKE '%" + pileToFind + @"%'";
                conn.ConnectionString = "Server=" + Properties.Settings.Default.SqlServerAddress + ";Database=nautilus;Trusted_Connection=True;";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(insString, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        pileName = (string)reader["name"];
                    }
                    return pileName;
                }
            }
        }

    }
}