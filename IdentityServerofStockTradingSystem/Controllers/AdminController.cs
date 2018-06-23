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

    public class UpdateInfo
    {
        [Required]
        public string AdminName { get; set; }
        [Required]
        public string AdminPassword { get; set; }
        [Required]
        public string OperateAccount { get; set; }
        [Required]
        public string item { get; set; }// 要修改的条目
        [Required]
        public string afterInfo { get; set; } // 修改后的信息
    }
    // 冻结与解冻接口
    public class Freeze
    {
        [Required]
        public string AdminName { get; set; }
        [Required]
        public string AdminPassword { get; set; }
        [Required]
        public string accountId;
    }

    public class DeleteStockInfo
    {
        [Required]
        public string AdminName { get; set; }
        [Required]
        public string AdminPassword { get; set; }
        [Required]
        public string stock_account { get; set; }
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
                MyDbContext.People.Add(person);
                await MyDbContext.SaveChangesAsync();
                var securitiesAccounts = MyDbContext.SecuritiesAccounts;
                var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                var id = timestamp.ToString();
                var account = new Data.SecuritiesAccount(id, accountInfo.person_id, accountInfo.account_type)
                {
                    AccountStatus = "n" // normal
                };
                MyDbContext.SecuritiesAccounts.Add(account);

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

        [HttpPost("freezeaccount")]
        public async Task<IActionResult> FreezeAccount([FromBody] Freeze freeze)
        {
            var name = freeze.AdminName;
            var password = freeze.AdminPassword;
            var thisUser = await (from u in MyDbContext.Administrators where u.Name.Equals(name) select u).ToArrayAsync();
            if (thisUser.Length == 0)
            {
                throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
            }
            // 校验密码
            var storedPassword = thisUser[0].Password;
            if (password.Equals(storedPassword))
            {
                var accountId = freeze.accountId;
                var accInfo = await (from i in MyDbContext.SecuritiesAccounts where i.Id==accountId select i).FirstOrDefaultAsync();
                try
                {
                    accInfo.AccountStatus = "a"; // abnormal
                    MyDbContext.SecuritiesAccounts.Update(accInfo);
                    await MyDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch
                {
                    throw new ActionResultException(HttpStatusCode.Unauthorized, "cannot freeze");
                }
            }
            throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
        }

        [HttpPost("unfreezeaccount")]
        public async Task<IActionResult> UnFreezeAccount([FromBody] Freeze freeze)
        {
            var name = freeze.AdminName;
            var password = freeze.AdminPassword;
            var thisUser = await (from u in MyDbContext.Administrators where u.Name.Equals(name) select u).ToArrayAsync();
            if (thisUser.Length == 0)
            {
                throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
            }
            // 校验密码
            var storedPassword = thisUser[0].Password;
            if (password.Equals(storedPassword))
            {
                var accountId = freeze.accountId;
                var accInfo = await (from i in MyDbContext.SecuritiesAccounts
                                     where i.Id.Equals(accountId)
                                     select i).FirstOrDefaultAsync();
                try
                {
                    accInfo.AccountStatus = "n"; //normal
                    MyDbContext.SecuritiesAccounts.Update(accInfo);
                    await MyDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch
                {
                    throw new ActionResultException(HttpStatusCode.Unauthorized, "cannot unfreeze");
                }
            }
            throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateInfo([FromBody] UpdateInfo updateInfo)
        {
            // 首先验证管理员身份
            var name = updateInfo.AdminName;
            var password = updateInfo.AdminPassword;
            var thisUser = await (from u in MyDbContext.Administrators where u.Name.Equals(name) select u).ToArrayAsync();
            if (thisUser.Length == 0)
            {
                throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
            }
            // 校验密码
            var storedPassword = thisUser[0].Password;
            if (password.Equals(storedPassword))
            {
                //获得当前操作的信息
                string accountId = updateInfo.OperateAccount;
                // 账户信息表
                var accInfo = await (from i in MyDbContext.SecuritiesAccounts
                                     where i.Id.Equals(accountId)
                                     select i).FirstOrDefaultAsync();
                // 资金表
                var funInfo = await (from i in MyDbContext.FundAccounts
                                     where i.AccountId.Equals(accountId)
                                     select i).FirstOrDefaultAsync();
                string funId = funInfo.Id;
                string personId = accInfo.PersonId;
                // 个人信息表
                var perInfo = await (from i in MyDbContext.People
                                     where i.PersonId.Equals(personId)
                                     select i).FirstOrDefaultAsync();
                // 理论上只能修改account_type和person表下的东西
                string item = updateInfo.item; // 取出要修改的列名
                string afterInfo = updateInfo.afterInfo; // 取出修改后值的字符串状态
                if (string.IsNullOrWhiteSpace(item) || string.IsNullOrWhiteSpace(afterInfo))
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "empty request");
                }
                // 修改账号状态
                // 获得三个表的引用
                if (funInfo == null || accInfo == null || perInfo == null)
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "no such person or account");
                }
                // 如果修改的是账户类型
                if (item == "account_type")
                {
                    string temp = afterInfo;
                    // 修正
                    if (temp != "g" && temp != "n")
                    {
                        throw new ActionResultException(HttpStatusCode.BadRequest, "illegal input");
                    }
                    accInfo.AccountType = temp;
                }
                else if (item == "fun_password")
                {
                    funInfo.Password = afterInfo;
                }
                else if (item == "name")
                {
                    perInfo.Name = afterInfo;
                }
                else if (item == "sex")
                {
                    perInfo.Sex = afterInfo;
                }
                else if (item == "address")
                {
                    perInfo.Address = afterInfo;
                }
                else if (item == "email")
                {
                    perInfo.Email = afterInfo;
                }
                else if (item == "phone_number")
                {
                    perInfo.PhoneNumber = afterInfo;
                }
                else // 不能修改的属性
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "illegal item");
                }
                try
                {
                    MyDbContext.SecuritiesAccounts.Update(accInfo);
                    MyDbContext.FundAccounts.Update(funInfo);
                    MyDbContext.People.Update(perInfo);
                    await MyDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "illegal input");
                }
            }
            throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
        }

        [HttpPost("destroy")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteStockInfo deleteStockInfo)
        {
            var name = deleteStockInfo.AdminName;
            var password = deleteStockInfo.AdminPassword;
            var thisUser = await (from u in MyDbContext.Administrators where u.Name.Equals(name) select u).ToArrayAsync();
            if (thisUser.Length == 0)
            {
                throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
            }
            var storedPassword = thisUser[0].Password;
            if (password.Equals(storedPassword))
            {
                var thisAccount = await (from a in MyDbContext.SecuritiesAccounts where a.Id.Equals(deleteStockInfo.stock_account) select a).FirstOrDefaultAsync();
                if (thisAccount == null)
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "The securities account is not exist");
                }
                // check if the securities account can be deleted;
                var fundsAccounts = MyDbContext.FundAccounts;
                var thisFundAccount = await (from f in fundsAccounts where f.Id.Equals(deleteStockInfo.stock_account) select f).FirstOrDefaultAsync();
                if (thisFundAccount != null)
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "The securities account is still binded with a fund account");
                }
                MyDbContext.SecuritiesAccounts.Remove(thisAccount);
                await MyDbContext.SaveChangesAsync();

                return Ok("delete success");
            }
            throw new ActionResultException(HttpStatusCode.Unauthorized, "no right");
        }
    }
}
