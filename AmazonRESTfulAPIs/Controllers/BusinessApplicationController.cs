using Amazon.Chime;
using Amazon.Chime.Model;
using Amazon.Connect;
using Amazon.Connect.Model;
using Amazon.WorkMail;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessApplicationController : ControllerBase
    {
        private readonly IAmazonConnect _connectClient;
        private readonly IAmazonWorkMail _workMailClient;
        private readonly IAmazonChime _chimeClient;

        public BusinessApplicationController(
            IAmazonConnect connectClient,
            IAmazonWorkMail workMailClient,
            IAmazonChime chimeClient)
        {
            _connectClient = connectClient;
            _workMailClient = workMailClient;
            _chimeClient = chimeClient;
        }

        // Amazon Connect Endpoints
        [HttpPost("contact-centers")]
        public async Task<IActionResult> CreateContactCenter([FromBody] ContactCenterRequest request)
        {
            try
            {
                var createRequest = new CreateInstanceRequest
                {
                    IdentityManagementType = request.IdentityManagementType,
                    InboundCallsEnabled = request.InboundCallsEnabled,
                    OutboundCallsEnabled = request.OutboundCallsEnabled,
                    InstanceAlias = request.InstanceAlias,
                    DirectoryId = request.DirectoryId
                };

                var response = await _connectClient.CreateInstanceAsync(createRequest);
                return Ok(new { InstanceId = response.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("contact-centers/{instanceId}/users")]
        public async Task<IActionResult> CreateUser(string instanceId, [FromBody] ConnectUserRequest request)
        {
            try
            {
                var createRequest = new Amazon.Connect.Model.CreateUserRequest
                {
                    InstanceId = instanceId,
                    Username = request.Username,
                    Password = request.Password,
                    IdentityInfo = new UserIdentityInfo
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email
                    },
                    PhoneConfig = new UserPhoneConfig
                    {
                        PhoneType = request.PhoneType,
                        AutoAccept = request.AutoAccept,
                        AfterContactWorkTimeLimit = request.AfterContactWorkTimeLimit
                    },
                    SecurityProfileIds = request.SecurityProfileIds,
                    RoutingProfileId = request.RoutingProfileId
                };

                var response = await _connectClient.CreateUserAsync(createRequest);
                return Ok(new { UserId = response.UserId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // WorkMail Endpoints
        [HttpPost("organizations")]
        public async Task<IActionResult> CreateOrganization([FromBody] OrganizationRequest request)
        {
            try
            {
                var createRequest = new Amazon.WorkMail.Model.CreateOrganizationRequest
                {
                    DirectoryId = request.DirectoryId,
                    EnableInteroperability = true,
                    Alias = string.Empty,
                };

                var response = await _workMailClient.CreateOrganizationAsync(createRequest, CancellationToken.None);
                return Ok(new { OrganizationId = response.OrganizationId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("organizations/{organizationId}/users")]
        public async Task<IActionResult> CreateMailUser(
            string organizationId,
            [FromBody] MailUserRequest request)
        {
            try
            {
                var createRequest = new Amazon.WorkMail.Model.CreateUserRequest
                {
                    OrganizationId = organizationId,
                    Name = request.Name,
                    DisplayName = request.DisplayName,
                    Password = request.Password
                };

                var response = await _workMailClient.CreateUserAsync(createRequest);
                return Ok(new { UserId = response.UserId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Chime Endpoints
        [HttpPost("meetings")]
        public async Task<IActionResult> CreateMeeting([FromBody] MeetingRequest request)
        {
            try
            {
                var createRequest = new CreateMeetingRequest
                {
                    ClientRequestToken = Guid.NewGuid().ToString(),
                    MediaRegion = request.MediaRegion,
                    MeetingHostId = request.HostId,
                    ExternalMeetingId = request.ExternalMeetingId,
                    //NotificationsConfiguration = new NotificationsConfiguration
                    //{
                    //    LambdaFunctionArn = request.LambdaFunctionArn
                    //}

                };

                var response = await _chimeClient.CreateMeetingAsync(createRequest);
                return Ok(response.Meeting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("meetings/{meetingId}/attendees")]
        public async Task<IActionResult> CreateAttendee(
            string meetingId,
            [FromBody] AttendeeRequest request)
        {
            try
            {
                var createRequest = new CreateAttendeeRequest
                {
                    MeetingId = meetingId,
                    ExternalUserId = request.ExternalUserId
                };

                var response = await _chimeClient.CreateAttendeeAsync(createRequest);
                return Ok(response.Attendee);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("chat-rooms")]
        public async Task<IActionResult> CreateChatRoom([FromBody] ChatRoomRequest request)
        {
            try
            {
                var createRequest = new CreateRoomRequest
                {
                    Name = request.Name,
                    ClientRequestToken = Guid.NewGuid().ToString(),
                    AccountId = request.AccountId
                };

                var response = await _chimeClient.CreateRoomAsync(createRequest);
                return Ok(response.Room);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("chat-rooms/{roomId}/messages")]
        public async Task<IActionResult> SendMessage(
            string roomId,
            [FromBody] MessageRequest request)
        {
            try
            {
                var sendRequest = new SendChannelMessageRequest
                {
                    ChannelArn = roomId,
                    Content = request.Content,
                    Type = request.Type,
                    Persistence = request.Persistence,
                    ClientRequestToken = Guid.NewGuid().ToString()
                };

                var response = await _chimeClient.SendChannelMessageAsync(sendRequest);
                return Ok(new { MessageId = response.MessageId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class ContactCenterRequest
        {
            public string IdentityManagementType { get; set; }
            public bool InboundCallsEnabled { get; set; }
            public bool OutboundCallsEnabled { get; set; }
            public string InstanceAlias { get; set; }
            public string DirectoryId { get; set; }
        }

        public class ConnectUserRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneType { get; set; }
            public bool AutoAccept { get; set; }
            public int AfterContactWorkTimeLimit { get; set; }
            public List<string> SecurityProfileIds { get; set; }
            public string RoutingProfileId { get; set; }
        }

        public class OrganizationRequest
        {
            public string DirectoryId { get; set; }
            public string Name { get; set; }
            public List<string> Aliases { get; set; }
        }

        public class MailUserRequest
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Password { get; set; }
        }

        public class MeetingRequest
        {
            public string MediaRegion { get; set; }
            public string HostId { get; set; }
            public string ExternalMeetingId { get; set; }
            public string NotificationTopicArn { get; set; }
        }

        public class AttendeeRequest
        {
            public string ExternalUserId { get; set; }
        }

        public class ChatRoomRequest
        {
            public string Name { get; set; }
            public string AccountId { get; set; }
        }

        public class MessageRequest
        {
            public string Content { get; set; }
            public string Type { get; set; }
            public string Persistence { get; set; }
        }
    }
}
