using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerofSystemTradingSystem.Data
{
     public class MyDbContext : DbContext
{

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {

    }

    // public DbSet<Book> Books { get; set; }

    // public DbSet<Card> Cards { get; set; }

    //public DbSet<Record> Records { get; set; }

    //  public DbSet<Authorization> Authorizations { get; set; }
}
}
