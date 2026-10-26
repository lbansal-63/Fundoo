using System.Security.Claims;
using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer;

namespace FundooAPI.Controllers
{
    [ApiController]
    [Route("api/labels")]
    [Authorize]
    public class LabelsController : ControllerBase
    {
        private readonly ILabelBusiness _business;

        public LabelsController(ILabelBusiness business)
        {
            _business = business;
        }

        // Owner hamesha validated JWT ke claim se aata hai, client se kabhi nahi
        private bool TryGetUserId(out int userId)
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out userId);
        }

        [HttpPost]
        public IActionResult Create(CreateLabelRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var label = _business.CreateLabel(userId, request.Name);
            return Created($"/api/labels/{label.LabelId}", label);
        }

        [HttpGet]
        public IActionResult GetMyLabels()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            return Ok(_business.GetMyLabels(userId));
        }

        [HttpPut("{labelId:int:min(1)}")]
        public IActionResult Rename(int labelId, RenameLabelRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            return Ok(_business.RenameLabel(userId, labelId, request.Name));
        }

        [HttpDelete("{labelId:int:min(1)}")]
        public IActionResult Delete(int labelId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            _business.DeleteLabel(userId, labelId);
            return NoContent();
        }
    }
}