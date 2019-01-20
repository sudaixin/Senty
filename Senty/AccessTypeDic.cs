using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;

namespace Senty
{
	static class AccessTypeDic
	{
		private static Dictionary<Type, OleDbType> _oracleTypeDic;

		static AccessTypeDic()
		{
			_oracleTypeDic = new Dictionary<Type, OleDbType>();
			_oracleTypeDic.Add(typeof(string), OleDbType.Char);
			_oracleTypeDic.Add(typeof(decimal), OleDbType.Decimal);
			_oracleTypeDic.Add(typeof(DateTime), OleDbType.Date);
			_oracleTypeDic.Add(typeof(bool), OleDbType.Boolean);
		}

		public static OleDbType Get(Type type)
		{
			try
			{
				return _oracleTypeDic[type];
			}
			catch (KeyNotFoundException)
			{
				throw new Exception("unknown type: only string, decimal, DateTime, bool supported");
			}
		}
	}
}
