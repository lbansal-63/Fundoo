using Microsoft.EntityFrameworkCore;
using Npgsql;
using RepositoryLayer.Context;
using RepositoryLayer.Entities;
using RepositoryLayer.Interfaces;

namespace RepositoryLayer.Services
{
    public class LabelRepository : ILabelRepository
    {
        private readonly FundooContext _db;

        public LabelRepository(FundooContext db)
        {
            _db = db;
        }

        private static bool IsUniqueViolation(DbUpdateException ex) =>
            ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

        public bool Add(LabelEntity label)
        {
            try
            {
                _db.Labels.Add(label);
                _db.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Do requests ek saath aayi aur dusri pehle insert ho gayi
                _db.Entry(label).State = EntityState.Detached;
                return false;
            }
        }

        public bool Update(LabelEntity label)
        {
            try
            {
                _db.Entry(label).State = EntityState.Modified;
                _db.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                return false;
            }
        }

        public List<LabelEntity> GetByUser(int userId)
        {
            return _db.Labels
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .OrderBy(l => l.NormalizedName)
                .ToList();
        }

        public LabelEntity? GetById(int userId, int labelId)
        {
            return _db.Labels.FirstOrDefault(l => l.LabelId == labelId && l.UserId == userId);
        }

        public bool ExistsByNormalizedName(int userId, string normalizedName, int? excludeLabelId = null)
        {
            return _db.Labels.AsNoTracking().Any(l =>
                l.UserId == userId
                && l.NormalizedName == normalizedName
                && (excludeLabelId == null || l.LabelId != excludeLabelId));
        }

        public void Delete(LabelEntity label)
        {
            _db.Labels.Remove(label);
            _db.SaveChanges();
        }

        public void AttachToNote(int noteId, int labelId)
        {
            // Pehle se juda hai to kuch nahi karna (PUT idempotent hai)
            if (_db.NoteLabels.Any(nl => nl.NoteId == noteId && nl.LabelId == labelId))
                return;

            try
            {
                _db.NoteLabels.Add(new NoteLabelEntity { NoteId = noteId, LabelId = labelId });
                _db.SaveChanges();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Race: kisi aur request ne usi waqt jod diya, result same hai
            }
        }

        public void RemoveFromNote(int noteId, int labelId)
        {
            var link = _db.NoteLabels.FirstOrDefault(nl => nl.NoteId == noteId && nl.LabelId == labelId);
            if (link == null)
                return;

            try
            {
                _db.NoteLabels.Remove(link);
                _db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Kisi aur request ne pehle hi hata diya
            }
        }
    }
}