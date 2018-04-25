using System;
//using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Data.Common;
using System.Data.SqlClient;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System.IO;
//using Excel = Microsoft.Office.Interop.Excel;

namespace T100ETLSQLTest
{
    enum DBType
    {
        oracle,
        sqlserver
    };

    enum SQLReturnType
    {
        cursor,
        table
    };

    class Func
    {
        public static string server;
        public static string user;
        public static string password;
        public static string dbtype;
		public static string dbname;
        public static SqlConnection MySQLConn;

        public static string t100_server;
        public static string t100_user;
        public static string t100_password;
        public static string t100_current_user;
        public static OracleConnection MyOraSqlConn;

        public static string t100_ent;
        public static string t100_site;
        public static string t100_aps;

        public static string txt_winmerge_path;

        public Func()
		{
            //SqlConnection _MySQLConn = new SqlConnection();
            //OleDbConnection _MyOleSQLConn = new OleDbConnection();
        }

        /// <summary>
        /// 將字串用''包起來,通常是給SQl字串使用
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AA(string value)
        {
            return "'" + value + "'";
        }

        /// <summary>
        ///取得某分隔字元及前分隔字元之間隔的字串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="Occur"></param>
        /// <param name="Delim"></param>
        /// <returns></returns>
        public static string Split(String str, int Occur, Char Delim)
        {
            string[] strArray = str.Split(new Char[] { Delim });
            for(int i=1; i<=strArray.Length;i++)
            {
                if (i == Occur)
                    return strArray[i-1];
            }
            return "";
        }

        public static DataRow[] GetRowsByFilter(DataTable tb,string expression)
        {
            // Presuming the DataTable has a column named Date.
            DataRow[] foundRows;

            // Use the Select method to find all rows matching the filter.
            foundRows = tb.Select(expression);

            return foundRows;
        }

        public static  void DbConn()
        {
            if (MySQLConn == null)
            {
                MySQLConn = new SqlConnection();
            }
            string connStr = String.Format("server=@localhost;user id=@user; password=@password; database=@db; pooling=false");
            connStr = connStr.Replace("@localhost", Func.server);
            connStr = connStr.Replace("@user", Func.user);
            connStr = connStr.Replace("@password", Func.password);
            connStr = connStr.Replace("@db", Func.dbname);

            MySQLConn.Close();
            MySQLConn.ConnectionString = connStr;
            MySQLConn.Open();
        
        }

        public static void OracleDbConn()
        {
            string connStr="";
            if (MyOraSqlConn == null)
            {
                //connStr = String.Format("Provider=OraOLEDB.Oracle;Data Source=@localhost;User ID=@user;Password=@password;");
                connStr = String.Format("Data Source=@localhost;Persist Security Info=True;User ID=@user;Password=@password;");
                connStr = connStr.Replace("@localhost", Func.t100_server);
                connStr = connStr.Replace("@user", Func.t100_user);
                connStr = connStr.Replace("@password", Func.t100_password);
                MyOraSqlConn = new OracleConnection(connStr);
                MyOraSqlConn.Open();
            }
            else
            {
                connStr = String.Format("Data Source=@localhost;Persist Security Info=True;User ID=@user;Password=@password;");
                connStr = connStr.Replace("@localhost", Func.t100_server);
                connStr = connStr.Replace("@user", Func.t100_user);
                connStr = connStr.Replace("@password", Func.t100_password);
                MyOraSqlConn.Close();
                MyOraSqlConn.ConnectionString = connStr;
                MyOraSqlConn.Open();
            };

        }

        public static void SqlOpen(DataSet ds, string sql)
        {
            DbConn();

            SqlDataAdapter adapter = new SqlDataAdapter(sql, MySQLConn);
            ds.Clear();
            adapter.Fill(ds);
        }

        public static void SqlOpen(DataTable ds, string sql)
        {
            DbConn();

            SqlDataAdapter adapter = new SqlDataAdapter(sql, MySQLConn);
            ds.Clear();
            adapter.Fill(ds);
        }

