﻿using System;
using System.Collections.Generic;

namespace Exodrifter.Rumor.Parser
{
	public delegate T Parser<T>(ref State state);

	/// <summary>
	/// This class represents the result of the parser when the parser has no
	/// value to return.
	/// </summary>
	public class Unit { }

	public static partial class Parse
	{
		/// <summary>
		/// Wraps a raw value in a parser.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to wrap in a parser.</param>
		/// <returns>
		/// A parser that doesn't do anything but return the value.
		/// </returns>
		public static Parser<T> Pure<T>(T value)
		{
			return (ref State state) =>
			{
				return value;
			};
		}

		#region Parenthesis

		/// <summary>
		/// Returns a parser that parses delimiters around another parser.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="before">The beginning delimiter.</param>
		/// <param name="after">The ending delimiter.</param>
		/// <param name="parser">The parser between the delimiters.</param>
		public static Parser<T> Parenthesis<T>
			(char before, char after, Parser<T> parser) =>
			Parenthesis(Char(before), Char(after), parser);

		/// <summary>
		/// Returns a parser that parses delimiters around another parser.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="before">The beginning delimiter.</param>
		/// <param name="after">The ending delimiter.</param>
		/// <param name="parser">The parser between the delimiters.</param>
		public static Parser<T> Parenthesis<T>
			(string before, string after, Parser<T> parser) =>
			Parenthesis(String(before), String(after), parser);

		/// <summary>
		/// Returns a parser that parses delimiters around another parser.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <typeparam name="U">The type of the beginning delimiter.</typeparam>
		/// <typeparam name="V">The type of the ending delimiter.</typeparam>
		/// <param name="before">The parser for the beginning delimiter.</param>
		/// <param name="after">The parser for the ending delimiter.</param>
		/// <param name="parser">The parser between the delimiters.</param>
		public static Parser<T> Parenthesis<T, U, V>
			(Parser<U> before, Parser<V> after, Parser<T> parser)
		{
			return (ref State state) =>
			{
				var temp = state;
				before(ref temp);
				var result = parser(ref temp);
				after(ref temp);
				state = temp;

				return result;
			};
		}

		/// <summary>
		/// Returns a parser that parses delimiters around another parser,
		/// allowing for whitespace that keeps the content in the same block.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="before">The beginning delimiter.</param>
		/// <param name="after">The ending delimiter.</param>
		/// <param name="parser">The parser between the delimiters.</param>
		/// <param name="indentType">The indentation parser.</param>
		public static Parser<T> Parenthesis<T>
			(char before, char after, Parser<T> parser,
			Parser<int> indentType) =>
			Parenthesis(Char(before), Char(after), parser, indentType);

		/// <summary>
		/// Returns a parser that parses delimiters around another parser,
		/// allowing for whitespace that keeps the content in the same block.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="before">The beginning delimiter.</param>
		/// <param name="after">The ending delimiter.</param>
		/// <param name="parser">The parser between the delimiters.</param>
		/// <param name="indentType">The indentation parser.</param>
		public static Parser<T> Parenthesis<T>
			(string before, string after, Parser<T> parser,
			Parser<int> indentType) =>
			Parenthesis(String(before), String(after), parser, indentType);

		/// <summary>
		/// Returns a parser that parses delimiters around another parser,
		/// allowing for whitespace that keeps the content in the same block.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <typeparam name="U">The type of the beginning delimiter.</typeparam>
		/// <typeparam name="V">The type of the ending delimiter.</typeparam>
		/// <param name="before">The parser for the beginning delimiter.</param>
		/// <param name="after">The parser for the ending delimiter.</param>
		/// <param name="parser">The parser between the delimiters.</param>
		/// <param name="indentType">The indentation parser.</param>
		public static Parser<T> Parenthesis<T, U, V>
			(Parser<U> before, Parser<V> after, Parser<T> parser,
			Parser<int> indentType)
		{
			return (ref State state) =>
			{
				var temp = state;
				before(ref temp);
				Whitespaces(ref temp);
				indentType(ref temp);
				var result = parser(ref temp);
				after(ref temp);
				state = temp;

				return result;
			};
		}

