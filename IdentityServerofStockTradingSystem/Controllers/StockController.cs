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
        public  StockController(MyDbContext DbContext)
        {
            MyDbContext = DbContext;

        }

        [HttpPost("operate")]
        public async Task<IActionResult> StockOperation([FromBody]StockMessage message)
        {
            //获取数据库的表
            var securitiesAccounts = MyDbContext.SecuritiesAccounts;
            var holders = MyDbContext.Holders;
            var fundAccounts = MyDbContext.FundAccounts;
            //linq 字符串 异步
            var FundAccount = await (from i in fundAccounts where i.AccountId.Equals(message.UserId) select i).FirstOrDefaultAsync();
            var holder = await (from i in holders where (i.AccountId == message.UserId && i.StockCode == message.StockCode) select i).FirstOrDefaultAsync();
            var account = await (from i in securitiesAccounts where i.Id.Equals(message.UserId) select i).FirstOrDefaultAsync();
            if (account != null)
            {
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
                else if (message.Type == "sell")
                {
                    if (holder == null || holder.SharesNum < message.Value)
                        throw new ActionResultException(HttpStatusCode.BadRequest, "The amount of stock is not enough");
                    else
                    {
                        FundAccount.BalanceAvailable += message.Price * message.Value;                
                        await MyDbContext.SaveChangesAsync();
                        holder.SharesNum -= message.Value;
                        if (holder.SharesNum == 0 && holder.UnavailableSharesNum == 0)
                            holders.Remove(holder);
                        await MyDbContext.SaveChangesAsync();
                        return Ok();
                    }

                }
                else
                    throw new ActionResultException(HttpStatusCode.BadRequest, "no such operation");
            }
            else
                throw new ActionResultException(HttpStatusCode.BadRequest, "this id doesn't exist");

        }

        //[HttpGet("account/{id}")]
        //public async Task<Holder[]> Get(string id)
        //{
        //    var holders = MyDbContext.Holders;
        //    var data = await (from a in holders where id == a.UserId select a).ToArrayAsync();
        //    return data;
        //}

        [HttpGet("{accountId}")]
        public async Task<Holder[]> GetStock(string accountId)
        {
            var holders = MyDbContext.Holders;
            var data = await (from a in holders where accountId == a.AccountId select a).ToArrayAsync();
            return data;
        }
    }

    public class StockMessage
    {
        public string UserId;
        public string StockCode;
        public int Value;
        public decimal Price;
        public string Type;
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