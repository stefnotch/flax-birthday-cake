using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AST.Tokens;

namespace AST.Tokens
{

	//TODO: Incremental parsing
	public class Tokenizer
	{
		private class TokenCreator
		{
			public ITokenType TokenType;
			public Regex Regex;
			public Func<string, object> ToValue;
			public Type ValueType;

			public TokenCreator(ITokenType tokenType, Regex regex)
			{
				TokenType = tokenType;
				Regex = regex;
			}
		}
		private readonly Dictionary<string, int> _hasUnaryVariantHack = new Dictionary<string, int>();

		private readonly List<TokenCreator> _tokenCreators = new List<TokenCreator>();
		private readonly List<Regex> _whitespaceTokens = new List<Regex>();

		public Tokenizer AddOperand(string name, string matcherRegex)
		{
			var tokenCreator = new TokenCreator(new OperandTokenType(name), new Regex(matcherRegex));
			_tokenCreators.Add(tokenCreator);
			return this;
		}

		public Tokenizer AddOperand<T>(string name, string matcherRegex, Func<string, T> toValue)
		{
			var tokenCreator = new TokenCreator(new OperandTokenType(name), new Regex(matcherRegex))
			{
				ToValue = s => toValue(s),
				ValueType = typeof(T)
			};
			_tokenCreators.Add(tokenCreator);
			return this;
		}

		/// <summary>
		/// Adds a unary operator. Is a convenience function that calls AddOperator
		/// </summary>
		/// <param name="name"></param>
		/// <param name="matcherRegex"></param>
		/// <param name="precedence">The precedence of this operator, usually has the same precedence as the exponent operator</param>
		/// <param name="isRightAssociative">Unary operators are usually right associative</param>
		/// <returns></returns>
		public Tokenizer AddUnaryOperator(string name, string matcherRegex, int precedence, bool isRightAssociative = true)
		{
			AddOperator(name, matcherRegex, 1, precedence, isRightAssociative);
			return this;
		}

		public Tokenizer AddBinaryOperator(string name, string matcherRegex, int precedence, bool isRightAssociative = false)
		{
			AddOperator(name, matcherRegex, 2, precedence, isRightAssociative);
			return this;
		}

		public Tokenizer AddOperator(string name, string matcherRegex, int argumentCount, int precedence, bool isRightAssociative = false)
		{
			_tokenCreators.Add(new TokenCreator(
					new OperatorTokenType(name)
					{
						ArgumentCount = argumentCount,
						Precedence = precedence,
						IsRightAssociative = isRightAssociative
					},
					new Regex(matcherRegex)
				));

			if (argumentCount == 2 || argumentCount == 1)
			{
				if (_hasUnaryVariantHack.TryGetValue(name, out int value))
				{
					_hasUnaryVariantHack[name] = value + argumentCount;
				}
				else
				{
					_hasUnaryVariantHack.Add(name, argumentCount);
				}

			}
			return this;
		}

		public Tokenizer AddBracket(string bracketARegex, string bracketBRegex)
		{
			_tokenCreators.Add(new TokenCreator(
					new BracketTokenType(bracketARegex),
					new Regex(bracketARegex)
				));

			_tokenCreators.Add(new TokenCreator(
					new BracketTokenType(bracketBRegex, bracketARegex),
					new Regex(bracketBRegex)
				));
			return this;
		}


		public Tokenizer AddWhitespace(string whitespaceRegex)
		{
			_whitespaceTokens.Add(new Regex(whitespaceRegex));
			return this;
		}

		public IEnumerable<IToken<ITokenType>> Tokenize(string input)
		{
			int index = 0;
			IToken<ITokenType> previousToken = null;
			SkipWhitespace(input, ref index);
			while (index < input.Length)
			{
				var token = GetNextToken(input, ref index, previousToken);
				previousToken = token;

				if (token == null)
				{
					FlaxEngine.Debug.Log(token);
					FlaxEngine.Debug.Log(input);
					FlaxEngine.Debug.Log(index);
					FlaxEngine.Debug.Log(previousToken);
				}
				yield return token;
				SkipWhitespace(input, ref index);
			}
		}

		private IToken<ITokenType> GetNextToken(string input, ref int startAt, IToken<ITokenType> previousToken = null)
		{
			foreach (var tokenCreator in _tokenCreators)
			{
				if (_hasUnaryVariantHack.TryGetValue(tokenCreator.TokenType.Name, out int unaryHackValue) && unaryHackValue == 3) //1+2
				{
					if (tokenCreator.TokenType is OperatorTokenType op && !(tokenCreator.TokenType is BracketTokenType))
					{
						/* Token types: operand, operator, left bracket, right bracket
						 * 
						 * binary if it follows an operand or a right parenthesis
						 * unary if it immediately follows another operator or a left parenthesis
						 */
						if (previousToken == null)
						{
							// Unary
							if (op.ArgumentCount != 1) continue;
						}
						else if ((previousToken.Type is OperatorTokenType && !(previousToken.Type is BracketTokenType))
							|| (previousToken.Type is BracketTokenType bracket && bracket.Previous == null))
						{
							// Unary
							if (op.ArgumentCount != 1) continue;
						}
						else
						{
							// Binary
							if (op.ArgumentCount == 1) continue;
						}
					}
				}

				Match match = tokenCreator.Regex.Match(input, startAt);

				if (match.Success && match.Index == startAt)
				{
					if (match.Length == 0) throw new TokenizerException("Empty token, caused by the following regex: " + tokenCreator.Regex);

					startAt += match.Length;

					IToken<ITokenType> token;
					if (tokenCreator.ToValue != null)
					{
						token = tokenCreator.TokenType.CreateToken(match.Value, tokenCreator.ToValue(match.Value), tokenCreator.ValueType);
					}
					else
					{
						token = tokenCreator.TokenType.CreateToken(match.Value);
					}
					if (token != null) return token;
				}
			}
			throw new TokenizerException("Remaining string starts with an unknown token: " + input.Substring(startAt));
		}

		private void SkipWhitespace(string input, ref int startAt)
		{
			int oldStartAt = startAt;
			// Skip the whitespace
			int newStartAt = SkipSingleWhitespace(input, oldStartAt);
			while (newStartAt != oldStartAt)
			{
				oldStartAt = newStartAt;
				newStartAt = SkipSingleWhitespace(input, oldStartAt);
			}

			startAt = oldStartAt;
		}

		private int SkipSingleWhitespace(string input, int startAt)
		{
			foreach (var regex in _whitespaceTokens)
			{
				Match match = regex.Match(input, startAt);

				if (match.Success && match.Index == startAt)
				{
					if (match.Length == 0) throw new TokenizerException("Empty token, caused by the following regex: " + regex);

					return startAt + match.Length;

				}
			}
			return startAt;
		}
	}
}