		#endregion

		#region Char

		/// <summary>
		/// Returns a parser that parses the specified character.
		/// </summary>
		/// <param name="ch">The character to parse.</param>
		public static Parser<char> Char(char ch) =>
			Char(ch.Equals, ch.ToString());

		/// <summary>
		/// Returns a parser that parses a digit.
		/// </summary>
		public static Parser<char> Letter =>
			Char(char.IsLetter, "letter");

		/// <summary>
		/// Returns a parser that parses a digit.
		/// </summary>
		public static Parser<char> Digit =>
			Char(char.IsDigit, "digit");

		/// <summary>
		/// Returns a parser that parses a letter or number.
		/// </summary>
		public static Parser<char> Alphanumeric =>
			Char(char.IsLetterOrDigit, "alphanumeric character");

		/// <summary>
		/// Returns a parser that parses any character.
		/// </summary>
		public static Parser<char> AnyChar =>
			Char(_ => true, "any character");

		/// <summary>
		/// Returns a parser that parses a space or tab.
		/// </summary>
		public static Parser<char> Space =>
			Char(ch => ch == ' ' || ch == '\t', "space");

		/// <summary>
		/// Returns a parser that parses zero or more spaces and tabs.
		/// </summary>
		public static Parser<string> Spaces =>
			Space.Many().String();

		/// <summary>
		/// Returns a parser that parses one or more spaces and tabs.
		/// </summary>
		public static Parser<string> Spaces1 =>
			Space.Many1().String();

		/// <summary>
		/// Returns a parser that parses a carriage return or newline.
		/// </summary>
		public static Parser<char> NewLine =>
			Char(ch => ch == '\r' || ch == '\n', "newline");

		/// <summary>
		/// Returns a parser that parses a whitespace character. This includes
		/// space, tab, carriage return, and newline.
		/// </summary>
		public static Parser<char> Whitespace =>
			Char(char.IsWhiteSpace, "whitespace");

		/// <summary>
		/// Returns a parser that parses zero or more whitespace characters.
		/// </summary>
		public static Parser<string> Whitespaces =>
			Whitespace.Many().String();

		/// <summary>
		/// Returns a parser that parses one or more whitespace characters.
		/// </summary>
		public static Parser<string> Whitespaces1 =>
			Whitespace.Many1().String();

		/// <summary>
		/// Returns a parser that parses any character that exists in the
		/// string.
		/// </summary>
		/// <param name="str">The characters to parse.</param>
		public static Parser<char> Char(string str) =>
			Char(str.ToCharArray());

		/// <summary>
		/// Returns a parser that parses any character that exists in the
		/// array.
		/// </summary>
		/// <param name="str">The characters to parse.</param>
		public static Parser<char> Char(params char[] chs)
		{
			Parser<char> parser = null;
			foreach (var ch in chs)
			{
				if (parser != null)
				{
					parser.Or(Char(ch));
				}
				else
				{
					parser = Char(ch);
				}
			}
			return parser;
		}

		/// <summary>
		/// Returns a parser that parses any character which satisfies a
		/// predicate.
		/// </summary>
		/// <param name="predicate">The predicate to satisfy.</param>
		/// <param name="expected">
		/// The expected value (used in error messages).
		/// </param>
		public static Parser<char> Char
			(Func<char, bool> predicate, string expected)
		{
			return (ref State state) =>
			{
				if (state.Source.Length <= state.Index)
				{
					throw new ParserException(state.Index, expected);
				}

				var ch = state.Source[state.Index];
				if (predicate(ch))
				{
					state = state.AddIndex(1);
					return ch;
				}

				throw new ParserException(state.Index, expected);
			};
		}

		#endregion

		#region Chain

