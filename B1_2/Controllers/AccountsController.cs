using B1_2.DB.Repositories.Abstractions;
using B1_2.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace B1_2.Controllers
{
    //контроллер, ответсвенный за обработку запросов по пути "accounts"

    [Controller]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        //используемые сервисы
        private readonly IExcelParserService _parser;
        private readonly IDbSaverService _dbSaver;
        private readonly IValidationService _validator;
        private readonly IBalanceSheetRepository _balanceSheetRepository;

        //получаем сервисы из DI
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

        //метод для загрузки файла на сервер
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync(IFormFile file, CancellationToken cancellationToken) //токен для поддержки прерываемых операций
        {
            //существует ли файл
            if (file == null || file.Length == 0)
                return BadRequest("Файл пустой");

            //проверка на расширение файла
            var allowedExtensions = new[] { ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Допустимы только файлы с расширением .xls");

            //проверка на существование файла с таким именем в бд
            if ((await _balanceSheetRepository.GetFilesAsync(cancellationToken)).
                    Select(uf => uf.FileName).
                    Contains(file.FileName)) 
                //если файл уже записан -> удаляем все записи из файла и данные о файле
                await _balanceSheetRepository.DeleteFileReportAsync(file.FileName, cancellationToken);

            //считываем файл в потоке
            using var stream = file.OpenReadStream();

            //парсер, который как раз и преобразовывает данные из потока в обьект отчета
            var report = _parser.Parse(stream);
            report.FileName = file.FileName;

            //валидация входных данных перед записью в бд
            _validator.Validate(report);

            //сохраняем данные в бд
            await _dbSaver.SaveAsync(report, cancellationToken);

            return Ok();
        }

        //метод для получения списка загруженных файлов
        [HttpGet("files")]
        public async Task<IActionResult> GetFiles(CancellationToken cancellationToken)
        {
            //получаем список файлов и возваращем его
            var files = await _balanceSheetRepository.GetFilesAsync(cancellationToken);
            return Ok(files);
        }

        //данные, записанные из определнного файла
        [HttpGet("files/{fileName}")] //имя файла
        public async Task<IActionResult> GetReportFromFile([FromRoute]string fileName, CancellationToken cancellationToken)
        {
            //получаем данные и возвращаем их
            var report = await _balanceSheetRepository.GetReportByFileAsync(fileName, cancellationToken);
            return Ok(report);
        }

    }
}
