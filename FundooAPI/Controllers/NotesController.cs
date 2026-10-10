using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FundooAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    [ApiExplorerSettings(IgnoreApi = true)]
    public class NotesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Token correct,Logged In successfully!!");
        }
    }
}