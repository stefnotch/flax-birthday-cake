using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AST.Tokens
{
	public class OperatorTokenType : ITokenType
	{
		public int Precedence { get; set; }

		public bool IsRightAssociative { get; set; }

		public int ArgumentCount { get; set; }

		public string Name { get; }

		public OperatorTokenType(string name)
		{
			Name = name;
		}

		/// <summary>
		/// A value that indicates if this operator's precedence is greater than, equal or less than
		/// the other operator's precedence
		/// </summary>
		/// <param name="other">The other operator</param>
		/// <returns>
		/// Positive if the precedence of this operator is greater.
		/// 0 if they have the same precedence.
		/// Negative if the precedence of this operator is lower.
		/// </returns>
		public int ComparePrecedence(OperatorTokenType other)
		{
			return Precedence.CompareTo(other.Precedence);
		}

		public virtual IToken<ITokenType> CreateToken(string text, object value = null, Type type = null)
		{
			return new Token<OperatorTokenType>(this, text, value, type);
		}
	}
}
