using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerofStockTradingSystem.Data
{
    [Table("holder")]
    public class Holder
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("account_id")]
        public string AccountId { get; set; }

        [Column("stock_code")]
        public string StockCode { get; set; }

        [Column("available_shares_num")]
        public int SharesNum { get; set; }

        [Column("unavailable_shares_num")]
        public int UnavailableSharesNum { get; set; }

        [Column("average_cost")]
        public decimal AverageCost { get; set; }
    }
}
