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

//��������� �������� ���� ������
builder.Services.AddDbContext<BalanceSheetsDbContext>(options =>
{
    //��������� ��������� + ������ ����������� (�� ������������)
    options.UseNpgsql(configuration.GetConnectionString("BalanceSheetsDb"));
});

//������������ ������� � DI ���������� (�������� ����� �����, ���������� � ����������)
builder.Services.AddScoped<IBalanceSheetRepository, BalanceSheetRepository>();
builder.Services.AddScoped<IDbSaverService, DbSaverService>();
builder.Services.AddScoped<IExcelParserService, ExcelParserService>();
builder.Services.AddScoped<IValidationService, ValidatingService>();

//���������� cors ��� ������ � �������
builder.Services.AddCors(options =>
{
    //���� ���������
    options.AddPolicy("AllowAllOrigins",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

//swagger ��� ������������
builder.Services.AddSwaggerGen();

//��������� ������������ + ��������� ������������ (�������� ���������� ������������ + ��������)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true; 
    });


var app = builder.Build();
//���������� ���������� ������ ��� �������
app.UseMiddleware<ExceptionMiddleware>();

//��������� �������� ������
app.UseCors("AllowAllOrigins");

//��� https
app.UseHttpsRedirection();

//��� ������������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//��������� ��������� ������������
app.MapControllers();

//��������� ��������
app.UseRouting();


//������
app.Run();
