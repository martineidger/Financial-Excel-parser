using System;
using System.Collections.Generic;

namespace B1_2.DB;

public partial class UploadedFile
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = null!;

    public DateTime UploadDate { get; set; }

    public virtual ICollection<AccountBalance> AccountBalances { get; set; } = new List<AccountBalance>();
}
