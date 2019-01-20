using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Senty
{
	public static class DBIO
	{
		public static SqlConnection NewConn
		{
			get
			{
				return new SqlConnection(new SqlConnectionStringBuilder { DataSource = ".", UserID = "", Password = "" }.ConnectionString);
			}
		}

		//public static DataTable ExecuteOracleSelectCommand(string commStr)
		//{
		//	//OracleDataAdapter oda = new OracleDataAdapter(commStr, NewOraConn);
		//	//DataTable dt = new DataTable(commStr);
		//	//oda.Fill(dt);
		//	//return dt;
		//	OracleCommand comm = new OracleCommand(commStr, NewOraConn);
		//	return ExecuteOracleSelectCommand(comm);
		//}

		/// <summary>
		/// Execute Oracle Select Command
		/// </summary>
		/// <param name="comm">can only be string or OracleCommand</param>
		/// <returns></returns>
		public static DataTable ExecuteSelectCommand(object comm)
		{
			SqlCommand cmd = MakeCommand(comm);
			SqlDataAdapter oda = new SqlDataAdapter(cmd);
			DataTable dt = new DataTable(cmd.CommandText);
			oda.Fill(dt);
			return dt;
		}

		/// <summary>
		/// Execute Oracle NonQuery Command
		/// </summary>
		/// <param name="commStr">can only be string or OracleCommand</param>
		/// <returns></returns>
		public static int ExecuteNonQueryCommand(object comm)
		{
			SqlCommand cmd = MakeCommand(comm);
			int resultCount = 0;
			using (cmd.Connection = NewConn)
			{
				cmd.Connection.Open();
				resultCount = cmd.ExecuteNonQuery();
				cmd.Connection.Close();
			}
			return resultCount;
		}

		/// <summary>
		/// Execute Oracle Scalar Command
		/// </summary>
		/// <param name="comm">can only be string or OracleCommand</param>
		/// <returns></returns>
		public static object ExecuteScalarCommand(object comm)
		{
			SqlCommand cmd = MakeCommand(comm);
			object resultObj;
			using (cmd.Connection = NewConn)
			{
				cmd.Connection.Open();
				resultObj = cmd.ExecuteScalar();
				cmd.Connection.Close();
			}
			return resultObj;
		}

		private static SqlCommand MakeCommand(object comm)
		{
			SqlCommand cmd = comm as SqlCommand;
			if (comm is string)
			{
				cmd = new SqlCommand((string)comm, NewConn);
			}
			else if (!(comm is SqlCommand))
			{
				throw (new Exception("Unknown comm.GetType()"));
			}
			if (cmd.Connection == null)
			{
				cmd.Connection = NewConn;
			}
			return cmd;
		}

		/// <summary>
		/// Execute Oracle Update By Table Name As Command
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static int ExecuteUpdate(DataTable dt)
		{
			SqlDataAdapter oda = new SqlDataAdapter(dt.TableName, NewConn);
			SqlCommandBuilder ocb = new SqlCommandBuilder(oda);
			return oda.Update(dt);
		}
	}
}
