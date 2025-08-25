using System.Linq.Expressions;

namespace OSPC.Utils.Helpers
{
	public static class ExpressionHelpers
	{
        public static string GetMemberName<T,P>(Expression<Func<T,P>> exp)
        {
            MemberExpression? me = exp.Body switch
            {
                   UnaryExpression u => u.Operand as MemberExpression,
                   MemberExpression m => m,
                   _ => throw new Exception()
            };

            if (me is null) throw new NullReferenceException(nameof(me));

            Span<char> name = stackalloc char[me.Member.Name.Length];
            me.Member.Name.AsSpan().CopyTo(name);
            name[0] = char.ToLower(name[0]);
            return new string(name);
        }
	}
}
