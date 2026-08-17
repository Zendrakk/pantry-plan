namespace PantryPlan.Api.Domain
{
    public class MealPlanEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MealPlanId { get; set; }
        public MealPlan MealPlan { get; set; } = null!;
        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        public DateOnly Date { get; set; }
        public MealType MealType { get; set; }
        public bool IsLeftover { get; set; } = false;
    }
}
