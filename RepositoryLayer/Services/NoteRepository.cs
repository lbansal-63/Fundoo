using Microsoft.EntityFrameworkCore;
using ModelLayer;
using RepositoryLayer.Context;
using RepositoryLayer.Entities;
using RepositoryLayer.Interfaces;

namespace RepositoryLayer.Services
{
    public class NoteRepository : INoteRepository
    {
        private readonly FundooContext _db;

        public NoteRepository(FundooContext db)
        {
            _db = db;
        }

        public void Add(NoteEntity note)
        {
            _db.Notes.Add(note);
            _db.SaveChanges();
        }

        public List<NoteEntity> GetByUser(int userId, NoteView view)
        {
            var query = _db.Notes.AsNoTracking().Where(n => n.UserId == userId);

            query = view switch
            {
                NoteView.Archived => query.Where(n => n.IsArchived && !n.IsDeleted),
                NoteView.Trash => query.Where(n => n.IsDeleted),
                _ => query.Where(n => !n.IsArchived && !n.IsDeleted)
            };

            return query.OrderByDescending(n => n.NoteId).ToList();
        }

        public NoteEntity? GetById(int userId, int noteId)
        {
            return _db.Notes.FirstOrDefault(n => n.NoteId == noteId && n.UserId == userId);
        }

        public void Update(NoteEntity note)
        {
            _db.Notes.Update(note);
            _db.SaveChanges();
        }

        public void Delete(NoteEntity note)
        {
            _db.Notes.Remove(note);
            _db.SaveChanges();
        }
    }
}