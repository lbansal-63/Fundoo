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

        public NoteResponseModel? ViewNotesById(int userId, int noteId)
        {
            var note = _repo.GetById(userId, noteId);
            return note == null ? null : ToResponse(note);
        }

        public NoteResponseModel? EditNotes(int userId, EditNotesModel model)
        {
            var note = _repo.GetById(userId, model.NoteId);
            if (note == null) return null;

            note.Title = model.Title.Trim();
            note.Description = model.Description;
            note.Colour = model.Colour;

            _repo.Update(note);
            _logger.LogInformation("AUDIT: user {UserId} edited note {NoteId}", userId, note.NoteId);
            return ToResponse(note);
        }

        public bool DeleteNotes(int userId, int noteId)
        {
            var note = _repo.GetById(userId, noteId);
            if (note == null) return false;

            _repo.Delete(note);
            _logger.LogInformation("AUDIT: user {UserId} permanently deleted note {NoteId}", userId, noteId);
            return true;
        }

        public NoteResponseModel? ArchiUnarchived(int userId, int noteId)
        {
            var note = _repo.GetById(userId, noteId);
            if (note == null) return null;

            note.IsArchived = !note.IsArchived;
            _repo.Update(note);
            return ToResponse(note);
        }

        public NoteResponseModel? TrashUntrash(int userId, int noteId)
        {
            var note = _repo.GetById(userId, noteId);
            if (note == null) return null;

            note.IsDeleted = !note.IsDeleted;
            _repo.Update(note);
            return ToResponse(note);
        }
    }
}