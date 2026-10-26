using ModelLayer;

namespace BusinessLayer.Interfaces
{
    public interface INoteBusiness
    {
        NoteResponseModel CreateNote(int userId, NoteCreationModel model);
        List<NoteResponseModel> ViewNotes(int userId, NoteView view);
        NoteResponseModel ViewNotesById(int userId, int noteId);
        NoteResponseModel EditNotes(int userId, EditNotesModel model);
        void DeleteNotes(int userId, int noteId);
        NoteResponseModel ArchiUnarchived(int userId, int noteId);
        NoteResponseModel TrashUntrash(int userId, int noteId);
        void AttachLabel(int userId, int noteId, int labelId);
        void RemoveLabel(int userId, int noteId, int labelId);
    }
}