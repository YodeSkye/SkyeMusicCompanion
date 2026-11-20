using System.Collections.ObjectModel;
using SkyeMusicCompanion.Services;

namespace SkyeMusicCompanion;

public partial class SettingsPage : ContentPage
{
    private readonly ObservableCollection<ServerProfile> _profiles = [];
    private ServerProfile? _currentProfile;
    private bool _isLoadingUi;
    private bool _isSaving;

    public SettingsPage()
    {
        InitializeComponent();

        ProfilePicker.ItemsSource = _profiles;
        LoadProfilesIntoPicker();

        TapActionPicker.SelectedIndex = Settings.PlaylistTapAction == TapAction.Play ? 0 : 1;
        KeepAwakeSwitch.IsToggled = Settings.KeepScreenAwake;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        SaveCurrentProfileFields();
        await App.Connection.ReconnectAsync();
    }

    private void LoadProfilesIntoPicker(string? selectId = null)
    {
        _isLoadingUi = true;

        var loaded = Settings.GetProfiles();
        var active = Settings.GetActiveProfile();
        string targetId = selectId ?? active.Id;

        _profiles.Clear();
        foreach (var item in loaded)
            _profiles.Add(item);

        var selected = _profiles.FirstOrDefault(p => p.Id == targetId) ?? _profiles.First();
        ProfilePicker.SelectedItem = selected;
        BindProfileToInputs(selected);

        _isLoadingUi = false;
    }

    private void BindProfileToInputs(ServerProfile profile)
    {
        _currentProfile = profile.Clone();
        Settings.SetActiveProfileId(profile.Id);

        ProfileNameEntry.Text = _currentProfile.Name;
        HostEntry.Text = _currentProfile.Host;
        HostPortEntry.Text = _currentProfile.HostPort.ToString();
        StreamPortEntry.Text = _currentProfile.StreamPort.ToString();
    }

    private void OnProfileSelectionChanged(object sender, EventArgs e)
    {
        if (_isLoadingUi || _isSaving)
            return;

        if (ProfilePicker.SelectedItem is not ServerProfile chosen)
            return;

        // Save current edits before switching to the newly chosen profile
        SaveCurrentProfileFields();

        _isLoadingUi = true;
        Settings.SetActiveProfileId(chosen.Id);
        var fresh = Settings.GetProfiles().FirstOrDefault(p => p.Id == chosen.Id) ?? chosen;
        BindProfileToInputs(fresh);
        _isLoadingUi = false;

        Log.Write($"SETTINGS: Active profile switched to '{fresh.Name}' ({fresh.Host}:{fresh.HostPort}/{fresh.StreamPort})");
    }

    private void OnProfileFieldChanged(object sender, EventArgs e)
    {
        if (_isLoadingUi || _isSaving)
            return;

        SaveCurrentProfileFields();
    }

