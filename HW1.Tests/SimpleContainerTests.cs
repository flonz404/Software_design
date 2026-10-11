using VyshKat.Infrastructure;
using VyshKat.Services;
using Xunit;

namespace VyshKat.Tests;

public class SimpleContainerTests
{
    [Fact]
    public void Resolve_WhenSingletonIsRequestedTwice_ReturnsSameInstance()
    {
        // Arrange.
        SimpleContainer container = new SimpleContainer();
        container.RegisterSingleton<IServiceCenter>(CreateServiceCenter);

        // Act.
        IServiceCenter first = container.Resolve<IServiceCenter>();
        IServiceCenter second = container.Resolve<IServiceCenter>();

        // Assert: один экземпляр — публичное обещание singleton.
        Assert.Same(first, second);
    }

    [Fact]
    public void Resolve_WhenContainersAreDifferent_ReturnsIndependentInstances()
    {
        // Arrange.
        SimpleContainer firstContainer = new SimpleContainer();
        SimpleContainer secondContainer = new SimpleContainer();
        firstContainer.RegisterSingleton<IServiceCenter>(CreateServiceCenter);
        secondContainer.RegisterSingleton<IServiceCenter>(CreateServiceCenter);

        // Act.
        IServiceCenter first = firstContainer.Resolve<IServiceCenter>();
        IServiceCenter second = secondContainer.Resolve<IServiceCenter>();

        // Assert.
        Assert.NotSame(first, second);
    }

    [Fact]
    public void RegisterSingleton_WhenFactoryIsRegistered_DelaysCreationUntilResolve()
    {
        // Arrange.
        SimpleContainer container = new SimpleContainer();
        int createdCount = 0;

        IServiceCenter CreateTrackedService(SimpleContainer services)
        {
            createdCount++;
            return new ServiceCenter();
        }

        // Act.
        container.RegisterSingleton<IServiceCenter>(CreateTrackedService);

        // Assert: регистрация не должна запускать создание сервиса.
        Assert.Equal(0, createdCount);
    }

    [Fact]
    public void Resolve_WhenServiceIsNotRegistered_Throws()
    {
        // Arrange.
        SimpleContainer container = new SimpleContainer();

        void ResolveMissingService()
        {
            container.Resolve<IServiceCenter>();
        }

        // Act + Assert.
        Assert.Throws<InvalidOperationException>(ResolveMissingService);
    }

    [Fact]
    public void RegisterSingleton_WhenServiceIsAlreadyRegistered_Throws()
    {
        // Arrange.
        SimpleContainer container = new SimpleContainer();
        container.RegisterSingleton<IServiceCenter>(CreateServiceCenter);

        void RegisterAgain()
        {
            container.RegisterSingleton<IServiceCenter>(CreateServiceCenter);
        }

        // Act + Assert.
        Assert.Throws<ArgumentException>(RegisterAgain);
    }

    [Fact]
    public void RegisterSingleton_WhenFactoryIsNull_Throws()
    {
        // Arrange.
        SimpleContainer container = new SimpleContainer();

        void RegisterNullFactory()
        {
            container.RegisterSingleton<IServiceCenter>(null!);
        }

        // Act + Assert.
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(RegisterNullFactory);
        Assert.Equal("factory", exception.ParamName);
    }

    [Fact]
    public void Resolve_WhenFactoryReturnsNull_Throws()
    {
        // Arrange.
        SimpleContainer container = new SimpleContainer();
        container.RegisterSingleton<IServiceCenter>(CreateNullService);

        void ResolveService()
        {
            container.Resolve<IServiceCenter>();
        }

        // Act + Assert.
        Assert.Throws<InvalidOperationException>(ResolveService);
    }

    private static IServiceCenter CreateServiceCenter(SimpleContainer container)
    {
        return new ServiceCenter();
    }

    private static IServiceCenter CreateNullService(SimpleContainer container)
    {
        // Намеренно нарушаем контракт фабрики, чтобы проверить защиту контейнера.
        return null!;
    }
}
