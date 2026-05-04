namespace Nyumba_api.Models.Authorization;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Landlord = "Landlord";
    public const string Agent = "Agent";
    public const string User = "User";

    public const string AllRolesPattern = "Admin|Landlord|Agent|User";
    public const string PublicRegistrationRolesPattern = "Landlord|Agent|User";

    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        Admin,
        Landlord,
        Agent,
        User
    };

    private static readonly HashSet<string> PublicRegistrationRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        Landlord,
        Agent,
        User
    };

    public static bool IsValid(string role) => ValidRoles.Contains(role);

    public static bool CanSelfRegister(string role) => PublicRegistrationRoles.Contains(role);

    public static string Normalize(string role)
    {
        if (role.Equals(Admin, StringComparison.OrdinalIgnoreCase)) return Admin;
        if (role.Equals(Landlord, StringComparison.OrdinalIgnoreCase)) return Landlord;
        if (role.Equals(Agent, StringComparison.OrdinalIgnoreCase)) return Agent;
        if (role.Equals(User, StringComparison.OrdinalIgnoreCase)) return User;

        return role;
    }
}
