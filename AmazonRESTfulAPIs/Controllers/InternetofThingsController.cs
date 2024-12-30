using Amazon.IoT;
using Amazon.IoT.Model;
using Amazon.IotData;
using Amazon.IotData.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternetofThingsController : ControllerBase
    {
        private readonly IAmazonIoT _iotClient;
        private readonly IAmazonIotData _iotDataClient;

        public InternetofThingsController(
            IAmazonIoT iotClient,
            IAmazonIotData iotDataClient)
        {
            _iotClient = iotClient;
            _iotDataClient = iotDataClient;
        }

        // Thing Management
        [HttpPost("things")]
        public async Task<IActionResult> CreateThing([FromBody] ThingRequest request)
        {
            try
            {
                var createRequest = new CreateThingRequest
                {
                    ThingName = request.ThingName,
                    ThingTypeName = request.ThingTypeName,
                    AttributePayload = new AttributePayload
                    {
                        Attributes = request.Attributes
                    }
                };

                var response = await _iotClient.CreateThingAsync(createRequest);
                return Ok(new
                {
                    ThingName = response.ThingName,
                    ThingArn = response.ThingArn
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("things")]
        public async Task<IActionResult> ListThings([FromQuery] string thingTypeName = null)
        {
            try
            {
                var request = new ListThingsRequest
                {
                    ThingTypeName = thingTypeName
                };
                var response = await _iotClient.ListThingsAsync(request);
                return Ok(response.Things);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Thing Types
        [HttpPost("thing-types")]
        public async Task<IActionResult> CreateThingType([FromBody] ThingTypeRequest request)
        {
            try
            {
                var createRequest = new CreateThingTypeRequest
                {
                    ThingTypeName = request.ThingTypeName,
                    ThingTypeProperties = new ThingTypeProperties
                    {
                        ThingTypeDescription = request.Description,
                        SearchableAttributes = request.SearchableAttributes
                    }
                };

                var response = await _iotClient.CreateThingTypeAsync(createRequest);
                return Ok(new
                {
                    ThingTypeName = response.ThingTypeName,
                    ThingTypeArn = response.ThingTypeArn
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Policies
        [HttpPost("policies")]
        public async Task<IActionResult> CreatePolicy([FromBody] PolicyRequest request)
        {
            try
            {
                var createRequest = new CreatePolicyRequest
                {
                    PolicyName = request.PolicyName,
                    PolicyDocument = JsonSerializer.Serialize(request.PolicyDocument)
                };

                var response = await _iotClient.CreatePolicyAsync(createRequest);
                return Ok(new
                {
                    PolicyName = response.PolicyName,
                    PolicyArn = response.PolicyArn
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Device Shadow
        [HttpGet("shadow/{thingName}")]
        public async Task<IActionResult> GetThingShadow(string thingName)
        {
            try
            {
                var request = new GetThingShadowRequest
                {
                    ThingName = thingName
                };

                var response = await _iotDataClient.GetThingShadowAsync(request);
                using var reader = new StreamReader(response.Payload);
                var shadowState = await reader.ReadToEndAsync();
                return Ok(JsonSerializer.Deserialize<object>(shadowState));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("shadow/{thingName}")]
        public async Task<IActionResult> UpdateThingShadow(string thingName, [FromBody] object shadowDocument)
        {
            try
            {
                var payload = JsonSerializer.Serialize(shadowDocument);
                var request = new UpdateThingShadowRequest
                {
                    ThingName = thingName,
                    Payload = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload))
                };

                var response = await _iotDataClient.UpdateThingShadowAsync(request);
                return Ok("Shadow updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // MQTT Message Publishing
        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage([FromBody] PublishRequest request)
        {
            try
            {
                var publishRequest = new Amazon.IotData.Model.PublishRequest
                {
                    Topic = request.Topic,
                    Payload = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(request.Message)),
                    Qos = request.Qos
                };

                await _iotDataClient.PublishAsync(publishRequest);
                return Ok("Message published successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Rules
        [HttpPost("rules")]
        public async Task<IActionResult> CreateTopicRule([FromBody] TopicRuleRequest request)
        {
            try
            {
                var createRequest = new CreateTopicRuleRequest
                {
                    RuleName = request.RuleName,
                    TopicRulePayload = new TopicRulePayload
                    {
                        Sql = request.SqlQuery,
                        Description = request.Description,
                        Actions = request.Actions,
                        RuleDisabled = false
                    }
                };

                await _iotClient.CreateTopicRuleAsync(createRequest);
                return Ok($"Rule {request.RuleName} created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class ThingRequest
        {
            public string ThingName { get; set; }
            public string ThingTypeName { get; set; }
            public Dictionary<string, string> Attributes { get; set; }
        }

        public class ThingTypeRequest
        {
            public string ThingTypeName { get; set; }
            public string Description { get; set; }
            public List<string> SearchableAttributes { get; set; }
        }

        public class PolicyRequest
        {
            public string PolicyName { get; set; }
            public object PolicyDocument { get; set; }
        }

        public class PublishRequest
        {
            public string Topic { get; set; }
            public string Message { get; set; }
            public int Qos { get; set; }
        }

        public class TopicRuleRequest
        {
            public string RuleName { get; set; }
            public string SqlQuery { get; set; }
            public string Description { get; set; }
            public List<Amazon.IoT.Model.Action> Actions { get; set; }
        }
    }
}
