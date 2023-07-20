using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CPUFramework
{
    public class SQLUtility
    {
        public static string ConnectionString = "";
        public static DataTable GetDataTable(string sqlstatement) //take a sql statement ans return a datatable
        {
            Debug.Print(sqlstatement);
            DataTable dt = new();
            SqlConnection conn = new();
            conn.ConnectionString = ConnectionString;
            conn.Open();

            var cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sqlstatement;
            var dr = cmd.ExecuteReader();
            dt.Load(dr);
            return dt;

            //CancellationToken you please call me
        }
    }
}
