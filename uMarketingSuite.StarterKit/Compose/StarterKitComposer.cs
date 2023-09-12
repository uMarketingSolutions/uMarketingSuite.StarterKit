using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace uMarketingSuite.StarterKit.Compose;

public class StarterKitComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // They are typescanned and registered automatically
        //builder.PackageMigrationPlans().Add<>()
    }
}