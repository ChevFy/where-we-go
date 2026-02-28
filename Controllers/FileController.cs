using Microsoft.AspNetCore.Mvc;

using where_we_go.DTO;
using where_we_go.Service;

namespace where_we_go.Controllers
{
    // This is example controller for file uploading and get presigned URL
    // Important note:
    // when upload file should ensure file name is unique
    // when get file should know that this is presigned url it can set expired time
    [ApiController]
    [Route("api/[controller]")]
    public class FileController(IFileService fileService) : ControllerBase
    {
        [HttpPost("upload")]
        [RequestSizeLimit(1024 * 1025 * 10)] // 10 mb
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file provided" });
            }

            try
            {
                var fileName = Guid.NewGuid() + file.FileName;
                await fileService.UploadFileAsync(new FileUploadDto()
                {
                    ObjectName = fileName,
                    File = file,
                });
                return Ok(new { Message = "Success", FileName = fileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Upload failed : {ex.Message}" });
            }
        }

        [HttpGet("presigned-url")]
        public async Task<IActionResult> GetPresignedUrl([FromQuery] FileDownloadDto dto)
        {
            try
            {
                var file = await fileService.GetPresignedUrlAsync(dto);
                return Ok(new { FileUrl = file });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Load file failed : {ex.Message}" });
            }
        }
    }
}