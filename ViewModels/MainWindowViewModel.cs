namespace RSVP.ViewModels;

using System.Net;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rsvpreader.Services;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly System.Timers.Timer _timer = new();

    [ObservableProperty]
    private string _url = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private string[] _words = Array.Empty<string>();

    [ObservableProperty]
    private string _currentWord = "Ready...";
    private int _currentIndex = 0;

    public MainWindowViewModel(ArticleService articleService) : base(articleService)
    {

    }

    [RelayCommand]
    public async Task LoadArticle()
    {
        try
        {
            ErrorMessage = "";

            var result = await _articleService.GetArticleAsync(Url);
            if (result.Words != null && result.Words.Length > 0)
            {
                Words = result.Words;
                if (Words.Length > 0)
                {
                    _currentIndex = 0;
                    CurrentWord = Words[_currentIndex];
                }
            }
            else
            {
                ErrorMessage = "No Words found >:3 me eated them!!";
            }
        }
        catch(Exception ex)
        {
            ErrorMessage = "Error: " + ex.Message;
        }

    }
    [RelayCommand]
    public void TogglePlay()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
        }
        else
        {
            _timer.Interval = 300;
            _timer.AutoReset = true; 
            _timer.Elapsed += (s, e) => 
            {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => NextWord());  
                };
            _timer.Start();
        }
    }

    [RelayCommand]
    public void NextWord()
    {
        if (Words != null && _currentIndex < Words.Length - 1)
        {
            _currentIndex++;
            CurrentWord = Words[_currentIndex];
        }
        else
        {
            _timer.Stop();
        }
    }
}
