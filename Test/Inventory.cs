namespace Test;

internal class Inventory
{
    public const int MaxWeight = 100;

    private readonly List<Item> _items = [];
    private readonly object _sync = new();
    private int _currentWeight;

    public IReadOnlyCollection<Item> Items
    {
        get
        {
            lock (_sync)
            {
                return _items.Select(CloneItem).ToList().AsReadOnly();
            }
        }
    }

    public void AddItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            EnsureWeightCanBeAdded(item.Weight);

            var existingIndex = FindIndexByName(item.Name);
            if (existingIndex >= 0)
            {
                var existing = _items[existingIndex];
                _items[existingIndex] = new Item(existing.Name, existing.Weight + item.Weight);
            }
            else
            {
                _items.Add(CloneItem(item));
            }

            _currentWeight += item.Weight;
        }
    }

    public bool RemoveItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            var index = FindIndexByName(item.Name);
            if (index < 0)
            {
                return false;
            }

            _currentWeight -= _items[index].Weight;
            _items.RemoveAt(index);
            return true;
        }
    }

    public IReadOnlyCollection<Item> FindByName(string namePart)
    {
        ArgumentNullException.ThrowIfNull(namePart);

        lock (_sync)
        {
            return _items
                .Where(i => i.Name.Contains(namePart, StringComparison.OrdinalIgnoreCase))
                .Select(CloneItem)
                .ToList()
                .AsReadOnly();
        }
    }

    private int FindIndexByName(string name) =>
        _items.FindIndex(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));

    private void EnsureWeightCanBeAdded(int weight)
    {
        if (_currentWeight + weight > MaxWeight)
        {
            throw new InvalidOperationException($"Cannot exceed max inventory weight of {MaxWeight}.");
        }
    }

    private static Item CloneItem(Item item) => new(item.Name, item.Weight);
}
