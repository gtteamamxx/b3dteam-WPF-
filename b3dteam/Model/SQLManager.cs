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
                //buuuu no pass here :)))

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
            string query = $"INSERT INTO USERS(login, password, email, usertype, lastactivity) VALUES('{login}', '{Cryptography.Sha256(password)}', '{email}', 0, {(Ball3DStatus.ClientStatus == Ball3DStatus.Ball3D_Status.Status_Offine ? 0 : TimeSpan.FromTicks(0).Ticks)});";

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
                MessageBox.Show("There was problem with registering an user." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
        }

        public static async Task<LoginAccountStatus?> LoginUser(string login, string password)
        {
            string query = $"SELECT * FROM USERS WHERE login = '{login}' AND password = '{Cryptography.Sha256(password)}';";

            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    await SqlConnection.OpenAsync();

                    var result = await command.ExecuteScalarAsync() as User;

                    SqlConnection.Close();

                    if(result != null)
                    {
                        if(result.usertype == 0)
                        {
                            return LoginAccountStatus.Account_Not_Activated;
                        }

                        MainWindow.ClientUser = result;

                        return LoginAccountStatus.Succesful;
                    }

                    return LoginAccountStatus.Bad_Authorization;
                }
            }
            catch (Exception ex)
            {
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
                MessageBox.Show("There was problem with registering an user." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static bool UpdateStatus(Ball3DStatus.Ball3D_Status status)
        {
            string query = "UPDATE `USERS` SET lastactivity = " + TimeSpan.FromTicks(0).Ticks + " WHERE `userid` = " + ";";//userid

            using (var coomand = new SqlCommand())
            return true;
        }
    }
}
