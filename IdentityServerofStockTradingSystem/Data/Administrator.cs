using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerofStockTradingSystem.Data
{
    [Table("administrator")]
    public class Administrator
    {
        [Key]
        [Column("name")]
        public string Name { get; set; }

        [Column("password")]
        public string Password { get; set; }
    }
}
