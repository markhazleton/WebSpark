using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TriviaSpark.Core.Services;

public class ApplicationUserManager(
    IUserStore<Entities.TriviaSparkWebUser> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<Entities.TriviaSparkWebUser> passwordHasher,
    IEnumerable<IUserValidator<Entities.TriviaSparkWebUser>> userValidators,
    IEnumerable<IPasswordValidator<Entities.TriviaSparkWebUser>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<Entities.TriviaSparkWebUser>> logger) : UserManager<Entities.TriviaSparkWebUser>(store,
        optionsAccessor,
        passwordHasher,
        userValidators,
        passwordValidators,
        keyNormalizer,
        errors,
        services,
        logger)
{

    /// <summary>
    /// Get User Model
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Models.UserModel?> GetUserModelById(string userId)
    {
        var user = await FindByIdAsync(userId);
        if (user == null) { return null; }

        return Create(user);
    }

    private static Models.UserModel Create(Entities.TriviaSparkWebUser user)
    {
        return new Models.UserModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
        };
    }
}
