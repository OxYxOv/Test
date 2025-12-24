namespace Test;

internal sealed record Item
{
    public Item(string name, int weight)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        if (weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");
        }

        Weight = weight;
    }

    public string Name { get; init; }

    public int Weight { get; init; }
}
