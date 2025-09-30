using System;
using System.Collections.Generic;

namespace B1_2.DB;

//модель периода
public partial class Period
{
    public Guid Id { get; set; }

    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }

    public virtual ICollection<AccountBalance> AccountBalances { get; set; } = new List<AccountBalance>();
}
