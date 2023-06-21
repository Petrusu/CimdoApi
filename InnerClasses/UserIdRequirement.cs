using Microsoft.AspNetCore.Authorization;

namespace CimdoApi.InnerClasses;

public class UserIdRequirement :IAuthorizationRequirement
{
    public int UserId { get; }

    public UserIdRequirement(int userId)
    {
        UserId = userId;
    }
}