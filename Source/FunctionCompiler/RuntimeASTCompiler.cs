using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AST;
using AST.Tokens;
using FlaxEngine;

namespace NinaBirthday.FunctionCompiler
{
	public class RuntimeASTCompiler
	{
		private Tokenizer _tokenizer;
		private Parser _parser;

		public RuntimeASTCompiler()
		{
			_tokenizer = new Tokenizer()
				.AddOperand("number", @"(\d*\.\d+)|(\d+)", float.Parse)
				.AddOperand("x", @"[xX]")
				.AddOperand("y", @"[yY]")
				.AddOperand("z", @"[zZ]")

				.AddBracket(@"\(", @"\)")

				.AddUnaryOperator("min", @"[mM]in", 6, true)
				.AddUnaryOperator("max", @"[mM]ax", 6, true)
				.AddUnaryOperator("pow", @"[pP]ow", 6, true)
				.AddUnaryOperator("atan", @"[aA]tan2?", 6, true)
				.AddUnaryOperator("sqrt", @"[sS]qrt", 6, true)
				.AddUnaryOperator("cos", @"[cC]os", 6, true)
				.AddUnaryOperator("sin", @"[sS]in", 6, true)

				.AddBinaryOperator("exponent", @"\*\*", 5, true)
				.AddBinaryOperator("exponent", @"\^", 5, true)

				.AddBinaryOperator("multiply", @"\*", 4)
				.AddBinaryOperator("divide", @"\/", 4)

				.AddUnaryOperator("minus", @"\-", 3, true)
				.AddUnaryOperator("plus", @"\+", 3, true)

				.AddBinaryOperator("minus", @"\-", 2)
				.AddBinaryOperator("plus", @"\+", 2)

				.AddBinaryOperator("comma", @",", 1)

				.AddWhitespace(" ");

			_parser = new Parser();
		}

		private ParameterExpression _parameterX;
		private ParameterExpression _parameterY;
		private ParameterExpression _parameterZ;

		public Func<float, float, float, float> CompileFunction(string text)
		{
			var topNode = _parser.Parse(_tokenizer.Tokenize(text));
			if (topNode == null) return null;

			_parameterX = Expression.Parameter(typeof(float), "x");
			_parameterY = Expression.Parameter(typeof(float), "y");
			_parameterZ = Expression.Parameter(typeof(float), "z");

			Expression mathExpression = ASTNodeToExpression(topNode);


			return Expression.Lambda<Func<float, float, float, float>>(
				mathExpression,
				new ParameterExpression[] { _parameterX, _parameterY, _parameterZ }
			).Compile();
		}

		private Expression ASTNodeToExpression(ASTNode node)
		{
			switch (node.Name)
			{
			case "number":
			{
				return Expression.Constant(node.Value, node.Type);
			}
			case "x":
			{
				return _parameterX;
			}
			case "y":
			{
				return _parameterY;
			}
			case "z":
			{
				return _parameterZ;
			}
			case "exponent":
			{
				return Expression.Call(typeof(Mathf).GetMethod("Pow"), ASTNodeToExpression(node.Children[0]), ASTNodeToExpression(node.Children[1]));
			}
			case "multiply":
			{
				return Expression.Multiply(ASTNodeToExpression(node.Children[0]), ASTNodeToExpression(node.Children[1]));
			}
			case "divide":
			{
				return Expression.Divide(ASTNodeToExpression(node.Children[0]), ASTNodeToExpression(node.Children[1]));
			}
			case "minus":
			{
				if (node.Children.Count == 1)
				{
					return Expression.Negate(ASTNodeToExpression(node.Children[0]));
				}
				else
				{
					return Expression.Subtract(ASTNodeToExpression(node.Children[0]), ASTNodeToExpression(node.Children[1]));
				}
			}
			case "plus":
			{
				if (node.Children.Count == 1)
				{
					return Expression.UnaryPlus(ASTNodeToExpression(node.Children[0]));
				}
				else
				{
					return Expression.Add(ASTNodeToExpression(node.Children[0]), ASTNodeToExpression(node.Children[1]));
				}
			}
			case "sin":
			{
				return Expression.Call(typeof(Mathf).GetMethod("Sin"), ASTNodeToExpression(node.Children[0]));
			}
			case "cos":
			{
				return Expression.Call(typeof(Mathf).GetMethod("Cos"), ASTNodeToExpression(node.Children[0]));
			}
			case "sqrt":
			{
				return Expression.Call(typeof(Mathf).GetMethod("Sqrt"), ASTNodeToExpression(node.Children[0]));
			}
			case "pow":
			{
				return Expression.Call(typeof(Mathf).GetMethod("Pow"), ASTNodeToExpression(node.Children[0].Children[0]), ASTNodeToExpression(node.Children[0].Children[1]));
			}
			case "atan":
			{
				return Expression.Call(typeof(Mathf).GetMethod("Atan2"), ASTNodeToExpression(node.Children[0].Children[0]), ASTNodeToExpression(node.Children[0].Children[1]));
			}
			case "min":
			{
				return Expression.Call(typeof(Mathf).GetMethod("Min", new[] { typeof(float), typeof(float) }), ASTNodeToExpression(node.Children[0].Children[0]), ASTNodeToExpression(node.Children[0].Children[1]));
			}
			case "max":
			{
				return Expression.Call(typeof(Mathf).GetMethod("Max", new[] { typeof(float), typeof(float) }), ASTNodeToExpression(node.Children[0].Children[0]), ASTNodeToExpression(node.Children[0].Children[1]));
			}
			default:
			{
				throw new Exception();
			}
			}
			throw new Exception();
		}
	}
}
