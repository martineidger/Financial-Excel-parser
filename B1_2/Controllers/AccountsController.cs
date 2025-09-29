using B1_2.DB.Repositories.Abstractions;
using B1_2.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace B1_2.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IExcelParserService _parser;
        private readonly IDbSaverService _dbSaver;
        private readonly IValidationService _validator;
        private readonly IBalanceSheetRepository _balanceSheetRepository;
        public AccountsController(
            IExcelParserService excelParserService,  
            IDbSaverService dbSaverService,
            IValidationService validationService, 
            IBalanceSheetRepository balanceSheetRepository)
        {
            _parser = excelParserService;
            _dbSaver = dbSaverService;
            _validator = validationService;
            _balanceSheetRepository = balanceSheetRepository;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл пустой");

            if((await _balanceSheetRepository.GetFilesAsync(cancellationToken)).
                    Select(uf => uf.FileName).
                    Contains(file.FileName)) 
                await _balanceSheetRepository.DeleteFileReportAsync(file.FileName, cancellationToken);

            using var stream = file.OpenReadStream();

            var report = _parser.Parse(stream);
            report.FileName = file.FileName;

            _validator.Validate(report);

            await _dbSaver.SaveAsync(report, cancellationToken);

            return Ok();
        }

        [HttpGet("files")]
        public async Task<IActionResult> GetFiles(CancellationToken cancellationToken)
        {
            var files = await _balanceSheetRepository.GetFilesAsync(cancellationToken);
            return Ok(files);
        }

        [HttpGet("files/{fileName}")]
        public async Task<IActionResult> GetReportFromFile([FromRoute]string fileName, CancellationToken cancellationToken)
        {
            var report = await _balanceSheetRepository.GetReportByFileAsync(fileName, cancellationToken);
            return Ok(report);
        }

    }
}
