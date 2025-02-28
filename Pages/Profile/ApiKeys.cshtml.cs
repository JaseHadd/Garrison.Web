using Garrison.Lib;
using Garrison.Lib.Models;

namespace Garrison.Web.Pages.Profile;

public class ApiKeysModel(GarrisonContext dbContext) : ProfileModel(dbContext)
{
    protected override  string[]        IncludedProperties  => [nameof(User.ApiKeys)];
    public              List<ApiKey>    ApiKeys             => User.ApiKeys;
}
