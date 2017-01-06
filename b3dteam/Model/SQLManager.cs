using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace b3dteam.Model
{
    public static class SQLManager
    {
        public static MySqlConnection SqlConnection;

        public static async Task<bool> ConnectToDatabase()
        {
            using (var _sqlConnection = new MySqlConnection())
            {
                MySqlConnectionStringBuilder mysqlcsbd = new MySqlConnectionStringBuilder();
                mysqlcsbd.UserID = "test";
                mysqlcsbd.Password = "test";
                mysqlcsbd.Server = "localhost";
                mysqlcsbd.Database = "b3dteam";

                _sqlConnection.ConnectionString = mysqlcsbd.ConnectionString;

                try
                {
                    await _sqlConnection.OpenAsync();

                    if (_sqlConnection.State == System.Data.ConnectionState.Open)
                    {
                        SqlConnection = _sqlConnection;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while connecting to server" + Environment.NewLine + ex.Message);
                    return false;
                }
            }
        }

        public static bool UpdateStatus(Ball3DStatus.Ball3D_Status status)
        {
            string query = "UPDATE `USERS` SET lastactivity = " + TimeSpan.FromTicks(0).Ticks + " WHERE `userid` = " + ";";//userid

            using (var coomand = new MySqlCommand())
            return true;
        }
    }
}
