using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityIdentityComplianceController : ControllerBase
    {
        private readonly IAmazonSecurityTokenService _stsClient;

        public SecurityIdentityComplianceController(IAmazonSecurityTokenService stsClient)
        {
            _stsClient = stsClient;
        }

        [HttpPost("assumeRole")]
        public async Task<IActionResult> AssumeRole([FromBody] AssumeRoleRequest request)
        {
            try
            {
                var response = await _stsClient.AssumeRoleAsync(request);
                return Ok(response.Credentials);
            }
            catch (AmazonServiceException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("getSessionToken")]
        public async Task<IActionResult> GetSessionToken([FromBody] GetSessionTokenRequest request)
        {
            try
            {
                var response = await _stsClient.GetSessionTokenAsync(request);
                return Ok(response.Credentials);
            }
            catch (AmazonServiceException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("assumeRoleWithWebIdentity")]
        public async Task<IActionResult> AssumeRoleWithWebIdentity([FromBody] AssumeRoleWithWebIdentityRequest request)
        {
            try
            {
                var response = await _stsClient.AssumeRoleWithWebIdentityAsync(request);
                return Ok(response.Credentials);
            }
            catch (AmazonServiceException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Add model classes for request validation
        public class SecurityCredentialsRequest
        {
            public string RoleArn { get; set; }
            public string RoleSessionName { get; set; }
            public int DurationSeconds { get; set; }
        }
    }
}
