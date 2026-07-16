namespace PantryPlan.Api.Domain
{
    public class RecipeIngredient
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        public Guid IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } = null!;
        public decimal Quantity { get; set; }
        public Unit Unit { get; set; }
    }
}