        public static void OracleSqlOpen(DataSet ds, string sql)
        {
            OracleDbConn();

            OracleDataAdapter adapter = new OracleDataAdapter(sql, MyOraSqlConn);

            ds.Clear();
            adapter.Fill(ds);
        }

        public static void OracleSqlOpen(DataTable ds, string sql)
        {
            OracleDbConn();

            OracleDataAdapter adapter = new OracleDataAdapter(sql, MyOraSqlConn);

            ds.Clear();
            adapter.Fill(ds);
        }

        public static void SqlExec(string sql)
        {
            DbConn();

            SqlCommand com = new SqlCommand(sql, MySQLConn);
            com.ExecuteNonQuery();

        }
        public static string SqlValue(string fieldname,string sql)
        {
            DbConn();

            SqlDataAdapter adapter = new SqlDataAdapter(sql, MySQLConn);
            DataSet ds = new DataSet();
            adapter.Fill(ds);

            string value = ds.Tables[0].Rows.Count > 0 ? ds.Tables[0].Rows[0][fieldname].ToString() : "";
            return value;
        }

        //訂義結構體
        public enum DateInterval
        {
            Second, Minute, Hour, Day, Week, Month, Quarter, Year
        }
        //多型1，傳入字串
        public static long  DateDiff(string Interval, DateTime arg_StartDate, DateTime arg_EndDate)
        {
            DateInterval objDateInterval;
            switch (Interval)
            {
                case "s":
                    objDateInterval = DateInterval.Second;
                    break;
                case "n":
                    objDateInterval = DateInterval.Minute;
                    break;
                case "h":
                    objDateInterval = DateInterval.Hour;
                    break;
                case "d":
                    objDateInterval = DateInterval.Day;
                    break;
                case "w":
                    objDateInterval = DateInterval.Week;
                    break;
                case "m":
                    objDateInterval = DateInterval.Month;
                    break;
                case "q":
                    objDateInterval = DateInterval.Quarter;
                    break;
                case "y":
                    objDateInterval = DateInterval.Year;
                    break;
                default:
                    objDateInterval = DateInterval.Second;
                    break;
            }
            return DateDiff(objDateInterval, arg_StartDate, arg_EndDate);
        }
        //多型2，傳入結構體
        public static long DateDiff(DateInterval arg_Interval, DateTime arg_StartDate, DateTime arg_EndDate)
        {
            long lngDateDiffValue = 0;
            System.TimeSpan objTimeSpan = new System.TimeSpan(arg_EndDate.Ticks - arg_StartDate.Ticks);
            switch (arg_Interval)
            {
                case DateInterval.Second:
                    lngDateDiffValue = (long)objTimeSpan.TotalSeconds;
                    break;
                case DateInterval.Minute:
                    lngDateDiffValue = (long)objTimeSpan.TotalMinutes;
                    break;
                case DateInterval.Hour:
                    lngDateDiffValue = (long)objTimeSpan.TotalHours;
                    break;
                case DateInterval.Day:
                    lngDateDiffValue = (long)objTimeSpan.TotalDays;
                    break;
                case DateInterval.Week:
                    lngDateDiffValue = (long)(objTimeSpan.TotalSeconds / (7 * 24 * 60 * 60)); //一個星期7天
                    break;
                case DateInterval.Month:
                    lngDateDiffValue = (long)(objTimeSpan.TotalSeconds / (30 * 24 * 60 * 60)); //一個月計30天
                    break;
                case DateInterval.Quarter:
                    lngDateDiffValue = (long)(objTimeSpan.TotalSeconds / (90 * 24 * 60 * 60)); //一季計3月
                    break;
                case DateInterval.Year:
                    lngDateDiffValue = (long)(objTimeSpan.TotalSeconds / (365 * 24 * 60 * 60));   //一年計365天
                    break;
            }
            return (lngDateDiffValue);
        }



    }
}
