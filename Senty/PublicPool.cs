using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Senty
{
	public static class PublicPool<T> where T : Entity
	{
		private static Dictionary<string, T> _PoolDic = null;

		public static void Load(T[] array)
		{
			IDObject idObj = typeof(T).GetIDColumn();
			if (idObj == null) throw new Exception("ID column not exists");
			Load(array, idObj.FldInfo);
		}

		public static void Load(T[] array, string keyField)
		{
			FieldInfo fi = typeof(T).GetField(keyField);
			if (fi == null) throw new Exception("keyField not exists");
			Load(array, fi);
		}

		public static void Load(T[] array, FieldInfo fi)
		{
			_PoolDic = new Dictionary<string, T>();
			foreach (T tObj in array)
			{
				_PoolDic.Add(fi.GetValue(tObj) as string, tObj);
			}
		}

		public static T GetById(string id)
		{
			T t = default(T);
			if (!string.IsNullOrEmpty(id))
			{
				_PoolDic.TryGetValue(id, out t);
			}
			return t;
		}

		public static T[] GetArray()
		{
			return _PoolDic.Values.ToArray();
		}
	}
}
