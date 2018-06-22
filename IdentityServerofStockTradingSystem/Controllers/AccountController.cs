using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IdentityServerofStockTradingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sakura.AspNetCore.Mvc;

namespace IdentityServerofStockTradingSystem.Controllers
{
    public class FreezeValueInfo
    {
        public string stock_account;
        public string value;
    }
    public class UpdateInfo
    {

    }
    [Route("api/[controller]")]
    public class AccountController:Controller
    {
        public MyDbContext MyDbContext { get; }

     /// <summary>
     /// 建立数据库连接 构造方法
     /// </summary>
     /// <param name="DbContext"></param>
        public AccountController(MyDbContext DbContext)
        {
            MyDbContext = DbContext;
            //        booksAmount = DbContext.Books.Count();
        }
        [HttpGet("test")]
        public async Task<IActionResult> Demo()
        {
            var token = Request.Headers["Authorization"];
            TResponse response = await Utility.GetIdentity(token);
            //拿单个数据 拿不到就是null
            var d = await (from i in MyDbContext.Accounts where i.balance == 2 select i).FirstOrDefaultAsync();

            //拿数据变成数组
            var a = await (from i in MyDbContext.Accounts where i.name.Equals("zju")select i).ToArrayAsync();

            //返回200
            return Ok();
            //返回报错
            //bad request 400 unauthorized 401  forbidden 403 and so on
            throw new ActionResultException(HttpStatusCode.BadRequest, "error message");
        }

        //http路由 这里是 /api/account/account
        //如果有参数  可以是 [HttpGet("account/{id}")] 然后Get(int id)取参数
        [HttpGet("account")]
        public async Task<Account[]> Get()
        {
            //获取数据库的表
            var accounts = MyDbContext.Accounts;
            //linq 字符串 异步
            //翻译成sql 差不多就是 select * from accounts
            //返回的是数组
            var data = await (from account in accounts select account).ToArrayAsync();
            return data;
        }

        [HttpGet("admin")]
        public async Task<Administrator[]> Get(string admin)
        {
            var admins = MyDbContext.Administrators;
            var data = await(from a in admins select a).ToArrayAsync();
            return data;
        }

        [HttpGet("freeze")]
        public async Task<IActionResult> FreezeValue([FromBody] FreezeValueInfo freezeValueInfo)
        {   
            string account = freezeValueInfo.stock_account;
            decimal value = decimal.Parse(freezeValueInfo.value);
            // 首先找出当前的活动资金和冻结资金
            var accountInfo = await (from i in MyDbContext.FundAccounts where i.AccountId.Equals(account) select i).FirstOrDefaultAsync();
            if(accountInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such account");
            }
            decimal avail = accountInfo.BalanceAvailable;
            decimal unavail = accountInfo.BalanceUnAvailable;
            if(avail < value)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "frozen value out of range");
            }
            else
            {
                accountInfo.BalanceAvailable -= value;
                accountInfo.BalanceUnAvailable += value;
                MyDbContext.FundAccounts.Update(accountInfo);
                await MyDbContext.SaveChangesAsync();
                return Ok();
            }
            throw new ActionResultException(HttpStatusCode.BadRequest, "no such account");
        }
        [HttpGet("unfreeze")]
        public async Task<IActionResult> UnFreezeValue([FromBody] FreezeValueInfo freezeValueInfo)
        {
            string account = freezeValueInfo.stock_account;
            decimal value = decimal.Parse(freezeValueInfo.value);
            // 首先找出当前的活动资金和冻结资金
            var accountInfo = await (from i in MyDbContext.FundAccounts where i.AccountId.Equals(account) select i).FirstOrDefaultAsync();
            if (accountInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such account");
            }
            decimal avail = accountInfo.BalanceAvailable;
            decimal unavail = accountInfo.BalanceUnAvailable;
            if (unavail < value)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "unfrozen value out of range");
            }
            else
            {
                accountInfo.BalanceAvailable += value;
                accountInfo.BalanceUnAvailable -= value;
                MyDbContext.FundAccounts.Update(accountInfo);
                await MyDbContext.SaveChangesAsync();
                return Ok();
            }
            throw new ActionResultException(HttpStatusCode.BadRequest, "no such account");
        }
    }
}
