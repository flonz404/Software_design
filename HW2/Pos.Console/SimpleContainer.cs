namespace Pos.ConsoleApp;

public sealed class SimpleContainer
{
    private readonly Dictionary<Type, Func<SimpleContainer, object>> _factories = new();
    private readonly Dictionary<Type, object> _instances = new();
    private readonly HashSet<Type> _resolving = new();

    public void Register<T>(Func<SimpleContainer, T> factory) where T : class
    {
        ArgumentNullException.ThrowIfNull(factory);
        if (_factories.ContainsKey(typeof(T)))
        {
            throw new InvalidOperationException($"Тип {typeof(T).Name} уже зарегистрирован.");
        }

        object Create(SimpleContainer container)
        {
            return factory(container);
        }

        _factories.Add(typeof(T), Create);
    }

    public T Resolve<T>() where T : class
    {
        Type type = typeof(T);
        if (_instances.TryGetValue(type, out object? instance))
        {
            return (T)instance;
        }
        if (!_factories.TryGetValue(type, out Func<SimpleContainer, object>? factory))
        {
            throw new InvalidOperationException($"Тип {type.Name} не зарегистрирован.");
        }
        if (!_resolving.Add(type))
        {
            throw new InvalidOperationException($"Циклическая зависимость: {type.Name}.");
        }
        try
        {
            object created = factory(this)
                ?? throw new InvalidOperationException($"Фабрика {type.Name} вернула null.");
            _instances.Add(type, created);
            return (T)created;
        }
        finally
        {
            _resolving.Remove(type);
        }
    }
}
