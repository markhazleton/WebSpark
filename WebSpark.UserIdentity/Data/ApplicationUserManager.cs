using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebSpark.UserIdentity.Data;

public class ApplicationUserManager : UserManager<Data.WebSparkUser>
{
    public ApplicationUserManager(
        IUserStore<Data.WebSparkUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<Data.WebSparkUser> passwordHasher,
        IEnumerable<IUserValidator<Data.WebSparkUser>> userValidators,
        IEnumerable<IPasswordValidator<Data.WebSparkUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<Data.WebSparkUser>> logger)
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