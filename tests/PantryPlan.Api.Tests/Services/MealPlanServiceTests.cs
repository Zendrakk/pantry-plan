using PantryPlan.Api.Domain;
using PantryPlan.Api.Models.MealPlans;
using PantryPlan.Api.Models.Recipes;
using PantryPlan.Api.Services.MealPlans;
using PantryPlan.Api.Services.Recipes;

namespace PantryPlan.Api.Tests.Services
{
    public class MealPlanServiceTests
    {
        [Fact]
        public async Task CreateMealPlanAsync_WithValidRequest_CreatesEmptyMealPlan()
        {
            await using var db = TestHelpers.CreateDbContext();
            var service = new MealPlanService(db);

            var result = await service.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            Assert.Equal(new DateOnly(2026, 7, 20), result.WeekStartDate);
            Assert.Empty(result.Entries);
        }

        [Fact]
        public async Task AddEntryAsync_WithOwnRecipe_AddsEntryAndReturnsUpdatedPlan()
        {
            await using var db = TestHelpers.CreateDbContext();
            var recipeService = new RecipeService(db);
            var mealPlanService = new MealPlanService(db);

            var recipe = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Pancakes", "Mix and cook", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));
            var mealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            var result = await mealPlanService.AddEntryAsync("user-1", mealPlan.Id,
                new AddMealPlanEntryRequest(recipe.Id, new DateOnly(2026, 7, 20), MealType.Dinner));

            Assert.NotNull(result);
            Assert.Single(result!.Entries);
            Assert.Equal("Pancakes", result.Entries[0].RecipeTitle);
            Assert.Equal(MealType.Dinner, result.Entries[0].MealType);
        }

        [Fact]
        public async Task AddEntryAsync_WithAnotherUsersRecipe_ThrowsArgumentException()
        {
            await using var db = TestHelpers.CreateDbContext();
            var recipeService = new RecipeService(db);
            var mealPlanService = new MealPlanService(db);

            var otherUsersRecipe = await recipeService.CreateRecipeAsync("user-2", new CreateRecipeRequest(
                "Secret Recipe", "Instructions", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));
            var mealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                mealPlanService.AddEntryAsync("user-1", mealPlan.Id,
                    new AddMealPlanEntryRequest(otherUsersRecipe.Id, new DateOnly(2026, 7, 20), MealType.Dinner)));
        }

        [Fact]
        public async Task AddEntryAsync_WhenMealPlanNotOwnedByCaller_ReturnsNull()
        {
            await using var db = TestHelpers.CreateDbContext();
            var recipeService = new RecipeService(db);
            var mealPlanService = new MealPlanService(db);

            var recipe = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Pancakes", "Mix and cook", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));
            var mealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            var result = await mealPlanService.AddEntryAsync("user-2", mealPlan.Id,
                new AddMealPlanEntryRequest(recipe.Id, new DateOnly(2026, 7, 20), MealType.Dinner));

            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveEntryAsync_WhenEntryExists_RemovesItAndReturnsUpdatedPlan()
        {
            await using var db = TestHelpers.CreateDbContext();
            var recipeService = new RecipeService(db);
            var mealPlanService = new MealPlanService(db);

            var recipe = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Pancakes", "Mix and cook", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));
            var mealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));
            var afterAdd = await mealPlanService.AddEntryAsync("user-1", mealPlan.Id,
                new AddMealPlanEntryRequest(recipe.Id, new DateOnly(2026, 7, 20), MealType.Dinner));

            var result = await mealPlanService.RemoveEntryAsync("user-1", mealPlan.Id, afterAdd!.Entries[0].Id);

