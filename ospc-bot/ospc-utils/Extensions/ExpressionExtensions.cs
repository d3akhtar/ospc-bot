using System.Linq.Expressions;

namespace OSPC.Utils
{
	public static class ExpUtils
	{
		public static string GetMemberName<TIn,TOut>(Expression<Func<TIn,TOut>> exp)
		{
			MemberExpression? me = exp.Body switch
			{
				UnaryExpression u => u.Operand as MemberExpression,
				MemberExpression m => m,
				_ => throw new Exception()
			};

			return me?.Member.Name ?? throw new NullReferenceException("GetMemberExpressionPropertyName<TIn,TOut>()");
		}
	}
}
