using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlan.Api.Models.Recipes;
using PantryPlan.Api.Services.Recipes;

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
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (userId is null)
            {
                return Unauthorized();
            }

            try
            {
                var response = await recipeService.CreateRecipeAsync(userId, request);
                return CreatedAtAction(nameof(CreateRecipe), new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
