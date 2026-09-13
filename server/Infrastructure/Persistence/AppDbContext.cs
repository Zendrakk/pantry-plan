using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PantryPlan.Api.Domain;

namespace PantryPlan.Api.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
        public DbSet<MealPlan> MealPlans => Set<MealPlan>();
        public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash)
                .IsUnique();

            // Enum-as-string conversions
            builder.Entity<RecipeIngredient>()
                .Property(ri => ri.Unit)
                .HasConversion<string>();

            builder.Entity<MealPlanEntry>()
                .Property(mpe => mpe.MealType)
                .HasConversion<string>();

            // Recipe -> RecipeIngredient: cascade delete
            builder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            // RecipeIngredient -> Ingredient: restrict (shared resource)
            builder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany()
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            // MealPlan -> MealPlanEntry: cascade delete
            builder.Entity<MealPlanEntry>()
                .HasOne(mpe => mpe.MealPlan)
                .WithMany(mp => mp.Entries)
                .HasForeignKey(mpe => mpe.MealPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // MealPlanEntry -> Recipe: restrict
            builder.Entity<MealPlanEntry>()
                .HasOne(mpe => mpe.Recipe)
                .WithMany()
                .HasForeignKey(mpe => mpe.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Recipe -> User (owner): restrict
            builder.Entity<Recipe>()
                .HasOne(r => r.Owner)
                .WithMany()
                .HasForeignKey(r => r.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // MealPlan -> User (owner): restrict
            builder.Entity<MealPlan>()
                .HasOne(mp => mp.Owner)
                .WithMany()
                .HasForeignKey(mp => mp.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ingredient -> User (owner): restrict
            builder.Entity<Ingredient>()
                .HasOne(i => i.Owner)
                .WithMany()
                .HasForeignKey(i => i.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
