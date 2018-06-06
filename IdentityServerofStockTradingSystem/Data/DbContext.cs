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
        //数据库表声明
     public DbSet<Account> Accounts { get; set; }

}
}
