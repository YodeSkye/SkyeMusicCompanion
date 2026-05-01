
using System.Collections.ObjectModel;
using System.Text.Json;
using SkyeMusicCompanion.Models;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class PlaylistPage : ContentPage
{
    private readonly CompanionConnection _connection;

    private readonly List<PlaylistItem> AllItems = [];
    private string _currentSearchText = string.Empty;
    public ObservableCollection<PlaylistItem> Items { get; } = [];

    public PlaylistPage()
    {
        InitializeComponent();

        _connection = App.Connection;

        PlaylistView.ItemsSource = Items;

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _connection.PlaylistReceived += OnPlaylistJsonReceived;
        _connection.PlaylistChanged += OnPlaylistChanged;
        _connection.Disconnected += OnDisconnected;
        _connection.NowPlayingReceived += OnNowPlayingReceived;
        _connection.PlaylistCleared += OnPlaylistCleared;

        if (_connection.IsConnected)
        {
            _connection.RequestPlaylist();
        }

    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _connection.PlaylistReceived -= OnPlaylistJsonReceived;
        _connection.PlaylistChanged -= OnPlaylistChanged;
        _connection.Disconnected -= OnDisconnected;
        _connection.NowPlayingReceived -= OnNowPlayingReceived;

    }

    private void OnPlaylistChanged()
    {
        if (_connection.IsConnected)
            _connection.RequestPlaylist();
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

                AllItems.Clear();
                AllItems.AddRange(list);

                ApplyFilter(_currentSearchText);
                SelectCurrentNowPlaying();
            }
            catch (Exception ex)
            {
                Log.Write("Playlist JSON error: " + ex.Message);
            }
        });
    }
    private void OnPlaylistCleared()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Items.Clear();
            AllItems.Clear();
            SearchBox.Text = string.Empty;
            _currentSearchText = string.Empty;
        });
    }
    private void OnDisconnected()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // If user is currently on PlaylistPage, go home
            if (Shell.Current.CurrentPage is PlaylistPage)
                Shell.Current.GoToAsync("//MainPage");
            // Clear stale playlist so it won't show if user returns
            Items.Clear();
            AllItems.Clear();
            SearchBox.Text = string.Empty;
            _currentSearchText = string.Empty;
        });
    }
    private void OnNowPlayingReceived()
    {
        SelectCurrentNowPlaying();
    }

    private void OnItemTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not PlaylistItem item)
            return;

        PlaylistView.SelectedItem = item;

        _connection.PlayPath(item.Path);
    }
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _currentSearchText = e.NewTextValue ?? string.Empty;
        ApplyFilter(_currentSearchText);
    }

    private void ApplyFilter(string text)
    {
        Items.Clear();

        if (string.IsNullOrWhiteSpace(text))
        {
            foreach (var item in AllItems)
                Items.Add(item);
        }
        else
        {
            text = text.ToLowerInvariant();

            foreach (var item in AllItems)
            {
                if ((item.Title?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (item.Path?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    Items.Add(item);
                }
            }
        }

        SelectCurrentNowPlaying();
    }
    private void SelectCurrentNowPlaying()
    {
        var now = App.Connection.now;

        // If no NowPlaying, clear selection
        if (string.IsNullOrWhiteSpace(now.Path))
        {
            PlaylistView.SelectedItem = null;
            return;
        }

        // Try to find the current song in the *visible* list
        var match = Items.FirstOrDefault(i => i.Path == now.Path);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // If not found → clear selection
            PlaylistView.SelectedItem = match;
        });
    }

}
