﻿using System.Collections.Generic;
using System.Linq;
using Samba.Infrastructure.Data;
using System;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocumentType : Entity, IOrderable
    {
        public AccountTransactionDocumentType()
        {
            _TransactionTypes = new List<AccountTransactionType>();
            _accountTransactionDocumentTypeMaps = new List<AccountTransactionDocumentTypeMap>();
            _accountTransactionDocumentAccountMaps = new List<AccountTransactionDocumentAccountMap>();
        }

        public string ButtonHeader { get; set; }

        private string _buttonColor;
        public string ButtonColor
        {
            get { return _buttonColor ?? "Gainsboro"; }
            set { _buttonColor = value; }
        }

        public int MasterAccountTypeId { get; set; }

        private readonly IList<AccountTransactionType> _TransactionTypes;
        public virtual IList<AccountTransactionType> TransactionTypes
        {
            get { return _TransactionTypes; }
        }

        private readonly IList<AccountTransactionDocumentTypeMap> _accountTransactionDocumentTypeMaps;
        public virtual IList<AccountTransactionDocumentTypeMap> AccountTransactionDocumentTypeMaps
        {
            get { return _accountTransactionDocumentTypeMaps; }
        }

        private IList<AccountTransactionDocumentAccountMap> _accountTransactionDocumentAccountMaps;
        public virtual IList<AccountTransactionDocumentAccountMap> AccountTransactionDocumentAccountMaps
        {
            get { return _accountTransactionDocumentAccountMaps; }
            set { _accountTransactionDocumentAccountMaps = value; }
        }

        public string DefaultAmount { get; set; }
        public string DescriptionTemplate { get; set; }
        public bool BatchCreateDocuments { get; set; }
        public int Filter { get; set; }
        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        public AccountTransactionDocument CreateDocument(Account account, string description, decimal amount, decimal exchangeRate, IList<Account> accounts)
        {
            // <pex>
            if (account == null)
                throw new ArgumentNullException("account");
            if (account.AccountTypeId != MasterAccountTypeId)
                throw new ArgumentException("Account Type should match Master Account Type");
            // </pex>

            var result = new AccountTransactionDocument { Name = Name };
            foreach (var accountTransactionType in TransactionTypes)
            {
                var transaction = AccountTransaction.Create(accountTransactionType);
                transaction.Name = description;
                transaction.UpdateAmount(amount, exchangeRate);
                transaction.UpdateAccounts(MasterAccountTypeId, account.Id);
                if (accounts != null && accounts.Count > 0)
                {
                    if (transaction.SourceAccountTypeId != MasterAccountTypeId &&
                        transaction.SourceTransactionValue.AccountId == 0)
                    {
                        Account ac =
                            accounts.FirstOrDefault(x => x.AccountTypeId == transaction.SourceAccountTypeId);
                        if (ac != null) transaction.SetSourceAccount(ac.AccountTypeId, ac.Id);
                    }
                    if (transaction.TargetAccountTypeId != MasterAccountTypeId &&
                        transaction.TargetTransactionValue.AccountId == 0)
                    {
                        Account ac =
                            accounts.FirstOrDefault(x => x.AccountTypeId == transaction.TargetAccountTypeId);
                        if (ac != null) transaction.SetTargetAccount(ac.AccountTypeId, ac.Id);
                    }
                }
                result.AccountTransactions.Add(transaction);
            }
            return result;
        }


        public void AddAccountTransactionDocumentTypeMap()
        {
            AccountTransactionDocumentTypeMaps.Add(new AccountTransactionDocumentTypeMap());
        }

        public List<int> GetNeededAccountTypes()
        {
            var result = new List<int>();
            foreach (var accountTransactionType in TransactionTypes)
            {
                if (accountTransactionType.TargetAccountTypeId != MasterAccountTypeId &&
                    accountTransactionType.DefaultTargetAccountId == 0)
                {
                    if (!result.Contains(accountTransactionType.TargetAccountTypeId))
                        result.Add(accountTransactionType.TargetAccountTypeId);
                }
                if (accountTransactionType.SourceAccountTypeId != MasterAccountTypeId &&
                    accountTransactionType.DefaultSourceAccountId == 0)
                {
                    if (!result.Contains(accountTransactionType.SourceAccountTypeId))
                        result.Add(accountTransactionType.SourceAccountTypeId);
                }
            }
            return result;
        }
    }
}