using System;
using System.Collections.Generic;

namespace B1_2.DB;

public partial class AccountClass
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
