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
            ValidateRecipeRequest(request.Title, request.Instructions, request.ServingSize, request.Ingredients);

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
                .OrderBy(r => r.Title)
                .Select(r => new RecipeSummaryResponse(r.Id, r.Title, r.ServingSize, r.Ingredients.Count))
                .ToListAsync();
        }

        public async Task<RecipeResponse?> UpdateRecipeAsync(string ownerId, Guid recipeId, UpdateRecipeRequest request)
        {
            ValidateRecipeRequest(request.Title, request.Instructions, request.ServingSize, request.Ingredients);

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

        public async Task<List<RecipeConflictMealPlanResponse>> GetMealPlansReferencingRecipeAsync(string ownerId, Guid recipeId)
        {
            return await db.MealPlanEntries
                .Where(e => e.RecipeId == recipeId && e.MealPlan.OwnerId == ownerId)
                .Select(e => e.MealPlan)
                .Distinct()
                .OrderBy(mp => mp.WeekStartDate)
                .Select(mp => new RecipeConflictMealPlanResponse(mp.Id, mp.WeekStartDate))
                .ToListAsync();
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

        private static void ValidateRecipeRequest(string title, string instructions, int servingSize, List<IngredientLineRequest> ingredients)
        {
            var trimmedTitle = title.Trim();

            if (trimmedTitle.Length < 3)
            {
                throw new ArgumentException("Title must be at least 3 characters long.");
            }

            if (trimmedTitle.Length > 200)
            {
                throw new ArgumentException("Title cannot be longer than 200 characters.");
            }

            var trimmedInstructions = instructions.Trim();

            if (trimmedInstructions.Length < 10)
            {
                throw new ArgumentException("Instructions must be at least 10 characters long.");
            }

            if (trimmedInstructions.Length > 5000)
            {
                throw new ArgumentException("Instructions cannot be longer than 5000 characters.");
            }

            if (servingSize < 1)
            {
                throw new ArgumentException("Serving size must be at least 1.");
            }

            if (ingredients.Count == 0)
            {
                throw new ArgumentException("At least one ingredient is required.");
            }

            foreach (var ingredient in ingredients)
            {
                var trimmedIngredientName = ingredient.IngredientName.Trim();

                if (trimmedIngredientName.Length < 2)
                {
                    throw new ArgumentException("Ingredient name must be at least 2 characters long.");
                }

                if (trimmedIngredientName.Length > 100)
                {
                    throw new ArgumentException("Ingredient name cannot be longer than 100 characters.");
                }

                if (ingredient.Quantity <= 0)
                {
                    throw new ArgumentException("Ingredient quantity must be greater than 0.");
                }
            }
        }
    }
}
