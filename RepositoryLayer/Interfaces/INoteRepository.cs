using ModelLayer.Entities;
using ModelLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
