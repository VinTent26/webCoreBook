using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

public class SetLoginStatusFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Kiểm tra nếu có token trong session, tức là đã đăng nhập
        var isLoggedIn = !string.IsNullOrEmpty(context.HttpContext.Session.GetString("UserToken"));
        // Lưu trữ trạng thái đăng nhập vào HttpContext
        context.HttpContext.Items["IsLoggedIn"] = isLoggedIn;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Không cần thực hiện gì ở đây
    }
}
