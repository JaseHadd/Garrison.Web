using System.Security.Claims;
using Garrison.Lib;
using Garrison.Lib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Garrison.Web.Pages.Profile;

[Authorize]
public abstract class ProfileModel(GarrisonContext dbContext) : PageModel
{
    protected new       User            User   => _user!;
    protected           uint            UserId => Convert.ToUInt32(base.User.FindFirstValue(ClaimTypes.NameIdentifier));
    protected           GarrisonContext DbContext => _dbContext;
    protected virtual   string[]        IncludedProperties { get; } = [];

    private User?           _user;
    private GarrisonContext _dbContext = dbContext;

    public void OnGet()
    {
        _user ??= GetUser();
        OnGetPartial();
    }

    [NonHandler]
    public virtual void OnGetPartial() { }

    protected User GetUser()
    {
        var enumerable = _dbContext.Users
            .Where(u => u.Id == UserId);

        foreach (var property in IncludedProperties)
            enumerable = enumerable.Include(property);

        return enumerable.ElementAt(0);
    }
}
