﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerofStockTradingSystem.Data
{
    public class MyDbContext : DbContext
    {

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {

        }
        //数据库表声明
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<SecuritiesAccount> SecuritiesAccounts { get; set; }
        public DbSet<FundAccount> FundAccounts { get; set; }
        public DbSet<Holder> Holders { get; set; }
        public DbSet<Person> People { get; set; }

    }
}
