using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Reflection;

namespace Senty
{
	public class SelectAttribute : Attribute
	{
		public SelectAttribute(string fromStr)
		{
			FromString = fromStr;
		}

		public readonly string FromString;
	}

	public class TableNameAttribute : Attribute
	{
		public TableNameAttribute(string tableName)
		{
			TableName = tableName;
		}

		public readonly string TableName;
	}

	public class InvokeAfterUpdateAttribute : Attribute { }

	public class IDObject
	{
		public readonly IDAttribute IDAttr;
		public readonly FieldInfo FldInfo;
		public string DbColumnName { get { return FldInfo.GetDbInfo().DbColumnName; } }

		public IDObject(IDAttribute idAttr, FieldInfo fi)
		{
			IDAttr = idAttr;
			FldInfo = fi;
		}
	}

	public struct DbProperty
	{
		public string DbColumnName;
		public bool NotDbColumn;
		public bool LoadFromDataset;
		public bool InsertIntoDb;
		public bool LoadFromDatasetSet;
		public bool InsertIntoDbSet;
		public bool ID;
	}

	public abstract class DbPropertyAttribute : Attribute
	{
		public abstract DbProperty SetProperty(DbProperty dbp);
	}

	public class IDAttribute : DbPropertyAttribute
	{
		public IDAttribute() { }
		public IDAttribute(string cmd)
		{
			IDSelectCommand = cmd;
		}

		public readonly string IDSelectCommand;

		public string GetNewID(OracleCommand cmd)
		{
			cmd.CommandText = IDSelectCommand;
			string idStr = cmd.ExecuteScalar() as string;
			if (string.IsNullOrEmpty(idStr))
			{
				throw new Exception("未取得ID");
			}
			return idStr;
		}

		public override DbProperty SetProperty(DbProperty dbp)
		{
			dbp.ID = true;
			return dbp;
		}

		//public string GetNewID()
		//{
		//	return GetNewID(Pub.NewOraConn);
		//}
	}

	public class NotDbColumnAttribute : DbPropertyAttribute
	{
		public override DbProperty SetProperty(DbProperty dbp)
		{
			dbp.NotDbColumn = true;
			if (!dbp.LoadFromDatasetSet)
			{
				dbp.LoadFromDataset = false;
			}
			if (!dbp.InsertIntoDbSet)
			{
				dbp.InsertIntoDb = false;
			}
			return dbp;
		}
	}

	public class DontLoadAttribute : DbPropertyAttribute
	{
		public override DbProperty SetProperty(DbProperty dbp)
		{
			dbp.LoadFromDataset = false;
			dbp.LoadFromDatasetSet = true;
			return dbp;
		}
	}

	public class DontInsertAttribute : DbPropertyAttribute
	{
		public override DbProperty SetProperty(DbProperty dbp)
		{
			dbp.InsertIntoDb = false;
			dbp.InsertIntoDbSet = true;
			return dbp;
		}
	}

	public class DbColumnNameAttribute : DbPropertyAttribute
	{
		public DbColumnNameAttribute(string nameStr)
		{
			Name = nameStr;
		}

		public readonly string Name;

		public override DbProperty SetProperty(DbProperty dbp)
		{
			dbp.DbColumnName = Name;
			return dbp;
		}
	}

	public class DbInfo
	{
		public readonly string DbColumnName;
		public readonly bool LoadFromDataset = true;
		public readonly bool InsertIntoDb = true;
		public readonly bool NotDbColumn = false;
		public readonly bool ID = false;
		//public readonly FieldInfo FldInfo;

		public DbInfo(FieldInfo fi)
		{
			DbProperty dbp = new DbProperty { DbColumnName = fi.Name, NotDbColumn = false, InsertIntoDb = true, InsertIntoDbSet = false, LoadFromDataset = true, LoadFromDatasetSet = false, ID = false };
			foreach (Attribute attr in Attribute.GetCustomAttributes(fi))
			{
				if (attr is DbPropertyAttribute)
				{
					dbp = ((DbPropertyAttribute)attr).SetProperty(dbp);
				}
			}
			//FldInfo = pi;
			DbColumnName = dbp.DbColumnName;
			NotDbColumn = dbp.NotDbColumn;
			LoadFromDataset = dbp.LoadFromDataset;
			InsertIntoDb = dbp.InsertIntoDb;
		}
	}

	//public static class DomainExtensions
	//{
	//	public static void ForEach<T>(this T[] array, Action<T> action)
	//	{
	//		Array.ForEach(array, action);
	//	}

	//	public static DbInfo GetDbInfo(this FieldInfo fi)
	//	{
	//		return new DbInfo(fi);
	//	}

	//	public static IDObject GetIDColumn(this Type type)
	//	{
	//		foreach (FieldInfo fi in type.GetFields())
	//		{
	//			Attribute idAttr = Attribute.GetCustomAttribute(fi, typeof(IDAttribute));
	//			if (idAttr != null)
	//			{
	//				return new IDObject((IDAttribute)idAttr, fi);
	//			}
	//		}
	//		return null;
	//	}

	//	private static string GetTableName(Type type)
	//	{
	//		string preStr = type.Name;
	//		TableNameAttribute tbAttr = (TableNameAttribute)Attribute.GetCustomAttribute(type, typeof(TableNameAttribute));
	//		if (tbAttr != null)
	//		{
	//			preStr = tbAttr.TableName;
	//		}
	//		return preStr;
	//	}

	//	public static string GetSelectString(this Type type)
	//	{
	//		string preStr = GetTableName(type);
	//		SelectAttribute sfAttr = (SelectAttribute)Attribute.GetCustomAttribute(type, typeof(SelectAttribute));
	//		if (sfAttr != null)
	//		{
	//			preStr = sfAttr.FromString;
	//		}
	//		if (!preStr.Contains("from"))
	//		{
	//			preStr = "from " + preStr;
	//		}
	//		if (!preStr.Contains("select"))
	//		{
	//			preStr = "select * " + preStr;
	//		}
	//		return preStr;
	//	}

	//	public static string GetInsertString(this Type type)
	//	{
	//		string preStr = GetTableName(type);
	//		preStr = "insert into " + preStr;
	//		return preStr;
	//	}

	//	public static string GetUpdateString(this Type type)
	//	{
	//		string preStr = GetTableName(type);
	//		preStr = "update " + preStr;
	//		return preStr;
	//	}

	//	public static string GetDeleteString(this Type type)
	//	{
	//		string preStr = GetTableName(type);
	//		preStr = "delete " + preStr;
	//		return preStr;
	//	}

	//	public static void InvokeAfterUpdate(this Type type)
	//	{
	//		MethodInfo[] ms = type.GetMethods();
	//		ms.ForEach(mi =>//foreach (MethodInfo mi in ms)
	//		{
	//			if (Attribute.GetCustomAttribute(mi, typeof(InvokeAfterUpdateAttribute)) != null)
	//			{
	//				mi.Invoke(null, null);
	//			}
	//		});
	//	}
	//}
}
