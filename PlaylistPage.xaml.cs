
using System.Text.Json;
using SkyeMusicCompanion.Models;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class PlaylistPage : ContentPage
{
    private readonly CompanionConnection _connection;
    private readonly List<PlaylistItem> _allItems = [];
    private string _currentSearchText = string.Empty;
    private bool _hasLoadedOnce;
    CancellationTokenSource? _searchCts;

    public PlaylistPage()
    {
        InitializeComponent();

        _connection = App.Connection;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Prevent double-subscribe
        _connection.PlaylistReceived -= OnPlaylistJsonReceived;
        _connection.PlaylistCleared -= OnPlaylistCleared;
        _connection.Disconnected -= OnDisconnected;
        _connection.NowPlayingReceived -= OnNowPlayingReceived;

        _connection.PlaylistReceived += OnPlaylistJsonReceived;
        _connection.PlaylistCleared += OnPlaylistCleared;
        _connection.Disconnected += OnDisconnected;
        _connection.NowPlayingReceived += OnNowPlayingReceived;

        if (!_hasLoadedOnce && _connection.IsConnected)
        {
            _hasLoadedOnce = true;
            _connection.RequestPlaylist();
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Always detach to prevent ghost events
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
        var index = _allItems.FindIndex(i => i.Path == path);
        if (index < 0)
            return;

        PlaylistView.ScrollTo(index, position: ScrollToPosition.Center, animate: false);
    }

}
