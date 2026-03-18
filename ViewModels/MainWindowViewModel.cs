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
using AngleSharp.Io;

public partial class MainWindowViewModel : ViewModelBase
{
    public event Action<int>? RequestHighlightWord;
    public event Action<string[]>? RequestPopulateArticle;
    public event Action<int>? RequestScrollToWord;
    private DispatcherTimer? _timer;
    private int _currentIndex = 0;

    public int CurrentIndex => _currentIndex;
    [ObservableProperty]
    public string _wordsOf = "Words " + 0 + "/" + 0;

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
    private string _articleContent = "No Article Avalible";
    [ObservableProperty]
    private string _currentWord = "Paste in a link to begin!";
    public MainWindowViewModel(ArticleService articleService) : base(articleService)
    {
    }

    [RelayCommand]
    public void JumpToWord(int index)
    {
        if (Words != null && index >= 0 && index < Words.Length)
        {
            _currentIndex = index;
            CurrentWord = Words[_currentIndex];
        }
    }

    [RelayCommand]
    public void ResetRsvp()
    {
        _currentIndex = 0;
        if (Words != null && Words.Length > 0)
        {
            CurrentWord = Words[0];
            RequestHighlightWord?.Invoke(0);
            UpdateProgessString();

        }
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
            RequestPopulateArticle?.Invoke(Words);
            RequestHighlightWord?.Invoke(_currentIndex);
            UpdateProgessString();
        }
    }

    [RelayCommand]
    public async Task LoadArticle()
    {        
        try
        {

            ArticleTitle = "Loading...";
            CurrentWord = "Indexing...";
            ArticleContent = "No Article Loaded";
            
            var result = await _articleService.GetArticleAsync(Url);

            ErrorMessage = "";
            
            if (result.Words != null && result.Words.Length > 0)
            {
                ArticleTitle = "RSVP Reading | " + result.Title ?? "Unknown Title";
                ArticleContent = string.Join(" ", result.Words);
                Words = result.Words;
                _currentIndex = 0;
                CurrentWord = Words[0];
                UpdateProgessString();

                RequestPopulateArticle?.Invoke(Words);
                RequestHighlightWord?.Invoke(_currentIndex);
            }
        }
        catch(Exception ex)
        {
            ErrorMessage = "Error: " + ex.Message;
            CurrentWord = "No Article Found :/";
            ArticleTitle = "Error Loading Title";
            ArticleContent = "Failed to load content.";
        }
    }
    private void UpdateProgessString()
    {
        int total = Words?.Length ?? 0;

        WordsOf = $"Words {_currentIndex+1}/{total}";
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
                DispatcherPriority.Normal,
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
            UpdateProgessString();


            if (IsShowingFullArticle)
            {
                RequestHighlightWord?.Invoke(_currentIndex);
            }
        }
    }


}
