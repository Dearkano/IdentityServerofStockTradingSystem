using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerofStockTradingSystem.Data
{
    /// <summary>
    /// 数据库列声明
    /// </summary>
    //表名
    [Table("Account")]
    public class Account
    {
        /// <summary>
        ///列名
        /// </summary>
        [Column("Id")]
        public int Id { get; set; }

        [Column("name")]
        public string name { get; set; }

        [Column("balance")]
        public int balance { get; set; }

    }
}
