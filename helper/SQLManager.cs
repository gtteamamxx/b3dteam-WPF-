using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace helper
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

        public enum Ball3D_Status
        {
            Status_Offine = 0,
            Status_Online
        }

        public static SqlConnection SqlConnection;

        public static async Task<bool> ConnectToDatabase()
        {

            using (var _sqlConnection = new SqlConnection())
            {
                SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();


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
                catch
                {
                    _sqlConnection.Close();
                    MessageBox.Show("Error while connecting to server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        public static async Task<User> GetUser(int? userid, string login = "")
        {
            string query = string.Empty;

            if (string.IsNullOrEmpty(login))
            {
                query = $"SELECT login, usertype, lastactivity, userteams FROM USERS WHERE userid = {userid};";
            }
            else
            {
                query = $"SELECT login, usertype, lastactivity, userteams FROM USERS WHERE login = {login};";
            }
             
            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    await SqlConnection.OpenAsync();

                    using (SqlDataReader rd = await command.ExecuteReaderAsync())
                    {
                        while (await rd.ReadAsync())
                        {
                            string _login = rd.GetString(0);
                            int _usertype = rd.GetInt32(1);
                            int _lastactivity = rd.GetInt32(2);
                            string _userteams = $"{rd.GetValue(3)}";

                            SqlConnection.Close();

                            return new User()
                            {
                                login = _login,
                                usertype = _usertype,
                                lastactivity = _lastactivity,
                                userteams = _userteams
                            };
                        }

                        return null;
                    }
                }
            }
            catch
            {
                SqlConnection.Close();
                return null;
            }
        }
        public static async Task<List<User>> GetUsers()
        {
            string query = $"SELECT login, usertype, lastactivity, userteams FROM USERS;";

            List<User> listOfUsers = new List<User>();

            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    await SqlConnection.OpenAsync();

                    using (SqlDataReader rd = await command.ExecuteReaderAsync())
                    {
                        while(await rd.ReadAsync())
                        {
                            string _login = rd.GetString(0);
                            int _usertype = rd.GetInt32(1);
                            int _lastactivity = rd.GetInt32(2);
                            string _userteams = $"{rd.GetValue(3)}";
                            listOfUsers.Add(new User()
                            {
                                login = _login,
                                usertype = _usertype,
                                lastactivity = _lastactivity,
                                userteams = _userteams
                            });
                        }

                        SqlConnection.Close();
                    }
                }

                return listOfUsers;
            }
            catch
            {
                SqlConnection.Close();
                return null;
            }
        }
        public static async Task<RegisterAccountStatus> RegisterNewUser(Ball3D_Status status, string login, string password, string email)
        {
            bool? userExists = await CheckIfUserExists(login);
            bool? emailExists = await CheckIfEmailWExists(email);

            if (userExists == true)
            {
                return RegisterAccountStatus.Login_Alerady_Exists;
            }
            else if (emailExists == true)
            {
                return RegisterAccountStatus.Email_Alerady_Exists;
            }
            else if ((await _RegisterNewUser(status, login, password, email)) == false || userExists == null || emailExists == null)
            {
                return RegisterAccountStatus.Failed;
            }

            return RegisterAccountStatus.Succesful;
        }

        private static async Task<bool?> _RegisterNewUser(Ball3D_Status status, string login, string password, string email)
        {
            string query = $"INSERT INTO USERS(login, password, email, usertype, lastactivity, regtime) VALUES('{login}', '{Cryptography.Sha256(password)}', '{email}', 0, {(status == Ball3D_Status.Status_Offine ? 0 : GetTimeStamp())}, {GetTimeStamp()});";

            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    await SqlConnection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    SqlConnection.Close();
                    User.ClientStatus = status;
                    return true;
                }
            }
            catch
            {
                SqlConnection.Close();
                MessageBox.Show("There was problem with registering an user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

        }

        public static async Task<Tuple<LoginAccountStatus, User>> LoginUser(string login, string password)
        {
            string query = $"SELECT * FROM USERS WHERE login = '{login}' AND password = '{password}';";

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
                            string _userfriends = $"{rd.GetValue(7)}";
                            string _messages = $"{rd.GetValue(7)}";
                            string _userteams = $"{rd.GetValue(7)}";

                            SqlConnection.Close();

                            if (_usertype == 0)
                            {
                                return new Tuple<LoginAccountStatus, User>(LoginAccountStatus.Account_Not_Activated, null);
                            }

                            return new Tuple<LoginAccountStatus, User>(LoginAccountStatus.Succesful, new User(_userid, _login, _password, _email, _lastactivity, _regtime, _usertype, _userfriends, _messages, _userteams));
                        }

                        SqlConnection.Close();

                        return new Tuple<LoginAccountStatus, User>(LoginAccountStatus.Bad_Authorization, null);
                    }
                }
            }
            catch
            {
                SqlConnection.Close();
                MessageBox.Show("There was problem with login.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Tuple<LoginAccountStatus, User>(LoginAccountStatus.Failed, null);
            }
        }

        private static async Task<bool?> CheckIfUserExists(string login)
        {
            string query = $"SELECT login FROM USERS WHERE login = '{login}';";

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
            catch
            {
                SqlConnection.Close();
                MessageBox.Show("There was problem with checking an user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private static async Task<bool?> CheckIfEmailWExists(string email)
        {
            string query = $"SELECT email FROM USERS WHERE email = '{email}';";

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
            catch
            {
                SqlConnection.Close();
                MessageBox.Show("There was problem with checking a email.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static async Task<bool> UpdateStatus(Ball3D_Status status, int userid)
        {
            if (status == Ball3D_Status.Status_Offine)
            {
                User.ClientStatus = status;
                return true;
            }

            string query = $"UPDATE USERS SET lastactivity = {GetTimeStamp()} WHERE userid = {userid};";

            try
            {
                using (var command = new SqlCommand(query, SqlConnection))
                {
                    User.ClientStatus = status;
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

        public static async Task<string> GetPlainHTMLOfEvents()
        {
            string query = "SELECT html FROM EVENTS";

            using (var command = new SqlCommand(query, SqlConnection))
            {
                try
                {
                    await SqlConnection.OpenAsync();

                    var returnPlainHtml = ((await command.ExecuteScalarAsync()) as string);

                    SqlConnection.Close();

                    return returnPlainHtml;
                }
                catch
                {
                    SqlConnection.Close();
                    return null;
                }
            }
        }

        public static async Task<string> GetPlainHTMLOfInformations()
        {
            string query = "SELECT html FROM INFORMATIONS";

            using (var command = new SqlCommand(query, SqlConnection))
            {
                try
                {
                    await SqlConnection.OpenAsync();

                    var returnPlainHtml = ((await command.ExecuteScalarAsync()) as string);

                    SqlConnection.Close();

                    return returnPlainHtml;
                }
                catch
                {
                    SqlConnection.Close();
                    return null;
                }
            }
        }
    }
}
