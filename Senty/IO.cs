using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

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
			SqlCommand ocmd = new SqlCommand(cmd.CommandText);
			foreach (var param in cmd.Parameters.Get())
			{
				if (param.Value is string[])
				{
					string[] valueList = param.Value as string[];
					string newParamName = "", paramName = param.ParamName;
					for (int i = 0; i < valueList.Length; i++)
					{
						string paramNamei = paramName + i.ToString() + "_";
						newParamName += "@" + paramNamei + ",";
						ocmd.Parameters.Add("@" + paramNamei, SqlDbType.Char).Value = valueList[i];
					}
					ocmd.CommandText = ocmd.CommandText.Replace("@" + paramName, newParamName.TrimEnd(','));
				}
				else
				{
					ocmd.Parameters.Add("@" + param.ParamName, SqldbTypeDic.Get(param.Value.GetType())).Value = param.Value;
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
		private static T[] Read(SqlCommand cmd)
		{
			Type type = typeof(T);
			if (!cmd.CommandText.Contains("from"))
			{
				cmd.CommandText = type.GetSelectString() + " " + cmd.CommandText;
			}
			DataTable dt = DBIO.ExecuteSelectCommand(cmd);
			int rowCount = dt.Rows.Count;
			T[] objList = (T[])Array.CreateInstance(type, rowCount);
			for (int i = 0; i < rowCount; i++)
			{
				objList[i] = Activator.CreateInstance<T>();
			}
			foreach (FieldInfo fi in type.GetFields())
			{
				DbInfo df = fi.GetDbInfo();
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
							fi.SetValue(objList[i], valueObj);
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

		public static T ReadOne(SqlCommand cmd)
		{
			T[] ts = Read(cmd);
			if (ts.Length > 0) return ts[0];
			else return default(T);
		}

		public static T ReadByID(string id)
		{
			Type type = typeof(T);
			IDObject idObj = type.GetIDColumn();
			SqlCommand cmd = new SqlCommand(type.GetSelectString() + " where " + idObj.DbColumnName + "=@id");
			cmd.Parameters.Add("@id", SqldbTypeDic.Get(idObj.FldInfo.FieldType)).Value = id;
			return ReadOne(cmd);
		}

		public static T[] InsertArray(T[] objArray)
		{
			int arrayLength = objArray.Length;
			if (arrayLength < 1) return null;
			Type objType = typeof(T);
			using (SqlConnection conn = DBIO.NewConn)
			{
				IDObject idObj = objType.GetIDColumn();
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = objType.GetInsertString();
				string columnStr = "(";
				string valuesStr = " values(";
				FieldInfo[] fis = objType.GetFields();
				foreach (FieldInfo fi in fis)
				{
					DbInfo df = fi.GetDbInfo();
					if (df.InsertIntoDb)
					{
						columnStr += df.DbColumnName + ",";
						valuesStr += "@" + fi.Name + ",";
						cmd.Parameters.Add("@" + fi.Name, SqldbTypeDic.Get(fi.FieldType));
					}
				}
				cmd.CommandText += columnStr.TrimEnd(',') + ")" + valuesStr.TrimEnd(',') + ")";
				conn.Open();
				using (cmd.Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
				{
					foreach (T aObj in objArray)
					{
						SqlCommand idCmd = conn.CreateCommand();
						idCmd.Transaction = cmd.Transaction;
						string newId = idObj.IDAttr.GetNewID(idCmd);
						idObj.FldInfo.SetValue(aObj, newId);
						foreach (FieldInfo fi in fis)
						{
							DbInfo df = fi.GetDbInfo();
							if (df.InsertIntoDb)
							{
								cmd.Parameters["@" + fi.Name].Value = fi.GetValue(aObj);
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
			using (SqlConnection conn = DBIO.NewConn)
			{
				IDObject idObj = objType.GetIDColumn();
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = objType.GetUpdateString();
				string cmdStr = " set ";
				FieldInfo[] fis = objType.GetFields();
				foreach (FieldInfo fi in fis)
				{
					DbInfo df = fi.GetDbInfo();
					if (df.InsertIntoDb && !df.ID)
					{
						cmdStr += df.DbColumnName + "=@" + fi.Name + ",";
					}
					cmd.Parameters.Add("@" + fi.Name, SqldbTypeDic.Get(fi.FieldType));
				}
				cmd.CommandText += cmdStr.TrimEnd(',') + " where " + idObj.DbColumnName + "=@" + idObj.FldInfo.Name;
				conn.Open();
				using (cmd.Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
				{
					foreach (T aObj in objArray)
					{
						foreach (FieldInfo fi in fis)
						{
							DbInfo df = fi.GetDbInfo();
							if (df.InsertIntoDb)
							{
								cmd.Parameters["@" + fi.Name].Value = fi.GetValue(aObj);
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
			using (SqlConnection conn = DBIO.NewConn)
			{
				IDObject idObj = objType.GetIDColumn();
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = objType.GetDeleteString() + " where " + idObj.DbColumnName + "=@id";
				cmd.Parameters.Add("@id", SqlDbType.VarChar);
				conn.Open();
				using (cmd.Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
				{
					foreach (T aObj in objArray)
					{
						cmd.Parameters["@id"].Value = idObj.FldInfo.GetValue(aObj);
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
