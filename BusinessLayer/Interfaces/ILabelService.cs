using ModelLayer;

namespace BusinessLayer.Interfaces
{
    // Label ke business rules yahan hain: trim, normalize, duplicate check, ownership
    public interface ILabelService
    {
        LabelResponseModel Create(int userId, string name);
        List<LabelResponseModel> GetAll(int userId);
        LabelResponseModel Rename(int userId, int labelId, string name);
        void Delete(int userId, int labelId);
        void AttachToNote(int userId, int noteId, int labelId);
        void RemoveFromNote(int userId, int noteId, int labelId);
    }
}