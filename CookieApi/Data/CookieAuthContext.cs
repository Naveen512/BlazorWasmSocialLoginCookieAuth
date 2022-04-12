using CookieApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CookieApi.Data;

public class CookieAuthContext:DbContext
{
    public CookieAuthContext(DbContextOptions<CookieAuthContext> option):base(option)
    {
        
    }

    public DbSet<User> User {get;set;}
}