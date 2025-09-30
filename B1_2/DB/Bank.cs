using System;
using System.Collections.Generic;

namespace B1_2.DB;

//модель банка
public partial class Bank
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
