using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebSpark.Domain.User.Data;

public class ApplicationUserManager : UserManager<WebSparkUser>
{
    public ApplicationUserManager(
        IUserStore<WebSparkUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<WebSparkUser> passwordHasher,
        IEnumerable<IUserValidator<WebSparkUser>> userValidators,
        IEnumerable<IPasswordValidator<WebSparkUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<WebSparkUser>> logger)
        : base(store,
              optionsAccessor,
              passwordHasher,
              userValidators,
              passwordValidators,
              keyNormalizer,
              errors,
              services,
              logger)
    {
    }
}