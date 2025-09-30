using System.ComponentModel.DataAnnotations;
using System.Net;

namespace B1_2.Middlewares
{
    //глобальный обработчик ошибок -> стоит в начале контейнера и ловит все ошибки (отдает с нужным кодом)
    public class ExceptionMiddleware
    {
        //ссылка на следующий компонент в конвейере обработки
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //главный метод, кторый вызывается когда конвейер вызывает этот компонент
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                //все запросы будут проходить через это try, т.о. все ошибки будут пробрасываться сюда
                await _next(context);
            }
            //ошибка операции
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HandleException(context, ex);
            }
            //базовый класс ошибки -> все ошибки обработаются 
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await HandleException(context, ex);
            }

        }
        //метод для обработки ошибки
        private async Task HandleException(HttpContext context, Exception ex)
        {

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                Details = ex.ToString()
            });
        }
    }
}
