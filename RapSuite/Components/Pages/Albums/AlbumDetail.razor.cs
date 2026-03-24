using Microsoft.AspNetCore.Components;
using RapSuite.Domain.Music;
using RapSuite.Infrastructure.Firebase;
using RapSuite.Infrastructure.Session;

namespace RapSuite.Components.Pages.Albums;

public partial class AlbumDetail
{
    [Inject] private IFirestoreService Firestore { get; set; } = default!;
    [Inject] private UserSessionService Session { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter]
    public string AlbumId { get; set; } = string.Empty;

    private Album? _album;
    private List<Song> _songs = new();
    private bool _isLoading = true;

    // Expand/Edit state
    private string? _expandedSongId;
    private string? _editingSongId;
    private string _editLyrics = string.Empty;

    // Delete state
    private bool _showDeleteConfirm;
    private Song? _deletingSong;
    private bool _isDeleting;

    protected override async Task OnInitializedAsync()
    {
        if (!Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        _isLoading = true;

        if (Session.UserId != null && Session.IdToken != null)
        {
            _album = await Firestore.GetAlbumAsync(Session.UserId, AlbumId, Session.IdToken);
            _songs = await Firestore.GetSongsAsync(Session.UserId, AlbumId, Session.IdToken);
        }

        _isLoading = false;
    }

    private void GoBack()
    {
        Navigation.NavigateTo("/albums");
    }

    private void ToggleSongExpand(string songId)
    {
        _expandedSongId = _expandedSongId == songId ? null : songId;
        _editingSongId = null;
    }

    private void StartEdit(Song song)
    {
        _editingSongId = song.Id;
        _editLyrics = song.Lyrics;
    }

    private void CancelEdit()
    {
        _editingSongId = null;
        _editLyrics = string.Empty;
    }

    private async Task SaveSongEdit(Song song)
    {
        if (Session.UserId == null || Session.IdToken == null) return;

        song.Lyrics = _editLyrics;
        song.WordCount = _editLyrics.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

        var minutes = song.WordCount / 140.0;
        var mins = (int)minutes;
        var secs = (int)((minutes - mins) * 60);
        song.EstimatedDuration = $"{mins}:{secs:D2}";

        await Firestore.UpdateSongAsync(Session.UserId, AlbumId, song, Session.IdToken);
        _editingSongId = null;
        StateHasChanged();
    }

    private void ConfirmDeleteSong(Song song)
    {
        _deletingSong = song;
        _showDeleteConfirm = true;
    }

    private async Task DeleteSong()
    {
        if (_deletingSong == null || Session.UserId == null || Session.IdToken == null) return;

        _isDeleting = true;

        try
        {
            await Firestore.DeleteSongAsync(Session.UserId, AlbumId, _deletingSong.Id, Session.IdToken);
            _showDeleteConfirm = false;
            _songs.Remove(_deletingSong);
            _deletingSong = null;
        }
        finally
        {
            _isDeleting = false;
        }
    }

    private static string TruncateLyrics(string lyrics, int maxLength)
    {
        if (string.IsNullOrEmpty(lyrics) || lyrics.Length <= maxLength)
            return lyrics;
        return lyrics[..maxLength] + "...";
    }
}
