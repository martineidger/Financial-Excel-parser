using B1_2.DB;
using B1_2.DB.Repositories;
using B1_2.DB.Repositories.Abstractions;
using B1_2.Middlewares;
using B1_2.Services;
using B1_2.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

//добавляем контекст базы данных
builder.Services.AddDbContext<BalanceSheetsDbContext>(options =>
{
    //указываем провайдер + строку подключения (из конфигурации)
    options.UseNpgsql(configuration.GetConnectionString("BalanceSheetsDb"));
});

//регистрируем сервисы в DI контейнере (указывем время жизни, интерфейсы и реализации)
builder.Services.AddScoped<IBalanceSheetRepository, BalanceSheetRepository>();
builder.Services.AddScoped<IDbSaverService, DbSaverService>();
builder.Services.AddScoped<IExcelParserService, ExcelParserService>();
builder.Services.AddScoped<IValidationService, ValidatingService>();

//подключаем cors для работы с фронтом
builder.Services.AddCors(options =>
{
    //всем разрешено
    options.AddPolicy("AllowAllOrigins",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

//swagger для тестирования
builder.Services.AddSwaggerGen();

//поддержка контроллеров + настройка сериализации (избежать циклическх зависимостей + удобство)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true; 
    });


var app = builder.Build();
//глоабльный обработчик ошибок как мидлвер
app.UseMiddleware<ExceptionMiddleware>();

//добавляем политику корсов
app.UseCors("AllowAllOrigins");

//для https
app.UseHttpsRedirection();

//для тестирования
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//добавляем обработку контроллеров
app.MapControllers();

//поддержка роутинга
app.UseRouting();


//запуск
app.Run();
