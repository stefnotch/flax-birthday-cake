using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace NinaBirthday.FunctionCompiler
{
	public static class RuntimeCSharpCompiler
	{
		public static Func<float, float, float, float> CompileFunction(string functionString)
		{
			string code = @"
using System;
namespace RuntimeCompiledFunction {
	public class RuntimeFunctionClass {
		public static float FUNCTIONNAME(float x, float y, float z) {
			return FUNCTIONSTRING;
		}
	}
}";
			string actualCode = code.Replace("FUNCTIONSTRING", functionString);

			try
			{
				CSharpCodeProvider codeProvider = new CSharpCodeProvider();
				CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(new CompilerParameters(), actualCode);
				FlaxEngine.Debug.Log(compilerResults.Errors[0]);
				Type functionType = compilerResults.CompiledAssembly.GetType("RuntimeCompiledFunction.RuntimeFunctionClass");
				return (Func<float, float, float, float>)Delegate.CreateDelegate(typeof(Func<float, float, float, float>), functionType.GetMethod("FUNCTIONNAME"));
			}
			catch (Exception e)
			{
				FlaxEngine.Debug.Log(e.StackTrace);
				throw;
			}
		}
	}
}
