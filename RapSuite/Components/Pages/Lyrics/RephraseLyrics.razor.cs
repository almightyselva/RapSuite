using Microsoft.AspNetCore.Components;
using RapSuite.Domain.Entities;
using RapSuite.Domain.Interfaces;
using RapSuite.Domain.Models;

namespace RapSuite.Components.Pages.Lyrics;

public partial class RephraseLyrics
{
    [Inject] private ILyricsAiService AiService { get; set; } = default!;
    [Inject] private IAlbumRepository AlbumRepository { get; set; } = default!;
    [Inject] private ISongRepository SongRepository { get; set; } = default!;
    [Inject] private IUserSessionService Session { get; set; } = default!;
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

        if (Session.IsAuthenticated && Session.UserId != null)
        {
            _albums = await AlbumRepository.GetByUserAsync(Session.UserId);
        }

        _showSaveDialog = true;
    }

    private async Task SaveToAlbum()
    {
        if (_result == null || !Session.IsAuthenticated || Session.UserId == null)
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
                var created = await AlbumRepository.CreateAsync(Session.UserId, newAlbum);
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

            var savedSong = await SongRepository.CreateAsync(Session.UserId, albumId, song);
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
