

﻿using System;
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
       
        public string stock_account {get; set;}
        public string value{get;set;}
    }
    // 修改信息接口
   
    public class UpdateValueInfo
    {
        public string funId; // 资金账户id
        public string type;  // recharge | withdraw
        public string value; // 取出/存入
    }

    public class Vip
    {
        public string accountId;
        public string cost;
    }
    public class NewPassword
    {
        public string old_password {get; set;}
        public string new_password {get; set;}
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
        public async Task<SecuritiesAccount[]> Demo()
        {
            //var token = Request.Headers["Authorization"];
           // TResponse response = await Utility.GetIdentity(token);
            //拿单个数据 拿不到就是null
            var d = await (from i in MyDbContext.Accounts where i.balance == 2 select i).FirstOrDefaultAsync();

            //拿数据变成数组
            var a = await (from i in MyDbContext.SecuritiesAccounts select i).ToArrayAsync();

            //返回200
            return a;
            //返回报错
            //bad request 400 unauthorized 401  forbidden 403 and so on
            throw new ActionResultException(HttpStatusCode.BadRequest, "error message");
        }

        //http路由 这里是 /api/account/account
        //如果有参数  可以是 [HttpGet("account/{id}")] 然后Get(int id)取参数
        [HttpGet("account")]
        public async Task<Person[]> Get()
        {
            //获取数据库的表
            var accounts = MyDbContext.Accounts;
            //linq 字符串 异步
            //翻译成sql 差不多就是 select * from accounts
            //返回的是数组
            var data = await (from account in MyDbContext.People select account).ToArrayAsync();
            return data;
        }

        [HttpGet("admin")]
        public async Task<Administrator[]> Get(string admin)
        {
            var admins = MyDbContext.Administrators;
            var data = await(from a in admins select a).ToArrayAsync();
            return data;
        }

        [HttpPost("freeze")]
        public async Task<IActionResult> FreezeValue([FromBody] FreezeValueInfo freezeValueInfo)
        {   
            string account = freezeValueInfo.stock_account;
            decimal value;
            try
            {
                value = decimal.Parse(freezeValueInfo.value);
            }
            catch
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "invalid input");
            }
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
        [HttpPost("unfreeze")]
        public async Task<IActionResult> UnFreezeValue([FromBody] FreezeValueInfo freezeValueInfo)
        {
            string account = freezeValueInfo.stock_account;
            decimal value;
            try
            {
                value = decimal.Parse(freezeValueInfo.value);
            }
            catch
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "invalid input");
            }
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
        
        [HttpPost("balance")]
        public async Task<IActionResult>UpdateValue([FromBody] UpdateValueInfo updateValueInfo)
        {
            
            string funId = updateValueInfo.funId; // 获取funId
            var funInfo = await (from i in MyDbContext.FundAccounts 
                        where i.Id.Equals(funId) select i).FirstOrDefaultAsync();
            if(funInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such account");
            }
            string accountId = funInfo.AccountId;
            // 检测账户是否处于冻结状态
            var accInfo = await (from i in MyDbContext.SecuritiesAccounts
                                 where i.Id.Equals(accountId)
                                 select i).FirstOrDefaultAsync();
            if(accInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such account");
            }
            if(accInfo.AccountStatus == "a") // 挂失/冻结状态
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "account frozen");
            }
            decimal value;
            try
            {
                 value = decimal.Parse(updateValueInfo.value); // 转换为小数
            }
            catch
            {
                 throw new ActionResultException(HttpStatusCode.BadRequest, "invalid input");
            }
            decimal nowBalance = funInfo.BalanceAvailable;
            string type = updateValueInfo.type;
            if(value < 0)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "negative value");
            }
            if(type == "recharge")
            {
                funInfo.BalanceAvailable += value;
            }
            else if(type == "withdraw")
            {
                if(value > nowBalance)
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "value out of range");
                }
                else
                {
                    funInfo.BalanceAvailable -= value;
                }
            }
            try
            {
                MyDbContext.FundAccounts.Update(funInfo);
                await MyDbContext.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "unexpected input");
            }
        }

        //查询账户信息
        [HttpGet("select")]
        public async Task<TResponse> GetAccount(string Id)
        {
            var token = Request.Headers["Authorization"];
            TResponse response;
            try
            {
                response = await Utility.GetIdentity(token);
            }
            catch
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "invalid token");
            }

            return response;
        }

        [HttpPost("vip")]
        public async Task<IActionResult> SetVip([FromBody] Vip vip)
        {
            var accInfo = await (from i in MyDbContext.SecuritiesAccounts
                                 where i.Id.Equals(vip.accountId)
                                 select i).FirstOrDefaultAsync();
            if(accInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such account");
            }
            decimal cost;
            try
            {
                cost = decimal.Parse(vip.cost.ToString());
            }
            catch
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "invalid input");
            }
            if(accInfo.AccountStatus == "a")
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "account forzen");
            }
            var funInfo = await (from i in MyDbContext.FundAccounts 
                        where i.AccountId.Equals(vip.accountId) select i).FirstOrDefaultAsync();
            if(funInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such fund account");
            }
            if(funInfo.BalanceAvailable < cost)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no enough money");
            }
            if(accInfo.AccountType == "g")
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "has been vip");
            }
            try
            {
                accInfo.AccountType = "g";
                funInfo.BalanceAvailable -= cost;
                MyDbContext.SecuritiesAccounts.Update(accInfo);
                await MyDbContext.SaveChangesAsync();
                MyDbContext.FundAccounts.Update(funInfo);
                await MyDbContext.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "invalid modification");
            }
        }
        [HttpPost("newpassword")]
        public async Task<IActionResult>NewPassword([FromBody] NewPassword newPassword)
        {
            // token中获取account信息
            var token = Request.Headers["Authorization"];
            TResponse response;
            try
            {
                response = await Utility.GetIdentity(token);
            }
            catch
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "invalid token");
            }
            string operatingAccount = response.Account_id;
            var funInfo = await (from i in MyDbContext.FundAccounts
                                 where i.AccountId.Equals(operatingAccount)
                                 select i).FirstOrDefaultAsync();
            if(funInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such fund account");
            }
            string oldPassword = funInfo.Password;
            string providePassword = newPassword.old_password;
            if(oldPassword == providePassword)
            {
                try
                {
                     string setPassword = newPassword.new_password;
                     funInfo.Password = setPassword;
                     MyDbContext.FundAccounts.Update(funInfo);
                     await MyDbContext.SaveChangesAsync();
                     return Ok();
                }
                catch
                {
                    throw new ActionResultException(HttpStatusCode.BadRequest, "invalid modification");
                }
            }
            throw new ActionResultException(HttpStatusCode.BadRequest, "wrong old password");
        }
    }
}

