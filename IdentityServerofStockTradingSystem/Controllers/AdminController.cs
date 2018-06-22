using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServerofStockTradingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;
using System.ComponentModel.DataAnnotations;
using Sakura.AspNetCore.Mvc;

namespace IdentityServerofStockTradingSystem.Controllers
{
    public class AdminInfo
    {
        [Required]
        public string AdminName { get; set; }
        [Required]
        public string AdminPassword { get; set; }
    }

    public class AccountInfo
    {
        // 用于验证管理员身份
        [Required]
        public string AdminName { get; set; }
        [Required]
        public string AdminPassword { get; set; }

        // 用于新建证券账户
        [Required]
        public string account_type { get; set; }
        [Required]
        public string person_id { get; set; }
        [Required]
        public string name { get; set; }

        public string sex { get; set; }
        public string phone_number { get; set; }
        public string address { get; set; }
        public string email { get; set; }
    }

    [Route("api/account")]
    public class AdminController:Controller
    {
        public MyDbContext MyDbContext { get; }
        public AdminController(MyDbContext dbContext)
        {
            MyDbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<string> Post([FromBody]AdminInfo adminInfo)
        {
            var name = adminInfo.AdminName;
            var password = adminInfo.AdminPassword;
            var thisUser = await (from u in MyDbContext.Administrators where u.Name.Equals(name) select u).ToArrayAsync();
            if (thisUser.Length == 0)
            {
                return "user doesn't exist";
            }
            var storedPassword = thisUser[0].Password;
            if (password.Equals(storedPassword))
            {
                return "login success";
            }
            return "password error";
        }

        [HttpPost("regist")]
        public async Task<IActionResult> AddAccount([FromBody] AccountInfo accountInfo)
        {
            var name = accountInfo.AdminName;
            var password = accountInfo.AdminPassword;
            var thisUser = await (from u in MyDbContext.Administrators where u.Name.Equals(name) select u).ToArrayAsync();
            if (thisUser.Length == 0)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no right");
            }
            var storedPassword = thisUser[0].Password;
            if (password.Equals(storedPassword))
            {
                var a = MyDbContext.SecuritiesAccounts;
                var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                var id = timestamp.ToString();
                var account = new Data.SecuritiesAccount(id, accountInfo.person_id, accountInfo.account_type)
                {
                    AccountStatus = "normal"
                };
                await a.AddAsync(account);
                await MyDbContext.SaveChangesAsync();
                return Ok(account);
            }
            throw new ActionResultException(HttpStatusCode.BadRequest, "no right");
        }
    }


}
