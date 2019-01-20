using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OracleClient;

namespace Senty
{
	class DBIO
	{
		public static OracleConnection NewOraConn
		{
			get
			{
				OracleConnectionStringBuilder ocsb = new OracleConnectionStringBuilder();
				ocsb.DataSource = ".";
				ocsb.UserID = "";
				ocsb.Password = "";
				return new OracleConnection(ocsb.ConnectionString);
			}
		}

		///// <summary>
		///// to read or write domain objects from database
		///// </summary>
		//public static class IO<T> where T : Entity
		//{
		//	/// <summary>
		//	/// read arraylist of domain classes objects from database
		//	/// return Read(OracleCommand)
		//	/// </summary>
		//	/// <typeparam name="T">Domain.T classes</typeparam>
		//	/// <param name="cmd">string</param>
		//	/// <returns>T[]</returns>
		//	public static T[] Read()
		//	{
		//		OracleCommand comm = new OracleCommand("", NewOraConn);
		//		return Read(comm);
		//	}

		//	/// <summary>
		//	/// read arraylist of domain classes objects from database
		//	/// return Read(OracleCommand)
		//	/// </summary>
		//	/// <typeparam name="T">Domain.T classes</typeparam>
		//	/// <param name="cmd">string</param>
		//	/// <returns>T[]</returns>
		//	public static T[] Read(string cmd)
		//	{
		//		OracleCommand comm = new OracleCommand(cmd, NewOraConn);
		//		return Read(comm);
		//	}

		//	/// <summary>
		//	/// read arraylist of domain classes objects from database
		//	/// </summary>
		//	/// <typeparam name="T">Domain.T classes</typeparam>
		//	/// <param name="cmd">OracleCommand only</param>
		//	/// <returns>T[]</returns>
		//	public static T[] Read(OracleCommand cmd)
		//	{
		//		Type type = typeof(T);
		//		if (!cmd.CommandText.Contains("from"))
		//		{
		//			cmd.CommandText = type.GetSelectString() + " " + cmd.CommandText;
		//		}
		//		DataTable dt = Pub.ExecuteOracleSelectCommand(cmd);
		//		int rowCount = dt.Rows.Count;
		//		T[] objList = (T[])Array.CreateInstance(type, rowCount);
		//		for (int i = 0; i < rowCount; i++)
		//		{
		//			objList[i] = Activator.CreateInstance<T>();
		//		}
		//		foreach (FieldInfo fi in type.GetFields())
		//		{
		//			DbInfo df = fi.GetDbInfo();
		//			if (df.LoadFromDataset)
		//			{
		//				if (dt.Columns.Contains(df.DbColumnName))
		//				{
		//					for (int i = 0; i < rowCount; i++)
		//					{
		//						object valueObj = dt.Rows[i][df.DbColumnName];
		//						if (valueObj is DBNull)
		//						{
		//							valueObj = null;
		//						}
		//						fi.SetValue(objList[i], valueObj);
		//					}
		//				}
		//			}
		//		}
		//		return objList;
		//	}

		//	public static T ReadOne(string cmdStr)
		//	{
		//		T[] ts = Read(cmdStr);
		//		if (ts.Length > 0) return ts[0];
		//		else return default(T);
		//	}

		//	public static T ReadOne(OracleCommand cmd)
		//	{
		//		T[] ts = Read(cmd);
		//		if (ts.Length > 0) return ts[0];
		//		else return default(T);
		//	}

		//	public static T ReadByID(string id)
		//	{
		//		Type type = typeof(T);
		//		IDObject idObj = type.GetIDColumn();
		//		OracleCommand cmd = new OracleCommand(type.GetSelectString() + " where " + idObj.DbColumnName + "=:id");
		//		cmd.Parameters.Add("id", OracleTypeDic[idObj.FldInfo.FieldType]).Value = id;
		//		return ReadOne(cmd);
		//	}