		/// <summary>
		/// Returns a new parser that repeats a parser one or more times,
		/// separated by <paramref name="op"/>, and returns the left associative
		/// application of the function returned by the <paramref name="op"/>
		/// on those values.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="parser">The parser to repeat.</param>
		/// <param name="op">The parser for the operator.</param>
		public static Parser<T> ChainL1<T>
			(this Parser<T> parser, Parser<Func<T, T, T>> op)
		{
			return (ref State state) =>
			{
				var x = parser(ref state);
				while (true)
				{
					try
					{
						var temp = state;
						var fn = op(ref temp);
						var y = parser(ref temp);
						x = fn(x, y);

						state = temp;
					}
					catch (ParserException)
					{
						break;
					}
				}
				return x;
			};
		}

		#endregion

		#region Number

		public static Parser<string> Sign =>
			Char('-').Or(Char('+')).String().Or(Pure(""));

		/// <summary>
		/// Parses an integer number.
		/// </summary>
		public static Parser<string> Number
		{
			get
			{
				return (ref State state) =>
				{
					var temp = state;
					var digit = Digit(ref temp);
					var rest = Digit.Or(Char('_')).Many().String()(ref temp);
					state = temp;

					return digit + rest;
				};
			}
		}

		/// <summary>
		/// Parses a integer number with an optional sign.
		/// </summary>
		public static Parser<string> SignedNumber
		{
			get
			{
				return (ref State state) =>
				{
					var temp = state;
					var sign = Sign(ref temp);
					var number = Number(ref temp);
					state = temp;

					return sign + number;
				};
			}
		}

		/// <summary>
		/// Parses a number with a decimal component.
		/// </summary>
		public static Parser<string> Decimal
		{
			get
			{
				return (ref State state) =>
				{
					var temp = state;
					var l = Number(ref temp);
					state = temp;

					try
					{
						var p = Char('.')(ref temp);
						var r = Number(ref temp);
						state = temp;
						return l + p + r;
					}
					catch (ParserException)
					{
						return l;
					}
				};
			}
		}

		/// <summary>
		/// Parses a number with a decimal component and an optional sign.
		/// </summary>
		public static Parser<string> SignedDecimal
		{
			get
			{
				return (ref State state) =>
				{
					var temp = state;
					var sign = Sign(ref temp);
					var number = Decimal(ref temp);
					state = temp;

					return sign + number;
				};
			}
		}

		/// <summary>
		/// Parses a double with an optional sign.
		/// </summary>
		public static Parser<double> Double
		{
			get
			{
				return (ref State state) =>
				{
					var temp = state;
					var str = SignedDecimal(ref temp);

					double result;
					if (!double.TryParse(str, out result))
					{
						throw new ParserException(state.Index, "double");
					}

					state = temp;
					return result;
				};
			}
		}

		#endregion

		#region EOF

		/// <summary>
		/// Returns a parser that succeeds if the parser is at the end of the
		/// line or at the end of a file.
		/// </summary>
		public static Parser<Unit> EOL =>
			NewLine.Then(new Unit()).Or(EOF);

		/// <summary>
		/// Returns a parser that succeeds if the parser is at the end of the
		/// file.
		/// </summary>
		public static Parser<Unit> EOF
		{
			get
			{
				return (ref State state) =>
				{
					if (state.EOF)
					{
						return new Unit();
					}
					else
					{
						throw new ParserException(state.Index, "end of file");
					}
				};
			}
		}

		#endregion

		#region Followed By

		/// <summary>
		/// Returns true if the parser would succeed.
		/// </summary>
		/// <typeparam name="T">The return type of the parser.</typeparam>
		/// <param name="parser">The parser to try.</param>
		public static Parser<bool> FollowedBy<T>(Parser<T> parser)
		{
			return (ref State state) =>
			{
				try
				{
					var temp = state;
					parser(ref temp);
					return true;
				}
				catch (ParserException)
				{
					return false;
				}
			};
		}

