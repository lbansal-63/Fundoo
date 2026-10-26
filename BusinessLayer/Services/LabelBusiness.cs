using BusinessLayer.Interfaces;
using BusinessLayer.Labels.Commands;
using BusinessLayer.Labels.Queries;
using ModelLayer;

namespace BusinessLayer.Services
{
    // NoteBusiness jaisa hi: controller sirf yahan baat karta hai, aage handlers
    public class LabelBusiness : ILabelBusiness
    {
        private readonly CreateLabelHandler _create;
        private readonly GetLabelsHandler _getLabels;
        private readonly RenameLabelHandler _rename;
        private readonly DeleteLabelHandler _delete;

        public LabelBusiness(
            CreateLabelHandler create,
            GetLabelsHandler getLabels,
            RenameLabelHandler rename,
            DeleteLabelHandler delete)
        {
            _create = create;
            _getLabels = getLabels;
            _rename = rename;
            _delete = delete;
        }

        public LabelResponseModel CreateLabel(int userId, string name) =>
            _create.Handle(new CreateLabelCommand(userId, name));

        public List<LabelResponseModel> GetMyLabels(int userId) =>
            _getLabels.Handle(new GetLabelsQuery(userId));

        public LabelResponseModel RenameLabel(int userId, int labelId, string name) =>
            _rename.Handle(new RenameLabelCommand(userId, labelId, name));

        public void DeleteLabel(int userId, int labelId) =>
            _delete.Handle(new DeleteLabelCommand(userId, labelId));
    }
}