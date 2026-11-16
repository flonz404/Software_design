using Pos.ConsoleApp;
using Pos.Domain;
using Xunit;

namespace Pos.Tests;

public class ContainerTests
{
    [Fact]
    public void Resolve_ReusesInstanceAndRejectsDuplicateRegistration()
    {
        SimpleContainer container = new SimpleContainer();
        int calls = 0;
        Weight CreateWeight(SimpleContainer services)
        {
            calls++;
            return new Weight(1m);
        }
        container.Register<Weight>(CreateWeight);
        Assert.Same(container.Resolve<Weight>(), container.Resolve<Weight>());
        Assert.Equal(1, calls);
        void Duplicate() { container.Register<Weight>(CreateWeight); }
        Assert.Throws<InvalidOperationException>(Duplicate);
    }

    [Fact]
    public void Resolve_RejectsMissingRegistration()
    {
        SimpleContainer container = new SimpleContainer();
        void Act() { container.Resolve<Weight>(); }
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void Resolve_RejectsCycleAndClearsResolutionStateAfterFailure()
    {
        SimpleContainer container = new SimpleContainer();
        bool cycle = true;
        Weight CreateWeight(SimpleContainer services)
        {
            if (cycle)
            {
                return services.Resolve<Weight>();
            }
            return new Weight(1m);
        }
        container.Register<Weight>(CreateWeight);
        void Act() { container.Resolve<Weight>(); }
        Assert.Throws<InvalidOperationException>(Act);
        cycle = false;
        Assert.Equal(1m, container.Resolve<Weight>().Value);
    }
}
