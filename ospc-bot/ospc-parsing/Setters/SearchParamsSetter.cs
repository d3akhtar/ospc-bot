namespace OSPC.Parsing
{
    public interface ISetter<T>
    {
        public void Set(T param, object value);
    }

    public class Setter<TParam, TValue> : ISetter<TParam>
    {
        private readonly Action<TParam, TValue> _set;

        public Setter(Action<TParam, TValue> set)
        {
            _set = set;
        }

        public void Set(TParam param, object value)
        {
            _set(param, (TValue)value!);
        }
    }
}