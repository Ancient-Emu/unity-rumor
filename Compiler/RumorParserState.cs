using System;
using System.Collections.Generic;
using Exodrifter.Rumor.Engine;
using Exodrifter.Rumor.Parser;

namespace Exodrifter.Rumor.Compiler
{
	public class RumorParserState : ParserUserState
	{
		/// <summary>
		/// A list of identifiers that have already been used.
		/// </summary>
		public HashSet<string> UsedIdentifiers { get; }

		/// <summary>
		/// A list of variables that have been referenced.
		/// </summary>
		public Dictionary<string, Engine.ValueType> UsedVariables { get; }

		/// <summary>
		/// A list of variables that have the same type, but we don't know what
		/// type the variables should be.
		/// </summary>
		public List<Tuple<string, string>> LinkedVariables { get; }

		/// <summary>
		/// A list of bindings that can be called.
		/// </summary>
		public Dictionary<string, BindingHint> BindingHints { get; }

		#region Constructors

		public RumorParserState()
		{
			UsedIdentifiers = new HashSet<string>();
			UsedVariables = new Dictionary<string, Engine.ValueType>();
			LinkedVariables = new List<Tuple<string, string>>();
			BindingHints = new Dictionary<string, BindingHint>();
		}

		public RumorParserState(RumorParserState other)
		{
			UsedIdentifiers = new HashSet<string>(other.UsedIdentifiers);
			UsedVariables = new Dictionary<string, Engine.ValueType>(other.UsedVariables);
			LinkedVariables = new List<Tuple<string, string>>(other.LinkedVariables);
			BindingHints = new Dictionary<string, BindingHint>(other.BindingHints);
		}

		public ParserUserState Clone()
		{
			return new RumorParserState(this);
		}

		#endregion

		#region Bindings

		public void LinkAction(string name)
		{
			BindingHints[name] = new BindingActionHint();
		}

		public void LinkAction(string name, Engine.ValueType p1)
		{
			BindingHints[name] = new BindingActionHint1(p1);
		}

		public void LinkAction
			(string name, Engine.ValueType p1, Engine.ValueType p2)
		{
			BindingHints[name] = new BindingActionHint2(p1, p2);
		}

		public void LinkAction
			(string name, Engine.ValueType p1, Engine.ValueType p2,
			Engine.ValueType p3)
		{
			BindingHints[name] = new BindingActionHint3(p1, p2, p3);
		}

		public void LinkAction
			(string name, Engine.ValueType p1, Engine.ValueType p2,
			Engine.ValueType p3, Engine.ValueType p4)
		{
			BindingHints[name] = new BindingActionHint4(p1, p2, p3, p4);
		}

		public void LinkFunction(string name, Engine.ValueType result)
		{
			BindingHints[name] = new BindingFunctionHint(result);
		}

		public void LinkFunction
			(string name, Engine.ValueType p1, Engine.ValueType result)
		{
			BindingHints[name] = new BindingFunctionHint1(p1, result);
		}

		public void LinkFunction
			(string name, Engine.ValueType p1, Engine.ValueType p2,
			Engine.ValueType result)
		{
			BindingHints[name] = new BindingFunctionHint2(p1, p2, result);
		}

		public void LinkFunction
			(string name, Engine.ValueType p1, Engine.ValueType p2,
			Engine.ValueType p3, Engine.ValueType result)
		{
			BindingHints[name] = new BindingFunctionHint3(p1, p2, p3, result);
		}

		public void LinkFunction
			(string name, Engine.ValueType p1, Engine.ValueType p2,
			Engine.ValueType p3, Engine.ValueType p4, Engine.ValueType result)
		{
			BindingHints[name] = new BindingFunctionHint4(p1, p2, p3, p4, result);
		}

		#endregion

		#region Variables

		public void LinkVariable(int errorIndex, string id, Engine.ValueType type)
		{
			if (UsedVariables.ContainsKey(id) && UsedVariables[id] != type)
			{
				throw new ReasonException(errorIndex,
					"we are trying to use the variable \"" + id +
					"\" as a " + type + ", but it has already been " +
					"used as a " + UsedVariables[id] + "!"
				);
			}

			UsedVariables[id] = type;
			ResolveVariables(errorIndex);
		}

		public void LinkVariables(int errorIndex, string a, string b)
		{
			LinkedVariables.Add(new Tuple<string, string>(a, b));
			ResolveVariables(errorIndex);
		}

		private void ResolveVariables(int errorIndex)
		{
			bool resolveAgain = false;

			for (int i = LinkedVariables.Count - 1; i >= 0; ++i)
			{
				var tuple = LinkedVariables[i];

				if (UsedVariables.ContainsKey(tuple.Item1))
				{
					LinkVariable(errorIndex, tuple.Item2, UsedVariables[tuple.Item1]);
					LinkedVariables.RemoveAt(i);
					resolveAgain = true;
				}
				else if (UsedVariables.ContainsKey(tuple.Item2))
				{
					LinkVariable(errorIndex, tuple.Item1, UsedVariables[tuple.Item2]);
					LinkedVariables.RemoveAt(i);
					resolveAgain = true;
				}
			}

			if (resolveAgain)
			{
				ResolveVariables(errorIndex);
			}
		}

		#endregion
	}
}
