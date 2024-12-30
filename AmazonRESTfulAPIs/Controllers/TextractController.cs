using Amazon.Textract;
using Amazon.Textract.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextractController : ControllerBase
    {
        private readonly IAmazonTextract _textractClient;

        public TextractController(IAmazonTextract textractClient)
        {
            _textractClient = textractClient;
        }

        // POST: api/textract/detecttext
        [HttpPost("detecttext")]
        public async Task<IActionResult> DetectDocumentText([FromBody] DocumentRequest request)
        {
            //Check against null or empty value
            if (string.IsNullOrEmpty(request.Base64Image))
            {
                return BadRequest("Base64Image is required");
            }

            //
            const int maxFileSizeBytes = 5 * 1024 * 1024; // 5MB
            byte[] documentBytes = Convert.FromBase64String(request.Base64Image);
            if (documentBytes.Length > maxFileSizeBytes)
            {
                return BadRequest("File size exceeds maximum limit of 5MB");
            }

            try
            {
                byte[] documentImage = Convert.FromBase64String(request.Base64Image);
                MemoryStream documentStream = new MemoryStream(documentImage);
                var detectRequest = new DetectDocumentTextRequest
                {
                    Document = new Document
                    {
                        Bytes = documentStream
                    }
                };

                var response = await _textractClient.DetectDocumentTextAsync(detectRequest);
                return Ok(response.Blocks);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing document: {ex.Message}");
            }
        }

        // POST: api/textract/analyzeDocument
        [HttpPost("analyzedocument")]
        public async Task<IActionResult> AnalyzeDocument([FromBody] DocumentRequest request)
        {
            if (string.IsNullOrEmpty(request.Base64Image))
            {
                return BadRequest("Base64Image is required");
            }

            const int maxFileSizeBytes = 5 * 1024 * 1024; // 5MB
            byte[] documentBytes = Convert.FromBase64String(request.Base64Image);
            if (documentBytes.Length > maxFileSizeBytes)
            {
                return BadRequest("File size exceeds maximum limit of 5MB");
            }

            try
            {
                byte[] documentImage = Convert.FromBase64String(request.Base64Image);
                MemoryStream documentStream = new MemoryStream(documentImage);
                var analyzeRequest = new AnalyzeDocumentRequest
                {
                    Document = new Document
                    {
                        Bytes = documentStream
                    },
                    FeatureTypes = new List<string> { "TABLES", "FORMS" }
                };

                var response = await _textractClient.AnalyzeDocumentAsync(analyzeRequest);

                var formattedResponse = new
                {
                    Tables = response.Blocks.Where(b => b.BlockType == "TABLE"),
                    Forms = response.Blocks.Where(b => b.BlockType == "KEY_VALUE_SET"),
                    Text = response.Blocks.Where(b => b.BlockType == "LINE")
                };

                return Ok(formattedResponse);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error analyzing document: {ex.Message}");
            }
        }
    }

    public class DocumentRequest
    {
        public string Base64Image { get; set; }
    }
}
