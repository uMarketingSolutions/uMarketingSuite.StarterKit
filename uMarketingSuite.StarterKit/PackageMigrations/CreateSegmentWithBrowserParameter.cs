using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using uMarketingSuite.Business.Events;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;

namespace uMarketingSuite.StarterKit.PackageMigrations;

public class CreateSegmentWithBrowserParameter : MigrationBase
{
    private readonly ILogger<CreateSegmentWithBrowserParameter> _logger;

    public CreateSegmentWithBrowserParameter(
        IMigrationContext context, 
        ILogger<CreateSegmentWithBrowserParameter> logger) 
        : base(context)
    {
        _logger = logger;
    }

    protected override void Migrate()
    {
        // Insert into two tables
        // uMarketingSuitePersonalizationSegment = uMarketingSuite.Data.Constants.Personalization.TableName.Segment;
        // uMarketingSuitePersonalizationSegmentRule = uMarketingSuite.Data.Constants.Personalization.TableName.SegmentRule;
    
        // Id of the Firefox segment.
        const long SEGMENT_ID = 1000;
        const long SEGMENT_RULE_ID = 1000;
        
        if (TableExists(Data.Constants.Personalization.TableName.Segment) == false &&
            TableExists(Data.Constants.Personalization.TableName.SegmentRule) == false)
        {
            _logger.LogError("Unable to find the uMarketingSuite DB tables to insert dummy data");
            return;
        }
        
        var time = DateTime.UtcNow;
        Insert.IntoTable(Data.Constants.Personalization.TableName.Segment)
            .EnableIdentityInsert()
            .Row(new
            {
                id = SEGMENT_ID, 
                name = "Firefox visitors", 
                description = "Targets all visitors using the Firefox browser.",
                created = time,
                createdByUmbracoUserId = -1,
                updated = time,
                updatedByUmbracoUserId = -1,
                endTime = (DateTime?) null,
                isTemporary = false,
                sortOrder = 0,
                controlGroupSize = 0.00
            })
            .Do();
        
        Insert.IntoTable(Data.Constants.Personalization.TableName.SegmentRule)
            .EnableIdentityInsert()
            .Row(new
            {
                id = SEGMENT_RULE_ID,
                segmentId = SEGMENT_ID,
                type = "Browser",
                config = "{\"Browsers\":[\"Firefox\"]}",
                created = time,
                updated = (DateTime?) null,
                isNegation = false
            })
            .Do();
        
        // Refresh UMS segment cache
        SystemEventService.Raise(new SegmentSavingEvent(SEGMENT_ID));
    }
}