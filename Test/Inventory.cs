namespace Test;

internal class Inventory
{
    private readonly List<Item> _items = [];
    public IReadOnlyList<Item> Items => _items.AsReadOnly();

    public void AddItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    public bool RemoveItem(Item item)
    {
        return _items.Remove(item);
    }
}
