using System.Security.Claims;

public interface IUserContextService
{
    string? UserId { get; }
}

public class UserContextService : IUserContextService
{
    public string? UserId { get; }

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        UserId = httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
