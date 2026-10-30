using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NileGuideApi.Services;

namespace NileGuideApi.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/admin/ai")]
    [ApiController]
    [Produces("application/json")]
    public class AdminAiController : ControllerBase
    {
        private readonly IAiAdminService _aiAdminService;

        public AdminAiController(IAiAdminService aiAdminService)
        {
            _aiAdminService = aiAdminService;
        }

        [HttpGet("files")]
        public async Task<IActionResult> GetFiles()
        {
            var (statusCode, responseBody) = await _aiAdminService.GetFilesAsync();
            return Content(responseBody, "application/json");
        }

        [HttpPost("files/multi-upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> files, [FromForm] int chunkSize = 500, [FromForm] int overlapSize = 20)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "No files were uploaded." });
            }

            var (statusCode, responseBody) = await _aiAdminService.UploadFilesAsync(files, chunkSize, overlapSize);
            return StatusCode(statusCode, responseBody);
        }

        [HttpPut("files/{fileId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateFile([FromRoute] string fileId, [FromForm] IFormFile file, [FromForm] int chunkSize = 500, [FromForm] int overlapSize = 20)
        {
            if (string.IsNullOrWhiteSpace(fileId) || file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Valid fileId and replacement file are required." });
            }

            var (statusCode, responseBody) = await _aiAdminService.UpdateFileAsync(fileId, file, chunkSize, overlapSize);
            return StatusCode(statusCode, responseBody);
        }

        [HttpDelete("files/{fileId}")]
        public async Task<IActionResult> DeleteFile([FromRoute] string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                return BadRequest(new { message = "Valid fileId is required." });
            }

            var (statusCode, responseBody) = await _aiAdminService.DeleteFileAsync(fileId);
            return StatusCode(statusCode, responseBody);
        }
    }
}
