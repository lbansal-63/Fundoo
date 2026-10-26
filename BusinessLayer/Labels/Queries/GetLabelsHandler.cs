using BusinessLayer.Interfaces;
using ModelLayer;

namespace BusinessLayer.Labels.Queries
{
    public class GetLabelsHandler
    {
        private readonly ILabelService _service;

        public GetLabelsHandler(ILabelService service)
        {
            _service = service;
        }

        public List<LabelResponseModel> Handle(GetLabelsQuery query) =>
            _service.GetAll(query.UserId);
    }
}