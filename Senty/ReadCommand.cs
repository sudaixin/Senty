using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senty
{
	public class ReadCommand
	{
		public ReadCommand() { }
		public ReadCommand(string cmdText) { CommandText = cmdText; }

		public string CommandText = "";

		private ParameterCollection _Parameters = new ParameterCollection();
		public ParameterCollection Parameters { get { return _Parameters; } }

		public void Add(string paramName, object value, string cmdText = "")
		{
			CommandText += cmdText;
			_Parameters.Add(paramName, value);
		}

		public class ParameterCollection
		{
			private List<ParameterPair> ParamList = new List<ParameterPair>();

			public struct ParameterPair
			{
				public string ParamName;
				public object Value;
			}

			public void Add(string paramName, object value)
			{
				ParamList.Add(new ParameterPair { ParamName = paramName, Value = value });
			}

			public ParameterPair[] Get()
			{
				return ParamList.ToArray();
			}
		}
	}
}
