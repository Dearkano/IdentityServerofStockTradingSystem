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
    [Route("api/[controller]")]
    public class StockController : Controller
    {
        public MyDbContext MyDbContext { get; }

        /// <summary>
        /// 建立数据库连接 构造方法
        /// </summary>
        /// <param name="DbContext"></param>
        public StockController(MyDbContext DbContext)
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
            var FundAccount = await (from i in MyDbContext.FundAccounts where i.AccountId.Equals(message.UserId) select i).FirstOrDefaultAsync();
            var holder = await (from i in holders where (i.AccountId.Equals(message.UserId) && i.StockCode.Equals(message.StockCode)) select i).FirstOrDefaultAsync();
            if (FundAccount == null)
                throw new ActionResultException(HttpStatusCode.BadRequest, "this id doesn't exist");
            if (FundAccount.AccountStatus == "a")
                throw new ActionResultException(HttpStatusCode.BadRequest, "account frozen");
            if (message.Value <= 0 || message.Price <= 0)
                throw new ActionResultException(HttpStatusCode.BadRequest, "negative value or price");
            //买股票
            if (message.Type == "buy")
            {
                if (FundAccount.BalanceUnAvailable < message.Price * message.Value)
                    throw new ActionResultException(HttpStatusCode.BadRequest, "the balance is not enough");
                else
                {
                    FundAccount.BalanceUnAvailable -= message.Price * message.Value;
                    fundAccounts.Update(FundAccount);
                    await MyDbContext.SaveChangesAsync();
                    if (holder != null)
                    {
                        decimal sum = holder.AverageCost * (holder.SharesNum + holder.UnavailableSharesNum) + message.Price * message.Value;
                        holder.SharesNum += message.Value;
                        holder.AverageCost = sum / (holder.SharesNum + holder.UnavailableSharesNum);
                        holders.Update(holder);
                        await MyDbContext.SaveChangesAsync();
                        return Ok();

                    }
                    else
                    {

                        Holder newHolder = new Holder
                        {
                            Id = message.UserId + message.StockCode,
                            AccountId = message.UserId,
                            StockCode = message.StockCode,
                            SharesNum = message.Value,
                            UnavailableSharesNum = 0,
                            AverageCost = message.Price

                        };
                        holders.Add(newHolder);
                        await MyDbContext.SaveChangesAsync();
                        return Ok();
                    }
                }
            }
            //卖股票
            else if (message.Type == "sell")
            {
                if (holder == null || holder.SharesNum < message.Value)
                    throw new ActionResultException(HttpStatusCode.BadRequest, "the amout of stock is not enough");
                else
                {
                    FundAccount.BalanceAvailable += message.Price * message.Value;
                    fundAccounts.Update(FundAccount);

                    await MyDbContext.SaveChangesAsync();
                    holder.UnavailableSharesNum -= message.Value;

                    if (holder.SharesNum == 0 && holder.UnavailableSharesNum == 0)
                        holders.Remove(holder);
                    else
                        holders.Update(holder);


                    await MyDbContext.SaveChangesAsync();
                    return Ok();
                }

            }
            else
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such operation");

        }


        [HttpGet("select")]
        public async Task<StockInfo[]> GetStock(string accountId)
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
            var holders = MyDbContext.Holders;
            var data = await (from a in holders where response.Account_id == a.AccountId select new StockInfo { StockCode = a.StockCode, SharesNum = a.SharesNum, AverageCost = a.AverageCost }).ToArrayAsync();
            return data;
        }

        [HttpPost("freeze")]
        public async Task<IActionResult> FreezeStock([FromBody] FreezeStockInfo freezeStockInfo)
        {
            string account = freezeStockInfo.stock_account;
            string stockId = freezeStockInfo.stock_id;
            int value;
            try
            {
                value = int.Parse(freezeStockInfo.value);
            }
            catch
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "invalid input");
            }
            var stockInfo = await (from i in MyDbContext.Holders where i.AccountId.Equals(account) && i.StockCode.Equals(stockId) select i).FirstOrDefaultAsync();
            if (stockInfo == null)
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "no such account or stock");
            }
            if (value > stockInfo.SharesNum)
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
            string stockId = freezeStockInfo.stock_id;
            int value;
            try
            {
                value = int.Parse(freezeStockInfo.value);
            }
            catch
            {
                throw new ActionResultException(HttpStatusCode.BadRequest, "invalid input");
            }
            var stockInfo = await (from i in MyDbContext.Holders where i.AccountId.Equals(account) && i.StockCode.Equals(stockId) select i).FirstOrDefaultAsync();
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
        public string UserId; //股票账户
        public string StockCode; //股票代码
        public int Value;       //股票数
        public decimal Price;   //股票价格
        public string Type;     //操作类型 buy/sell
    }
    //查询返回的股票信息
    public class StockInfo
    {
        public string StockCode;
        public int SharesNum;
        public decimal AverageCost;
    }



    // 用于股票的冻结解冻：by shen
    public class FreezeStockInfo
    {
        public string stock_account; // 股票账户
        public string stock_id; // 操作id
        public string value;   // 要冻结的股量
    }


}