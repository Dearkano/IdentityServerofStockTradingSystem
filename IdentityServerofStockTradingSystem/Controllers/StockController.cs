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
    [Route("api/[controller]")]
    public class StockController : Controller
    {
        public MyDbContext MyDbContext { get; }

        /// <summary>
        /// 建立数据库连接 构造方法
        /// </summary>
        /// <param name="DbContext"></param>
        public  StockController(MyDbContext DbContext)
        {
            MyDbContext = DbContext;

        }

        [HttpPost("operate")]
        public async Task<IActionResult> StockOperation([FromBody]StockMessage message)
        {
            //获取数据库的表

            var holders = MyDbContext.Holders;
            var fundAccounts = MyDbContext.FundAccounts;
            //linq 字符串 异步
            var FundAccount = await (from i in fundAccounts where i.AccountId.Equals(message.UserId) select i).FirstOrDefaultAsync();
            var holder = await (from i in holders where (i.AccountId == message.UserId && i.StockCode == message.StockCode) select i).FirstOrDefaultAsync();

            if (fundAccounts != null)
            {
                //买股票
                if (message.Type == "buy")
                {
                    if (FundAccount.BalanceAvailable < message.Price * message.Value)
                        throw new ActionResultException(HttpStatusCode.BadRequest, "the balance is not enough");
                    else
                    {
                        FundAccount.BalanceAvailable -= message.Price * message.Value;
                        if (holder != null)
                        {
                            decimal sum = holder.AverageCost * (holder.SharesNum + holder.UnavailableSharesNum) + message.Price * message.Value;
                            try
                            {
                                holder.SharesNum += message.Value;
                                holder.AverageCost = sum / (holder.SharesNum + holder.UnavailableSharesNum);

                                await MyDbContext.SaveChangesAsync();
                                return Ok();
                            }
                            catch (Exception e)
                            {
                                throw new ActionResultException(HttpStatusCode.BadRequest, "card message is not right");
                            }

                        }
                        else
                        {
                            Holder newHolder = new Holder
                            {
                                AccountId = message.UserId,
                                StockCode = message.StockCode,
                                SharesNum = message.Value,
                                UnavailableSharesNum = 0,
                                AverageCost = message.Price
                            };
                            await holders.AddAsync(newHolder);
                            await MyDbContext.SaveChangesAsync();
                            return Ok();
                        }
                    }
                }
                //卖股票
                else if (message.Type == "sell")
                {
                    if (holder == null || holder.SharesNum < message.Value)
                        throw new ActionResultException(HttpStatusCode.BadRequest, "something is error");
                    else
                    {
                        try
                        {
                            FundAccount.BalanceAvailable += message.Price * message.Value;
                            holder.SharesNum -= message.Value;
                            if (holder.SharesNum == 0 && holder.UnavailableSharesNum == 0)
                                holders.Remove(holder);
                            await MyDbContext.SaveChangesAsync();
                            return Ok();
                        }
                        catch (Exception e)
                        {
                            throw new ActionResultException(HttpStatusCode.BadRequest, "something is error");
                        }
                    }

                }
                else
                    throw new ActionResultException(HttpStatusCode.BadRequest, "no such operation");
            }
            else
                throw new ActionResultException(HttpStatusCode.BadRequest, "this id doesn't exist");

        }


        [HttpGet("{accountId}")]
        public async Task<Holder[]> GetStock(string accountId)
        {
            var holders = MyDbContext.Holders;
            var data = await (from a in holders where accountId == a.AccountId select a).ToArrayAsync();
            return data;
        }

        [HttpPost("freeze")]
        public async Task<IActionResult> FreezeStock([FromBody] FreezeStockInfo freezeStockInfo)
        {
            string account = freezeStockInfo.stock_account;
            string code = freezeStockInfo.stock_id;
            int value = int.Parse(freezeStockInfo.value);
            var stockInfo = await (from i in MyDbContext.Holders where i.AccountId.Equals(account) && i.StockCode.Equals(code) select i).FirstOrDefaultAsync();
            if(stockInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such account or stock");
            }
            if(value > stockInfo.SharesNum)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "frozen value out of range");
            }
            else
            {
                stockInfo.SharesNum -= value;
                stockInfo.UnavailableSharesNum += value;
                MyDbContext.Holders.Update(stockInfo);
                await MyDbContext.SaveChangesAsync();
                return Ok();
            }
            throw new ActionResultException(HttpStatusCode.BadRequest, "no such account or stock");
        }

        [HttpPost("unfreeze")]
        public async Task<IActionResult> UnFreezeStock([FromBody] FreezeStockInfo freezeStockInfo)
        {
            string account = freezeStockInfo.stock_account;
            string code = freezeStockInfo.stock_id;
            int value = int.Parse(freezeStockInfo.value);
            var stockInfo = await (from i in MyDbContext.Holders where i.AccountId.Equals(account) && i.StockCode.Equals(code) select i).FirstOrDefaultAsync();
            if (stockInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such account or stock");
            }
            if (value > stockInfo.UnavailableSharesNum)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "unfrozen value out of range");
            }
            else
            {
                stockInfo.SharesNum += value;
                stockInfo.UnavailableSharesNum -= value;
                MyDbContext.Holders.Update(stockInfo);
                await MyDbContext.SaveChangesAsync();
                return Ok();
            }
            throw new ActionResultException(HttpStatusCode.BadRequest, "no such account or stock");
        }
    }
    //用于买卖股票
    public class StockMessage
    {
        public string UserId;
        public string StockCode;
        public int Value;
        public decimal Price;
        public string Type;
    }

    // 用于股票的冻结解冻：by shen
    public class FreezeStockInfo
    {
        public string stock_account; // 股票账户
        public string stock_id; // 操作id
        public string value;   // 要冻结的股量
    }

    //public class AccountMessage
    //{
    //    public string AccountId;
    //    public string Name;
    //    public string Sex;
    //    public string PersonId;
    //    public string AccountType;
    //    public decimal BalanceAvailable;
    //    public decimal BalanceUnAvailable;
    //}

}