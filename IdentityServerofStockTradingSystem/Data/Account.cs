using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
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

    [Table("securities_account")]
    public class SecuritiesAccount
    {
        [Column("account_num")]
        public string AccountNum { get; set; }

        [Column("person_id")]
        public string PersonId { get; set; }

        [Column("person_name")]
        public string PersonName { get; set; }

        [Column("person_sex")]
        public string PersonSex { get; set; }

        [Column("person_address")]
        public string PersonAddress { get; set; }

        [Column("account_type")]
        public string AccountType { get; set; }

        [Column("account_status")]
        public string AccountStatus { get; set; }
    }

}
