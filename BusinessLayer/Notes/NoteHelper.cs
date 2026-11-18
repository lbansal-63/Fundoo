using Middleware.ExceptionHandling;
using Middleware.Services.Cache;
using ModelLayer;
using RepositoryLayer.Entities;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Notes
{
    internal static class NoteHelper
    {
        public static NoteResponseModel ToResponse(NoteEntity n) => new()
        {
            NoteId = n.NoteId,
            Title = n.Title,
            Description = n.Description,
            Colour = n.Colour,
            IsArchived = n.IsArchived,
            IsDeleted = n.IsDeleted
        };

        // Note na mile ya kisi aur user ka ho, dono me same NotFound
        public static NoteEntity GetOwned(INoteRepository repo, int userId, int noteId) =>
            repo.GetById(userId, noteId) ?? throw new NotFoundException("Note not found");

        // Kisi bhi write ke baad is user ki teeno lists (Active/Archived/Trash) clear
        public static void InvalidateCache(ICacheService cache, int userId)
        {
            foreach (var v in Enum.GetValues<NoteView>())
                cache.Remove(CacheKeys.Notes(userId, v.ToString()));
        }
    }
}