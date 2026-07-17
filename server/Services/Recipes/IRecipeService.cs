using PantryPlan.Api.Models.Recipes;

namespace PantryPlan.Api.Services.Recipes
{
    public interface IRecipeService
    {
        Task<RecipeResponse> CreateRecipeAsync(string ownerId, CreateRecipeRequest request);
        Task<RecipeResponse?> GetRecipeAsync(string ownerId, Guid recipeId);
        Task<List<RecipeSummaryResponse>> ListRecipesAsync(string ownerId);
        Task<RecipeResponse?> UpdateRecipeAsync(string ownerId, Guid recipeId, UpdateRecipeRequest request);
        Task<bool> DeleteRecipeAsync(string ownerId, Guid recipeId);
    }
}
