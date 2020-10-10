﻿namespace Exodrifter.Rumor.Parser
{
	public class ThenParser<T, U> : Parser<U>
	{
		private readonly Parser<T> first;
		private readonly Parser<U> second;

		public ThenParser(Parser<T> first, Parser<U> second)
		{
			this.first = first;
			this.second = second;
		}

		public override Result<U> Parse(State state)
		{
			var result = first.Parse(state);
			if (result.IsSuccess)
			{
				return second.Parse(result.NextState);
			}
			else
			{
				return Result<U>.Error(result.ErrorIndex, result.Expected);
			}
		}
	}
}