    private void SaveCurrentProfileFields()
    {
        if (_currentProfile == null || _isSaving)
            return;

        _isSaving = true;
        try
        {
            string name = ProfileNameEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
                name = "Unnamed Profile";

            string cleanHost = NormalizeHost(HostEntry.Text?.Trim() ?? string.Empty);
            int hostPort = ParsePort(HostPortEntry.Text, fallback: 5050);
            int streamPort = ParsePort(StreamPortEntry.Text, fallback: 5051);

            _currentProfile.Name = name;
            _currentProfile.Host = cleanHost;
            _currentProfile.HostPort = hostPort;
            _currentProfile.StreamPort = streamPort;

            Settings.UpsertProfile(_currentProfile);

            // Update the matching item in the existing collection without rebuilding ItemsSource
            var existingInList = _profiles.FirstOrDefault(p => p.Id == _currentProfile.Id);
            if (existingInList != null)
            {
                existingInList.Name = _currentProfile.Name;
                existingInList.Host = _currentProfile.Host;
                existingInList.HostPort = _currentProfile.HostPort;
                existingInList.StreamPort = _currentProfile.StreamPort;
            }

            _isLoadingUi = true;
            if (ProfileNameEntry.Text != _currentProfile.Name)
                ProfileNameEntry.Text = _currentProfile.Name;
            if (HostEntry.Text != _currentProfile.Host)
                HostEntry.Text = _currentProfile.Host;
            if (HostPortEntry.Text != _currentProfile.HostPort.ToString())
                HostPortEntry.Text = _currentProfile.HostPort.ToString();
            if (StreamPortEntry.Text != _currentProfile.StreamPort.ToString())
                StreamPortEntry.Text = _currentProfile.StreamPort.ToString();
            _isLoadingUi = false;

            Log.Write($"SETTINGS: Saved profile '{_currentProfile.Name}' -> {_currentProfile.Host} (Host:{_currentProfile.HostPort}, Stream:{_currentProfile.StreamPort})");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private static string NormalizeHost(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "192.168.1.1";

        if (Uri.TryCreate(input, UriKind.Absolute, out Uri? uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return uri.Host;
        }

        int slashIndex = input.IndexOf('/');
        if (slashIndex >= 0)
            input = input[..slashIndex];

        return input.Trim();
    }

    private static int ParsePort(string? text, int fallback)
    {
        if (int.TryParse(text?.Trim(), out int port) && port is >= 1 and <= 65535)
            return port;

        return fallback;
    }

    private async void OnNewProfileClicked(object sender, EventArgs e)
    {
        SaveCurrentProfileFields();

        string? name = await DisplayPromptAsync("New Profile", "Profile name:", "Create", "Cancel", placeholder: "e.g., Laptop / Backup");
        if (string.IsNullOrWhiteSpace(name))
            return;

        string? host = await DisplayPromptAsync("Server Host", "IP address or domain:", "Create", "Cancel", initialValue: _currentProfile?.Host ?? "192.168.1.1");
        if (string.IsNullOrWhiteSpace(host))
            return;

        var newProfile = new ServerProfile
        {
            Name = name.Trim(),
            Host = NormalizeHost(host.Trim()),
            HostPort = _currentProfile?.HostPort ?? 5050,
            StreamPort = _currentProfile?.StreamPort ?? 5051
        };

        Settings.UpsertProfile(newProfile);
        LoadProfilesIntoPicker(newProfile.Id);
        Log.Write($"SETTINGS: Created profile '{newProfile.Name}'");
    }

    private async void OnDeleteProfileClicked(object sender, EventArgs e)
    {
        if (_currentProfile == null)
            return;

        if (_profiles.Count <= 1)
        {
            await DisplayAlertAsync("Cannot Delete", "At least one connection profile must remain.", "OK");
            return;
        }

        bool confirm = await DisplayAlertAsync(
            "Delete Profile",
            $"Delete profile '{_currentProfile.Name}'?",
            "Delete",
            "Cancel"
        );

        if (!confirm)
            return;

        string deletedId = _currentProfile.Id;
        Settings.DeleteProfile(deletedId);
        LoadProfilesIntoPicker();
        Log.Write($"SETTINGS: Deleted profile ID {deletedId}");
    }

    private void OnTapActionChanged(object sender, EventArgs e)
    {
        if (_isLoadingUi)
            return;

        Settings.PlaylistTapAction =
            TapActionPicker.SelectedIndex == 0 ? TapAction.Play : TapAction.Queue;

        Log.Write("SETTINGS: PlaylistTapAction = " + Settings.PlaylistTapAction);
    }

    private void OnKeepAwakeToggled(object sender, ToggledEventArgs e)
    {
        if (_isLoadingUi)
            return;

        Settings.KeepScreenAwake = e.Value;
        ScreenBehaviorManager.ApplyBehavior();
        Log.Write("SETTINGS: KeepScreenAwake toggled to " + e.Value);
    }

    private async void OnDeleteLogFileClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            "Delete Log File",
            "This will permanently delete the Companion log file. Continue?",
            "Delete",
            "Cancel"
        );

        if (!confirm)
            return;

        try
        {
            Log.Clear();
            await DisplayAlertAsync("Done", "Log file deleted.", "OK");
        }
        catch (Exception ex)
        {
            Log.Write("DeleteLogFile error: " + ex);
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}