		//	private static Dictionary<Type, OracleType> _oracleTypeDic;
		//	public static Dictionary<Type, OracleType> OracleTypeDic
		//	{
		//		get
		//		{
		//			if (_oracleTypeDic == null)
		//			{
		//				_oracleTypeDic = new Dictionary<Type, OracleType>();
		//				_oracleTypeDic.Add(typeof(string), OracleType.VarChar);
		//				_oracleTypeDic.Add(typeof(decimal), OracleType.Number);
		//				_oracleTypeDic.Add(typeof(DateTime), OracleType.DateTime);
		//			}
		//			return _oracleTypeDic;
		//		}
		//	}

		//	public static T[] InsertArray(T[] objArray)
		//	{
		//		int arrayLength = objArray.Length;
		//		if (arrayLength < 1) return null;
		//		Type objType = typeof(T);
		//		using (OracleConnection conn = NewOraConn)
		//		{
		//			IDObject idObj = objType.GetIDColumn();
		//			OracleCommand cmd = conn.CreateCommand();
		//			cmd.CommandText = objType.GetInsertString();
		//			string columnStr = "(";
		//			string valuesStr = " values(";
		//			FieldInfo[] fis = objType.GetFields();
		//			foreach (FieldInfo fi in fis)
		//			{
		//				DbInfo df = fi.GetDbInfo();
		//				if (df.InsertIntoDb)
		//				{
		//					columnStr += df.DbColumnName + ",";
		//					valuesStr += ":" + fi.Name + ",";
		//					cmd.Parameters.Add(fi.Name, OracleTypeDic[fi.FieldType]);
		//				}
		//			}
		//			cmd.CommandText += columnStr.TrimEnd(',') + ")" + valuesStr.TrimEnd(',') + ")";
		//			conn.Open();
		//			using (cmd.Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
		//			{
		//				foreach (T aObj in objArray)
		//				{
		//					OracleCommand idCmd = conn.CreateCommand();
		//					idCmd.Transaction = cmd.Transaction;
		//					string newId = idObj.IDAttr.GetNewID(idCmd);
		//					idObj.FldInfo.SetValue(aObj, newId);
		//					foreach (FieldInfo fi in fis)
		//					{
		//						DbInfo df = fi.GetDbInfo();
		//						if (df.InsertIntoDb)
		//						{
		//							cmd.Parameters[fi.Name].Value = fi.GetValue(aObj);
		//						}
		//					}
		//					cmd.ExecuteNonQuery();
		//				}
		//				cmd.Transaction.Commit();
		//			}
		//			conn.Close();
		//		}
		//		objType.InvokeAfterUpdate();
		//		return objArray;
		//	}

		//	public static T Insert(T obj)
		//	{
		//		T[] objArray = (T[])Array.CreateInstance(typeof(T), 1);
		//		objArray[0] = obj;
		//		return InsertArray(objArray)[0];
		//	}

		//	public static T[] UpdateArray(T[] objArray)
		//	{
		//		int arrayLength = objArray.Length;
		//		if (arrayLength < 1) return null;
		//		Type objType = typeof(T);
		//		using (OracleConnection conn = NewOraConn)
		//		{
		//			IDObject idObj = objType.GetIDColumn();
		//			OracleCommand cmd = conn.CreateCommand();
		//			cmd.CommandText = objType.GetUpdateString();
		//			string cmdStr = " set ";
		//			FieldInfo[] fis = objType.GetFields();
		//			foreach (FieldInfo fi in fis)
		//			{
		//				DbInfo df = fi.GetDbInfo();
		//				if (df.InsertIntoDb && !df.ID)
		//				{
		//					cmdStr += df.DbColumnName + "=:" + fi.Name + ",";
		//				}
		//				cmd.Parameters.Add(fi.Name, OracleTypeDic[fi.FieldType]);
		//			}
		//			cmd.CommandText += cmdStr.TrimEnd(',') + " where " + idObj.DbColumnName + "=:" + idObj.FldInfo.Name;
		//			conn.Open();
		//			using (cmd.Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
		//			{
		//				foreach (T aObj in objArray)
		//				{
		//					foreach (FieldInfo fi in fis)
		//					{
		//						DbInfo df = fi.GetDbInfo();
		//						if (df.InsertIntoDb)
		//						{
		//							cmd.Parameters[fi.Name].Value = fi.GetValue(aObj);
		//						}
		//					}
		//					cmd.ExecuteNonQuery();
		//				}
		//				cmd.Transaction.Commit();
		//			}
		//			conn.Close();
		//		}
		//		objType.InvokeAfterUpdate();
		//		return objArray;
		//	}

