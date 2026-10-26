using ModelLayer;

namespace BusinessLayer.Interfaces
{
    public interface ILabelBusiness
    {
        LabelResponseModel CreateLabel(int userId, string name);
        List<LabelResponseModel> GetMyLabels(int userId);
        LabelResponseModel RenameLabel(int userId, int labelId, string name);
        void DeleteLabel(int userId, int labelId);
    }
}