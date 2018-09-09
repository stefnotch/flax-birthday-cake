using AST.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST
{
	//TODO: Incremental parsing
	public class Parser
	{
		public ASTNode Parse(IEnumerable<IToken<ITokenType>> input)
		{
			var stack = new Stack<IToken<OperatorTokenType>>();
			var output = new Stack<ASTNode>();

			foreach (var token in input)
			{
				switch (token)
				{
					case IToken<BracketTokenType> bracket:
					{
						//Opening bracket
						if (bracket.Type.Previous == null)
						{
							stack.Push(bracket);
						}
						//Closing bracket
						else
						{
							bool foundOpeningBracket = false;
							while (stack.Count > 0)
							{
								var stackToken = stack.Pop();
								if (stackToken is IToken<BracketTokenType> && bracket.Type.Previous.Match(stackToken.Text).Success)
								{
									foundOpeningBracket = true;
									break;
								}
								else
								{
									var args = PopMultiple(output, stackToken.Type.ArgumentCount);
									output.Push(stackToken.ToASTNode(args));
								}
							}

							if (!foundOpeningBracket)
							{
								throw new ParserException("Mismatched parentheses.");
							}
						}

						break;
					}
					case IToken<OperatorTokenType> op:
					{
						while (stack.Count > 0)
						{
							var top = stack.Peek();
							int precedenceComparison = op.Type.ComparePrecedence(top.Type);
							if (precedenceComparison < 0
								|| (!op.Type.IsRightAssociative && precedenceComparison == 0))
							{
								stack.Pop();

								var args = PopMultiple(output, top.Type.ArgumentCount);
								output.Push(top.ToASTNode(args));
							}
							else
							{
								break;
							}
						}

						stack.Push(op);
						break;
					}
					case IToken<OperandTokenType> operand:
						output.Push(token.ToASTNode());
						break;
					default:
					{
						throw new ParserException("Unknown token type");
					}
				}
			}

			while (stack.Count > 0)
			{
				var token = stack.Pop();
				if (token.Type is BracketTokenType)
				{
					throw new ParserException("Mismatched parentheses.");
				}

				var args = PopMultiple(output, token.Type.ArgumentCount);
				output.Push(token.ToASTNode(args));
			}

			if (output.Count > 1) throw new ParserException("Too many operands");

			return output.Pop();
		}

		private List<T> PopMultiple<T>(Stack<T> stack, int count)
		{
			//TODO: Create ??? nodes instead or something (better error handling)
			if (stack.Count < count) throw new ParserException("Not enough arguments");
			var list = new List<T>(new T[count]);

			for (int i = count - 1; i >= 0; i--)
			{
				list[i] = stack.Pop();
			}

			return list;
		}
	}
}
