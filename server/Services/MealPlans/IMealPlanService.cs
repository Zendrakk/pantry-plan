using PantryPlan.Api.Models.MealPlans;

namespace PantryPlan.Api.Services.MealPlans
{
    public interface IMealPlanService
    {
        Task<MealPlanResponse> CreateMealPlanAsync(string ownerId, CreateMealPlanRequest request);
        Task<MealPlanResponse?> GetMealPlanAsync(string ownerId, Guid mealPlanId);
        Task<List<MealPlanSummaryResponse>> ListMealPlansAsync(string ownerId);
        Task<bool> DeleteMealPlanAsync(string ownerId, Guid mealPlanId);
        Task<MealPlanResponse?> AddEntryAsync(string ownerId, Guid mealPlanId, AddMealPlanEntryRequest request);
        Task<MealPlanResponse?> RemoveEntryAsync(string ownerId, Guid mealPlanId, Guid entryId);
        Task<ShoppingListResponse?> GetShoppingListAsync(string ownerId, Guid mealPlanId);
    }
}
