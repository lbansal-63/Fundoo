using System.Security.Claims;
using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTOs;
using ModelLayer.Models;

namespace FundooAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly INoteBusiness _business;

        public NotesController(INoteBusiness business)
        {
            _business = business;
        }

        private bool TryGetUserId(out int userId)
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out userId);
        }

        [HttpPost("CreateNote")]
        public IActionResult CreateNote(NoteCreationModel model)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var note = _business.CreateNote(userId, model);
            return CreatedAtAction(nameof(ViewNotesById), new { noteId = note.NoteId }, note);
        }

        [HttpGet("ViewNotes")]
        public IActionResult ViewNotes([FromQuery] NoteView view = NoteView.Active)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            return Ok(_business.ViewNotes(userId, view));
        }

        [HttpGet("ViewNotesById")]
        public IActionResult ViewNotesById([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var note = _business.ViewNotesById(userId, model.NoteId);
            if (note == null) return NotFound("Note not found");
            return Ok(note);
        }

        [HttpPut("EditNotes")]
        public IActionResult EditNotes(EditNotesModel model)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var note = _business.EditNotes(userId, model);
            if (note == null) return NotFound("Note not found");
            return Ok(note);
        }

        [HttpDelete("DeleteNotes")]
        public IActionResult DeleteNotes([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            if (!_business.DeleteNotes(userId, model.NoteId))
                return NotFound("Note not found");
            return Ok("Note deleted permanently");
        }

        [HttpPatch("ArchiUnarchived")]
        public IActionResult ArchiUnarchived(NotesIdModel model)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var note = _business.ArchiUnarchived(userId, model.NoteId);
            if (note == null) return NotFound("Note not found");
            return Ok(note);
        }

        [HttpPatch("TrashUntrash")]
        public IActionResult TrashUntrash(NotesIdModel model)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var note = _business.TrashUntrash(userId, model.NoteId);
            if (note == null) return NotFound("Note not found");
            return Ok(note);
        }
    }
}