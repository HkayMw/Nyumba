
using Microsoft.EntityFrameworkCore;
using Nyumba_api.Models.Entities;

namespace Nyumba_api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Property> Properties { get; set; }
    public DbSet<User> Users { get; set; }
}