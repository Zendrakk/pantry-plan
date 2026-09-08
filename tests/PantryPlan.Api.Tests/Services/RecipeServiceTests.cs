using Microsoft.EntityFrameworkCore;
using PantryPlan.Api.Domain;
using PantryPlan.Api.Models.MealPlans;
using PantryPlan.Api.Models.Recipes;
using PantryPlan.Api.Services.MealPlans;
using PantryPlan.Api.Services.Recipes;

namespace PantryPlan.Api.Tests.Services
{
    public class RecipeServiceTests
    {
        [Fact]
        public async Task CreateRecipeAsync_WithValidRequest_CreatesRecipeAndIngredients()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);
            var request = new CreateRecipeRequest(
                "Pancakes",
                "Mix and cook",
                4,
                [new IngredientLineRequest("Flour", 2, Unit.Cup)]
            );

            var result = await service.CreateRecipeAsync("user-1", request);

            Assert.Equal("Pancakes", result.Title);
            Assert.Single(result.Ingredients);
            Assert.Equal("Flour", result.Ingredients[0].IngredientName);
            Assert.Equal(2, result.Ingredients[0].Quantity);
            Assert.Equal(Unit.Cup, result.Ingredients[0].Unit);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithBlankTitle_ThrowsArgumentException()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);
            var request = new CreateRecipeRequest(
                "   ",
                "Mix and cook",
                4,
                [new IngredientLineRequest("Flour", 2, Unit.Cup)]
            );

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateRecipeAsync("user-1", request));
        }

        [Fact]
        public async Task CreateRecipeAsync_WithNoIngredients_ThrowsArgumentException()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);
            var request = new CreateRecipeRequest("Pancakes", "Mix and cook", 4, []);

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateRecipeAsync("user-1", request));
        }

        [Fact]
        public async Task CreateRecipeAsync_WithExistingIngredientDifferentCase_ReusesSameIngredientRow()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);

            await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Recipe One", "Instructions", 2, [new IngredientLineRequest("flour", 1, Unit.Cup)]));

            await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Recipe Two", "Instructions", 2, [new IngredientLineRequest("Flour", 1, Unit.Cup)]));

            var ingredientCount = await db.Ingredients.CountAsync();
            Assert.Equal(1, ingredientCount);
        }

        [Fact]
        public async Task GetRecipeAsync_WhenOwnedByCaller_ReturnsRecipe()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);
            var created = await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Pancakes", "Mix and cook", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));

            var result = await service.GetRecipeAsync("user-1", created.Id);

            Assert.NotNull(result);
            Assert.Equal("Pancakes", result!.Title);
        }

        [Fact]
        public async Task GetRecipeAsync_WhenOwnedByDifferentUser_ReturnsNull()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);
            var created = await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Pancakes", "Mix and cook", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));

            var result = await service.GetRecipeAsync("user-2", created.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetRecipeAsync_WhenRecipeDoesNotExist_ReturnsNull()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);

            var result = await service.GetRecipeAsync("user-1", Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteRecipeAsync_WhenOwnedByCaller_DeletesAndReturnsTrue()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);
            var created = await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Pancakes", "Mix and cook", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));

            var deleted = await service.DeleteRecipeAsync("user-1", created.Id);
            var afterDelete = await service.GetRecipeAsync("user-1", created.Id);

            Assert.True(deleted);
            Assert.Null(afterDelete);
        }

        [Fact]
        public async Task DeleteRecipeAsync_WhenOwnedByDifferentUser_ReturnsFalseAndDoesNotDelete()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);
            var created = await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Pancakes", "Mix and cook", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));

            var deleted = await service.DeleteRecipeAsync("user-2", created.Id);
            var stillExists = await service.GetRecipeAsync("user-1", created.Id);

            Assert.False(deleted);
            Assert.NotNull(stillExists);
        }

        [Fact]
        public async Task ListRecipesAsync_ReturnsRecipesSortedAlphabeticallyByTitle()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new RecipeService(db);

            // Create recipes deliberately out of alphabetical order, so a
            // passing result can only mean the sort actually reordered them.
            await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Zucchini Bread", "Instructions", 2, [new IngredientLineRequest("Zucchini", 1, Unit.Cup)]));
            await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Apple Pie", "Instructions", 2, [new IngredientLineRequest("Apple", 1, Unit.Cup)]));
            await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Mango Salsa", "Instructions", 2, [new IngredientLineRequest("Mango", 1, Unit.Cup)]));

            var result = await service.ListRecipesAsync("user-1");

            Assert.Equal(3, result.Count);
            Assert.Equal("Apple Pie", result[0].Title);
            Assert.Equal("Mango Salsa", result[1].Title);
            Assert.Equal("Zucchini Bread", result[2].Title);
        }

        [Fact]
        public async Task GetMealPlansReferencingRecipeAsync_ReturnsDistinctMealPlansSortedByDate()
        {
            await using var db = TestHelpers.CreateDbContext();
            var recipeService = new RecipeService(db);
            var mealPlanService = new MealPlanService(db);

            var recipe = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Chili", "Instructions", 4, [new IngredientLineRequest("Beans", 2, Unit.Cup)]));

            var laterMealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 8, 3)));
            var earlierMealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            // Reference the same recipe twice within earlierMealPlan (Monday AND
            // Thursday), plus once in laterMealPlan - the earlier plan should
            // only appear ONCE in the result, not twice.
            await mealPlanService.AddEntryAsync("user-1", earlierMealPlan.Id,
                new AddMealPlanEntryRequest(recipe.Id, new DateOnly(2026, 7, 20), MealType.Dinner, false));
            await mealPlanService.AddEntryAsync("user-1", earlierMealPlan.Id,
                new AddMealPlanEntryRequest(recipe.Id, new DateOnly(2026, 7, 23), MealType.Dinner, true));
            await mealPlanService.AddEntryAsync("user-1", laterMealPlan.Id,
                new AddMealPlanEntryRequest(recipe.Id, new DateOnly(2026, 8, 3), MealType.Dinner, false));

            var result = await recipeService.GetMealPlansReferencingRecipeAsync("user-1", recipe.Id);

            Assert.Equal(2, result.Count);
            Assert.Equal(earlierMealPlan.Id, result[0].Id);
            Assert.Equal(laterMealPlan.Id, result[1].Id);
        }

        [Fact]
        public async Task GetMealPlansReferencingRecipeAsync_WhenRecipeIsNotUsedAnywhere_ReturnsEmptyList()
        {
            await using var db = TestHelpers.CreateDbContext();
            var recipeService = new RecipeService(db);

            var recipe = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Chili", "Instructions", 4, [new IngredientLineRequest("Beans", 2, Unit.Cup)]));

            var result = await recipeService.GetMealPlansReferencingRecipeAsync("user-1", recipe.Id);

            Assert.Empty(result);
        }
    }
}
