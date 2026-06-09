using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace LabSto.Filters;

public class CustomActionFilterAttribute : Attribute, IActionFilter
{
    private readonly ILogger<CustomActionFilterAttribute> _logger;

    public CustomActionFilterAttribute(ILogger<CustomActionFilterAttribute> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.ActionDescriptor.RouteValues["controller"];
        var actionName = context.ActionDescriptor.RouteValues["action"];
        var parameters = context.ActionArguments.Count > 0
            ? Newtonsoft.Json.JsonConvert.SerializeObject(context.ActionArguments)
            : "无";
        _logger.LogInformation($"[Action执行前] {controllerName}/{actionName}，参数：{parameters}");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var controllerName = context.ActionDescriptor.RouteValues["controller"];
        var actionName = context.ActionDescriptor.RouteValues["action"];

        if (context.Exception != null)
        {
            _logger.LogError(context.Exception,
                $"[Action执行异常] {controllerName}/{actionName}，异常：{context.Exception.Message}");
            return;
        }

        var result = context.Result switch
        {
            ViewResult vr => $"View({vr.ViewName ?? "默认"})",
            RedirectToActionResult rta => $"RedirectTo({rta.ActionName},{rta.ControllerName})",
            RedirectResult r => $"Redirect({r.Url})",
            ContentResult cr => $"Content({cr.ContentType})",
            EmptyResult => "Empty",
            _ => context.Result?.GetType().Name ?? "未知"
        };

        _logger.LogInformation($"[Action执行后] {controllerName}/{actionName}，结果：{result}");
    }
}
