using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PantryPlan.Api.Domain;
using PantryPlan.Api.Infrastructure.Persistence;

namespace PantryPlan.Api.Tests
{
    public static class TestHelpers
    {
        public static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // Builds a REAL UserManager backed by the in-memory db, using the same
        // password/email rules configured in Program.cs, so tests exercise
        // genuine Identity validation rather than a hand-rolled fake.
        public static UserManager<User> CreateUserManager(AppDbContext db)
        {
            var store = new UserStore<User>(db);

            var identityOptions = Options.Create(new IdentityOptions
            {
                Password = new PasswordOptions
                {
                    RequiredLength = 8,
                    RequireNonAlphanumeric = true,
                    RequireUppercase = true,
                    RequireDigit = true,
                    RequireLowercase = false,
                    RequiredUniqueChars = 0
                },
                User = new UserOptions
                {
                    RequireUniqueEmail = true
                }
            });

            var passwordHasher = new PasswordHasher<User>();
            var userValidators = new List<IUserValidator<User>> { new UserValidator<User>() };
            var passwordValidators = new List<IPasswordValidator<User>> { new PasswordValidator<User>() };
            var normalizer = new UpperInvariantLookupNormalizer();
            var errorDescriber = new IdentityErrorDescriber();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            return new UserManager<User>(
                store,
                identityOptions,
                passwordHasher,
                userValidators,
                passwordValidators,
                normalizer,
                errorDescriber,
                serviceProvider,
                NullLogger<UserManager<User>>.Instance);
        }
    }
}
