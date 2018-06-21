using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServerofSystemTradingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerofSystemTradingSystem.Controllers
{
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
    }
}