		/// <summary>
		/// Returns a parser that only succeeds if the following parser
		/// would also succeed directly after the first.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <typeparam name="U">The type of the following parser.</typeparam>
		/// <param name="parser">The parser.</param>
		/// <param name="after">The following parser that must succeed.</param>
		public static Parser<T> FollowedBy<T, U>
			(this Parser<T> parser, Parser<U> after)
		{
			return (ref State state) =>
			{
				var temp = state;
				var result = parser(ref temp);

				var tempAfter = temp;
				after(ref tempAfter);

				state = temp;
				return result;
			};
		}

		/// <summary>
		/// Returns true if the parser would fail.
		/// </summary>
		/// <typeparam name="T">The return type of the parser.</typeparam>
		/// <param name="parser">The parser to try.</param>
		public static Parser<bool> NotFollowedBy<T>(Parser<T> parser) =>
			FollowedBy(parser).Select(x => !x);

		/// <summary>
		/// Returns a parser that only succeeds if the following parser
		/// would also fail directly after the first.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <typeparam name="U">The type of the following parser.</typeparam>
		/// <param name="parser">The parser.</param>
		/// <param name="after">The following parser that must fail.</param>
		/// <param name="failure">
		/// The expected failure (used in error messages).
		/// </param>
		public static Parser<T> NotFollowedBy<T, U>
			(this Parser<T> parser, Parser<U> after, string failure)
		{
			return (ref State state) =>
			{
				var temp = state;
				var result = parser(ref temp);

				try
				{
					var tempAfter = temp;
					after(ref tempAfter);
				}
				catch (ParserException)
				{
					state = temp;
					return result;
				}

				throw new ParserException(temp.Index, failure);
			};
		}

		#endregion

		#region Indented

		/// <summary>
		/// Returns a parser that returns the current column if the current
		/// parser position is less than the reference indentation level.
		/// </summary>
		public static Parser<int> Unindented
		{
			get
			{
				return (ref State state) =>
				{
					int current = CalculateIndentFrom(state, state.Index);
					int indent = CalculateIndentFrom(state, state.IndentIndex);

					if (current == indent)
					{
						return current;
					}
					else
					{
						throw new ParserException(
							state.Index,
							"line indented less than column " + indent
						);
					}
				};
			}
		}

		/// <summary>
		/// Returns a parser that returns the current column if the current
		/// parser position is the same as the reference indentation level.
		/// </summary>
		public static Parser<int> Same
		{
			get
			{
				return (ref State state) =>
				{
					int current = CalculateIndentFrom(state, state.Index);
					int indent = CalculateIndentFrom(state, state.IndentIndex);

					if (current == indent)
					{
						return current;
					}
					else
					{
						throw new ParserException(
							state.Index,
							"line indented to column " + indent
						);
					}
				};
			}
		}

		/// <summary>
		/// Returns a parser that returns the current column if the current
		/// parser position is at the same indentation level or more than the
		/// reference indentation level.
		/// </summary>
		public static Parser<int> SameOrIndented
		{
			get
			{
				return (ref State state) =>
				{
					int current = CalculateIndentFrom(state, state.Index);
					int indent = CalculateIndentFrom(state, state.IndentIndex);

					if (current >= indent)
					{
						return current;
					}
					else
					{
						throw new ParserException(
							state.Index,
							"line indented to column " + indent + " or more"
						);
					}
				};
			}
		}

		/// <summary>
		/// Returns a parser that returns the current column if the current
		/// parser position is at a greater indentation level than the reference
		/// indentation level.
		/// </summary>
		public static Parser<int> Indented
		{
			get
			{
				return (ref State state) =>
				{
					int current = CalculateIndentFrom(state, state.Index);
					int indent = CalculateIndentFrom(state, state.IndentIndex);

					if (current > indent)
					{
						return current;
					}
					else
					{
						throw new ParserException(
							state.Index,
							"line indented to column " + indent + " or more"
						);
					}
				};
			}
		}

