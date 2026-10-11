namespace VyshKat.Services;
using VyshKat.Domain;

public class ServiceCenter : IServiceCenter
{
    public bool Inspect(Vehicle vehicle)
    {
        return vehicle.IsOperational;
    }
}