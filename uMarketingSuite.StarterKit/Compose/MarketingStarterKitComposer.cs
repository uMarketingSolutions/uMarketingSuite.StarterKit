using uMarketingSuite.Common.Composing;
using uMarketingSuite.StarterKit.DataGeneration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace uMarketingSuite.StarterKit.Compose;

// This Composer from uMarketingSuite - is running the components
// Which then contains one for setting up the DB stuff
[ComposeAfter(typeof(AttributeBasedComposer))]
public class MarketingStarterKitComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Register the IPageviewDataGenerator in DI
        builder.Services.AddUnique<IPageviewDataGenerator, PageviewDataGenerator>();
        
        // Register the component - which triggers the migrations
        builder.Components().Append<MarketingStarterKitComponent>();
    }
}