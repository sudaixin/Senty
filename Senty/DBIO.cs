using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace Senty
{
	class DBIO
	{
		public static OleDbConnection NewAccessConn
		{
			get
			{
				return new OleDbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SentyConnectionString"].ConnectionString);
			}
		}

		/// <summary>
		/// Execute Access Select Command
		/// </summary>
		/// <param name="comm">can only be string or AccessCommand</param>
		/// <returns></returns>
		public static DataTable ExecuteAccessSelectCommand(object comm)
		{
			OleDbCommand cmd = MakeCommand(comm);
			OleDbDataAdapter oda = new OleDbDataAdapter(cmd);
			DataTable dt = new DataTable(cmd.CommandText);
			oda.Fill(dt);
			return dt;
		}

		/// <summary>
		/// Execute Access NonQuery Command
		/// </summary>
		/// <param name="commStr">can only be string or AccessCommand</param>
		/// <returns></returns>
		public static int ExecuteAccessNonQueryCommand(object comm)
		{
			OleDbCommand cmd = MakeCommand(comm);
			int resultCount = 0;
			using (cmd.Connection = NewAccessConn)
			{
				cmd.Connection.Open();
				resultCount = cmd.ExecuteNonQuery();
				cmd.Connection.Close();
			}
			return resultCount;
		}

		/// <summary>
		/// Execute Access Scalar Command
		/// </summary>
		/// <param name="comm">can only be string or AccessCommand</param>
		/// <returns></returns>
		public static object ExecuteAccessScalarCommand(object comm)
		{
			OleDbCommand cmd = MakeCommand(comm);
			object resultObj;
			using (cmd.Connection = NewAccessConn)
			{
				cmd.Connection.Open();
				resultObj = cmd.ExecuteScalar();
				cmd.Connection.Close();
			}
			return resultObj;
		}

		private static OleDbCommand MakeCommand(object comm)
		{
			OleDbCommand cmd = comm as OleDbCommand;
			if (comm is string)
			{
				cmd = new OleDbCommand((string)comm, NewAccessConn);
			}
			else if (!(comm is OleDbCommand))
			{
				throw (new Exception("Unknown comm.GetType()"));
			}
			if (cmd.Connection == null)
			{
				cmd.Connection = NewAccessConn;
			}
			return cmd;
		}

		/// <summary>
		/// Execute Access Update By Table Name As Command
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static int ExecuteAccessUpdate(DataTable dt)
		{
			OleDbDataAdapter oda = new OleDbDataAdapter(dt.TableName, NewAccessConn);
			OleDbCommandBuilder ocb = new OleDbCommandBuilder(oda);
			return oda.Update(dt);
		}
	}
}
