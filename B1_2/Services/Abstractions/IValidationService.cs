using B1_2.DTO;

namespace B1_2.Services.Abstractions
{
    //абстракция сервиса для валидации входных данных
    public interface IValidationService
    {
        void Validate(ParsedReportDto report);
    }
}
