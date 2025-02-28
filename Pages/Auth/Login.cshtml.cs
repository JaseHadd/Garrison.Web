using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Garrison.Web.Pages.Auth;

[ValidateAntiForgeryToken]
public class LoginModel(ILogger<LoginModel> logger, IUserManager userManager) : PageModel
{
    private readonly ILogger<LoginModel> _logger = logger;
    private readonly IUserManager _userManager = userManager;

    [BindProperty, Required, StringLength(32), MinLength(3)]
    public string? UserName { get; set; }

    [BindProperty, Required, DataType(DataType.Password)]
    public string? Password { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return RedirectToPage();
        
        try
        {
            await _userManager.SignIn(HttpContext, UserName!, Password!);
            return RedirectToPage("/Profile/Index");
        }
        catch (Exception e)
        {
            ModelState.AddModelError("summary", e.Message);
            return RedirectToPage();
        }
    }
}