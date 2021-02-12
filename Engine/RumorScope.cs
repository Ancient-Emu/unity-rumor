using System;
using System.Collections.Generic;

namespace Exodrifter.Rumor.Engine
{
	public class RumorScope
	{
		private readonly Dictionary<string, Value> vars;

		public RumorScope()
		{
			vars = new Dictionary<string, Value>();
		}

		public Value Get(string name)
		{
			if (vars.ContainsKey(name))
			{
				return vars[name];
			}
			return null;
		}

		public void Set(string name, Value value)
		{
			if (vars.ContainsKey(name) && vars[name].Type != value.Type)
			{
				throw new InvalidOperationException(
					"Cannot change the type of \"" + name + "\"!"
				);
			}
			else
			{
				vars[name] = value;
			}
		}

		public void Set(string name, bool value)
		{
			Set(name, new BooleanValue(value));
		}

		public void Set(string name, double value)
		{
			Set(name, new NumberValue(value));
		}

		public void Set(string name, string value)
		{
			Set(name, new StringValue(value));
		}
	}
}
