using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST
{
	public class ASTNode
	{
		public string Name { get; }

		public string Text { get; }

		public Type Type { get; }

		public object Value { get; }

		public List<ASTNode> Children { get; }

		public ASTNode(string text, List<ASTNode> children = null)
		{
			Text = text;
			Children = children ?? new List<ASTNode>();
		}

		public ASTNode(string name, string text, List<ASTNode> children = null) : this(text, children)
		{
			Name = name;
		}

		public ASTNode(string name, string text, object value, Type type, List<ASTNode> children = null) : this(name, text, children)
		{
			Value = value;
			Type = type;
		}

		/// <summary>
		/// Pre-order depth first traversal
		/// </summary>
		public IEnumerable<ASTNode> DepthFirst()
		{
			var stack = new Stack<ASTNode>();
			stack.Push(this);
			while (stack.Count > 0)
			{
				var currentElement = stack.Pop();
				yield return currentElement;

				for (int i = currentElement.Children.Count - 1; i >= 0; i--)
				{
					stack.Push(currentElement.Children[i]);
				}
			}
		}
	}
}
