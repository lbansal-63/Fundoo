using ModelLayer;
using RepositoryLayer.Entities;

namespace RepositoryLayer.Interfaces
{
    public interface INoteRepository
    {
        void Add(NoteEntity note);
        List<NoteEntity> GetByUser(int userId, NoteView view);
        NoteEntity? GetById(int userId, int noteId);
        void Update(NoteEntity note);
        void Delete(NoteEntity note);
    }
}