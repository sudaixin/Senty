using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.OleDb;

namespace Senty
{
	public static class IO<T> where T : Entity
	{
		/// <summary>
		/// read arraylist of domain classes objects from database
		/// return Read(OracleCommand)
		/// </summary>
		/// <typeparam name="T">Domain.T classes</typeparam>
		/// <param name="cmd">string</param>
		/// <returns>T[]</returns>
		public static T[] Read()
		{
			return Read("");
		}

		/// <summary>
		/// read arraylist of domain classes objects from database
		/// return Read(OracleCommand)
		/// </summary>
		/// <typeparam name="T">Domain.T classes</typeparam>
		/// <param name="cmd">string</param>
		/// <returns>T[]</returns>
		public static T[] Read(string cmd)
		{
			//OracleCommand comm = new OracleCommand(cmd);
			return Read(new ReadCommand(cmd));
		}

		public static T[] Read(ReadCommand cmd)
		{
			OleDbCommand ocmd = new OleDbCommand(cmd.CommandText);
			foreach (var param in cmd.Parameters.Get())
			{
				if (param.Value is string[])
				{
					throw new Exception("此处未完成");
					string[] valueList = param.Value as string[];
					string newParamName = "", paramName = param.ParamName;
					for (int i = 0; i < valueList.Length; i++)
					{
						string paramNamei = paramName + i.ToString() + "_";
						newParamName += ":" + paramNamei + ",";
						ocmd.Parameters.Add(paramNamei, OleDbType.Char).Value = valueList[i];
					}
					ocmd.CommandText = ocmd.CommandText.Replace(":" + paramName, newParamName.TrimEnd(','));
				}
				else
				{
					ocmd.Parameters.Add(param.ParamName, AccessTypeDic.Get(param.Value.GetType())).Value = param.Value;
				}
			}
			return Read(ocmd);
		}

		/// <summary>
		/// read arraylist of domain classes objects from database
		/// </summary>
		/// <typeparam name="T">Domain.T classes</typeparam>
		/// <param name="cmd">OracleCommand only</param>
		/// <returns>T[]</returns>
		private static T[] Read(OleDbCommand cmd)
		{
			Type type = typeof(T);
			if (!cmd.CommandText.Contains("from"))
			{
				cmd.CommandText = type.GetSelectString() + " " + cmd.CommandText;
			}
			DataTable dt = DBIO.ExecuteAccessSelectCommand(cmd);
			int rowCount = dt.Rows.Count;
			T[] objList = (T[])Array.CreateInstance(type, rowCount);
			for (int i = 0; i < rowCount; i++)
			{
				objList[i] = Activator.CreateInstance<T>();
			}
			foreach (PropertyInfo pi in type.GetProperties())
			{
				DbInfo df = pi.GetDbInfo();
				if (df.LoadFromDataset)
				{
					if (dt.Columns.Contains(df.DbColumnName))
					{
						for (int i = 0; i < rowCount; i++)
						{
							object valueObj = dt.Rows[i][df.DbColumnName];
							if (valueObj is DBNull)
							{
								valueObj = null;
							}
							pi.SetValue(objList[i], valueObj, null);
						}
					}
				}
			}
			return objList;
		}

		public static T ReadOne(string cmdStr)
		{
			T[] ts = Read(cmdStr);
			if (ts.Length > 0) return ts[0];
			else return default(T);
		}

		public static T ReadOne(OleDbCommand cmd)
		{
			T[] ts = Read(cmd);
			if (ts.Length > 0) return ts[0];
			else return default(T);
		}

		public static T ReadByID(string id)
		{
			Type type = typeof(T);
			IDObject idObj = type.GetIDProperty();
			OleDbCommand cmd = new OleDbCommand(type.GetSelectString() + " where " + idObj.DbColumnName + "=@id");
			cmd.Parameters.Add("@id", AccessTypeDic.Get(idObj.pptInfo.PropertyType)).Value = id;
			return ReadOne(cmd);
		}

