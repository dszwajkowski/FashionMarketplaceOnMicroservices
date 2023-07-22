using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IdentityService.UnitTests.Helpers;
internal static class MockHelpers
{
    internal static Mock<UserManager<User>> MockUserManager(List<User> users)
    {
        var store = new Mock<IUserStore<User>>();
        var mgr = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<User>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<User>());


        mgr.Setup(x => x.Users).Returns(users.AsQueryable);
        mgr.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string x) =>
            {
                var user = users.Where(u => u.Email == x)
                  .SingleOrDefault();
                return user;
            });
        mgr.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync((User x, string pwd) =>
            {
                return x.PasswordHash == pwd;
            });
        mgr.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<User, string>((x, y) => users.Add(x));
        mgr.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

        return mgr;
    }
}
