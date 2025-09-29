using B1_2.DTO;

namespace B1_2.Services.Abstractions
{
    public interface IValidationService
    {
        void Validate(ParsedReportDto report);
    }
}
