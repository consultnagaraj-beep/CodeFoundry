using MySql.Data.MySqlClient;
using System.Data;

namespace CodeFoundry.Generator.IO
{
    public static class DbHelper
    {
        public static MySqlConnection OpenConnection(string connStr)
        {
            var c = new MySqlConnection(connStr);
            c.Open();
            return c;
        }
    }
}