		//	public static T Update(T obj)
		//	{
		//		T[] objArray = (T[])Array.CreateInstance(typeof(T), 1);
		//		objArray[0] = obj;
		//		return UpdateArray(objArray)[0];
		//	}

		//	public static int DeleteArray(T[] objArray)
		//	{
		//		int arrayLength = objArray.Length;
		//		if (arrayLength < 1) return 0;
		//		int resultCount = 0;
		//		Type objType = typeof(T);
		//		using (OracleConnection conn = NewOraConn)
		//		{
		//			IDObject idObj = objType.GetIDColumn();
		//			OracleCommand cmd = conn.CreateCommand();
		//			cmd.CommandText = objType.GetDeleteString() + " where " + idObj.DbColumnName + "=:id";
		//			cmd.Parameters.Add("id", OracleType.VarChar);
		//			conn.Open();
		//			using (cmd.Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
		//			{
		//				foreach (T aObj in objArray)
		//				{
		//					cmd.Parameters["id"].Value = idObj.FldInfo.GetValue(aObj);
		//					resultCount += cmd.ExecuteNonQuery();
		//				}
		//				cmd.Transaction.Commit();
		//			}
		//			conn.Close();
		//		}
		//		objType.InvokeAfterUpdate();
		//		return resultCount;
		//	}

		//	public static int Delete(T obj)
		//	{
		//		T[] objArray = (T[])Array.CreateInstance(typeof(T), 1);
		//		objArray[0] = obj;
		//		return DeleteArray(objArray);
		//	}
		//}

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
		public static DataTable ExecuteOracleSelectCommand(object comm)
		{
			OracleCommand cmd = MakeCommand(comm);
			OracleDataAdapter oda = new OracleDataAdapter(cmd);
			DataTable dt = new DataTable(cmd.CommandText);
			oda.Fill(dt);
			return dt;
		}

		/// <summary>
		/// Execute Oracle NonQuery Command
		/// </summary>
		/// <param name="commStr">can only be string or OracleCommand</param>
		/// <returns></returns>
		public static int ExecuteOracleNonQueryCommand(object comm)
		{
			OracleCommand cmd = MakeCommand(comm);
			int resultCount = 0;
			using (cmd.Connection = NewOraConn)
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
		public static object ExecuteOracleScalarCommand(object comm)
		{
			OracleCommand cmd = MakeCommand(comm);
			object resultObj;
			using (cmd.Connection = NewOraConn)
			{
				cmd.Connection.Open();
				resultObj = cmd.ExecuteScalar();
				cmd.Connection.Close();
			}
			return resultObj;
		}

		private static OracleCommand MakeCommand(object comm)
		{
			OracleCommand cmd = comm as OracleCommand;
			if (comm is string)
			{
				cmd = new OracleCommand((string)comm, NewOraConn);
			}
			else if (!(comm is OracleCommand))
			{
				throw (new Exception("Unknown comm.GetType()"));
			}
			if (cmd.Connection == null)
			{
				cmd.Connection = NewOraConn;
			}
			return cmd;
		}

		/// <summary>
		/// Execute Oracle Update By Table Name As Command
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static int ExecuteOracleUpdate(DataTable dt)
		{
			OracleDataAdapter oda = new OracleDataAdapter(dt.TableName, NewOraConn);
			OracleCommandBuilder ocb = new OracleCommandBuilder(oda);
			return oda.Update(dt);
		}
	}
}
