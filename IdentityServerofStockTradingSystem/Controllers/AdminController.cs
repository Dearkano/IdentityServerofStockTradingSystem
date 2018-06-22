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

    public class BindInfo
    {
        // 用于验证管理员身份
        [Required]
        public string AdminName { get; set; }
        [Required]
        public string AdminPassword { get; set; }

        // 用于新建资金账户
        [Required]
        public string stock_account { get; set;}
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
            var thisUser = await (from u in MyDbContext.Administrators where u.Name.Trim().Equals(name) select u).ToArrayAsync();
            if (thisUser.Length == 0)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no right");
            }
            var storedPassword = thisUser[0].Password;
            if (password.Equals(storedPassword))
            {
                var people = MyDbContext.People;
                var thisPerson = await (from p in MyDbContext.People where p.PersonId.Equals(accountInfo.person_id) select p).FirstOrDefaultAsync();
                if (thisPerson != null)
                {
                    var thisAccount = await (from a in MyDbContext.SecuritiesAccounts where a.PersonId.Equals(accountInfo.person_id) select a).FirstOrDefaultAsync();
                    if (thisAccount != null && thisAccount.AccountStatus != "a")
                    {
                        throw new ActionResultException(HttpStatusCode.BadRequest, "This person has a securities account already");
                    }
                    else if (thisAccount != null && thisAccount.AccountStatus == "a") // 挂失状态
                    {
                        thisAccount.AccountStatus = "n"; // 账户找回
                        MyDbContext.SecuritiesAccounts.Update(thisAccount);
                        return Ok(thisAccount);
                    }
                    else
                    {
                        throw new ActionResultException(HttpStatusCode.InternalServerError);
                    }
                }

                var person = new Person(accountInfo.person_id, accountInfo.name, accountInfo.sex, accountInfo.address, accountInfo.email, accountInfo.phone_number);
                await people.AddAsync(person);

                var securitiesAccounts = MyDbContext.SecuritiesAccounts;
                var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                var id = timestamp.ToString();
                var account = new Data.SecuritiesAccount(id, accountInfo.person_id, accountInfo.account_type)
                {
                    AccountStatus = "n" // normal
                };
                await securitiesAccounts.AddAsync(account);

                await MyDbContext.SaveChangesAsync();
                return Ok(account);
            }
            throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
        }

        [HttpPost("bind")]
        public async Task<IActionResult> Bind([FromBody] BindInfo bindInfo)
        {
            var name = bindInfo.AdminName;
            var password = bindInfo.AdminPassword;
            var thisUser = await (from u in MyDbContext.Administrators where u.Name.Equals(name) select u).ToArrayAsync();
            if (thisUser.Length == 0)
            {
                throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
            }
            var storedPassword = thisUser[0].Password;
            if (password.Equals(storedPassword))
            {
                var fundsAccounts = MyDbContext.FundAccounts;
                var thisAccount = await (from a in MyDbContext.SecuritiesAccounts where a.Id.Equals(bindInfo.stock_account) select a).FirstOrDefaultAsync();
                if (thisAccount == null || thisAccount.AccountStatus == "a")
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "The securities account is not valid");
                }
                var thisFundAccount = await (from f in MyDbContext.FundAccounts where f.Id.Equals(bindInfo.stock_account) select f).FirstOrDefaultAsync();
                if (thisFundAccount != null && thisFundAccount.AccountStatus != "a")
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "The securities account is already binded with a fund account");
                }
                else if (thisFundAccount != null)
                {
                    thisFundAccount.AccountStatus = "n"; // 找回资金账户
                    MyDbContext.FundAccounts.Update(thisFundAccount);
                    return Ok(thisFundAccount);
                }

                var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                var id = (timestamp + 3160103827).ToString();
                MD5 md5 = new MD5CryptoServiceProvider();
                var initial_password = BitConverter.ToString((md5.ComputeHash(Encoding.UTF8.GetBytes(id.Substring(6))))).Replace("-", "");
                var newFundAccount = new FundAccount(id, bindInfo.stock_account, initial_password.Substring(6, 6));
                await fundsAccounts.AddAsync(newFundAccount);
                await MyDbContext.SaveChangesAsync();

                return Ok(newFundAccount);
            }
            throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
        }
    }
}
