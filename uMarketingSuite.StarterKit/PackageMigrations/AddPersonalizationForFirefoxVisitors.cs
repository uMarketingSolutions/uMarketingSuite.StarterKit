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

public class AddPersonalizationForFirefoxVisitors : MigrationBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IContentService _contentService;
    private readonly ILogger<AddPersonalizationForFirefoxVisitors> _logger;

    public AddPersonalizationForFirefoxVisitors(
        IMigrationContext context, 
        ILocalizationService localizationService, 
        IContentService contentService, 
        ILogger<AddPersonalizationForFirefoxVisitors> logger) 
        : base(context)
    {
        _localizationService = localizationService;
        _contentService = contentService;
        _logger = logger;
    }

    protected override void Migrate()
    {
        // Activate personalization on whatever the root document of the current website is.
        // As we are installing into the StarterKit, we know that we have published content at the root
        var rootContentTypeId = _contentService.GetRootContent()?.FirstOrDefault()?.ContentTypeId;
        var defaultLanguage = _localizationService.GetDefaultLanguageIsoCode();

        // Ids used for entries
        const long PERSONALIZATION_ID = 1000;
        const long PERSONALIZATION_CONTENT_TYPE_ID = 1000;

        // Firefox segment id (created in CreateSegmentWithBrowserParameter).
        const long SEGMENT_ID = 1000;

        if (TableExists(Data.Constants.Personalization.TableName.AppliedPersonalization) == false &&
            TableExists(Data.Constants.Personalization.TableName.PersonalizationContentType) == false)
        {
            _logger.LogError("Unable to find the uMarketingSuite DB tables to insert dummy data");
            return;
        }
        
        Insert.IntoTable(Data.Constants.Personalization.TableName.AppliedPersonalization)
            .EnableIdentityInsert()
            .Row(new
            {
                id = PERSONALIZATION_ID,
                segmentId = SEGMENT_ID,
                type = 2,
                name = "Personalize Firefox visitors",
                description = "Shows an alert to Firefox visitors",
                css = (string?)null,
                javascript = "alert(\"Hello Firefox visitor\");",
                started = (DateTime?)null,
                isActive = true,
                created = DateTime.UtcNow,
                createdByUmbracoUserId = -1,
                updated = (DateTime?)null,
                updatedByUmbracoUserId = (int?)null,
                invalid = false
            })
            .Do();

        if (rootContentTypeId.HasValue)
        {
            Insert.IntoTable(Data.Constants.Personalization.TableName.PersonalizationContentType)
                .EnableIdentityInsert()
                .Row(new
                {
                    id = PERSONALIZATION_CONTENT_TYPE_ID,
                    personalizationId = PERSONALIZATION_ID,
                    contentTypeId = rootContentTypeId,
                    culture = defaultLanguage
                })
                .Do();
            
            // Refresh UMS cache
            SystemEventService.Raise(new AppliedPersonalizationSavingEvent());
        }
        
    }
}