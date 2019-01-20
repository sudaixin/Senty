using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Senty
{
	public static class Extensions
	{
		public static void ForEach<T>(this T[] array, Action<T> action)
		{
			Array.ForEach(array, action);
		}

		public static DbInfo GetDbInfo(this PropertyInfo pi)
		{
			return new DbInfo(pi);
		}

		public static IDObject GetIDProperty(this Type type)
		{
			IDObject idObj = null;
			foreach (PropertyInfo pi in type.GetProperties())
			{
				Attribute idAttr = Attribute.GetCustomAttribute(pi, typeof(IDAttribute));
				if (idAttr != null)
				{
					idObj = new IDObject((IDAttribute)idAttr, pi);
					break;
				}
				else
				{
					if (pi.Name.ToLower() == "id" && idObj == null)
					{
						idObj = new IDObject(new IDAttribute(), pi);
					}
				}
			}
			if (idObj == null) throw new Exception("id property not found!");
			return idObj;
		}

		private static string GetTableName(Type type)
		{
			string preStr = type.Name;
			TableNameAttribute tbAttr = (TableNameAttribute)Attribute.GetCustomAttribute(type, typeof(TableNameAttribute));
			if (tbAttr != null)
			{
				preStr = tbAttr.TableName;
			}
			return preStr;
		}

		public static string GetSelectString(this Type type)
		{
			string preStr = GetTableName(type);
			SelectAttribute sfAttr = (SelectAttribute)Attribute.GetCustomAttribute(type, typeof(SelectAttribute));
			if (sfAttr != null)
			{
				preStr = sfAttr.FromString;
			}
			if (!preStr.Contains("from"))
			{
				preStr = "from " + preStr;
			}
			if (!preStr.Contains("select"))
			{
				preStr = "select * " + preStr;
			}
			return preStr;
		}

		public static string GetInsertString(this Type type)
		{
			string preStr = GetTableName(type);
			preStr = "insert into " + preStr;
			return preStr;
		}

		public static string GetUpdateString(this Type type)
		{
			string preStr = GetTableName(type);
			preStr = "update " + preStr;
			return preStr;
		}

		public static string GetDeleteString(this Type type)
		{
			string preStr = GetTableName(type);
			preStr = "delete " + preStr;
			return preStr;
		}

		public static void InvokeAfterUpdate(this Type type)
		{
			MethodInfo[] ms = type.GetMethods();
			ms.ForEach(mi =>//foreach (MethodInfo mi in ms)
			{
				if (Attribute.GetCustomAttribute(mi, typeof(InvokeAfterUpdateAttribute)) != null)
				{
					mi.Invoke(null, null);
				}
			});
		}
	}
}