            Assert.NotNull(result);
            Assert.Empty(result!.Entries);
        }

        [Fact]
        public async Task GetShoppingListAsync_WithSharedIngredientSameUnit_MergesQuantities()
        {
            await using var db = TestHelpers.CreateDbContext();
            var recipeService = new RecipeService(db);
            var mealPlanService = new MealPlanService(db);

            var recipeOne = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Recipe One", "Instructions", 2, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));
            var recipeTwo = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Recipe Two", "Instructions", 2, [new IngredientLineRequest("Flour", 1, Unit.Cup)]));
            var mealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            await mealPlanService.AddEntryAsync("user-1", mealPlan.Id,
                new AddMealPlanEntryRequest(recipeOne.Id, new DateOnly(2026, 7, 20), MealType.Dinner));
            await mealPlanService.AddEntryAsync("user-1", mealPlan.Id,
                new AddMealPlanEntryRequest(recipeTwo.Id, new DateOnly(2026, 7, 21), MealType.Breakfast));

            var shoppingList = await mealPlanService.GetShoppingListAsync("user-1", mealPlan.Id);

            Assert.NotNull(shoppingList);
            var flourItem = Assert.Single(shoppingList!.Items, i => i.IngredientName == "Flour");
            var quantity = Assert.Single(flourItem.Quantities);
            Assert.Equal(3, quantity.Quantity);
            Assert.Equal(Unit.Cup, quantity.Unit);
        }

        [Fact]
        public async Task GetShoppingListAsync_WithSharedIngredientDifferentUnits_KeepsLinesSeparate()
        {
            await using var db = TestHelpers.CreateDbContext();
            var recipeService = new RecipeService(db);
            var mealPlanService = new MealPlanService(db);

            var recipeOne = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Recipe One", "Instructions", 2, [new IngredientLineRequest("Sugar", 1, Unit.Cup)]));
            var recipeTwo = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Recipe Two", "Instructions", 2, [new IngredientLineRequest("Sugar", 50, Unit.Gram)]));
            var mealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            await mealPlanService.AddEntryAsync("user-1", mealPlan.Id,
                new AddMealPlanEntryRequest(recipeOne.Id, new DateOnly(2026, 7, 20), MealType.Dinner));
            await mealPlanService.AddEntryAsync("user-1", mealPlan.Id,
                new AddMealPlanEntryRequest(recipeTwo.Id, new DateOnly(2026, 7, 21), MealType.Breakfast));

            var shoppingList = await mealPlanService.GetShoppingListAsync("user-1", mealPlan.Id);

            var sugarItem = Assert.Single(shoppingList!.Items, i => i.IngredientName == "Sugar");
            Assert.Equal(2, sugarItem.Quantities.Count);
        }

        [Fact]
        public async Task GetShoppingListAsync_WithSameRecipePlannedTwice_DoublesQuantity()
        {
            await using var db = TestHelpers.CreateDbContext();
            var recipeService = new RecipeService(db);
            var mealPlanService = new MealPlanService(db);

            var recipe = await recipeService.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Leftovers Special", "Instructions", 2, [new IngredientLineRequest("Rice", 1, Unit.Cup)]));
            var mealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            await mealPlanService.AddEntryAsync("user-1", mealPlan.Id,
                new AddMealPlanEntryRequest(recipe.Id, new DateOnly(2026, 7, 20), MealType.Dinner));
            await mealPlanService.AddEntryAsync("user-1", mealPlan.Id,
                new AddMealPlanEntryRequest(recipe.Id, new DateOnly(2026, 7, 22), MealType.Dinner));

            var shoppingList = await mealPlanService.GetShoppingListAsync("user-1", mealPlan.Id);

            var riceItem = Assert.Single(shoppingList!.Items, i => i.IngredientName == "Rice");
            var quantity = Assert.Single(riceItem.Quantities);
            Assert.Equal(2, quantity.Quantity);
        }

        [Fact]
        public async Task GetShoppingListAsync_WhenMealPlanNotOwnedByCaller_ReturnsNull()
        {
            await using var db = TestHelpers.CreateDbContext();
            var mealPlanService = new MealPlanService(db);
            var mealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            var result = await mealPlanService.GetShoppingListAsync("user-2", mealPlan.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetShoppingListAsync_WithNoEntries_ReturnsEmptyItemsList()
        {
            await using var db = TestHelpers.CreateDbContext();
            var mealPlanService = new MealPlanService(db);
            var mealPlan = await mealPlanService.CreateMealPlanAsync("user-1", new CreateMealPlanRequest(new DateOnly(2026, 7, 20)));

            var shoppingList = await mealPlanService.GetShoppingListAsync("user-1", mealPlan.Id);

            Assert.NotNull(shoppingList);
            Assert.Empty(shoppingList!.Items);
        }
    }
}
