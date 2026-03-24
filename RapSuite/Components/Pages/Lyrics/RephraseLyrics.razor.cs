using Microsoft.AspNetCore.Components;
using RapSuite.Domain.Lyrics;
using RapSuite.Domain.Music;
using RapSuite.Infrastructure.AI;
using RapSuite.Infrastructure.Firebase;
using RapSuite.Infrastructure.Session;

namespace RapSuite.Components.Pages.Lyrics;

public partial class RephraseLyrics
{
    [Inject] private ILyricsAiService AiService { get; set; } = default!;
    [Inject] private IFirestoreService Firestore { get; set; } = default!;
    [Inject] private UserSessionService Session { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private string _originalLyrics = string.Empty;
    private string _language = "English";
    private string _mood = "Energetic";
    private string _style = "Rap";
    private bool _isProcessing;
    private LyricsResult? _result;

    // Save dialog
    private bool _showSaveDialog;
    private List<Album> _albums = new();
    private string _selectedAlbumId = string.Empty;
    private string _newAlbumName = string.Empty;
    private bool _isSaving;
    private string? _saveError;
    private string? _saveSuccess;

    protected override void OnInitialized()
    {
        if (!Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
        }
    }

    private async Task HandleRephrase()
    {
        if (string.IsNullOrWhiteSpace(_originalLyrics))
            return;

        _isProcessing = true;
        _result = null;
        StateHasChanged();

        var request = new RephraseRequest
        {
            OriginalLyrics = _originalLyrics,
            Language = _language,
            Mood = _mood,
            Style = _style
        };

        _result = await AiService.RephraseLyricsAsync(request);
        _isProcessing = false;
    }

    private async Task ShowSaveDialog()
    {
        _saveError = null;
        _saveSuccess = null;
        _selectedAlbumId = string.Empty;
        _newAlbumName = string.Empty;

        if (Session.IsAuthenticated && Session.IdToken != null && Session.UserId != null)
        {
            _albums = await Firestore.GetAlbumsAsync(Session.UserId, Session.IdToken);
        }

        _showSaveDialog = true;
    }

    private async Task SaveToAlbum()
    {
        if (_result == null || !Session.IsAuthenticated || Session.UserId == null || Session.IdToken == null)
            return;

        _isSaving = true;
        _saveError = null;
        _saveSuccess = null;

        try
        {
            string albumId = _selectedAlbumId;

            if (string.IsNullOrEmpty(albumId))
            {
                if (string.IsNullOrWhiteSpace(_newAlbumName))
                {
                    _saveError = "Please enter an album name.";
                    return;
                }

                var newAlbum = new Album { Name = _newAlbumName };
                var created = await Firestore.CreateAlbumAsync(Session.UserId, newAlbum, Session.IdToken);
                if (created == null)
                {
                    _saveError = "Failed to create album.";
                    return;
                }
                albumId = created.Id;
            }

            var song = new Song
            {
                Title = _result.Title,
                Lyrics = _result.Lyrics,
                Language = _language,
                Mood = _mood,
                GenerationType = "rephrased",
                WordCount = _result.WordCount,
                EstimatedDuration = _result.EstimatedDuration
            };

            var savedSong = await Firestore.CreateSongAsync(Session.UserId, albumId, song, Session.IdToken);
            if (savedSong != null)
            {
                _saveSuccess = "Lyrics saved successfully! \ud83c\udf89";
            }
            else
            {
                _saveError = "Failed to save lyrics.";
            }
        }
        finally
        {
            _isSaving = false;
        }
    }
}
