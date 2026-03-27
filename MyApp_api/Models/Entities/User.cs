namespace MyApp_api.Models.Entities;
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public List<Property> Properties { get; set; }
}