namespace PantryPlan.Api.Domain
{
    public class Ingredient
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Category { get; set; }
    }
}
