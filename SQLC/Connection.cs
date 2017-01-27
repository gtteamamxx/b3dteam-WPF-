using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLC
{
    public class Connection
    {
        public static MySqlConnection GetConnection()
        {
            MySqlConnectionStringBuilder sqlsb = new MySqlConnectionStringBuilder();



            return new MySqlConnection(sqlsb.ConnectionString);
        }
    }
}
