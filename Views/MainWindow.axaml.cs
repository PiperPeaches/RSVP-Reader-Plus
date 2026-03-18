using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Documents;
using RSVP.ViewModels;
using System.Collections.Generic;

namespace RSVP.Views;

public partial class MainWindow : Window
{
    public bool _isInternalUpdate = false;
    private int _lastIndex = -1;
    public MainWindow()
    {
        InitializeComponent();
        var listBox = this.FindControl<ListBox>("ArticleListBox");
        if (listBox != null)
        {
            listBox.SelectionChanged += ArticleListBox_SelectionChanged;
        }
        
        DataContextChanged += (s, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.RequestHighlightWord += SyncListBox;
            }
        };
    }

    private void ArticleListBox_SelectionChanged(object? sender ,SelectionChangedEventArgs e)
    {
        if (_isInternalUpdate) return;

        if (sender is ListBox lb && lb.SelectedIndex != -1)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.JumpToWordCommand.Execute(lb.SelectedIndex);
            }
        }
    }
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.RequestPopulateArticle += PopulateArticle;
            vm.RequestHighlightWord += HighlightWord;
        }
    }

    private void SyncListBox(int index)
    {
        var listBox = this.FindControl<ListBox>("ArticleListBox");
        if (listBox == null) return;

        _isInternalUpdate = true;
        listBox.SelectedIndex = index;
        listBox.ScrollIntoView(index);
        _isInternalUpdate = false;
    }

    private void PopulateArticle(string[] words)
    {
        var textBlock = this.FindControl<SelectableTextBlock>("FullArticleText");
        if (textBlock == null) return;

        var inlines = new InlineCollection();
        foreach (var word in words)
        {
            inlines.Add(new Run(word + " "));
        }
        textBlock.Inlines = inlines;
    }

    private void HighlightWord(int index)
    {
        var textBlock = this.FindControl<SelectableTextBlock>("FullArticleText");
        if (textBlock?.Inlines == null || index < 0 || index >= textBlock.Inlines.Count) return;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_lastIndex >= 0 && _lastIndex < textBlock.Inlines.Count)
            {
                if (textBlock.Inlines[_lastIndex] is Run oldRun)
                {
                    oldRun.Background = Brushes.Transparent;
                    oldRun.Foreground = Brushes.White;                 
                }
            }
            if (textBlock.Inlines[index] is Run newRun)
            {
                newRun.Background = Brushes.Gold;
                newRun.Foreground = Brushes.Black;

                textBlock.SelectionStart = index;
                textBlock.SelectionEnd = index;

            }
            _lastIndex = index;
        }, Avalonia.Threading.DispatcherPriority.Background);
    }
}