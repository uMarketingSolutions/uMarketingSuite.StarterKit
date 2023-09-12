using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Infrastructure.Packaging;

namespace uMarketingSuite.StarterKit.PackageMigrations;

public class CustomPackageMigrationPlan : PackageMigrationPlan
{
    public CustomPackageMigrationPlan() : base("uMarketingSuite.The-Starter-Kit")
    {
    }
    
    protected override void DefinePlan()
    {
        // TODO: Check how do we know our Package Migration plan is gonna run
        // After uMarketingSuite and TheStarterKit packages?
        
        From(InitialState)
            .To<UpdateMasterTemplate>(new Guid("16F3CA63-E362-416E-8F20-5CDFEFCFA43F"));
    }
}