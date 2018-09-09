using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AST.Tokens
{
	public class Token<TType> : IToken<TType> where TType : ITokenType
	{
		public string Text { get; }
		public object Value { get; }
		public Type ValueType { get; }
		public TType Type { get; }

		internal Token(TType type, string text, object value, Type valueType)
		{
			Type = type;
			Text = text;
			Value = value;
			ValueType = valueType;
		}

		public ASTNode ToASTNode(List<ASTNode> children = null)
		{
			if (Value != null)
			{
				return new ASTNode(Type.Name, Text, Value, ValueType, children);
			}
			else
			{
				return new ASTNode(Type.Name, Text, children);
			}
		}
	}
}
