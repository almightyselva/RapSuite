using Microsoft.AspNetCore.Components;
using RapSuite.Domain.Entities;
using RapSuite.Domain.Interfaces;

namespace RapSuite.Components.Pages.Albums;

public partial class Albums
{
    [Inject] private IAlbumRepository AlbumRepository { get; set; } = default!;
    [Inject] private IUserSessionService Session { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private List<Album> _albums = new();
    private bool _isLoading = true;

    // Dialog state
    private bool _showDialog;
    private Album? _editingAlbum;
    private string _albumName = string.Empty;
    private bool _isSaving;
    private string? _dialogError;

    // Delete state
    private bool _showDeleteConfirm;
    private Album? _deletingAlbum;
    private bool _isDeleting;

    protected override async Task OnInitializedAsync()
    {
        if (!Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadAlbums();
    }

    private async Task LoadAlbums()
    {
        _isLoading = true;
        if (Session.UserId != null)
        {
            _albums = await AlbumRepository.GetByUserAsync(Session.UserId);
        }
        _isLoading = false;
    }

    private void ViewAlbum(string albumId)
    {
        Navigation.NavigateTo($"/albums/{albumId}");
    }

    private void ShowCreateDialog()
    {
        _editingAlbum = null;
        _albumName = string.Empty;
        _dialogError = null;
        _showDialog = true;
    }

    private void ShowEditDialog(Album album)
    {
        _editingAlbum = album;
        _albumName = album.Name;
        _dialogError = null;
        _showDialog = true;
    }

    private void CloseDialog()
    {
        _showDialog = false;
        _editingAlbum = null;
    }

    private async Task SaveAlbum()
    {
        if (string.IsNullOrWhiteSpace(_albumName))
        {
            _dialogError = "Please enter an album name.";
            return;
        }

        if (Session.UserId == null) return;

        _isSaving = true;
        _dialogError = null;

        try
        {
            if (_editingAlbum == null)
            {
                var album = new Album { Name = _albumName };
                var created = await AlbumRepository.CreateAsync(Session.UserId, album);
                if (created == null)
                {
                    _dialogError = "Failed to create album.";
                    return;
                }
            }
            else
            {
                _editingAlbum.Name = _albumName;
                var success = await AlbumRepository.UpdateAsync(Session.UserId, _editingAlbum);
                if (!success)
                {
                    _dialogError = "Failed to update album.";
                    return;
                }
            }

            CloseDialog();
            await LoadAlbums();
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void ConfirmDelete(Album album)
    {
        _deletingAlbum = album;
        _showDeleteConfirm = true;
    }

    private async Task DeleteAlbum()
    {
        if (_deletingAlbum == null || Session.UserId == null) return;

        _isDeleting = true;

        try
        {
            await AlbumRepository.DeleteAsync(Session.UserId, _deletingAlbum.Id);
            _showDeleteConfirm = false;
            _deletingAlbum = null;
            await LoadAlbums();
        }
        finally
        {
            _isDeleting = false;
        }
    }
}
