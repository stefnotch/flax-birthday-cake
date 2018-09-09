using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST.Tokens
{
	public class OperandTokenType : ITokenType
	{
		public string Name { get; }

		public OperandTokenType(string name)
		{
			Name = name;
		}

		public IToken<ITokenType> CreateToken(string text, object value = null, Type type = null)
		{
			return new Token<OperandTokenType>(this, text, value, type);
		}
	}
}
