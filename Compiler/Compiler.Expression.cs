﻿using Exodrifter.Rumor.Engine;
using Exodrifter.Rumor.Parser;
using System;

namespace Exodrifter.Rumor.Compiler
{
	// Define type aliases for convenience
	using NumberOp =
		Func<Expression<NumberValue>, Expression<NumberValue>, Expression<NumberValue>>;
	using BooleanOp =
		Func<Expression<BooleanValue>, Expression<BooleanValue>, Expression<BooleanValue>>;

	public static partial class Compiler
	{
		/// <summary>
		/// Helper for constructing operator parsers that can have leading
		/// whitespace.
		/// </summary>
		/// <typeparam name="T">The type to return.</typeparam>
		/// <param name="value">The value to return.</param>
		/// <param name="ops">The strings which represent the operator.</param>
		private static Parser<T> Op<T>(T value, params string[] ops)
		{
			return state =>
			{
				using (var transaction = new Transaction(state))
				{
					Parse.Whitespaces(state);
					Parse.SameOrIndented(state);
					Parse.String(ops)(state);

					transaction.CommitIndex();
					return value;
				}
			};
		}

		#region Comparison

		public static Parser<Expression<BooleanValue>> Comparison =>
			BooleanComparison
			.Or(NumberComparison)
			.Or(StringComparison);

		private static Parser<Expression<BooleanValue>> BooleanComparison
		{
			get
			{
				return state =>
				{
					var p = Logic.Or(ComparisonParenthesis);

					var l = p(state);
					var op = ComparisonOps<BooleanValue>()(state);
					var r = p(state);
					return op(l, r);
				};
			}
		}

		private static Parser<Expression<BooleanValue>> NumberComparison
		{
			get
			{
				return state =>
				{
					var l = Math(state);
					var op = ComparisonOps<NumberValue>()(state);
					var r = Math(state);
					return op(l, r);
				};
			}
		}

		private static Parser<Expression<BooleanValue>> StringComparison
		{
			get
			{
				return state =>
				{
					var l = QuoteLiteral(state);
					var op = ComparisonOps<StringValue>()(state);
					var r = QuoteLiteral(state);
					return op(l, r);
				};
			}
		}

		/// <summary>
		/// Parses a comparison operator, which will return a
		/// <see cref="BooleanLiteral"/> when evaluated.
		/// </summary>
		private static Parser
			<Func<Expression<T>, Expression<T>, Expression<BooleanValue>>>
			ComparisonOps<T>() where T : Value
		{
			return IsNot<T>()
				.Or(Is<T>());
		}

		/// <summary>
		/// Parses a logic is operator.
		/// </summary>
		private static Parser
			<Func<Expression<T>, Expression<T>, Expression<BooleanValue>>>
			Is<T>() where T : Value =>
			Op<Func<Expression<T>, Expression<T>, Expression<BooleanValue>>>
			((l, r) => new IsExpression<T>(l, r), "is", "==");

		/// <summary>
		/// Parses a logic not equal operator.
		/// </summary>
		private static Parser
			<Func<Expression<T>, Expression<T>, Expression<BooleanValue>>>
			IsNot<T>() where T : Value =>
			Op<Func<Expression<T>, Expression<T>, Expression<BooleanValue>>>
			((l, r) => new IsNotExpression<T>(l, r), "is not", "!=");

		/// <summary>
		/// Parses a comparison expression wrapped in parenthesis.
		/// </summary>
		private static Parser<Expression<BooleanValue>> ComparisonParenthesis
		{
			get
			{
				return state =>
				{
					using (var transaction = new Transaction(state))
					{
						Parse.Whitespaces(state);
						Parse.SameOrIndented(state);
						var logic = Parse.SurroundBlock('(', ')',
							Comparison, Parse.SameOrIndented
						)(state);

						transaction.CommitIndex();
						return logic;
					}
				};
			}
		}

		#endregion

		#region Logic

		/// <summary>
		/// Parses a logic expression, which will return a
		/// <see cref="BooleanLiteral"/> when evaluated.
		/// </summary>
		public static Parser<Expression<BooleanValue>> Logic =>
			Parse.ChainL1(AndPieces, Or);

		// Groups the "and" operators
		private static Parser<Expression<BooleanValue>> AndPieces =>
			Parse.ChainL1(XorPieces, And);

		// Groups the "xor" operators
		private static Parser<Expression<BooleanValue>> XorPieces =>
			Parse.ChainL1(LogicPiece, Xor);

		private static Parser<Expression<BooleanValue>> LogicPiece =>
			LogicParenthesis
				.Or(NotExpression)
				.Or(BooleanLiteral);

		/// <summary>
		/// Parses a logic or operator.
		/// </summary>
		private static Parser<BooleanOp> Or =>
			Op<BooleanOp>((l, r) => new OrExpression(l, r), "or", "||");

		/// <summary>
		/// Parses a logic and operator.
		/// </summary>
		private static Parser<BooleanOp> And =>
			Op<BooleanOp>((l, r) => new AndExpression(l, r), "and", "&&");

