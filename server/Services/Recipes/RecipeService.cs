using Microsoft.EntityFrameworkCore;
using PantryPlan.Api.Domain;
using PantryPlan.Api.Infrastructure.Persistence;
using PantryPlan.Api.Models.Recipes;

namespace PantryPlan.Api.Services.Recipes
{
    public class RecipeService(AppDbContext db) : IRecipeService
    {
        public async Task<RecipeResponse> CreateRecipeAsync(string ownerId, CreateRecipeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Title is required.");
            }

            if (request.Ingredients.Count == 0)
            {
                throw new ArgumentException("At least one ingredient is required.");
            }

            var recipe = new Recipe
            {
                Title = request.Title,
                Instructions = request.Instructions,
                ServingSize = request.ServingSize,
                OwnerId = ownerId
            };

            foreach (var line in request.Ingredients)
            {
                var normalizedName = line.IngredientName.Trim();

                var ingredient = await db.Ingredients
                    .FirstOrDefaultAsync(i => i.Name.ToLower() == normalizedName.ToLower());

                if (ingredient is null)
                {
                    ingredient = new Ingredient { Name = normalizedName };
                    db.Ingredients.Add(ingredient);
                }

                recipe.Ingredients.Add(new RecipeIngredient
                {
                    Ingredient = ingredient,
                    Quantity = line.Quantity,
                    Unit = line.Unit
                });
            }

            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();

            return new RecipeResponse(
                recipe.Id,
                recipe.Title,
                recipe.Instructions,
                recipe.ServingSize,
                recipe.Ingredients.Select(ri => new IngredientLineResponse(ri.Ingredient.Name, ri.Quantity, ri.Unit)).ToList()
            );
        }
    }
}