		private static int CalculateIndentFrom(State state, int index)
		{
			var line = "";
			for (int i = index - 1; i >= 0; i--)
			{
				var ch = state.Source[i];
				if (ch == '\n' || ch == '\r')
				{
					break;
				}
				else
				{
					line += ch;
				}
			}

			// The line is backwards, so to read the line in order we need to
			// read it starting at the end.
			var column = 1;
			for (int i = line.Length - 1; i >= 0; i--)
			{
				var ch = line[i];
				if (ch == ' ')
				{
					column++;
				}
				else if (ch == '\t')
				{
					column += state.TabSize - (column % state.TabSize);
				}
				else
				{
					break;
				}
			}

			return column;
		}

		/// <summary>
		/// Parses an indented block of one or more occurrences of
		/// <paramref name="parser"/>.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="parser">The parser to use.</param>
		/// <param name="indentType">The indentation parser.</param>
		/// <param name="minimum">
		/// The minimum number of times the parser must be successful.
		/// </param>
		public static Parser<List<T>> Block1<T>
			(this Parser<T> parser, Parser<int> indentType) =>
			Block(parser, indentType, 1);

		/// <summary>
		/// Parses an indented block of zero or more occurrences of
		/// <paramref name="parser"/>.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="parser">The parser to use.</param>
		/// <param name="indentType">The indentation parser.</param>
		/// <param name="minimum">
		/// The minimum number of times the parser must be successful.
		/// </param>
		public static Parser<List<T>> Block<T>
			(Parser<T> parser, Parser<int> indentType, int minimum = 0)
		{
			return (ref State state) =>
			{
				var results = new List<T>();

				while (true)
				{
					var temp = state;
					results.Add(parser(ref temp));

					// Consume the rest of the line
					Space.Until(NewLine.Then(new Unit()).Or(EOF))
						.Then(NewLine.Then(new Unit()).Or(EOF))(ref temp);
					state = temp;

					// Stop if we are at the end of the file
					if (FollowedBy(EOF)(ref state))
					{
						return results;
					}

					// Check if the block continues
					try
					{
						Spaces.Then(indentType)(ref temp);
						state = temp;
					}
					catch (ParserException exception)
					{
						if (results.Count < minimum)
						{
							var delta = minimum - results.Count;
							throw new ParserException(
								exception.Index,
								"at least " + delta + " more of " +
								string.Join(", ", exception.Expected)
							);
						}
						else
						{
							return results;
						}
					}
				}
			};
		}

		#endregion

		#region LINQ-like

		/// <summary>
		/// Returns a new parser that returns the result of running an arbitrary
		/// function over the result of this parser.
		/// </summary>
		/// <typeparam name="U">The new type of the result.</typeparam>
		/// <param name="fn">The function to use over the result.</param>
		public static Parser<U> Select<T, U>
			(this Parser<T> parser, Func<T, U> fn)
		{
			return (ref State state) =>
			{
				return fn(parser(ref state));
			};
		}

		/// <summary>
		/// Returns a new parser that returns the result of another parser only
		/// if its result satisfies some predicate.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="parser">The parser to check the result of.</param>
		/// <param name="predicate">The predicate to satisfy.</param>
		/// <param name="expected">
		/// The expected value (used in error messages).
		/// </param>
		public static Parser<T> Where<T>
			(this Parser<T> parser, Func<T, bool> predicate, string expected)
		{
			return (ref State state) =>
			{
				var temp = state;
				var result = parser(ref temp);
				if (predicate(result))
				{
					state = temp;
					return result;
				}
				else
				{
					throw new ParserException(state.Index, expected);
				}
			};
		}

		#endregion

		#region Many

		/// <summary>
		/// Returns a new parser that repeats a parser one or more times.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="parser">The parser to repeat.</param>
		public static Parser<List<T>> Many1<T>(this Parser<T> parser) =>
			Many(parser, 1);

