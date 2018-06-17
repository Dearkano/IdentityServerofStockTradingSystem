﻿using System;
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

    [Table("administrator(name, password)")]
    public class Administrator
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("password")]
        public string Password { get; set; }
    }

    [Table("securities_account")]
    public class SecuritiesAccount
    {
        [Column("account_num")]
        public string AccountNum { get; set; }

        [Column("person_id")]
        public string PersonId { get; set; }

        [Column("account_type")]
        public string AccountType { get; set; }

        [Column("account_status")]
        public string AccountStatus { get; set; }
    }

    [Table("person")]
    public class Person
    {
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

    [Table("funds_account")]
    public class FundAccount
    {
        [Column("funds_account_num")]
        public string FundsAccountNum { get; set; }

        [Column("account_num")]
        public string AccountNum { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("balance_available")]
        public decimal BalanceAvailable { get; set; }

        [Column("balance_unavailable")]
        public decimal BalanceUnAvailable { get; set; }

        [Column("account_status")]
        public string AccountStatus { get; set; }
    }

    [Table("holder")]
    public class Holder
    {
        [Column("account_num")]
        public string AccountNum { get; set; }

        [Column("stock_code")]
        public string StockCode { get; set; }

        [Column("shares_num")]
        public decimal SharesNum { get; set; }
    }
}
