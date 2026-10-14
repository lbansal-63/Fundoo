using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using Microsoft.Extensions.Logging;
using ModelLayer.DTOs;
using ModelLayer.Entities;
using ModelLayer.Models;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class NoteBusiness : INoteBusiness
    {
        private readonly INoteRepository _repo;
        private readonly ILogger<NoteBusiness> _logger;

        public NoteBusiness(INoteRepository repo, ILogger<NoteBusiness> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        private static NoteResponseModel ToResponse(NoteEntity n) => new()
        {
            NoteId = n.NoteId,
            Title = n.Title,
            Description = n.Description,
            Colour = n.Colour,
            IsArchived = n.IsArchived,
            IsDeleted = n.IsDeleted
        };

        // Note na mile ya kisi aur user ka ho, dono me same NotFound
        private NoteEntity GetOwnedNote(int userId, int noteId)
        {
            return _repo.GetById(userId, noteId)
                ?? throw new NotFoundException("Note not found");
        }

        public NoteResponseModel CreateNote(int userId, NoteCreationModel model)
        {
            var note = new NoteEntity
            {
                Title = model.Title.Trim(),
                Description = model.Description,
                Colour = model.Colour,
                UserId = userId
            };

            _repo.Add(note);
            _logger.LogInformation("AUDIT: user {UserId} created note {NoteId}", userId, note.NoteId);
            return ToResponse(note);
        }

        public List<NoteResponseModel> ViewNotes(int userId, NoteView view)
        {
            return _repo.GetByUser(userId, view).Select(ToResponse).ToList();
        }

        public NoteResponseModel ViewNotesById(int userId, int noteId)
        {
            return ToResponse(GetOwnedNote(userId, noteId));
        }

        public NoteResponseModel EditNotes(int userId, EditNotesModel model)
        {
            var note = GetOwnedNote(userId, model.NoteId);

            note.Title = model.Title.Trim();
            note.Description = model.Description;
            note.Colour = model.Colour;

            _repo.Update(note);
            _logger.LogInformation("AUDIT: user {UserId} edited note {NoteId}", userId, note.NoteId);
            return ToResponse(note);
        }

        public void DeleteNotes(int userId, int noteId)
        {
            var note = GetOwnedNote(userId, noteId);

            _repo.Delete(note);
            _logger.LogInformation("AUDIT: user {UserId} permanently deleted note {NoteId}", userId, noteId);
        }

        public NoteResponseModel ArchiUnarchived(int userId, int noteId)
        {
            var note = GetOwnedNote(userId, noteId);

            note.IsArchived = !note.IsArchived;
            _repo.Update(note);
            return ToResponse(note);
        }

        public NoteResponseModel TrashUntrash(int userId, int noteId)
        {
            var note = GetOwnedNote(userId, noteId);

            note.IsDeleted = !note.IsDeleted;
            _repo.Update(note);
            return ToResponse(note);
        }
    }
}