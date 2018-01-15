﻿using Exodrifter.Rumor.Engine;
using System;
using System.Runtime.Serialization;

namespace Exodrifter.Rumor.Expressions
{
	/// <summary>
	/// Represents an equals operator that is used to check the equality of
	/// two arguments.
	/// </summary>
	[Serializable]
	public class EqualsExpression : OpExpression
	{
		public EqualsExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		public override Value Evaluate(Scope scope, Bindings bindings)
		{
			var l = left.Evaluate(scope, bindings) ?? new ObjectValue(null);
			var r = right.Evaluate(scope, bindings) ?? new ObjectValue(null);
			return new BoolValue(l.EqualTo(r).AsBool());
		}

		public override string ToString()
		{
			return left + "==" + right;
		}

		#region Serialization

		public EqualsExpression
			(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion
	}
}
