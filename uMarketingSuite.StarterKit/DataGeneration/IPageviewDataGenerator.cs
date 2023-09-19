using uMarketingSuite.Business.Analytics.Processed;

namespace uMarketingSuite.StarterKit.DataGeneration;

public interface IPageviewDataGenerator
{
    IEnumerable<IPageview> GeneratePageviews(DateTime from, DateTime to, int amount);
}