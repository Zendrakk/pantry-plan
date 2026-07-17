using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlan.Api.Models.MealPlans;
using PantryPlan.Api.Services.MealPlans;

namespace PantryPlan.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MealPlansController(IMealPlanService mealPlanService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateMealPlan([FromBody] CreateMealPlanRequest request)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var response = await mealPlanService.CreateMealPlanAsync(userId, request);
            return CreatedAtAction(nameof(GetMealPlan), new { id = response.Id }, response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMealPlan(Guid id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var mealPlan = await mealPlanService.GetMealPlanAsync(userId, id);
            return mealPlan is null ? NotFound() : Ok(mealPlan);
        }

        [HttpGet]
        public async Task<IActionResult> ListMealPlans()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var mealPlans = await mealPlanService.ListMealPlansAsync(userId);
            return Ok(mealPlans);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteMealPlan(Guid id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var deleted = await mealPlanService.DeleteMealPlanAsync(userId, id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("{id:guid}/entries")]
        public async Task<IActionResult> AddEntry(Guid id, [FromBody] AddMealPlanEntryRequest request)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var updated = await mealPlanService.AddEntryAsync(userId, id, request);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:guid}/entries/{entryId:guid}")]
        public async Task<IActionResult> RemoveEntry(Guid id, Guid entryId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var updated = await mealPlanService.RemoveEntryAsync(userId, id, entryId);
            return updated is null ? NotFound() : Ok(updated);
        }

        private string? GetUserId() => User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    }
}
