using B1_2.DTO;
using B1_2.Services.Abstractions;

namespace B1_2.Services
{
    public class ValidatingService : IValidationService
    {
        public void Validate(ParsedReportDto report)
        {


            foreach (var balance in report.AccountBalances)
            {
                bool needsRecalc = balance.OutpBalanceActive == 0
                                   || balance.OutpBalancePassive == 0
                                   || balance.OutpBalanceActive != balance.InpBalanceActive + balance.TurnoverDebit - balance.TurnoverCredit
                                   || balance.OutpBalancePassive != balance.InpBalancePassive + balance.TurnoverCredit - balance.TurnoverDebit;

                if (needsRecalc)
                {
                    balance.OutpBalanceActive = balance.InpBalanceActive + balance.TurnoverDebit - balance.TurnoverCredit;
                    balance.OutpBalancePassive = balance.InpBalancePassive + balance.TurnoverCredit - balance.TurnoverDebit;
                }
            }

        }
    }
}
