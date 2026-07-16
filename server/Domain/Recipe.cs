namespace PantryPlan.Api.Domain
{
    public class Recipe
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = null!;
        public string Instructions { get; set; } = null!;
        public int ServingSize { get; set; }
        public string OwnerId { get; set; } = null!;
        public User Owner { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        public List<RecipeIngredient> Ingredients { get; set; } = [];
    }
}
