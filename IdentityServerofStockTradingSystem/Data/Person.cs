using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerofStockTradingSystem.Data
{
    [Table("person")]
    public class Person
    {
        public Person() { }

        public Person(string person_id, string name, string sex, string address, string email, string phone)
        {
            PersonId = person_id;
            Name = name;
            Sex = sex;
            Address = address;
            Email = email;
            PhoneNumber = phone;
        }
        
        [Key]
        [Column("person_id")]
        public string PersonId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("sex")]
        public string Sex { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; }
    }
}
