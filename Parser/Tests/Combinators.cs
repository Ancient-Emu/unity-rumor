﻿using NUnit.Framework;
using System;

namespace Exodrifter.Rumor.Parser.Tests
{
	public static class Combinators
	{
		#region Chain

		[Test]
		public static void ChainL1Success()
		{
			var state = new State("a,z,b", 4, 0);

			Func<char, char, char> fn = (l, r) => {
				if (l > r)
				{
					return l;
				}
				else
				{
					return r;
				}
			};

			var result = Parse.Letter
				.ChainL1(Parse.Char(',').Then(fn))(state);

			Assert.AreEqual('z', result);
		}

		#endregion

		#region Many

		[Test]
		public static void ManySuccess()
		{
			var state = new State("aaa", 4, 0);

			var result = Parse.Char('a').Many(0)(state);
			Assert.AreEqual(new char[] { 'a', 'a', 'a' }, result);
		}

		[Test]
		public static void Many0Success()
		{
			var state = new State("a", 4, 0);

			var result = Parse.Char('z').Many(0)(state);
			Assert.AreEqual(new char[] { }, result);
		}

		[Test]
		public static void Many1Success()
		{
			var state = new State("a", 4, 0);

			var result = Parse.Char('a').Many(1)(state);
			Assert.AreEqual(new char[] { 'a' }, result);
		}

		[Test]
		public static void ManyFailure()
		{
			var state = new State("a", 4, 0);

			var exception = Assert.Throws<ParserException>(() =>
				Parse.Char('z').Many(1)(state)
			);
			Assert.AreEqual(0, exception.Index);
			Assert.AreEqual(
				new string[] { "at least 1 more of z" },
				exception.Expected
			);
		}

		#endregion

		#region Or

		[Test]
		public static void OrLeftSuccess()
		{
			var state = new State("a", 4, 0);

			var result = Parse.Char('a').Or(Parse.Char('z'))(state);
			Assert.AreEqual('a', result);
		}

		[Test]
		public static void OrRightSuccess()
		{
			var state = new State("z", 4, 0);

			var result = Parse.Char('a').Or(Parse.Char('z'))(state);
			Assert.AreEqual('z', result);
		}

		[Test]
		public static void OrFailure()
		{
			var state = new State("m", 4, 0);

			var exception = Assert.Throws<ParserException>(() =>
				Parse.Char('a').Or(Parse.Char('z'))(state)
			);
			Assert.AreEqual(0, exception.Index);
			Assert.AreEqual(new string[] { "a", "z" }, exception.Expected);
		}

		#endregion

		#region Rollback

		/// <summary>
		/// These test if the parser rolls back successfully when trying other
		/// options.
		/// </summary>
		[Test]
		public static void RollbackSuccess()
		{
			var state = new State("az", 4, 0);

			var result = Parse.Char('a').Then(Parse.Char('b'))
				.Or(Parse.Char('a').Then(Parse.Char('z')))(state);
			Assert.AreEqual('z', result);
		}

		#endregion

		#region Then

		[Test]
		public static void ThenSuccess()
		{
			var state = new State("ab", 4, 0);

			var result = Parse.Char('a').Then(Parse.Char('b'))(state);
			Assert.AreEqual('b', result);
		}

		[Test]
		public static void ThenLeftFailure()
		{
			var state = new State("ab", 4, 0);

			var exception = Assert.Throws<ParserException>(() =>
				Parse.Char('z').Then(Parse.Char('b'))(state)
			);
			Assert.AreEqual(0, exception.Index);
			Assert.AreEqual(new string[] { "z" }, exception.Expected);
		}

		[Test]
		public static void ThenRightFailure()
		{
			var state = new State("ab", 4, 0);

			var exception = Assert.Throws<ParserException>(() =>
				Parse.Char('a').Then(Parse.Char('z'))(state)
			);
			Assert.AreEqual(1, exception.Index);
			Assert.AreEqual(new string[] { "z" }, exception.Expected);
		}

		#endregion

		#region Until

		[Test]
		public static void UntilSuccess()
		{
			var state = new State("aaaab", 4, 0);

			var result = Parse.Char('a').Until(Parse.Char('b'))
				.Then(Parse.Char('b'))(state);

			Assert.AreEqual('b', result);
		}

		[Test]
		public static void UntilFailure()
		{
			var state = new State("aaaa", 4, 0);

			var exception = Assert.Throws<ParserException>(() =>
				Parse.Char('a').Until(Parse.Char('b')).String()(state)
			);
			Assert.AreEqual(4, exception.Index);
			Assert.AreEqual(new string[] { "b" }, exception.Expected);
		}

		#endregion
	}
}
