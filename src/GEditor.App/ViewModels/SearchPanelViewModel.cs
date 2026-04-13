using GEditor.Core.Buffer;
using GEditor.Core.Documents;
using GEditor.Core.Search;
using System.Windows.Input;

namespace GEditor.App.ViewModels;

/// <summary>
/// 搜索面板 ViewModel
/// </summary>
public sealed class SearchPanelViewModel : ViewModelBase
{
    private readonly ISearchService _searchService;
    private readonly Document _document;
    private readonly EditorBuffer _buffer;

    private string _searchText = string.Empty;
    private string _replaceText = string.Empty;
    private bool _matchCase;
    private bool _wholeWord;
    private bool _useRegex;
    private bool _isVisible;
    private bool _showReplacePanel;

    private SearchMatch? _currentMatch;
    private IReadOnlyList<SearchMatch> _allMatches = Array.Empty<SearchMatch>();
    private int _currentMatchIndex = -1;
    private int _totalMatches;

    public SearchPanelViewModel(ISearchService searchService, Document document)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _buffer = document.Buffer;

        FindNextCommand = new RelayCommand(FindNext, () => !string.IsNullOrEmpty(SearchText));
        FindPreviousCommand = new RelayCommand(FindPrevious, () => !string.IsNullOrEmpty(SearchText));
        ReplaceCommand = new RelayCommand(ReplaceCurrent, () => _currentMatch != null);
        ReplaceAllCommand = new RelayCommand(ReplaceAll, () => !string.IsNullOrEmpty(SearchText));
        CloseCommand = new RelayCommand(() => IsVisible = false);
        ToggleReplaceCommand = new RelayCommand(() => ShowReplacePanel = !ShowReplacePanel);
    }

    #region 属性

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                UpdateMatches();
            }
        }
    }

    public string ReplaceText
    {
        get => _replaceText;
        set => SetProperty(ref _replaceText, value);
    }

    public bool MatchCase
    {
        get => _matchCase;
        set
        {
            if (SetProperty(ref _matchCase, value))
            {
                UpdateMatches();
            }
        }
    }

    public bool WholeWord
    {
        get => _wholeWord;
        set
        {
            if (SetProperty(ref _wholeWord, value))
            {
                UpdateMatches();
            }
        }
    }

    public bool UseRegex
    {
        get => _useRegex;
        set
        {
            if (SetProperty(ref _useRegex, value))
            {
                UpdateMatches();
            }
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public bool ShowReplacePanel
    {
        get => _showReplacePanel;
        set => SetProperty(ref _showReplacePanel, value);
    }

    public int CurrentMatchIndex
    {
        get => _currentMatchIndex;
        private set => SetProperty(ref _currentMatchIndex, value);
    }

    public int TotalMatches
    {
        get => _totalMatches;
        private set => SetProperty(ref _totalMatches, value);
    }

    public string MatchInfo => TotalMatches > 0 ? $"{CurrentMatchIndex + 1} / {TotalMatches}" : "无匹配";

    public SearchMatch? CurrentMatch
    {
        get => _currentMatch;
        private set
        {
            _currentMatch = value;
            OnPropertyChanged(nameof(CurrentMatch));
        }
    }

    #endregion

    #region 命令

    public ICommand FindNextCommand { get; }
    public ICommand FindPreviousCommand { get; }
    public ICommand ReplaceCommand { get; }
    public ICommand ReplaceAllCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ToggleReplaceCommand { get; }

    #endregion

    #region 方法

    private void UpdateMatches()
    {
        if (string.IsNullOrEmpty(SearchText))
        {
            _allMatches = Array.Empty<SearchMatch>();
            CurrentMatchIndex = -1;
            TotalMatches = 0;
            CurrentMatch = null;
            OnPropertyChanged(nameof(MatchInfo));
            return;
        }

        var query = SearchQuery.Create(SearchText, new SearchOptions
        {
            MatchCase = MatchCase,
            WholeWord = WholeWord,
            UseRegex = UseRegex
        });

        _allMatches = _searchService.FindAll(_buffer, query);
        TotalMatches = _allMatches.Count;
        OnPropertyChanged(nameof(MatchInfo));

        if (_allMatches.Count > 0)
        {
            CurrentMatchIndex = 0;
            CurrentMatch = _allMatches[0];
        }
        else
        {
            CurrentMatchIndex = -1;
            CurrentMatch = null;
        }
    }

    public void FindNext()
    {
        if (_allMatches.Count == 0) return;

        CurrentMatchIndex = (CurrentMatchIndex + 1) % _allMatches.Count;
        CurrentMatch = _allMatches[CurrentMatchIndex];
        OnPropertyChanged(nameof(MatchInfo));
    }

    public void FindPrevious()
    {
        if (_allMatches.Count == 0) return;

        CurrentMatchIndex = CurrentMatchIndex <= 0 ? _allMatches.Count - 1 : CurrentMatchIndex - 1;
        CurrentMatch = _allMatches[CurrentMatchIndex];
        OnPropertyChanged(nameof(MatchInfo));
    }

    private void ReplaceCurrent()
    {
        if (CurrentMatch == null) return;

        var query = SearchQuery.Create(SearchText, new SearchOptions
        {
            MatchCase = MatchCase,
            WholeWord = WholeWord,
            UseRegex = UseRegex
        });

        var command = new Core.Editing.ReplaceTextCommand(
            CurrentMatch.Line,
            CurrentMatch.Column,
            CurrentMatch.Length,
            CurrentMatch.MatchedText,
            ReplaceText);

        _document.ExecuteCommand(command);
        UpdateMatches();
    }

    private void ReplaceAll()
    {
        if (string.IsNullOrEmpty(SearchText)) return;

        var query = SearchQuery.Create(SearchText, new SearchOptions
        {
            MatchCase = MatchCase,
            WholeWord = WholeWord,
            UseRegex = UseRegex
        });

        var command = _searchService.CreateReplaceAllCommand(_buffer, query, ReplaceText);
        _document.ExecuteCommand(command);
        UpdateMatches();
    }

    public void Show()
    {
        IsVisible = true;
        ShowReplacePanel = false;
    }

    public void ShowWithReplace()
    {
        IsVisible = true;
        ShowReplacePanel = true;
    }

    #endregion
}
