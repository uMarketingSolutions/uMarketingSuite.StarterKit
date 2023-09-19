using System.Text;
using uMarketingSuite.Business.AbTesting;
using uMarketingSuite.Business.Analytics.Goals;
using uMarketingSuite.Business.Analytics.Processed;
using uMarketingSuite.Business.Analytics.Processing.Extractors;
using uMarketingSuite.Business.Personalization.AppliedPersonalizations;
using uMarketingSuite.Data.Analytics.Collection.Pageview;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace uMarketingSuite.StarterKit.DataGeneration;

public class PageviewDataGenerator : IPageviewDataGenerator
{
    private static readonly Random _rng = new Random();
    private const int SECONDS_PER_DAY = 24 * 60 * 60;
    
    private static readonly Lazy<bool> _lazyFalse = new Lazy<bool>(() => false);
    private static readonly IEnumerable<AbTestVariant> _emptyAbTestVariants = Array.Empty<AbTestVariant>();
    private static readonly Lazy<AppliedPersonalization> _lazyPersonalization = new Lazy<AppliedPersonalization>(() => null);
    private static readonly Lazy<List<PageviewSegment>> _lazySegments = new Lazy<List<PageviewSegment>>(() => new List<PageviewSegment>());
    private static readonly Lazy<List<IPersonaScore>> _lazyPersonaScores = new Lazy<List<IPersonaScore>>(() => new List<IPersonaScore>());
    private static readonly Lazy<List<ICustomerJourneyStepScore>> _lazyCjStepScore = new Lazy<List<ICustomerJourneyStepScore>>(() => new List<ICustomerJourneyStepScore>());
    private static readonly Lazy<List<IGoalCompletion>> _lazyGoalCompletions = new Lazy<List<IGoalCompletion>>(() => new List<IGoalCompletion>());
    
    private readonly IProfilingLogger _profilingLogger;
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly IRawPageviewSessionExtractor _rawPageviewSessionExtractor;
    private readonly IRawPageviewIpAddressExtractor _rawPageviewIpAddressExtractor;

    public PageviewDataGenerator(
        IProfilingLogger profilingLogger,
        IUmbracoContextFactory umbracoContextFactory,
        IVariationContextAccessor variationContextAccessor,
        ILocalizationService localizationService,
        IRawPageviewSessionExtractor rawPageviewSessionExtractor,
        IRawPageviewIpAddressExtractor rawPageviewIpAddressExtractor)
    {
        _profilingLogger = profilingLogger;
        _umbracoContextFactory = umbracoContextFactory;
        _variationContextAccessor = variationContextAccessor;
        _localizationService = localizationService;
        _rawPageviewSessionExtractor = rawPageviewSessionExtractor;
        _rawPageviewIpAddressExtractor = rawPageviewIpAddressExtractor;
    }
    
    public IEnumerable<IPageview> GeneratePageviews(DateTime from, DateTime to, int amount)
    {
        using (_profilingLogger.DebugDuration<PageviewDataGenerator>(
                       "Generate PageViews for uMarketingSuite",
                       "Finished Generating PageViews for uMarketingSuite"))
        {
            // Calculate a starting pageviews amount
            var avgPageviewsPerDay = amount / (to - from).TotalDays;

            var pages = GetAllPages();

            // Iterate through the days in the range (From -> To)
            for (DateTime dt = from; dt <= to; dt = dt.AddDays(1))
            {
                // Multiply avg pageviews by anywhere between 25% and 175%.
                // Expected value remains at 100% but we have some variance per day.
                var factor = _rng.Next(25, 175 + 1) / 100f;
                var pageviewAmount = (long)Math.Round(factor * avgPageviewsPerDay);
                if (pageviewAmount <= 0) continue;

                var timestampDelta = SECONDS_PER_DAY / pageviewAmount;

                // For each number of random pageviews we want to insert
                // Generate a random browser and device type pageview
                for (int i = 0; i < pageviewAmount; i++)
                {
                    // A random number between 0 and 99
                    var roll = _rng.Next(100);

                    // Pick a random browser and device type
                    var browser = Pick(_weightedBrowsers, roll);
                    var deviceType = Pick(_weightedDeviceTypes, roll);

                    // TODO: Perhaps generate a fixed set of visitor ids so 1 visitor can have multiple pageviews
                    var visitorId = Guid.NewGuid();
                    var userAgent = GetBrowserUserAgent(browser, deviceType);

                    var timestamp = dt.AddSeconds(i * timestampDelta);

                    var ipAddress = GenerateRandomIpv4Address();

                    var (url, page, umbPageVariant) = pages[roll % pages.Length];

                    yield return GeneratePageview(timestamp, visitorId, url, page, umbPageVariant, ipAddress, userAgent);
                }
            }
        }
    }
    
    public enum Browser { Chrome, Edge, Firefox }

    public enum DeviceType { Desktop, Mobile }
    
    private static readonly (Browser browser, int p)[] _weightedBrowsers = new[]
    {
        (Browser.Chrome, 60),
        (Browser.Edge, 90),
        (Browser.Firefox, 100),
    };

