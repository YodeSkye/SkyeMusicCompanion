
using System.Collections.ObjectModel;
using System.Text.Json;
using SkyeMusicCompanion.Models;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class PlaylistPage : ContentPage
{
    private readonly CompanionConnection _connection;

    // Full playlist snapshot
    private readonly List<PlaylistItem> AllItems = [];

    // Current search text
    private string _currentSearchText = string.Empty;

    // Items bound to the UI
    public ObservableCollection<PlaylistItem> Items { get; } = [];

    private bool _hasLoadedOnce;

    public PlaylistPage()
    {
        InitializeComponent();

        _connection = App.Connection;
        PlaylistView.ItemsSource = Items;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Only listen for the playlist ONCE
        _connection.PlaylistReceived += OnPlaylistJsonReceived;
        _connection.PlaylistCleared += OnPlaylistCleared;
        _connection.Disconnected += OnDisconnected;

        if (!_hasLoadedOnce && _connection.IsConnected)
        {
            _hasLoadedOnce = true;
            _connection.RequestPlaylist();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _connection.PlaylistReceived -= OnPlaylistJsonReceived;
        _connection.PlaylistCleared -= OnPlaylistCleared;
        _connection.Disconnected -= OnDisconnected;
    }

    private void OnPlaylistJsonReceived(string json)
    {
        try
        {
            var list = JsonSerializer.Deserialize<List<PlaylistItem>>(json);
            if (list == null)
                return;

            AllItems.Clear();
            AllItems.AddRange(list);

            ApplyFilter(_currentSearchText);
        }
        catch (Exception ex)
        {
            Log.Write("Playlist JSON error: " + ex.Message);
        }
    }

    private void OnPlaylistCleared()
    {
        Items.Clear();
        AllItems.Clear();
        SearchBox.Text = string.Empty;
        _currentSearchText = string.Empty;
        _hasLoadedOnce = false;
    }

    private async void OnDisconnected()
    {
        if (Shell.Current.CurrentPage is PlaylistPage)
            await Shell.Current.GoToAsync("//MainPage");

        Items.Clear();
        AllItems.Clear();
        SearchBox.Text = string.Empty;
        _currentSearchText = string.Empty;
        _hasLoadedOnce = false;
    }

    private void OnItemTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is PlaylistItem item)
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
    }
}
