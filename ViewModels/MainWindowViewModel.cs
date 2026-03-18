namespace RSVP.ViewModels;

using Avalonia.Threading;
using System.Net;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rsvpreader.Services;
using System;
using System.Security.Cryptography.X509Certificates;
using Avalonia.Metadata;

public partial class MainWindowViewModel : ViewModelBase
{
    // private readonly System.Timers.Timer _timer = new();
    private DispatcherTimer? _timer;
    private int _currentIndex = 0;

    [ObservableProperty]
    private string _articleTitle = "RSVP Reader Plus";
    [ObservableProperty]
    private string _url = "";
    [ObservableProperty]
    private string _errorMessage = "";
    [ObservableProperty]
    private int _wpm = 300;
    private double WpmToMilliseconds(int wpm) => 60000.0/wpm;
    [ObservableProperty]
    private string[] _words = Array.Empty<string>();
    [ObservableProperty]
    private bool _isShowingFullArticle = false;
    [ObservableProperty]
    private string _articleContent = "";
    [ObservableProperty]
    private string _currentWord = "Paste in a link to begin!";
    public MainWindowViewModel(ArticleService articleService) : base(articleService)
    {
    }

    [RelayCommand]
    public void ToggleFullArticle()
    {
        if (_timer?.IsEnabled == true)
        {
            TogglePlay();
        }
        IsShowingFullArticle = !IsShowingFullArticle;
    }

    [RelayCommand]
    public void OpenFullArticle()
    {
        if (Words != null)
        {
            ArticleContent = String.Join(" ", Words);

            ToggleFullArticle();
        }
    }

    [RelayCommand]
    public async Task ResetRsvp()
    {
        _currentIndex = 0;
    }

    [RelayCommand]
    public async Task LoadArticle()
    {        
        try
        {
            var result = await _articleService.GetArticleAsync(Url);

            ArticleTitle = "Loading...";
            CurrentWord = "Indexing...";
            ArticleContent = "No Article Loaded";

            ErrorMessage = "";
            
            if (result.Words != null && result.Words.Length > 0)
            {
                ArticleTitle = "RSVP Reading | " + result.Title ?? "Unknown Title";
                ArticleContent = string.Join(" ", result.Words);
                Words = result.Words;

                if (Words.Length > 0)
                {
                    _currentIndex = 0;
                    CurrentWord = Words[_currentIndex];
                }
            }
        }
        catch(Exception ex)
        {
            ErrorMessage = "Error: " + ex.Message;
            ArticleTitle = "Errir Loading Title";
        }
    }

    partial void OnWpmChanged(int value)
    {
        if (_timer != null)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(WpmToMilliseconds(value));
        }
    }

    [RelayCommand]
    public void TogglePlay()
    {
        if (_timer?.IsEnabled == true)
        {
            _timer.Stop();
        }
        else
        {
            _timer ??= new DispatcherTimer(
                TimeSpan.FromMilliseconds(300),
                DispatcherPriority.Render,
                (s, e) => NextWord()
            );

            _timer.Tick -= Timer_Tick;
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }
    }

    private void Timer_Tick(object? sender, EventArgs e) => NextWord();

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
            _timer?.Stop();
        }
    }
}
