using Nyumba_api.Models.Authorization;
using Xunit;

namespace Nyumba_api.Tests;

public class RolePolicyTests
{
    [Fact]
    public void PublicRegistrationDisallowsAdmin()
    {
        Assert.False(AppRoles.CanSelfRegister(AppRoles.Admin));
    }

    [Theory]
    [InlineData(AppRoles.Landlord)]
    [InlineData(AppRoles.Agent)]
    [InlineData(AppRoles.User)]
    public void PublicRegistrationAllowsNonAdminRoles(string role)
    {
        Assert.True(AppRoles.CanSelfRegister(role));
    }

    [Theory]
    [InlineData("admin", AppRoles.Admin)]
    [InlineData("landlord", AppRoles.Landlord)]
    [InlineData("AGENT", AppRoles.Agent)]
    [InlineData("user", AppRoles.User)]
    public void NormalizePreservesCanonicalRoleNames(string input, string expected)
    {
        Assert.Equal(expected, AppRoles.Normalize(input));
    }
}
