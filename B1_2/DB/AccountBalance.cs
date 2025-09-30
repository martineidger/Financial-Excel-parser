using System;
using System.Collections.Generic;

namespace B1_2.DB;

//модель баланса счета
public partial class AccountBalance
{
    public Guid Id { get; set; }

    public Guid? AccountId { get; set; }

    public Guid? PeriodId { get; set; }

    public Guid UploadedFileId { get; set; }

    public decimal InpBalanceActive { get; set; }

    public decimal InpBalancePassive { get; set; }

    public decimal TurnoverDebit { get; set; }

    public decimal TurniverCredit { get; set; }

    public decimal OutpBalanceActive { get; set; }

    public decimal OutpBalancePassive { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Period? Period { get; set; }

    public virtual UploadedFile UploadedFile { get; set; } = null!;
}
