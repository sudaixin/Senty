using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
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
		public readonly PropertyInfo pptInfo;
		public string DbColumnName { get { return pptInfo.GetDbInfo().DbColumnName; } }

		public IDObject(IDAttribute idAttr, PropertyInfo pi)
		{
			IDAttr = idAttr;
			pptInfo = pi;
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
		public IDAttribute(bool guid)
		{
			GUID = guid;
		}

		public readonly string IDSelectCommand;
		public readonly bool GUID = false;

		public string GetNewID(OleDbCommand cmd)
		{
			string idStr;
			if (GUID)
			{
				idStr = Guid.NewGuid().ToString("N");
			}
			else
			{
				cmd.CommandText = IDSelectCommand;
				idStr = cmd.ExecuteScalar() as string;
				if (string.IsNullOrEmpty(idStr))
				{
					throw new Exception("未取得ID");
				}
			}
			return idStr;
		}

		public override DbProperty SetProperty(DbProperty dbp)
		{
			dbp.ID = true;
			return dbp;
		}
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

		public DbInfo(PropertyInfo pi)
		{
			DbProperty dbp = new DbProperty { DbColumnName = pi.Name, NotDbColumn = false, InsertIntoDb = true, InsertIntoDbSet = false, LoadFromDataset = true, LoadFromDatasetSet = false, ID = false };
			foreach (Attribute attr in Attribute.GetCustomAttributes(pi))
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
}
