using Microsoft.EntityFrameworkCore;
using PantryPlan.Api.Domain;
using PantryPlan.Api.Infrastructure.Persistence;
using PantryPlan.Api.Models.MealPlans;

namespace PantryPlan.Api.Services.MealPlans
{
    public class MealPlanService(AppDbContext db) : IMealPlanService
    {
        public async Task<MealPlanResponse> CreateMealPlanAsync(string ownerId, CreateMealPlanRequest request)
        {
            var mealPlan = new MealPlan
            {
                OwnerId = ownerId,
                WeekStartDate = request.WeekStartDate
            };

            db.MealPlans.Add(mealPlan);
            await db.SaveChangesAsync();

            return MapToResponse(mealPlan);
        }

        public async Task<MealPlanResponse?> GetMealPlanAsync(string ownerId, Guid mealPlanId)
        {
            var mealPlan = await db.MealPlans
                .Include(mp => mp.Entries)
                .ThenInclude(e => e.Recipe)
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId && mp.OwnerId == ownerId);

            return mealPlan is null ? null : MapToResponse(mealPlan);
        }

        public async Task<List<MealPlanSummaryResponse>> ListMealPlansAsync(string ownerId)
        {
            return await db.MealPlans
                .Where(mp => mp.OwnerId == ownerId)
                .Select(mp => new MealPlanSummaryResponse(mp.Id, mp.WeekStartDate, mp.Entries.Count))
                .ToListAsync();
        }

        public async Task<bool> DeleteMealPlanAsync(string ownerId, Guid mealPlanId)
        {
            var mealPlan = await db.MealPlans
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId && mp.OwnerId == ownerId);

            if (mealPlan is null)
            {
                return false;
            }

            db.MealPlans.Remove(mealPlan);
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<MealPlanResponse?> AddEntryAsync(string ownerId, Guid mealPlanId, AddMealPlanEntryRequest request)
        {
            var mealPlan = await db.MealPlans
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId && mp.OwnerId == ownerId);

            if (mealPlan is null)
            {
                return null;
            }

            var recipeExists = await db.Recipes
                .AnyAsync(r => r.Id == request.RecipeId && r.OwnerId == ownerId);

            if (!recipeExists)
            {
                throw new ArgumentException("Recipe not found.");
            }

            db.MealPlanEntries.Add(new MealPlanEntry
            {
                MealPlanId = mealPlan.Id,
                RecipeId = request.RecipeId,
                Date = request.Date,
                MealType = request.MealType,
                IsLeftover = request.IsLeftover
            });

            await db.SaveChangesAsync();

            return await GetMealPlanAsync(ownerId, mealPlanId);
        }

        public async Task<MealPlanResponse?> RemoveEntryAsync(string ownerId, Guid mealPlanId, Guid entryId)
        {
            var mealPlan = await db.MealPlans
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId && mp.OwnerId == ownerId);

            if (mealPlan is null)
            {
                return null;
            }

            var entry = await db.MealPlanEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.MealPlanId == mealPlanId);

            if (entry is not null)
            {
                db.MealPlanEntries.Remove(entry);
                await db.SaveChangesAsync();
            }

            return await GetMealPlanAsync(ownerId, mealPlanId);
        }

        public async Task<ShoppingListResponse?> GetShoppingListAsync(string ownerId, Guid mealPlanId)
        {
            var mealPlanExists = await db.MealPlans
                .AnyAsync(mp => mp.Id == mealPlanId && mp.OwnerId == ownerId);

            if (!mealPlanExists)
            {
                return null;
            }

            var recipeIngredients = await db.MealPlanEntries
                .Where(e => e.MealPlanId == mealPlanId && !e.IsLeftover)
                .SelectMany(e => e.Recipe.Ingredients)
                .Include(ri => ri.Ingredient)
                .ToListAsync();

            var items = recipeIngredients
                .GroupBy(ri => ri.Ingredient)
                .Select(ingredientGroup => new ShoppingListItemResponse(
                    ingredientGroup.Key.Name,
                    ingredientGroup.Key.Category,
                    ingredientGroup
                        .GroupBy(ri => ri.Unit)
                        .Select(unitGroup => new ShoppingListQuantityResponse(unitGroup.Sum(ri => ri.Quantity), unitGroup.Key))
                        .ToList()
                ))
                .OrderBy(item => item.Category)
                .ThenBy(item => item.IngredientName)
                .ToList();

            return new ShoppingListResponse(mealPlanId, items);
        }

        private static MealPlanResponse MapToResponse(MealPlan mealPlan)
        {
            var sortedEntries = mealPlan.Entries
                .OrderBy(e => e.Date)
                .ThenBy(e => e.MealType)
                .Select(e => new MealPlanEntryResponse(e.Id, e.RecipeId, e.Recipe.Title, e.Date, e.MealType, e.IsLeftover))
                .ToList();

            return new MealPlanResponse(
                mealPlan.Id,
                mealPlan.WeekStartDate,
                sortedEntries
            );
        }
    }
}
