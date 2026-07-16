using PantryPlan.Api.Models.Recipes;

namespace PantryPlan.Api.Services.Recipes
{
    public interface IRecipeService
    {
        Task<RecipeResponse> CreateRecipeAsync(string ownerId, CreateRecipeRequest request);
    }
}
