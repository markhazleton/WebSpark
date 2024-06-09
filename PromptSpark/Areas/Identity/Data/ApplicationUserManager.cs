using Microsoft.Extensions.Options;

namespace PromptSpark.Areas.Identity.Data;

public class ApplicationUserManager : UserManager<AdminUser>
{
    public ApplicationUserManager(
        IUserStore<AdminUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<AdminUser> passwordHasher,
        IEnumerable<IUserValidator<AdminUser>> userValidators,
        IEnumerable<IPasswordValidator<AdminUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<AdminUser>> logger)
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