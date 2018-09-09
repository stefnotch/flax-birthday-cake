using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AST.Tokens
{
	public class BracketTokenType : OperatorTokenType
	{
		public Regex Previous { get; }

		public BracketTokenType(string name) : base(name)
		{
		}

		public BracketTokenType(string name, string previous) : this(name)
		{
			Previous = new Regex(previous);
		}

		public override IToken<ITokenType> CreateToken(string text, object value = null, Type type = null)
		{
			return new Token<BracketTokenType>(this, text, value, type);
		}
	}
}
