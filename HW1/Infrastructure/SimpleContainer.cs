namespace VyshKat.Infrastructure;

public class SimpleContainer
{
    private readonly Dictionary<Type, Func<SimpleContainer, object>>
        _factories = new();

    private readonly Dictionary<Type, object>
        _instances = new();

    public void RegisterSingleton<TService>(
        Func<SimpleContainer, TService> factory)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        object CreateService(SimpleContainer container)
        {
            TService service = factory(container);

            if (service is null)
            {
                throw new InvalidOperationException("Фабрика вернула null.");
            }

            return service;
        }

        _factories.Add(typeof(TService), CreateService);
    }

    public TService Resolve<TService>()
        where TService : class
    {
        Type serviceType = typeof(TService);

        if (_instances.TryGetValue(serviceType, out var instance))
        {
            return (TService)instance;
        }

        if (!_factories.TryGetValue(serviceType, out var factory))
        {
            throw new InvalidOperationException(
                $"Сервис {serviceType.Name} не зарегистрирован.");
        }

        TService service = (TService)factory(this);

        _instances.Add(serviceType, service);

        return service;
    }
}
