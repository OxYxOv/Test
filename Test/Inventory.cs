namespace Test;

internal class Inventory
{
    public const int MaxWeight = 100;

    private readonly Dictionary<string, Item> _items = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();
    private int _currentWeight;

    public IReadOnlyCollection<Item> Items
    {
        get
        {
            lock (_sync)
            {
                var snapshot = _items.Values.ToArray();
                return Array.AsReadOnly(snapshot);
            }
        }
    }

    public void AddItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            if (_items.TryGetValue(item.Name, out var existingItem))
            {
                var newWeight = checked(existingItem.Weight + item.Weight);
                var potentialTotalWeight = checked(_currentWeight - existingItem.Weight + newWeight);
                ApplyItemUpdate(item.Name, newWeight, potentialTotalWeight);
            }
            else
            {
                var potentialTotalWeight = checked(_currentWeight + item.Weight);
                ApplyItemUpdate(item.Name, item.Weight, potentialTotalWeight);
            }
        }
    }

    public bool RemoveItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return RemoveItemByName(item.Name);
    }

    public bool RemoveItemByName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_sync)
        {
            if (!_items.TryGetValue(name, out var existingItem))
            {
                return false;
            }

            _currentWeight -= existingItem.Weight;
            _items.Remove(name);
            return true;
        }
    }

    public IReadOnlyCollection<Item> FindByName(string namePart)
    {
        ArgumentNullException.ThrowIfNull(namePart);

        lock (_sync)
        {
            var matches = _items.Values
                .Where(i => i.Name.Contains(namePart, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            return Array.AsReadOnly(matches);
        }
    }

    private void EnsureTotalWeightWithinLimit(int totalWeight)
    {
        if (totalWeight > MaxWeight)
        {
            throw new InvalidOperationException($"Cannot exceed max inventory weight of {MaxWeight}.");
        }
    }

    private void ApplyItemUpdate(string name, int weight, int potentialTotalWeight)
    {
        EnsureTotalWeightWithinLimit(potentialTotalWeight);
        _items[name] = new Item(name, weight);
        _currentWeight = potentialTotalWeight;
    }
}
