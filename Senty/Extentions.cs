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

		public static DbInfo GetDbInfo(this FieldInfo fi)
		{
			return new DbInfo(fi);
		}

		public static IDObject GetIDColumn(this Type type)
		{
			foreach (FieldInfo fi in type.GetFields())
			{
				Attribute idAttr = Attribute.GetCustomAttribute(fi, typeof(IDAttribute));
				if (idAttr != null)
				{
					return new IDObject((IDAttribute)idAttr, fi);
				}
			}
			return null;
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
