using PantryPlan.Api.Domain;

namespace PantryPlan.Api.Models.Recipes;

public record IngredientLineRequest(string IngredientName, decimal Quantity, Unit Unit);

public record CreateRecipeRequest(string Title, string Instructions, int ServingSize, List<IngredientLineRequest> Ingredients);

public record IngredientLineResponse(string IngredientName, decimal Quantity, Unit Unit);

public record RecipeResponse(Guid Id, string Title, string Instructions, int ServingSize, List<IngredientLineResponse> Ingredients);

public record UpdateRecipeRequest(string Title, string Instructions, int ServingSize, List<IngredientLineRequest> Ingredients);

public record RecipeSummaryResponse(Guid Id, string Title, int ServingSize, int IngredientCount);

public record RecipeConflictMealPlanResponse(Guid Id, DateOnly WeekStartDate);
