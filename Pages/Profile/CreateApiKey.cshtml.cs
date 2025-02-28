using Garrison.Lib;
using Garrison.Lib.Models;
using Microsoft.AspNetCore.Mvc;

namespace Garrison.Web.Pages.Profile;

public class CreateApiKeyModel(GarrisonContext dbContext) : ProfileModel(dbContext)
{
    [BindProperty]
    public ApiKey ApiKey { get; set; }

    public string NewKey { get; } = ApiKey.GenerateKey();

    public override void OnGetPartial()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Console.WriteLine("PENIS");
        var key = new ApiKey();

        await TryUpdateModelAsync(key, nameof(ApiKey), k => k.Name, k => k.Key);
        ModelState.Remove("ApiKey.Owner");
        key.Owner = GetUser();

        if (ModelState.IsValid)
        {
            DbContext.Add(key);
            await DbContext.SaveChangesAsync();
            return RedirectToPage("/Profile/ApiKeys"); 
        }

        Console.WriteLine(key.Key);

        return Page();
    }
}
