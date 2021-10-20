using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace CountryRiskPortal
{
    public class DBAccess
    {
        public DataTable GetRcdSetByCmdTrans(SqlCommand cmd)
        {
            SqlConnection sqlCon = new SqlConnection();
            SqlTransaction trans;
            string cs = ConfigurationManager.ConnectionStrings["connect"].ConnectionString;
            sqlCon = new SqlConnection(cs);
            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();
            trans = sqlCon.BeginTransaction();
            cmd.Transaction = trans;
            cmd.Connection = sqlCon;
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                trans.Commit();
                sqlCon.Close();
                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                trans.Rollback();
                sqlCon.Close();
                throw ex;
            }
        }
        public DataTable GetRcdSetByCmd(SqlCommand cmd)
        {
            SqlConnection sqlCon = new SqlConnection();
            string cs = ConfigurationManager.ConnectionStrings["connect"].ConnectionString;
            sqlCon = new SqlConnection(cs);
            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();
            cmd.Connection = sqlCon;
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                sqlCon.Close();
                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                sqlCon.Close();
                throw ex;
            }
        }
        public bool ExecuteCmd(SqlCommand cmd)
        {
            SqlConnection sqlCon = new SqlConnection();
            string cs = ConfigurationManager.ConnectionStrings["connect"].ConnectionString;
            sqlCon = new SqlConnection(cs);
            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();
            cmd.Connection = sqlCon;
            try
            {
                cmd.ExecuteNonQuery();
                sqlCon.Close();
                return true;
            }
            catch (Exception ex)
            {
                sqlCon.Close();
                throw ex;
            }
        }
        public DataTable GetRcdSetByQry(String Qry)
        {
            SqlConnection sqlCon = new SqlConnection();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["connect"].ConnectionString;
                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlDataAdapter da = new SqlDataAdapter(Qry, con);
                    da.SelectCommand.CommandType = CommandType.Text;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                sqlCon.Close();
                throw ex;
            }
            finally
            {
                sqlCon.Close();
            }
        }
        public string ExecuteScalar(SqlCommand cmd)
        {
            SqlConnection sqlCon = new SqlConnection();
            string cs = ConfigurationManager.ConnectionStrings["connect"].ConnectionString;
            sqlCon = new SqlConnection(cs);
            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();
            cmd.Connection = sqlCon;
            try
            {
                string result = "";
                result = cmd.ExecuteScalar().ToString();
                sqlCon.Close();
                return result;
            }
            catch (Exception ex)
            {
                sqlCon.Close();
                throw ex;
            }
        }
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}