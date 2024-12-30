using Microsoft.AspNetCore.Mvc;
using Amazon.Organizations;
using Amazon.Organizations.Model;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.ServiceQuotas;
using Amazon.ServiceQuotas.Model;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerEnableController : ControllerBase
    {
        private readonly IAmazonOrganizations _orgClient;
        private readonly IAmazonIdentityManagementService _iamClient;
        private readonly IAmazonSimpleSystemsManagement _ssmClient;
        private readonly IAmazonServiceQuotas _quotasClient;

        public CustomerEnableController(
            IAmazonOrganizations orgClient,
            IAmazonIdentityManagementService iamClient,
            IAmazonSimpleSystemsManagement ssmClient,
            IAmazonServiceQuotas quotasClient)
        {
            _orgClient = orgClient;
            _iamClient = iamClient;
            _ssmClient = ssmClient;
            _quotasClient = quotasClient;
        }

        #region Organization Management

        // Create Organization Unit
        [HttpPost("organization/ou")]
        public async Task<IActionResult> CreateOrganizationUnit([FromBody] CreateOuRequest request)
        {
            try
            {
                var response = await _orgClient.CreateOrganizationalUnitAsync(new CreateOrganizationalUnitRequest
                {
                    ParentId = request.ParentId,
                    Name = request.Name
                });

                return Ok(response.OrganizationalUnit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Create Account in Organization
        [HttpPost("organization/accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            try
            {
                var response = await _orgClient.CreateAccountAsync(new Amazon.Organizations.Model.CreateAccountRequest
                {
                    AccountName = request.AccountName,
                    Email = request.Email,
                    RoleName = request.RoleName
                });

                return Ok(response.CreateAccountStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // List Organization Accounts
        [HttpGet("organization/accounts")]
        public async Task<IActionResult> ListAccounts()
        {
            try
            {
                var response = await _orgClient.ListAccountsAsync(new ListAccountsRequest());
                return Ok(response.Accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region IAM Management

        // Create IAM User
        [HttpPost("iam/users")]
        public async Task<IActionResult> CreateIamUser([FromBody] CreateIamUserRequest request)
        {
            try
            {
                var createUserResponse = await _iamClient.CreateUserAsync(new CreateUserRequest
                {
                    UserName = request.UserName,
                    Path = request.Path
                });

                if (request.AddToGroup)
                {
                    await _iamClient.AddUserToGroupAsync(new AddUserToGroupRequest
                    {
                        UserName = request.UserName,
                        GroupName = request.GroupName
                    });
                }

                return Ok(createUserResponse.User);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Create IAM Role
        [HttpPost("iam/roles")]
        public async Task<IActionResult> CreateIamRole([FromBody] CreateRoleRequest request)
        {
            try
            {
                var response = await _iamClient.CreateRoleAsync(request);
                return Ok(response.Role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Attach Policy to Role
        [HttpPost("iam/roles/{roleName}/policies")]
        public async Task<IActionResult> AttachRolePolicy(string roleName, [FromBody] AttachRolePolicyRequest request)
        {
            try
            {
                await _iamClient.AttachRolePolicyAsync(new AttachRolePolicyRequest
                {
                    RoleName = roleName,
                    PolicyArn = request.PolicyArn
                });

                return Ok(new { Message = "Policy attached successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region Service Quotas

        // Get Service Quotas
        [HttpGet("quotas/{serviceName}")]
        public async Task<IActionResult> GetServiceQuotas(string serviceName)
        {
            try
            {
                var response = await _quotasClient.ListServiceQuotasAsync(new ListServiceQuotasRequest
                {
                    ServiceCode = serviceName
                });

                return Ok(response.Quotas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Request Quota Increase
        [HttpPost("quotas/increase-request")]
        public async Task<IActionResult> RequestQuotaIncrease([FromBody] RequestQuotaIncreaseRequest request)
        {
            try
            {
                var response = await _quotasClient.RequestServiceQuotaIncreaseAsync(
                    new RequestServiceQuotaIncreaseRequest
                    {
                        ServiceCode = request.ServiceCode,
                        QuotaCode = request.QuotaCode,
                        DesiredValue = request.DesiredValue
                    });

                return Ok(response.RequestedQuota);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region SSM Parameters

        // Create SSM Parameter
        [HttpPost("parameters")]
        public async Task<IActionResult> CreateParameter([FromBody] CreateParameterRequest request)
        {
            try
            {
                await _ssmClient.PutParameterAsync(new PutParameterRequest
                {
                    Name = request.Name,
                    Value = request.Value,
                    Type = request.Type,
                    Description = request.Description,
                    Overwrite = request.Overwrite
                });

                return Ok(new { Message = "Parameter created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Get SSM Parameter
        [HttpGet("parameters/{parameterName}")]
        public async Task<IActionResult> GetParameter(string parameterName)
        {
            try
            {
                var response = await _ssmClient.GetParameterAsync(new GetParameterRequest
                {
                    Name = parameterName,
                    WithDecryption = true
                });

                return Ok(response.Parameter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion
    }

    // Request Models
    public class CreateOuRequest
    {
        public string ParentId { get; set; }
        public string Name { get; set; }
    }

    public class CreateAccountRequest
    {
        public string AccountName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
    }

    public class CreateIamUserRequest
    {
        public string UserName { get; set; }
        public string Path { get; set; }
        public bool AddToGroup { get; set; }
        public string GroupName { get; set; }
    }

    public class RequestQuotaIncreaseRequest
    {
        public string ServiceCode { get; set; }
        public string QuotaCode { get; set; }
        public double DesiredValue { get; set; }
    }

    public class CreateParameterRequest
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool Overwrite { get; set; }
    }   
}
