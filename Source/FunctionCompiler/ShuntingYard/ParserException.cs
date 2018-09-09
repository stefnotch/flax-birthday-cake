using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST
{
	public class ParserException : Exception
	{
		public ParserException() : base()
		{
		}

		public ParserException(string message) : base(message)
		{
		}

		public ParserException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ParserException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
