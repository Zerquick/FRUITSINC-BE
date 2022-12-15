using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class KwekkerContext : DbContext
{
    public KwekkerContext(DbContextOptions<KwekkerContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
}