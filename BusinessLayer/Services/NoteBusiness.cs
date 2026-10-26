using BusinessLayer.Interfaces;
using BusinessLayer.Notes.Commands;
using BusinessLayer.Notes.Queries;
using ModelLayer;
using BusinessLayer.Labels.Commands;

namespace BusinessLayer.Services
{
    public class NoteBusiness : INoteBusiness
    {
        private readonly AddNoteHandler _add;
        private readonly EditNoteHandler _edit;
        private readonly DeleteNoteHandler _delete;
        private readonly ArchiveNoteHandler _archive;
        private readonly TrashNoteHandler _trash;
        private readonly GetNotesHandler _getNotes;
        private readonly GetNoteByIdHandler _getById;
        private readonly AttachLabelToNoteHandler _attachLabel;
        private readonly RemoveLabelFromNoteHandler _removeLabel;

        public NoteBusiness(
            AddNoteHandler add,
            EditNoteHandler edit,
            DeleteNoteHandler delete,
            ArchiveNoteHandler archive,
            TrashNoteHandler trash,
            GetNotesHandler getNotes,
            GetNoteByIdHandler getById,
            AttachLabelToNoteHandler attachLabel,
            RemoveLabelFromNoteHandler removeLabel)
        {
            _add = add;
            _edit = edit;
            _delete = delete;
            _archive = archive;
            _trash = trash;
            _getNotes = getNotes;
            _getById = getById;
            _attachLabel = attachLabel;
            _removeLabel = removeLabel;
        }

        public NoteResponseModel CreateNote(int userId, NoteCreationModel model) =>
            _add.Handle(new AddNoteCommand(userId, model.Title, model.Description, model.Colour));

        public List<NoteResponseModel> ViewNotes(int userId, NoteView view) =>
            _getNotes.Handle(new GetNotesQuery(userId, view));

        public NoteResponseModel ViewNotesById(int userId, int noteId) =>
            _getById.Handle(new GetNoteByIdQuery(userId, noteId));

        public NoteResponseModel EditNotes(int userId, EditNotesModel model) =>
            _edit.Handle(new EditNoteCommand(userId, model.NoteId, model.Title, model.Description, model.Colour));

        public void DeleteNotes(int userId, int noteId) =>
            _delete.Handle(new DeleteNoteCommand(userId, noteId));

        public NoteResponseModel ArchiUnarchived(int userId, int noteId) =>
            _archive.Handle(new ArchiveNoteCommand(userId, noteId));

        public NoteResponseModel TrashUntrash(int userId, int noteId) =>
            _trash.Handle(new TrashNoteCommand(userId, noteId));

        public void AttachLabel(int userId, int noteId, int labelId) =>
    _attachLabel.Handle(new AttachLabelToNoteCommand(userId, noteId, labelId));

        public void RemoveLabel(int userId, int noteId, int labelId) =>
            _removeLabel.Handle(new RemoveLabelFromNoteCommand(userId, noteId, labelId));
    }
}