using PantryPlan.Api.Domain;

namespace PantryPlan.Api.Models.MealPlans;

public record CreateMealPlanRequest(DateOnly WeekStartDate);

public record AddMealPlanEntryRequest(Guid RecipeId, DateOnly Date, MealType MealType);

public record MealPlanEntryResponse(Guid Id, Guid RecipeId, string RecipeTitle, DateOnly Date, MealType MealType);

public record MealPlanResponse(Guid Id, DateOnly WeekStartDate, List<MealPlanEntryResponse> Entries);

public record MealPlanSummaryResponse(Guid Id, DateOnly WeekStartDate, int EntryCount);

public record ShoppingListQuantityResponse(decimal Quantity, Unit Unit);

public record ShoppingListItemResponse(string IngredientName, string? Category, List<ShoppingListQuantityResponse> Quantities);

public record ShoppingListResponse(Guid MealPlanId, List<ShoppingListItemResponse> Items);