		public static T[] InsertArray(T[] objArray)
		{
			int arrayLength = objArray.Length;
			if (arrayLength < 1) return null;
			Type objType = typeof(T);
			using (OleDbConnection conn = DBIO.NewAccessConn)
			{
				IDObject idObj = objType.GetIDProperty();
				OleDbCommand cmd = conn.CreateCommand();
				cmd.CommandText = objType.GetInsertString();
				string columnStr = "(";
				string valuesStr = " values(";
				PropertyInfo[] pis = objType.GetProperties();
				foreach (PropertyInfo pi in pis)
				{
					DbInfo df = pi.GetDbInfo();
					if (df.InsertIntoDb)
					{
						columnStr += df.DbColumnName + ",";
						valuesStr += "@" + pi.Name + ",";
						cmd.Parameters.Add("@" + pi.Name, AccessTypeDic.Get(pi.PropertyType));
					}
				}
				cmd.CommandText += columnStr.TrimEnd(',') + ")" + valuesStr.TrimEnd(',') + ")";
				conn.Open();
				using (cmd.Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
				{
					foreach (T aObj in objArray)
					{
						OleDbCommand idCmd = conn.CreateCommand();
						idCmd.Transaction = cmd.Transaction;
						string newId = idObj.IDAttr.GetNewID(idCmd);
						idObj.pptInfo.SetValue(aObj, newId, null);
						foreach (PropertyInfo pi in pis)
						{
							DbInfo df = pi.GetDbInfo();
							if (df.InsertIntoDb)
							{
								cmd.Parameters["@" + pi.Name].Value = pi.GetValue(aObj, null);
							}
						}
						cmd.ExecuteNonQuery();
					}
					cmd.Transaction.Commit();
				}
				conn.Close();
			}
			objType.InvokeAfterUpdate();
			return objArray;
		}

		public static T Insert(T obj)
		{
			T[] objArray = (T[])Array.CreateInstance(typeof(T), 1);
			objArray[0] = obj;
			return InsertArray(objArray)[0];
		}

		public static T[] UpdateArray(T[] objArray)
		{
			int arrayLength = objArray.Length;
			if (arrayLength < 1) return null;
			Type objType = typeof(T);
			using (OleDbConnection conn = DBIO.NewAccessConn)
			{
				IDObject idObj = objType.GetIDProperty();
				OleDbCommand cmd = conn.CreateCommand();
				cmd.CommandText = objType.GetUpdateString();
				string cmdStr = " set ";
				PropertyInfo[] pis = objType.GetProperties();
				foreach (PropertyInfo pi in pis)
				{
					DbInfo df = pi.GetDbInfo();
					if (df.InsertIntoDb && !df.ID)
					{
						cmdStr += df.DbColumnName + "=@" + pi.Name + ",";
					}
					cmd.Parameters.Add("@" + pi.Name, AccessTypeDic.Get(pi.PropertyType));
				}
				cmd.CommandText += cmdStr.TrimEnd(',') + " where " + idObj.DbColumnName + "=@" + idObj.pptInfo.Name;
				conn.Open();
				using (cmd.Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
				{
					foreach (T aObj in objArray)
					{
						foreach (PropertyInfo pi in pis)
						{
							DbInfo df = pi.GetDbInfo();
							if (df.InsertIntoDb)
							{
								cmd.Parameters["@" + pi.Name].Value = pi.GetValue(aObj, null);
							}
						}
						cmd.ExecuteNonQuery();
					}
					cmd.Transaction.Commit();
				}
				conn.Close();
			}
			objType.InvokeAfterUpdate();
			return objArray;
		}

		public static T Update(T obj)
		{
			T[] objArray = (T[])Array.CreateInstance(typeof(T), 1);
			objArray[0] = obj;
			return UpdateArray(objArray)[0];
		}

		public static int DeleteArray(T[] objArray)
		{
			int arrayLength = objArray.Length;
			if (arrayLength < 1) return 0;
			int resultCount = 0;
			Type objType = typeof(T);
			using (OleDbConnection conn = DBIO.NewAccessConn)
			{
				IDObject idObj = objType.GetIDProperty();
				OleDbCommand cmd = conn.CreateCommand();
				cmd.CommandText = objType.GetDeleteString() + " where " + idObj.DbColumnName + "=@id";
				cmd.Parameters.Add("@id", OleDbType.VarChar);
				conn.Open();
				using (cmd.Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
				{
					foreach (T aObj in objArray)
					{
						cmd.Parameters["@id"].Value = idObj.pptInfo.GetValue(aObj, null);
						resultCount += cmd.ExecuteNonQuery();
					}
					cmd.Transaction.Commit();
				}
				conn.Close();
			}
			objType.InvokeAfterUpdate();
			return resultCount;
		}

		public static int Delete(T obj)
		{
			T[] objArray = (T[])Array.CreateInstance(typeof(T), 1);
			objArray[0] = obj;
			return DeleteArray(objArray);
		}
	}
}
