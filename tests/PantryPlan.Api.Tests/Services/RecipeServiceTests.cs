using Microsoft.EntityFrameworkCore;
using PantryPlan.Api.Domain;
using PantryPlan.Api.Infrastructure.Persistence;
using PantryPlan.Api.Models.Recipes;
using PantryPlan.Api.Services.Recipes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PantryPlan.Api.Tests.Services
{
    public class RecipeServiceTests
    {
        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithValidRequest_CreatesRecipeAndIngredients()
        {
            await using var db = CreateDbContext();
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
            await using var db = CreateDbContext();
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
            await using var db = CreateDbContext();
            var service = new RecipeService(db);
            var request = new CreateRecipeRequest("Pancakes", "Mix and cook", 4, []);

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateRecipeAsync("user-1", request));
        }

        [Fact]
        public async Task CreateRecipeAsync_WithExistingIngredientDifferentCase_ReusesSameIngredientRow()
        {
            await using var db = CreateDbContext();
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
            await using var db = CreateDbContext();
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
            await using var db = CreateDbContext();
            var service = new RecipeService(db);
            var created = await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Pancakes", "Mix and cook", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));

            var result = await service.GetRecipeAsync("user-2", created.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetRecipeAsync_WhenRecipeDoesNotExist_ReturnsNull()
        {
            await using var db = CreateDbContext();
            var service = new RecipeService(db);

            var result = await service.GetRecipeAsync("user-1", Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteRecipeAsync_WhenOwnedByCaller_DeletesAndReturnsTrue()
        {
            await using var db = CreateDbContext();
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
            await using var db = CreateDbContext();
            var service = new RecipeService(db);
            var created = await service.CreateRecipeAsync("user-1", new CreateRecipeRequest(
                "Pancakes", "Mix and cook", 4, [new IngredientLineRequest("Flour", 2, Unit.Cup)]));

            var deleted = await service.DeleteRecipeAsync("user-2", created.Id);
            var stillExists = await service.GetRecipeAsync("user-1", created.Id);

            Assert.False(deleted);
            Assert.NotNull(stillExists);
        }
    }
}
