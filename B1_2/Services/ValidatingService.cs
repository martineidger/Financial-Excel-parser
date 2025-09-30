using B1_2.DTO;
using B1_2.Services.Abstractions;

namespace B1_2.Services
{
    //сервис для валидации входных данных
    public class ValidatingService : IValidationService
    {
        public void Validate(ParsedReportDto report)
        {
            //проверяет, чтобы исходящее сальдо было рассчитано верно на основе входящего сальдо и активов
            foreach (var balance in report.AccountBalances)
            {
                bool needsRecalc = balance.OutpBalanceActive == 0
                                   || balance.OutpBalancePassive == 0
                                   || balance.OutpBalanceActive != balance.InpBalanceActive + balance.TurnoverDebit - balance.TurnoverCredit
                                   || balance.OutpBalancePassive != balance.InpBalancePassive + balance.TurnoverCredit - balance.TurnoverDebit;
                //если значения пустые или рассчитаны неверно -> пересчиать значения верно
                if (needsRecalc)
                {
                    balance.OutpBalanceActive = balance.InpBalanceActive + balance.TurnoverDebit - balance.TurnoverCredit;
                    balance.OutpBalancePassive = balance.InpBalancePassive + balance.TurnoverCredit - balance.TurnoverDebit;
                }
            }

        }
    }
}
