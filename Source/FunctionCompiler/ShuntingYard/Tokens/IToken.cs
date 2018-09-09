using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST.Tokens
{
	public interface IToken<out TType> where TType : ITokenType
	{
		string Text { get; }

		TType Type { get; }

		ASTNode ToASTNode(List<ASTNode> children = null);
	}
}
