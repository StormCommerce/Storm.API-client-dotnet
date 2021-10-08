namespace Enferno.StormApiClient.Test.Builders
{
    public abstract class AbstractBuilder<T>
    {
        public abstract T Build();

        public static implicit operator T(AbstractBuilder<T> builder)
        {
            return builder.Build();
        }
    }
}