		/// <summary>
		/// Parses a logic xor operator.
		/// </summary>
		private static Parser<BooleanOp> Xor =>
			Op<BooleanOp>((l, r) => new XorExpression(l, r), "xor", "^");

		/// <summary>
		/// Parses a boolean literal.
		/// </summary>
		private static Parser<Expression<BooleanValue>> BooleanLiteral
		{
			get
			{
				return state =>
				{
					using (var transaction = new Transaction(state))
					{
						Parse.Whitespaces(state);
						Parse.SameOrIndented(state);
						var b = Parse.String("true").Then(true)
								.Or(Parse.String("false").Then(false))
								(state);

						transaction.CommitIndex();
						return new BooleanLiteral(b);
					}
				};
			}
		}

		/// <summary>
		/// Parses a logic not operator and the logic expression associated
		/// with it.
		/// </summary>
		private static Parser<Expression<BooleanValue>> NotExpression
		{
			get
			{
				return state =>
				{
					using (var transaction = new Transaction(state))
					{
						Parse.Whitespaces(state);
						Parse.SameOrIndented(state);
						Parse.String("not", "!")(state);
						var logic = Logic.Or(Comparison)(state);

						transaction.CommitIndex();
						return new NotExpression(logic);
					}
				};
			}
		}

		/// <summary>
		/// Parses a logic expression wrapped in parenthesis.
		/// </summary>
		private static Parser<Expression<BooleanValue>> LogicParenthesis
		{
			get
			{
				return state =>
				{
					using (var transaction = new Transaction(state))
					{
						Parse.Whitespaces(state);
						Parse.SameOrIndented(state);
						var logic = Parse.SurroundBlock('(', ')',
							Logic, Parse.SameOrIndented
						)(state);

						transaction.CommitIndex();
						return logic;
					}
				};
			}
		}

		#endregion

		#region Math

		/// <summary>
		/// Parses an arithmetic expression, which will return a
		/// <see cref="NumberValue"/> when evaluated.
		/// </summary>
		public static Parser<Expression<NumberValue>> Math =>
			Parse.ChainL1(MultipliedPieces, AddOrSubtract);

		// Groups the multiplication and division operators
		private static Parser<Expression<NumberValue>> MultipliedPieces =>
			Parse.ChainL1(MathPiece, MultiplyOrDivide);

		private static Parser<Expression<NumberValue>> MathPiece =>
			MathParenthesis.Or(NumberLiteral);

		/// <summary>
		/// Parses an addition or subtraction operator.
		/// </summary>
		private static Parser<NumberOp> AddOrSubtract =>
			Add.Or(Subtract);

		/// <summary>
		/// Parses an addition operator.
		/// </summary>
		private static Parser<NumberOp> Add =>
			Op<NumberOp>((l, r) => new AddExpression(l, r), "+");

		/// <summary>
		/// Parses a subtraction operator.
		/// </summary>
		private static Parser<NumberOp> Subtract =>
			Op<NumberOp>((l, r) => new SubtractExpression(l, r), "-");

		/// <summary>
		/// Parses a multiplication or division operator.
		/// </summary>
		private static Parser<NumberOp> MultiplyOrDivide =>
			Multiply.Or(Divide);

		/// <summary>
		/// Parses an multiplication operator.
		/// </summary>
		private static Parser<NumberOp> Multiply =>
			Op<NumberOp>((l, r) => new MultiplyExpression(l, r), "*");

		/// <summary>
		/// Parses a division operator.
		/// </summary>
		private static Parser<NumberOp> Divide =>
			Op<NumberOp>((l, r) => new DivideExpression(l, r), "/");

		/// <summary>
		/// Parses a number literal.
		/// </summary>
		private static Parser<Expression<NumberValue>> NumberLiteral
		{
			get
			{
				return state =>
				{
					using (var transaction = new Transaction(state))
					{
						Parse.Whitespaces(state);
						Parse.SameOrIndented(state);
						var num = Parse.Double(state);

						transaction.CommitIndex();
						return new NumberLiteral(num);
					}
				};
			}
		}

		/// <summary>
		/// Parses a math expression wrapped in parenthesis.
		/// </summary>
		private static Parser<Expression<NumberValue>> MathParenthesis
		{
			get
			{
				return state =>
				{
					Parse.Whitespaces(state);
					Parse.SameOrIndented(state);
					var math = Parse.SurroundBlock('(', ')',
						Math, Parse.SameOrIndented
					)(state);

					return math;
				};
			}
		}

		#endregion

		#region Substitution

		/// <summary>
		/// Parses a substitution. Shared by the <see cref="Text"/> and
		/// <see cref="Quote"/> parsers.
		/// </summary>
		private static Parser<Expression<StringValue>> Substitution =>
			Parse.Surround('{', '}',
				Math.Select(x => (Expression<StringValue>)
					new ToStringExpression<NumberValue>(x))
				.Or(Logic.Select(x => (Expression<StringValue>)
					new ToStringExpression<BooleanValue>(x)))
				.Or(Quote)
			);

