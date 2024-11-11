

using WordFlux.ApiService.Caching;
using WordFlux.Infrastructure.ImageSearch;

namespace WordFlux.ApiService.Endpoints;

public static class ImagesEndpoints
{
    public static WebApplication MapImagesEndpoints(this WebApplication app)
    {
        app.MapGet("images", async (BingImageSearchService bingSearch, UnsplashImageSearchService unsplashSearch, string keyword, bool useBing = false) =>
            {
                if (useBing)
                {
                    return await bingSearch.GetImagesByKeyword(keyword);
                }

                return await unsplashSearch.GetImagesByKeyword(keyword);

            })
            .CacheOutput(p =>
            {
                p.AddPolicy<OutputCachePolicy>();
                p.Expire(TimeSpan.FromHours(2));
            });
        return app;
    }
}