		/// <summary>
		/// Returns a new parser that repeats a parser for a specified minimum
		/// number of successful times.
		/// </summary>
		/// <typeparam name="T">The return type of the parser.</typeparam>
		/// <param name="parser">The parser to repeat.</param>
		/// <param name="minimum">The minimum number of successes.</param>
		public static Parser<List<T>> Many<T>
			(this Parser<T> parser, int minimum = 0)
		{
			return (ref State state) =>
			{
				var results = new List<T>();

				while (true)
				{
					try
					{
						var temp = state;
						var result = parser(ref temp);
						state = temp;
						results.Add(result);
					}
					catch (ParserException exception)
					{
						if (results.Count < minimum)
						{
							var delta = minimum - results.Count;
							throw new ParserException(
								exception.Index,
								"at least " + delta + " more of " +
								string.Join(", ", exception.Expected)
							);
						}
						else
						{
							return results;
						}
					}
				}
			};
		}

		#endregion

		#region Maybe

		/// <summary>
		/// Returns a parser that returns the default value if the parser fails.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <param name="parser">The parser to try.</param>
		public static Parser<Maybe<T>> Maybe<T>(this Parser<T> parser)
		{
			return (ref State state) =>
			{
				try
				{
					return new Maybe<T>(parser(ref state));
				}
				catch (ParserException)
				{
					return new Maybe<T>();
				}
			};
		}

		#endregion

		#region Or

		/// <summary>
		/// Returns a parser that returns the first successful result.
		/// </summary>
		/// <typeparam name="T">The type of the parsers to use.</typeparam>
		/// <param name="parser">The first parser to try.</param>
		/// <param name="others">The second parser to try.</param>
		public static Parser<T> Or<T>(this Parser<T> first, Parser<T> second)
		{
			return (ref State state) =>
			{
				try
				{
					var temp1 = state;
					var result = first(ref temp1);
					state = temp1;
					return result;
				}
				catch (ParserException e1)
				{
					try
					{
						var temp2 = state;
						var result = second(ref temp2);
						state = temp2;
						return result;
					}
					catch (ParserException e2)
					{
						var expected = new List<string>(
							e1.Expected.Length + e2.Expected.Length
						);
						expected.AddRange(e1.Expected);
						expected.AddRange(e2.Expected);
						throw new ParserException
							(state.Index, expected.ToArray());
					}
				}
			};
		}

		/// <summary>
		/// Returns the first parser that returns a successful result.
		/// </summary>
		/// <typeparam name="T">The type of the parsers to use.</typeparam>
		/// <param name="parsers">The parsers to try.</param>
		public static Parser<T> Or<T>(params Parser<T>[] parsers)
		{
			Parser<T> result = null;
			foreach (var parser in parsers)
			{
				if (result != null)
				{
					result.Or(parser);
				}
				else
				{
					result = parser;
				}
			}
			return result;
		}

		/// <summary>
		/// Returns a parser that returns the first successful result.
		/// </summary>
		/// <typeparam name="T">The type of the parsers to use.</typeparam>
		/// <param name="parser">The first parser to try.</param>
		/// <param name="others">The other parsers to try.</param>
		public static Parser<T> Or<T>(this Parser<T> parser, params Parser<T>[] others)
		{
			Parser<T> result = parser;
			foreach (var other in others)
			{
				result.Or(other);
			}
			return result;
		}

		#endregion

		#region Reference

		/// <summary>
		/// Refer to another parser indirectly, which can be used to allow a set
		/// of circular-dependent parsers to be created.
		/// </summary>
		/// <typeparam name="T">The type of the parser to refer to</typeparam>
		/// <param name="reference">
		/// A function that produces the indirectly-referenced parser.
		/// </param>
		public static Parser<T> Ref<T>(Func<Parser<T>> reference)
		{
			Parser<T> p = null;

			return (ref State state) =>
			{
				if (p == null)
					p = reference();

				return p(ref state);
			};
		}