    private static readonly (DeviceType device, int p)[] _weightedDeviceTypes = new[]
    {
        (DeviceType.Desktop, 50),
        (DeviceType.Mobile, 100),
    };

    


    private static T Pick<T>((T item, int p)[] items, int roll)
    {
        return items.First(ip => roll < ip.p).item;
    }
    
    private (string url, IPage page, IUmbracoPageVariant umbPageVariant)[] GetAllPages()
    {
        using (_profilingLogger.DebugDuration<PageviewDataGenerator>(
                   "GetAllPages in CMS for uMarketingSuite PageView generation",
                   "Finished GetAllPages in CMS for uMarketingSuite PageView generation"))
        {
            using (var ctxRef = _umbracoContextFactory.EnsureUmbracoContext())
            {
                // If the variation context is null, we need to set it to the default language
                if (_variationContextAccessor.VariationContext == null)
                {
                    var defaultLangIso = _localizationService.GetDefaultLanguageIsoCode();
                    _variationContextAccessor.VariationContext = new VariationContext(defaultLangIso);
                }
            
                var culture = _variationContextAccessor.VariationContext.Culture;
                        
                if (ctxRef.UmbracoContext.Content == null)
                    return Array.Empty<(string url, IPage page, IUmbracoPageVariant umbPageVariant)>();
            
                // From the content root get all descendants
                // Could be expensive in larger sites - but this is a small starter kit
                return ctxRef.UmbracoContext.Content
                    .GetAtRoot(false)
                    .SelectMany(c => c.DescendantsOrSelf())
                    .Select(ipContent =>
                    {
                        var url = ipContent.Url(mode: UrlMode.Absolute);
                    
                        // Check we have an absolute url and validates as a valid Uri
                        if (string.IsNullOrWhiteSpace(url)) return default;
                        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return default;
                    
                        // uMarketingSuite Page
                        IPage page = new Page(uri.Scheme, uri.Authority, uri.AbsolutePath, uri.Query, uri.Port, true);
                    
                        // Variant of the page
                        IUmbracoPageVariant umbVariant = new UmbracoPageVariant(ipContent.Id, culture, null, ipContent.ContentType.Id);
                    
                        return (url, page, umbVariant);
                    })
                    .Where(t => t != default)
                    .ToArray();
            }
        }
    }
    
    private string GenerateRandomIpv4Address()
    {
        // An IP4 address is made up of 4 parts
        // Each part is a number between 0 and 255
        var ip = new StringBuilder();
        
        // For each part, generate a random number between 0 and 255
        for (var i = 0; i < 4; i++)
        {
            var octet = _rng.Next(256);
            
            ip.Append(octet);
            if (i < 3) ip.Append('.');
        }
        return ip.ToString();
    }
    
    private static string GetBrowserUserAgent(Browser browser, DeviceType deviceType)
    {
        switch (browser)
        {
            case Browser.Chrome:
                switch (deviceType)
                {
                    case DeviceType.Desktop: return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.67 Safari/537.36";
                    case DeviceType.Mobile: return "Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.61 Mobile Safari/537.36";
                }
                break;

            case Browser.Edge:
                switch (deviceType)
                {
                    case DeviceType.Desktop: return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.67 Safari/537.36 Edg/100.0.1185.39";
                    case DeviceType.Mobile: return "Mozilla/5.0 (Linux; Android 10; HD1913) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.61 Mobile Safari/537.36 EdgA/100.0.1185.50";
                }
                break;

            case Browser.Firefox:
                switch (deviceType)
                {
                    case DeviceType.Desktop: return "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:100.0) Gecko/20100101 Firefox/100.0";
                    case DeviceType.Mobile: return "Mozilla/5.0 (Android 12; Mobile; rv:68.0) Gecko/68.0 Firefox/100.0";
                }
                break;
        }

        throw new InvalidOperationException($"No user agent available for {browser} on {deviceType}");
    }
    
    private IPageview GeneratePageview(DateTime timestamp, Guid visitorId, string url, IPage page, IUmbracoPageVariant umbPageVariant, string ipAddress, string userAgent)
    {
        var rpv = new RawPageview
        {
            ExternalId = visitorId,
            UserAgent = userAgent,
            Headers = null,
            StatusCode = 200,
            Timestamp = timestamp,
            Url = url,
            IpAddress = ipAddress,
            PageviewGuid = Guid.NewGuid(),
        };

        var session = _rawPageviewSessionExtractor.Extract(rpv);
        var ip = _rawPageviewIpAddressExtractor.Extract(rpv);

        return new Pageview(
            default, rpv.PageviewGuid.Value, timestamp, session, ip, page, null, null,
            Array.Empty<ISearchQuery>(), umbPageVariant, null, _lazyFalse, _emptyAbTestVariants, _lazyPersonalization,
            _lazySegments, _lazyPersonaScores, _lazyCjStepScore, _lazyGoalCompletions);
    }
}