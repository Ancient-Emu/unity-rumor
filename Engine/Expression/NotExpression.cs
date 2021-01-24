﻿namespace Exodrifter.Rumor.Engine
{
	public class NotExpression : Expression<BooleanValue>
	{
		private readonly Expression<BooleanValue> expression;

		public NotExpression(Expression<BooleanValue> expression)
		{
			this.expression = expression;
		}

		public override BooleanValue Evaluate(RumorScope scope)
		{
			return !expression.Evaluate(scope);
		}

		public override Expression<BooleanValue> Simplify()
		{
			if (expression is BooleanLiteral)
			{
				var e = expression as BooleanLiteral;
				return new BooleanLiteral(!e.Value);
			}
			else
			{
				var e = expression.Simplify();

				if (e == expression)
				{
					return this;
				}
				else
				{
					return new NotExpression(e).Simplify();
				}
			}
		}

		#region Equality

		public override bool Equals(object obj)
		{
			return Equals(obj as NotExpression);
		}

		public bool Equals(NotExpression other)
		{
			if (other == null)
			{
				return false;
			}

			return Equals(expression, other.expression);
		}

		public override int GetHashCode()
		{
			return Util.GetHashCode(expression);
		}

		#endregion

		public override string ToString()
		{
			return "(not " + expression.ToString() + ")";
		}
	}
}
