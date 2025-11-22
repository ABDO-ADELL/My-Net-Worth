using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PRISM;
using PRISM.Models.Authmodels;
using System.Security.Claims;

public class BaseController : Controller
{
    protected readonly AppDbContext _context;
    protected AppUser CurrentUser;
    public BaseController(AppDbContext context)
    {
        _context = context;
    }
    protected string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != null)
        {
            CurrentUser = _context.Users.Find(userId);
        }

        base.OnActionExecuting(context);
    }

    protected void SetUserId<T>(T entity) where T : class
    {
        var userIdProperty = entity.GetType().GetProperty("UserId");
        if (userIdProperty != null && userIdProperty.CanWrite)
        {
            userIdProperty.SetValue(entity, GetCurrentUserId());
        }
    }
}