using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AxRetailSO.Classes
{
    public class STM
    {
        public static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AxConnectionString"].ConnectionString.ToString();
        //public static string connectionString = @"Data Source=10.11.0.64; Initial Catalog=AX63_STM_Live; Persist Security Info=True; User ID=stmm;Password=stmm@48624; Application Name=RetailSO; Pooling=false;";
        public static DataTable QuerySelect(string sql)
        {
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter ad = new SqlDataAdapter();
            DataTable dt = new DataTable();
            try
            {
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                ad.SelectCommand = cmd;
                ad.Fill(dt);
                con.Close();
            }
            catch (Exception)
            {
                throw;
            }
           
            return dt;
        }
    }
}