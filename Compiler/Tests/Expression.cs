﻿using Exodrifter.Rumor.Engine;
using Exodrifter.Rumor.Parser;
using NUnit.Framework;

namespace Exodrifter.Rumor.Compiler.Tests
{
	public static class ExpressionTest
	{
		#region Literal

		[Test]
		public static void IntegerSuccess()
		{
			var state = new State("01234567890", 4, 0);

			var n = Compiler.NumberLiteral()(ref state);
			Assert.AreEqual(new NumberValue(1234567890d), n.Evaluate());
		}

		[Test]
		public static void PositiveIntegerSuccess()
		{
			var state = new State("+01234567890", 4, 0);

			var n = Compiler.NumberLiteral()(ref state);
			Assert.AreEqual(new NumberValue(1234567890d), n.Evaluate());
		}

		[Test]
		public static void NegativeIntegerSuccess()
		{
			var state = new State("-01234567890", 4, 0);

			var n = Compiler.NumberLiteral()(ref state);
			Assert.AreEqual(new NumberValue(-1234567890d), n.Evaluate());
		}

		[Test]
		public static void DecimalSuccess()
		{
			var state = new State("01234567890.123456789", 4, 0);

			var n = Compiler.NumberLiteral()(ref state);
			Assert.AreEqual
				(new NumberValue(1234567890.123456789d), n.Evaluate());
		}

		[Test]
		public static void PositiveDecimalSuccess()
		{
			var state = new State("+01234567890.123456789", 4, 0);

			var n = Compiler.NumberLiteral()(ref state);
			Assert.AreEqual
				(new NumberValue(1234567890.123456789d), n.Evaluate());
		}

		[Test]
		public static void NegativeDecimalSuccess()
		{
			var state = new State("-01234567890.123456789", 4, 0);

			var n = Compiler.NumberLiteral()(ref state);
			Assert.AreEqual
				(new NumberValue(-1234567890.123456789d), n.Evaluate());
		}

		#endregion

		#region Addition

		[Test]
		public static void AdditionSuccess()
		{
			var state = new State("1+2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(3d), exp.Evaluate());
		}

		[Test]
		public static void AdditionPlusSignSuccess()
		{
			var state = new State("1++2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(3d), exp.Evaluate());
		}

		[Test]
		public static void AdditionMinusSignSuccess()
		{
			var state = new State("1+-2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(-1d), exp.Evaluate());
		}

		[Test]
		public static void AdditionWhitespaceSuccess()
		{
			var state = new State("1  +   2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(3d), exp.Evaluate());
		}

		[Test]
		public static void AdditionMultilineSuccess()
		{
			var state = new State("1\n   +\n   2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(3d), exp.Evaluate());
		}

		[Test]
		public static void AdditionMultipleSuccess()
		{
			var state = new State("1 + 2 + 3 + 4", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(10d), exp.Evaluate());
		}

		[Test]
		public static void AdditionMultipleMultilineSuccess()
		{
			var state = new State("1 +\n 2 +\n 3 + 4", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(10d), exp.Evaluate());
		}

		#endregion

		#region Subtraction

		[Test]
		public static void SubtractionSuccess()
		{
			var state = new State("1-2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(-1d), exp.Evaluate());
		}

		[Test]
		public static void SubtractionPlusSignSuccess()
		{
			var state = new State("1-+2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(-1d), exp.Evaluate());
		}

		[Test]
		public static void SubtractionMinusSignSuccess()
		{
			var state = new State("1--2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(3d), exp.Evaluate());
		}

		[Test]
		public static void SubtractionWhitespaceSuccess()
		{
			var state = new State("1  -   2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(-1d), exp.Evaluate());
		}

		[Test]
		public static void SubtractionMultilineSuccess()
		{
			var state = new State("1\n   -\n   2", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(-1d), exp.Evaluate());
		}

		[Test]
		public static void SubtractionMultipleSuccess()
		{
			var state = new State("1 - 2 - 3 - 4", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(-8d), exp.Evaluate());
		}

		[Test]
		public static void SubtractionMultipleMultilineSuccess()
		{
			var state = new State("1 -\n 2 -\n 3 - 4", 4, 0);

			var exp = Compiler.Math()(ref state);
			Assert.AreEqual(new NumberValue(-8d), exp.Evaluate());
		}

		#endregion
	}
}
