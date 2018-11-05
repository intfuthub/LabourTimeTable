using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
/// <summary>
/// Summary description for Logger
/// </summary>
public class Logger
{
    public Logger()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public static void Log(Exception exception)
    {

        StringBuilder sbExceptionMessage = new StringBuilder();

        do
        {
            sbExceptionMessage.Append("Exception Type " + Environment.NewLine);
            sbExceptionMessage.Append(exception.GetType().Name);
            sbExceptionMessage.Append(Environment.NewLine + Environment.NewLine);
            sbExceptionMessage.Append("Message " + Environment.NewLine);
            sbExceptionMessage.Append(exception.Message + Environment.NewLine + Environment.NewLine);
            sbExceptionMessage.Append("Stack Trace " + Environment.NewLine);
            sbExceptionMessage.Append(exception.StackTrace + Environment.NewLine + Environment.NewLine);

            exception = exception.InnerException;
        }
        while (exception != null);

        LogToDB(sbExceptionMessage.ToString());
    }

    private static void LogToDB(string log)
    {
        //// ConfigurationManager class is in System.Configuration namespace
        //string connectionString = ConfigurationManager.ConnectionStrings["InterfutureConnectionString1"].ConnectionString;
        //// SqlConnection is in System.Data.SqlClient namespace
        //using (SqlConnection con = new SqlConnection(connectionString))
        //{
        //    SqlCommand cmd = new SqlCommand("spInsertLog", con);
        //    // CommandType is in System.Data namespace
        //    cmd.CommandType = CommandType.StoredProcedure;

        //    SqlParameter parameter = new SqlParameter("@ExceptionMessage", log);
        //    cmd.Parameters.Add(parameter);

        //    con.Open();
        //    cmd.ExecuteNonQuery();
        //    con.Close();
        //}
    }

}