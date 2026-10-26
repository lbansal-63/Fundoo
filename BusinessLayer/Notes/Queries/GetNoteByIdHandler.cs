using ModelLayer;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Notes.Queries
{
    public class GetNoteByIdHandler
    {
        private readonly INoteRepository _repo;

        public GetNoteByIdHandler(INoteRepository repo)
        {
            _repo = repo;
        }

        public NoteResponseModel Handle(GetNoteByIdQuery query) =>
            NoteHelper.ToResponse(NoteHelper.GetOwned(_repo, query.UserId, query.NoteId));
    }
}