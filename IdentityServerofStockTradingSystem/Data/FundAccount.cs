using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerofStockTradingSystem.Data
{
    [Table("funds_account")]
    public class FundAccount
    {
        public FundAccount() { }

        public FundAccount(string id, string account_id, string password)
        {
            Id = id;
            AccountId = account_id;
            Password = password;
            BalanceAvailable = 0;
            BalanceUnAvailable = 0;
            AccountStatus = "n";
        }
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [ForeignKey("account_id")]
        [Column("account_id")]
        public string AccountId { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("balance_available")]
        public decimal BalanceAvailable { get; set; }

        [Column("balance_unavailable")]
        public decimal BalanceUnAvailable { get; set; }

        [Column("account_status")]
        public string AccountStatus { get; set; }
    }
}