		#endregion

		#region String

		/// <summary>
		/// Converts a parser that returns a character into a parser that
		/// returns a string.
		/// </summary>
		/// <param name="parser">The parser to convert.</param>
		public static Parser<string> String(this Parser<char> parser) =>
			parser.Select(x => x.ToString());

		/// <summary>
		/// Converts a parser that returns a list of characters into a parser
		/// that returns a string.
		/// </summary>
		/// <param name="parser">The parser to convert.</param>
		public static Parser<string> String(this Parser<List<char>> parser)
		{
			return parser.Select(chs => new string(chs.ToArray()));
		}

		/// <summary>
		/// Parses the specified string.
		/// </summary>
		/// <param name="str">The string to parse.</param>
		public static Parser<string> String(string str)
		{
			return (ref State state) =>
			{
				if (string.IsNullOrEmpty(str))
				{
					return str;
				}

				if (state.Source.Length <= state.Index + str.Length - 1)
				{
					throw new ParserException(state.Index, str);
				}

				if (state.Source.Substring(state.Index, str.Length) == str)
				{
					state = state.AddIndex(str.Length);
					return str;
				}

				throw new ParserException(state.Index, str);
			};
		}

		/// <summary>
		/// Parses any one of the specified strings.
		/// </summary>
		/// <param name="str">The string to parse.</param>
		public static Parser<string> String(params string[] strs)
		{
			Parser<string> result = null;
			foreach (var str in strs)
			{
				if (result != null)
				{
					result.Or(String(str));
				}
				else
				{
					result = String(str);
				}
			}
			return result;
		}

		#endregion

		#region Then

		/// <summary>
		/// Returns a parser which runs the first parser, discards the result,
		/// then returns a different value.
		/// </summary>
		/// <typeparam name="T">The type of the parser.</typeparam>
		/// <typeparam name="U">The type of the value to return.</typeparam>
		/// <param name="first">The parser.</param>
		/// <param name="second">The value to return.</param>
		public static Parser<U> Then<T, U>(this Parser<T> first, U value)
		{
			return (ref State state) =>
			{
				var result = first(ref state);
				return value;
			};
		}

		/// <summary>
		/// Returns a parser which runs the first parser, discards the result,
		/// then runs the second parser.
		/// </summary>
		/// <typeparam name="T">The type to discard.</typeparam>
		/// <typeparam name="U">The type to return.</typeparam>
		/// <param name="first">The parser to discard the result of.</param>
		/// <param name="second">The parser to return the result of.</param>
		public static Parser<U> Then<T, U>
			(this Parser<T> first, Parser<U> second)
		{
			return (ref State state) =>
			{
				first(ref state);
				return second(ref state);
			};
		}

		#endregion

		#region Until

		/// <summary>
		/// Runs a parser until the result of another parser is successful.
		/// </summary>
		/// <typeparam name="T">The type of the parser to run.</typeparam>
		/// <typeparam name="U">The type of the parser to check.</typeparam>
		/// <param name="parser">The parser to run.</param>
		/// <param name="until">If this parser succeeds, stop.</param>
		public static Parser<List<T>> Until<T, U>
			(this Parser<T> parser, Parser<U> until)
		{
			return (ref State state) =>
			{
				var results = new List<T>();
				while (true)
				{
					try
					{
						var temp = state;
						until(ref temp);
						break;
					}
					catch (ParserException untilException)
					{
						try
						{
							results.Add(parser(ref state));
						}
						catch (ParserException parserException)
						{
							// If we're at the end of the source file, throw
							// the parsing exception for the until parser
							// instead, since that is more likely descriptive
							// of the content that is missing.
							if (state.EOF)
							{
								throw untilException;
							}
							else
							{
								throw parserException;
							}
						}
					}
				}
				return results;
			};
		}

		#endregion
	}
}
