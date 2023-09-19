using uMarketingSuite.Common.Composing;
using uMarketingSuite.StarterKit.PackageMigrations;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Scoping;
namespace uMarketingSuite.StarterKit.Compose;

public class MarketingStarterKitComponent : IComponent
{
    private readonly IMigrationPlanExecutor _migrationPlanExecutor;
    private readonly IScopeProvider _scopeProvider;
    private readonly IKeyValueService _keyValueService;

    public MarketingStarterKitComponent(
        IMigrationPlanExecutor migrationPlanExecutor,
        IScopeProvider scopeProvider,
        IKeyValueService keyValueService)
    {
        _migrationPlanExecutor = migrationPlanExecutor;
        _scopeProvider = scopeProvider;;
        _keyValueService = keyValueService;
    }
    
    public void Initialize()
    {
        // Once we have composed after uMarketingSuite
        // We can run the migrations to install demo data
        var plan = new MigrationPlan("uMarketingSuite.The-Starter-Kit");
        plan.From(string.Empty)
            .To<UpdateMasterTemplate>(new Guid("16F3CA63-E362-416E-8F20-5CDFEFCFA43F"))
            .To<CreateSegmentWithBrowserParameter>(new Guid("F3E52742-9CAF-4EC1-8431-A0677584C0BF"))
            .To<AddPersonalizationForFirefoxVisitors>(new Guid("EBBFD6BF-C731-4EC7-B3CC-D05A641D1983"))
            .To<AddPageViewData>(new Guid("AF2BAC07-5333-46A4-92A9-5C3675D45970"));
        
        var upgrader = new Upgrader(plan);
        upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
    }

    public void Terminate()
    {
    }
}