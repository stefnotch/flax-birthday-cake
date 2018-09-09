using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST.Tokens
{
	public interface ITokenType
	{
		string Name { get; }

		IToken<ITokenType> CreateToken(string text, object value = null, Type type = null);
	}
}
