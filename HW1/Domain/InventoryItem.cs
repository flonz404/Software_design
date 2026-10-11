namespace VyshKat.Domain;

public abstract class InventoryItem : IInventoryItem
{
    public string Name { get; }
    public int Id { get; }
    
    protected InventoryItem(string name, int id)
    {
        Name = name;
        Id = id;
    }
}
