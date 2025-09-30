namespace B1_2.DTO
{
    //dto для парсинга данных аккаунтов
    public class ParsedAccountDto
    {
        public int AccountNumber { get; set; }
        public string AccountClassCode { get; set; } = "";
    }
}