		#endregion

		#region Text

		/// <summary>
		/// Parses a text expression, or a block of unquoted strings, which will
		/// return a <see cref="StringValue"/> when evaluated.
		/// </summary>
		public static Parser<Expression<StringValue>> Text =>
			PrefixText<Unit>(null);

		/// <summary>
		/// Parses a text expression, or a block of unquoted strings, that are
		/// prefixed with <paramref name="prefix"/> which will return a
		/// <see cref="StringValue"/> when evaluated.
		/// </summary>
		/// <param name="prefix">
		/// The prefix parser to use at the beginning of each line, or null for
		/// no prefix parsing.
		/// </param>
		public static Parser<Expression<StringValue>> PrefixText<T>
			(Parser<T> prefix)
		{
			return state =>
			{
				using (var transaction = new Transaction(state))
				{
					// Parse each line of the text
					var lines = Parse.PrefixBlock1(
						prefix,
						TextLine,
						Parse.Indented
					)(state);

					// Combine each line of the text into a single expression
					Expression<StringValue> result = null;
					foreach (var line in lines)
					{
						if (result != null)
						{
							var s = new StringValue(" ");
							result = new ConcatExpression(
								new ConcatExpression(result, new StringLiteral(s)),
								line
							);
						}
						else
						{
							result = line;
						}
					}

					transaction.CommitIndex();
					return result.Simplify();
				}
			};
		}

		/// <summary>
		/// Parses a single line of a text block.
		/// </summary>
		private static Parser<Expression<StringValue>> TextLine
		{
			get
			{
				return state =>
				{
					using (var transaction = new Transaction(state))
					{
						// If there is at least one space, prepend the resulting
						// string with a space. This is to maintain the space
						// between words across different lines.
						var s = "";
						if (Parse.FollowedBy(Parse.Spaces1)(state))
						{
							Parse.Spaces1(state);
							s = " ";
						}

						var rest =
							Parse.AnyChar
							.Until(Parse.EOL.Or(Parse.Char('{').Then(new Unit())))
							.String()(state);

						// If we're at the end of the line, there is nothing
						// left to parse.
						if (Parse.FollowedBy(Parse.EOL)(state))
						{
							transaction.CommitIndex();
							return new StringLiteral(s + rest);
						}

						// Otherwise, we've found a substitution that needs to
						// be parsed.
						else
						{
							var substitution = Substitution(state);

							// Parse the rest of the line (which may contain
							// another substitution)
							var remaining = TextLine(state);

							transaction.CommitIndex();
							return new ConcatExpression(
								new ConcatExpression(
									new StringLiteral(s + rest),
									substitution
								),
								remaining
							);
						}
					}
				};
			}
		}

		#endregion

		#region Quote

		/// <summary>
		/// Parses a quote expression, or a quoted string, which will return a
		/// <see cref="StringValue"/> when evaluated.
		/// </summary>
		public static Parser<Expression<StringValue>> Quote =>
			Parse.Surround('\"', '\"', QuoteInternal);

		private static Parser<Expression<StringValue>> QuoteLiteral
		{
			get
			{
				return state =>
				{
					using (var transaction = new Transaction(state))
					{
						Parse.Whitespaces(state);
						Parse.SameOrIndented(state);
						var quote = Quote(state);

						transaction.CommitIndex();
						return quote;
					}
				};
			}
		}

		private static Parser<Expression<StringValue>> QuoteInternal
		{
			get
			{
				return state =>
				{
					using (var transaction = new Transaction(state))
					{
						var start = Parse.AnyChar
							.Until(Parse.Char('\\', '{', '\"'))
							.String()
							.Select(str => new StringLiteral(str))
							(state);

						var rest = EscapeSequence
							.Or(SubstitutionQuote)
							.Or(Parse.Pure<Expression<StringValue>>(null))
							(state);

						transaction.CommitIndex();
						if (rest != null)
						{
							return new ConcatExpression(start, rest);
						}
						else
						{
							return start;
						}
					}
				};
			}
		}

		private static Parser<Expression<StringValue>> EscapeSequence
		{
			get
			{
				return Parse.String("\\n").Then("\n")
					.Or(Parse.String("\\r").Then("\r"))
					.Or(Parse.String("\\{").Then("{"))
					.Or(Parse.String("\\\"").Then("\""))
					.Or(Parse.String("\\\\").Then("\\"))
					.Select(str =>
						(Expression<StringValue>)new StringLiteral(str)
					);
			}
		}

		private static Parser<Expression<StringValue>> SubstitutionQuote
		{
			get
			{
				return state =>
				{
					using (var transaction = new Transaction(state))
					{
						var sub = Substitution(state);
						var rest = QuoteInternal(state);

						transaction.CommitIndex();
						return new ConcatExpression(sub, rest);
					}
				};
			}

		}

		#endregion
	}
}
