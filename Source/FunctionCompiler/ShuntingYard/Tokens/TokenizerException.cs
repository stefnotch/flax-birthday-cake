using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST.Tokens
{
	public class TokenizerException : Exception
	{
		public TokenizerException() : base()
		{
		}

		public TokenizerException(string message) : base(message)
		{
		}

		public TokenizerException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected TokenizerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
