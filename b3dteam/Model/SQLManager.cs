using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace b3dteam.Model
{
    public static class SQLManager
    {
        public enum RegisterAccountStatus
        {
            Login_Alerady_Exists,
            Email_Alerady_Exists,
            Failed,
            Succesful
        }

       public enum LoginAccountStatus
        {
            Account_Not_Activated,
            Bad_Authorization,
            Failed,
            Succesful
        }
        public static SqlConnection SqlConnection;

        public static async Task<bool> ConnectToDatabase()
        {
            
            using (var _sqlConnection = new SqlConnection())
            {
                SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();



                _sqlConnection.ConnectionString = sqlsb.ConnectionString;

                try
                {
                    await _sqlConnection.OpenAsync();

                    if (_sqlConnection.State == System.Data.ConnectionState.Open)
                    {
                        SqlConnection = new SqlConnection(sqlsb.ConnectionString);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _sqlConnection.Close();
                    MessageBox.Show("Error while connecting to server" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);                    return false;
                }
            }
        }

        public static async Task<RegisterAccountStatus> RegisterNewUser(string login, string password, string email)
        {
            bool? userExists = await CheckIfUserExists(login);
            bool? emailExists = await CheckIfEmailWExists(email);
            
            if (userExists == true)
            {
                return RegisterAccountStatus.Login_Alerady_Exists;
            }
            else if(emailExists == true)
            {
                return RegisterAccountStatus.Email_Alerady_Exists;
            }
            else if((await _RegisterNewUser(login, password, email)) == false || userExists == null || emailExists == null)
            {
                return RegisterAccountStatus.Failed;
            }

            return RegisterAccountStatus.Succesful;
        }

        private static async Task<bool?> _RegisterNewUser(string login, string password, string email)
        {
            string query = $"INSERT INTO USERS(login, password, email, usertype, lastactivity, regtime) VALUES('{login}', '{Cryptography.Sha256(password)}', '{email}', 0, {(Ball3DStatus.ClientStatus == Ball3DStatus.Ball3D_Status.Status_Offine ? 0 : GetTimeStamp())}, {GetTimeStamp()});";

            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    await SqlConnection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    SqlConnection.Close();
                    return true;
                }
            }
            catch(Exception ex)
            {
                SqlConnection.Close();
                MessageBox.Show("There was problem with registering an user." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
        }

        public static async Task<LoginAccountStatus?> LoginUser(string login, string password)
        {
            var pass = Properties.Settings.Default.rememberme == true && password.Length > 16 ? Properties.Settings.Default.password : Cryptography.Sha256(password);

            string query = $"SELECT * FROM USERS WHERE login = '{login}' AND password = '{pass}';";

            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    await SqlConnection.OpenAsync();

                    using (SqlDataReader rd = await command.ExecuteReaderAsync())
                    {
                        rd.Read();

                        if (rd.HasRows == true)
                        {
                            int _userid = rd.GetInt32(0);
                            string _login = rd.GetString(1);
                            string _password = rd.GetString(2);
                            string _email = rd.GetString(3);
                            int _usertype = rd.GetInt32(4);
                            int _lastactivity = rd.GetInt32(5);
                            int _regtime = rd.GetInt32(6);

                            SqlConnection.Close();

                            if (_usertype == 0)
                            {
                                return LoginAccountStatus.Account_Not_Activated;
                            }

                            MainWindow.ClientUser = new User(_userid, _login, _password, _email, _lastactivity, _regtime, _usertype);
                            MainWindow.ClientUser.Save();

                            return LoginAccountStatus.Succesful;
                        }

                        SqlConnection.Close();

                        return LoginAccountStatus.Bad_Authorization;
                    }
                }
            }
            catch (Exception ex)
            {
                SqlConnection.Close();
                MessageBox.Show("There was problem with login." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return LoginAccountStatus.Failed;
            }
        }

        private static async Task<bool?> CheckIfUserExists(string login)
        {
            string query = "SELECT login FROM USERS WHERE login = '" + login + "';";

            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    await SqlConnection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    SqlConnection.Close();

                    return (string)result == login;
                }
            }
            catch(Exception ex)
            {
                SqlConnection.Close();
                MessageBox.Show("There was problem with registering an user." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private static async Task<bool?> CheckIfEmailWExists(string email)
        {
            string query = "SELECT email FROM USERS WHERE email = '" + email + "';";

            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    await SqlConnection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    SqlConnection.Close();

                    return (string)result == email;
                }
            }
            catch(Exception ex)
            {
                SqlConnection.Close();
                MessageBox.Show("There was problem with registering an user." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static async Task<bool> UpdateStatus(Ball3DStatus.Ball3D_Status status, int userid)
        {
            if(status == Ball3DStatus.Ball3D_Status.Status_Offine)
            {
                return true;
            }

            string query = "UPDATE USERS SET lastactivity = " + GetTimeStamp() + " WHERE userid = " + userid + ";";

            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    await SqlConnection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    SqlConnection.Close();

                    return true;
                }
            }
            catch
            {
                SqlConnection.Close();
                return false;
            }
        }

        public static int GetTimeStamp()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
