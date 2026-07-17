using Microsoft.EntityFrameworkCore;
using PantryPlan.Api.Domain;
using PantryPlan.Api.Infrastructure.Persistence;
using PantryPlan.Api.Models.Recipes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

            return MapToResponse(recipe);
        }

        public async Task<RecipeResponse?> GetRecipeAsync(string ownerId, Guid recipeId)
        {
            var recipe = await db.Recipes
                .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == recipeId && r.OwnerId == ownerId);

            return recipe is null ? null : MapToResponse(recipe);
        }

        public async Task<List<RecipeSummaryResponse>> ListRecipesAsync(string ownerId)
        {
            return await db.Recipes
                .Where(r => r.OwnerId == ownerId)
                .Select(r => new RecipeSummaryResponse(r.Id, r.Title, r.ServingSize, r.Ingredients.Count))
                .ToListAsync();
        }

        public async Task<RecipeResponse?> UpdateRecipeAsync(string ownerId, Guid recipeId, UpdateRecipeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Title is required.");
            }

            if (request.Ingredients.Count == 0)
            {
                throw new ArgumentException("At least one ingredient is required.");
            }

            var recipe = await db.Recipes
                .FirstOrDefaultAsync(r => r.Id == recipeId && r.OwnerId == ownerId);

            if (recipe is null)
            {
                return null;
            }

            var existingIngredients = await db.RecipeIngredients
                .Where(ri => ri.RecipeId == recipeId)
                .ToListAsync();

            db.RecipeIngredients.RemoveRange(existingIngredients);

            recipe.Title = request.Title;
            recipe.Instructions = request.Instructions;
            recipe.ServingSize = request.ServingSize;
            recipe.UpdatedAt = DateTimeOffset.UtcNow;

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

                db.RecipeIngredients.Add(new RecipeIngredient
                {
                    RecipeId = recipe.Id,
                    Ingredient = ingredient,
                    Quantity = line.Quantity,
                    Unit = line.Unit
                });
            }

            await db.SaveChangesAsync();

            var updated = await db.Recipes
                .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstAsync(r => r.Id == recipeId);

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteRecipeAsync(string ownerId, Guid recipeId)
        {
            var recipe = await db.Recipes
                .FirstOrDefaultAsync(r => r.Id == recipeId && r.OwnerId == ownerId);

            if (recipe is null)
            {
                return false;
            }

            db.Recipes.Remove(recipe);
            await db.SaveChangesAsync();

            return true;
        }

        private static RecipeResponse MapToResponse(Recipe recipe)
        {
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
