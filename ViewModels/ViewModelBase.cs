using CommunityToolkit.Mvvm.ComponentModel;
using rsvpreader.Services;

namespace RSVP.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected readonly ArticleService _articleService;

    public ViewModelBase(ArticleService articleService)
    {
        _articleService = articleService;
    }
}
