using System;
using System.Collections.Generic;

namespace B1_2.DB;

public partial class Account
{
    public Guid Id { get; set; }

    public int AccountNumber { get; set; }

    public Guid? BankId { get; set; }

    public Guid? AccountClassId { get; set; }

    public virtual ICollection<AccountBalance> AccountBalances { get; set; } = new List<AccountBalance>();

    public virtual AccountClass? AccountClass { get; set; }

    public virtual Bank? Bank { get; set; }
}
