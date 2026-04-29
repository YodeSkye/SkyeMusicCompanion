
using System.Collections.ObjectModel;
using System.Text.Json;
using SkyeMusicCompanion.Models;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class PlaylistPage : ContentPage
{
    private readonly CompanionConnection _connection;

    public ObservableCollection<PlaylistItem> Items { get; } = new();

    public PlaylistPage()
    {
        InitializeComponent();

        _connection = App.Connection;

        PlaylistView.ItemsSource = Items;

        _connection.PlaylistReceived += OnPlaylistJsonReceived;
        _connection.PlaylistChanged += OnPlaylistChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _connection.RequestPlaylist();

    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _connection.PlaylistReceived -= OnPlaylistJsonReceived;
        _connection.PlaylistChanged -= OnPlaylistChanged;
       
    }

    private void OnPlaylistChanged()
    {
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

                Items.Clear();
                foreach (var item in list)
                    Items.Add(item);

            }
            catch (Exception ex)
            {
                Log.Write("Playlist JSON error: " + ex.Message);
            }
        });
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is PlaylistItem item)
        {
            _connection.PlayPath(item.Path);
        }
    }
}
