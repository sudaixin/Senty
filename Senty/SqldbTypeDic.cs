using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Senty
{
	static class SqldbTypeDic
	{
		private static Dictionary<Type, SqlDbType> _SqlDbTypeDic;

		static SqldbTypeDic()
		{
			_SqlDbTypeDic = new Dictionary<Type, SqlDbType>();
			_SqlDbTypeDic.Add(typeof(string), SqlDbType.Char);
			_SqlDbTypeDic.Add(typeof(decimal), SqlDbType.Decimal);
			_SqlDbTypeDic.Add(typeof(DateTime), SqlDbType.DateTime);
		}

		public static SqlDbType Get(Type type)
		{
			try
			{
				return _SqlDbTypeDic[type];
			}
			catch (KeyNotFoundException)
			{
				throw new Exception("unknown type: only string, decimal, DateTime supported");
			}
		}
	}
}
