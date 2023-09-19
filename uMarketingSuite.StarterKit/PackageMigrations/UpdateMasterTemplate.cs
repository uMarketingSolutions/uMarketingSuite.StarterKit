using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;

namespace uMarketingSuite.StarterKit.PackageMigrations;

public class UpdateMasterTemplate : MigrationBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<UpdateMasterTemplate> _logger;

    public UpdateMasterTemplate(
        IMigrationContext context, 
        IFileService fileService, 
        ILogger<UpdateMasterTemplate> logger) 
        : base(context)
    {
        _fileService = fileService;
        _logger = logger;
    }

    protected override void Migrate()
    {
        // Try & find the 'master' template installed from TheStarterKit
        var masterTemplate = _fileService.GetTemplate("master");
        if (masterTemplate == null)
        {
            _logger.LogError("Unable to find the 'master' template. Please ensure you have installed Umbraco.TheStarterKit nuget package.");
            return;
        }
        
        // Get the existing template file contents
        var fileContents = masterTemplate.Content;

        if (string.IsNullOrWhiteSpace(fileContents))
        {
            _logger.LogError("The template with alias master is empty");
            return;
        }
        
        // Find the position of the closing </body> tag
        var positionOfVBodyTag = fileContents.IndexOf("</body>", StringComparison.InvariantCultureIgnoreCase);

        // Only update the template if we found the closing </body> tag
        if (positionOfVBodyTag < 0)
        {
            _logger.LogError("Unable to find the closing </body> tag in the template with alias master");
            return;
        }
        
        // Check for existance of 'uMarketingSuite Cockpit' which is part of a comment
        if (fileContents.Contains("uMarketingSuite Cockpit"))
        {
            _logger.LogError("We have already added uMarketingSuite Cockpit");
            return;
        }
        
        // Includes the TAB character as well to line up the HTML with the rest of the template
        var newContent = new StringBuilder();
        newContent.AppendLine("");
        newContent.AppendLine("    @* uMarketingSuite Clientside tracking (Track time on page, scroll depth etc) *@");
        newContent.AppendLine("    <script src=\"/Assets/uMarketingSuite/Scripts/uMarketingSuite.analytics.js\"></script>");
        newContent.AppendLine("");

        newContent.AppendLine("    @*");
        newContent.AppendLine("    uMarketingSuite Cockpit: Displayed only if you are logged into backoffice");
        newContent.AppendLine("    Displays debugging information to help understand the data collected");
        newContent.AppendLine("    *@");
        newContent.AppendLine("    <partial name=\"Partials/uMarketingSuite/Cockpit\"/>");
        newContent.AppendLine("");
            
        // Update the existing content and insert our updates before closing </body> tag
        masterTemplate.Content = fileContents.Insert(positionOfVBodyTag, newContent.ToString());    
        
        // Update the file contents with our new content
        // Save the template with updated content
        _fileService.SaveTemplate(masterTemplate);
    }

}