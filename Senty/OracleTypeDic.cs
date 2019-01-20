using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;

namespace Senty
{
	static class OracleTypeDic
	{
		private static Dictionary<Type, OracleType> _oracleTypeDic;

		static OracleTypeDic()
		{
			_oracleTypeDic = new Dictionary<Type, OracleType>();
			_oracleTypeDic.Add(typeof(string), OracleType.Char);
			_oracleTypeDic.Add(typeof(decimal), OracleType.Number);
			_oracleTypeDic.Add(typeof(DateTime), OracleType.DateTime);
		}

		public static OracleType Get(Type type)
		{
			try
			{
				return _oracleTypeDic[type];
			}
			catch (KeyNotFoundException)
			{
				throw new Exception("unknown type: only string, decimal, DateTime supported");
			}
		}
	}
}
