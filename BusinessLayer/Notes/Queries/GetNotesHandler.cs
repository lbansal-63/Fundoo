using Middleware.Services.Cache;
using ModelLayer;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Notes.Queries
{
    public class GetNotesHandler
    {
        private static readonly TimeSpan NotesTtl = TimeSpan.FromMinutes(10);

        private readonly INoteRepository _repo;
        private readonly ICacheService _cache;

        public GetNotesHandler(INoteRepository repo, ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public List<NoteResponseModel> Handle(GetNotesQuery query)
        {
            var key = CacheKeys.Notes(query.UserId, query.View.ToString());

            var cached = _cache.Get<List<NoteResponseModel>>(key);
            if (cached != null) return cached;

            var notes = _repo.GetByUser(query.UserId, query.View)
                .Select(NoteHelper.ToResponse)
                .ToList();

            _cache.Set(key, notes, NotesTtl);
            return notes;
        }
    }
}










