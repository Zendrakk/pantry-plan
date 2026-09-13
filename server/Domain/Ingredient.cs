namespace PantryPlan.Api.Domain
{
    public class Ingredient
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Category { get; set; }
        public string OwnerId { get; set; } = null!;
        public User Owner { get; set; } = null!;
    }
}
