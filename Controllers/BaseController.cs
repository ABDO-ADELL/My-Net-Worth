using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PRISM.DataAccess;
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
            // Load user with their businesses
            CurrentUser = _context.Users
                .Include(u => u.Business)
                .FirstOrDefault(u => u.Id == userId);
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

    // Helper method to get user's business IDs
    protected List<int> GetUserBusinessIds()
    {
        if (CurrentUser?.Business != null && CurrentUser.Business.Any())
        {
            return CurrentUser.Business.Select(b => b.BusinessId).ToList();
        }
        return new List<int>();
    }
}


