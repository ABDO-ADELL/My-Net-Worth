using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class BaseController : Controller
{
    protected string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
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