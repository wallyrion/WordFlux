

namespace WordFlux.ApiService.Endpoints;

public static class ImagesEndpoints
{
    public static WebApplication MapImagesEndpoints(this WebApplication app)
    {
        app.MapGet("images", async (BingImageSearchService searchService, UnsplashImageSearchService unsplashSearch, string keyword, bool isUnsplash = true) =>
            {
                if (isUnsplash)
                {
                    return await unsplashSearch.GetImagesByKeyword(keyword);
                }

                return await searchService.GetImagesByKeyword(keyword);
            })
            .CacheOutput(p =>
            {
                p.AddPolicy<OutputCachePolicy>();
                p.Expire(TimeSpan.FromHours(2));
            });
        return app;
    }
}