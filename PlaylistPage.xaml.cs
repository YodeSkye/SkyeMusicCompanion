
using System.Text.Json;
using SkyeMusicCompanion.Models;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class PlaylistPage : ContentPage
{
    private readonly CompanionConnection _connection;
    private readonly List<PlaylistItem> _allItems = [];
    private string _currentSearchText = string.Empty;
    CancellationTokenSource? _searchCts;
    private bool _hasLoadedOnce;
    private bool _pendingPlaylistRefresh;

    public PlaylistPage()
    {
        InitializeComponent();

        _connection = App.Connection;

        _connection.PlaylistChanged += OnPlaylistChanged;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _connection.PlaylistReceived -= OnPlaylistJsonReceived;
        _connection.PlaylistCleared -= OnPlaylistCleared;
        _connection.Disconnected -= OnDisconnected;
        _connection.NowPlayingReceived -= OnNowPlayingReceived;

        _connection.PlaylistReceived += OnPlaylistJsonReceived;
        _connection.PlaylistCleared += OnPlaylistCleared;
        _connection.Disconnected += OnDisconnected;
        _connection.NowPlayingReceived += OnNowPlayingReceived;

        // handle pending refresh BEFORE anything else
        if (_pendingPlaylistRefresh)
        {
            _pendingPlaylistRefresh = false;
            _connection.RequestPlaylist();
            return; // stop here — playlist will arrive momentarily
        }

        // Load playlist once
        if (!_hasLoadedOnce && _connection.IsConnected)
        {
            _hasLoadedOnce = true;
            _connection.RequestPlaylist();
            return;
        }

        // Highlight + scroll
        if (_connection.now.Path != null)
        {
            UpdateHighlight(_connection.now.Path);
            ScrollToCurrent(_connection.now.Path);
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _connection.PlaylistReceived -= OnPlaylistJsonReceived;
        _connection.PlaylistCleared -= OnPlaylistCleared;
        _connection.Disconnected -= OnDisconnected;
        _connection.NowPlayingReceived -= OnNowPlayingReceived;

    }

    private void OnPlaylistJsonReceived(string json)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<PlaylistItem>>(json);
                if (list == null)
                    return;

                _allItems.Clear();
                _allItems.AddRange(list);

                ApplyFilter(_currentSearchText);

                UpdateHighlight(_connection.now.Path);
                ScrollToCurrent(_connection.now.Path);
            }
            catch (Exception ex)
            {
                Log.Write("Playlist JSON error: " + ex.Message);
            }
        });
    }
    private void OnPlaylistCleared()
    {
        _allItems.Clear();
        PlaylistView.ItemsSource = null;
        SearchBox.Text = string.Empty;
        _currentSearchText = string.Empty;
        _hasLoadedOnce = false;
    }
    private async void OnDisconnected()
    {
        if (Shell.Current.CurrentPage is PlaylistPage)
            await Shell.Current.GoToAsync("//MainPage");

        _allItems.Clear();
        PlaylistView.ItemsSource = null;
        SearchBox.Text = string.Empty;
        _currentSearchText = string.Empty;
        _hasLoadedOnce = false;
    }
    private void OnNowPlayingReceived()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateHighlight(_connection.now.Path);
            ScrollToCurrent(_connection.now.Path);
        });
    }
    private void OnPlaylistChanged()
    {
        if (Shell.Current.CurrentPage is PlaylistPage)
        {
            _connection.RequestPlaylist();
        }
        else
        {
            _pendingPlaylistRefresh = true;
        }
    }

    private void OnItemTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is PlaylistItem item)
            _connection.PlayPath(item.Path);
    }
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue ?? string.Empty;

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        Task.Delay(120, token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _currentSearchText = text;
                ApplyFilter(text);
            });
        });
    }

    private void ApplyFilter(string text)
    {
        IEnumerable<PlaylistItem> source = _allItems;

        if (!string.IsNullOrWhiteSpace(text))
        {
            var lower = text.ToLowerInvariant();

            source = _allItems.Where(item =>
                (item.Title?.Contains(lower, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (item.Path?.Contains(lower, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // Build a new list and swap ItemsSource in one shot
        var viewList = source.ToList();
        PlaylistView.ItemsSource = viewList;

        // After filtering, re-scroll to current if visible
        if (!string.IsNullOrEmpty(_connection.now.Path))
        {
            Dispatcher.Dispatch(async () =>
            {
                await Task.Delay(50);
                ScrollToCurrent(_connection.now.Path);
            });
        }
    }
    public void UpdateHighlight(string currentPath)
    {
        //Log.Write("Highlighting: " + currentPath);

        foreach (var item in _allItems)
        {
            item.IsCurrent = (item.Path == currentPath);
            //if (item.Path == currentPath)
                //Log.Write("MATCH FOUND: " + item.Title);
        }

        ApplyFilter(_currentSearchText);
    }
    private void ScrollToCurrent(string path)
    {
        if (PlaylistView.ItemsSource is not IList<PlaylistItem> visibleList)
            return;

        var index = -1;

        for (int i = 0; i < visibleList.Count; i++)
        {
            if (visibleList[i].Path == path)
            {
                index = i;
                break;
            }
        }

        if (index < 0)
            return;

        Dispatcher.Dispatch(async () =>
        {
            await Task.Delay(50);
            PlaylistView.ScrollTo(index, position: ScrollToPosition.Center, animate: false);
        });
    }

}
