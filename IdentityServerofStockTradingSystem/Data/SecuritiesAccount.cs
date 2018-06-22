using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerofStockTradingSystem.Data
{
    [Table("securities_account")]
    public class SecuritiesAccount
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("person_id")]
        public string PersonId { get; set; }

        [Column("account_type")]
        public string AccountType { get; set; }

        [Column("account_status")]
        public string AccountStatus { get; set; }
    }
}
