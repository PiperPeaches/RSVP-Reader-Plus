using SmartReader;
using System.Threading.Tasks;
using System;

namespace rsvpreader.Services;

public class ArticleService
{
    public async Task<(string Title, string[] Words)> GetArticleAsync(string url)
    {
        var Reader = new Reader(url);
        var artical = await Reader.GetArticleAsync();

        if(artical.IsReadable)
        {
            var words = artical.TextContent
                .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            return (artical.Title, words);
        }
        return ("Error", new[] { "Could", "not", "extract", "content."});
    }
}