using uMarketingSuite.Business.Analytics.Processed;
using uMarketingSuite.StarterKit.DataGeneration;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace uMarketingSuite.StarterKit.PackageMigrations;

public class AddPageViewData : MigrationBase
{
    private readonly IProfilingLogger _profilingLogger;
    private readonly IPageviewDataGenerator _dataGenerator;
    private readonly IPageviewService _pageviewService;

    public AddPageViewData(
        IMigrationContext context,
        IProfilingLogger profilingLogger,
        IPageviewDataGenerator dataGenerator,
        IPageviewService  pageviewService) 
        : base(context)
    {
        _profilingLogger = profilingLogger;
        _dataGenerator = dataGenerator;
        _pageviewService = pageviewService;
    }

    protected override void Migrate()
    {
     
        // Add to MiniProfiler & Logs about the duration...
        //_profilingLogger.DebugDuration<AddPageViewData>("")
        
        var from = DateTime.Now.AddMonths(-3);
        var to = DateTime.Now;
        
        // We would like to add way more pageviews (~ 300K)
        // but inserting just 10K already takes multiple minutes
        // and we cannot really go higher at the moment.
        //int amount = 10000; // 10K
        int amount = 1000; // 1k

        // Generate the dummy data we want to insert
        // Profile logging is done inside this method to track the various parts to generate the dummy data
        var pageviews = _dataGenerator.GeneratePageviews(from, to, amount).ToArray();
        
        using (_profilingLogger.DebugDuration<AddPageViewData>("Inserting dummy PageView data into uMarketingSuite with PageViewService"))
        {
            // Bulk insert the collected pageview data into uMarketingSuite
            _pageviewService.Insert(pageviews);
        }
    }
}