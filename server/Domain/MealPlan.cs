namespace PantryPlan.Api.Domain
{
    public class MealPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OwnerId { get; set; } = null!;
        public User Owner { get; set; } = null!;
        public DateOnly WeekStartDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public List<MealPlanEntry> Entries { get; set; } = [];
    }
}
