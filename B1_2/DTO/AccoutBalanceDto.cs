namespace B1_2.DTO
{
    public class AccountBalanceDto
    {
        public decimal InpBalanceActive { get; set; }
        public decimal InpBalancePassive { get; set; }
        public decimal TurnoverDebit { get; set; }
        public decimal TurniverCredit { get; set; }
        public decimal OutpBalanceActive { get; set; }
        public decimal OutpBalancePassive { get; set; }
        public string AccountNumber { get; set; }
        public string AccountClassName { get; set; }
    }
}
