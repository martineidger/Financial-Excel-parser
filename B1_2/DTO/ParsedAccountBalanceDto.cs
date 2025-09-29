namespace B1_2.DTO
{
    public class ParsedAccountBalanceDto
    {
        public int AccountNumber { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal InpBalanceActive { get; set; }
        public decimal InpBalancePassive { get; set; }
        public decimal TurnoverDebit { get; set; }
        public decimal TurnoverCredit { get; set; }
        public decimal OutpBalanceActive { get; set; }
        public decimal OutpBalancePassive { get; set; }
    }
}
