namespace Marraia.POC.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Product(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero.", nameof(price));

        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        CreatedAt = DateTime.UtcNow;
    }
}
