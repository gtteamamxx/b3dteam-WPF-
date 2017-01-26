using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLC
{
    public class Connection
    {
        public static SqlConnection GetConnection()
        {
            SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();


            return new SqlConnection(sqlsb.ConnectionString);
        }
    }
}
