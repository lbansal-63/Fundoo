using ModelLayer.DTOs;
using ModelLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface INoteBusiness
    {
        NoteResponseModel CreateNote(int userId, NoteCreationModel model);
        List<NoteResponseModel> ViewNotes(int userId, NoteView view);
        NoteResponseModel? ViewNotesById(int userId, int noteId);
        NoteResponseModel? EditNotes(int userId, EditNotesModel model);
        bool DeleteNotes(int userId, int noteId);
        NoteResponseModel? ArchiUnarchived(int userId, int noteId);
        NoteResponseModel? TrashUntrash(int userId, int noteId);
    }
}
