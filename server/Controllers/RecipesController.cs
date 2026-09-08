using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PantryPlan.Api.Models.Recipes;
using PantryPlan.Api.Services.Recipes;
using System.IdentityModel.Tokens.Jwt;

namespace PantryPlan.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecipesController(IRecipeService recipeService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeRequest request)
        {
            var userId = GetUserId();

            if (userId is null) return Unauthorized();

            try
            {
                var response = await recipeService.CreateRecipeAsync(userId, request);
                return CreatedAtAction(nameof(GetRecipe), new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRecipe(Guid id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var recipe = await recipeService.GetRecipeAsync(userId, id);
            return recipe is null ? NotFound() : Ok(recipe);
        }

        [HttpGet]
        public async Task<IActionResult> ListRecipes()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var recipes = await recipeService.ListRecipesAsync(userId);
            return Ok(recipes);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRecipe(Guid id, [FromBody] UpdateRecipeRequest request)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var updated = await recipeService.UpdateRecipeAsync(userId, id, request);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRecipe(Guid id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var deleted = await recipeService.DeleteRecipeAsync(userId, id);
                return deleted ? NoContent() : NotFound();
            }
            catch (DbUpdateException)
            {
                var referencingMealPlans = await recipeService.GetMealPlansReferencingRecipeAsync(userId, id);

                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Recipe is in use",
                    detail: "This recipe is used in one or more meal plans and cannot be deleted.",
                    extensions: new Dictionary<string, object?>
                    {
                        ["mealPlans"] = referencingMealPlans
                    });
            }
        }

        private string? GetUserId() => User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    }
}
