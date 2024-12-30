using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Tag = Amazon.EventBridge.Model.Tag;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationIntegrationController : ControllerBase
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IAmazonEventBridge _eventBridgeClient;

        public ApplicationIntegrationController(
            IAmazonSQS sqsClient,
            IAmazonSimpleNotificationService snsClient,
            IAmazonEventBridge eventBridgeClient)
        {
            _sqsClient = sqsClient;
            _snsClient = snsClient;
            _eventBridgeClient = eventBridgeClient;
        }

        // SQS Queue Operations
        [HttpPost("queues")]
        public async Task<IActionResult> CreateQueue([FromBody] QueueRequest request)
        {
            try
            {
                var createRequest = new Amazon.SQS.Model.CreateQueueRequest
                {
                    QueueName = request.QueueName,
                    Attributes = new Dictionary<string, string>
                    {
                        { "DelaySeconds", request.DelaySeconds.ToString() },
                        { "MessageRetentionPeriod", request.MessageRetentionPeriod.ToString() },
                        { "VisibilityTimeout", request.VisibilityTimeout.ToString() }
                    }
                };

                if (request.IsFifo)
                {
                    createRequest.Attributes.Add("FifoQueue", "true");
                    createRequest.Attributes.Add("ContentBasedDeduplication",
                        request.ContentBasedDeduplication.ToString().ToLower());
                }

                var response = await _sqsClient.CreateQueueAsync(createRequest);
                return Ok(new { QueueUrl = response.QueueUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("queues/{queueUrl}/messages")]
        public async Task<IActionResult> SendMessage(string queueUrl, [FromBody] MessageRequest request)
        {
            try
            {
                var sendRequest = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = request.MessageBody,
                    DelaySeconds = request.DelaySeconds
                    //MessageAttributes = request.MessageAttributes
                };

                if (!string.IsNullOrEmpty(request.MessageGroupId))
                {
                    sendRequest.MessageGroupId = request.MessageGroupId;
                }

                var response = await _sqsClient.SendMessageAsync(sendRequest);
                return Ok(new { MessageId = response.MessageId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("queues/{queueUrl}/messages")]
        public async Task<IActionResult> ReceiveMessages(string queueUrl, [FromQuery] int maxMessages = 10)
        {
            try
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = maxMessages,
                    WaitTimeSeconds = 20, // Long polling
                    AttributeNames = new List<string> { "All" },
                    MessageAttributeNames = new List<string> { "All" }
                };

                var response = await _sqsClient.ReceiveMessageAsync(receiveRequest);
                return Ok(response.Messages);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // SNS Topic Operations
        [HttpPost("topics")]
        public async Task<IActionResult> CreateTopic([FromBody] TopicRequest request)
        {
            try
            {
                var createRequest = new CreateTopicRequest
                {
                    Name = request.TopicName,
                    Attributes = request.Attributes
                };

                var response = await _snsClient.CreateTopicAsync(createRequest);
                return Ok(new { TopicArn = response.TopicArn });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("topics/{topicArn}/subscriptions")]
        public async Task<IActionResult> Subscribe(string topicArn, [FromBody] SubscriptionRequest request)
        {
            try
            {
                var subscribeRequest = new SubscribeRequest
                {
                    TopicArn = topicArn,
                    Protocol = request.Protocol,
                    Endpoint = request.Endpoint
                };

                var response = await _snsClient.SubscribeAsync(subscribeRequest);
                return Ok(new { SubscriptionArn = response.SubscriptionArn });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("topics/{topicArn}/messages")]
        public async Task<IActionResult> PublishMessage(string topicArn, [FromBody] PublishRequest request)
        {
            try
            {
                var publishRequest = new Amazon.SimpleNotificationService.Model.PublishRequest
                {
                    TopicArn = topicArn,
                    Message = request.Message,
                    Subject = request.Subject
                    //MessageAttributes = request.MessageAttributes
                };

                var response = await _snsClient.PublishAsync(publishRequest);
                return Ok(new { MessageId = response.MessageId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // EventBridge Operations
        [HttpPost("event-buses")]
        public async Task<IActionResult> CreateEventBus([FromBody] EventBusRequest request)
        {
            try
            {
                var createRequest = new CreateEventBusRequest
                {
                    Name = request.EventBusName,
                    Tags = request.Tags?.Select(t => new Tag { Key = t.Key, Value = t.Value }).ToList()
                };

                var response = await _eventBridgeClient.CreateEventBusAsync(createRequest);
                return Ok(new { EventBusArn = response.EventBusArn });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("event-buses/{eventBusName}/rules")]
        public async Task<IActionResult> CreateRule(string eventBusName, [FromBody] RuleRequest request)
        {
            try
            {
                var createRequest = new PutRuleRequest
                {
                    Name = request.RuleName,
                    EventBusName = eventBusName,
                    EventPattern = request.EventPattern,
                    ScheduleExpression = request.ScheduleExpression,
                    State = request.Enabled ? RuleState.ENABLED : RuleState.DISABLED
                };

                var response = await _eventBridgeClient.PutRuleAsync(createRequest);
                return Ok(new { RuleArn = response.RuleArn });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("event-buses/{eventBusName}/events")]
        public async Task<IActionResult> PutEvents(string eventBusName, [FromBody] List<EventEntry> events)
        {
            try
            {
                var putRequest = new PutEventsRequest
                {
                    Entries = events.Select(e => new PutEventsRequestEntry
                    {
                        EventBusName = eventBusName,
                        Source = e.Source,
                        DetailType = e.DetailType,
                        Detail = JsonSerializer.Serialize(e.Detail)
                    }).ToList()
                };

                var response = await _eventBridgeClient.PutEventsAsync(putRequest);
                return Ok(new
                {
                    FailedEntryCount = response.FailedEntryCount,
                    Entries = response.Entries
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class QueueRequest
        {
            public string QueueName { get; set; }
            public int DelaySeconds { get; set; } = 0;
            public int MessageRetentionPeriod { get; set; } = 345600;
            public int VisibilityTimeout { get; set; } = 30;
            public bool IsFifo { get; set; }
            public bool ContentBasedDeduplication { get; set; }
        }

        public class MessageRequest
        {
            public string MessageBody { get; set; }
            public int DelaySeconds { get; set; }
            public Dictionary<string, Amazon.SimpleNotificationService.Model.MessageAttributeValue> MessageAttributes { get; set; }
            public string MessageGroupId { get; set; }
        }

        public class TopicRequest
        {
            public string TopicName { get; set; }
            public Dictionary<string, string> Attributes { get; set; }
        }

        public class SubscriptionRequest
        {
            public string Protocol { get; set; }
            public string Endpoint { get; set; }
        }

        public class PublishRequest
        {
            public string Message { get; set; }
            public string Subject { get; set; }
            public Dictionary<string, Amazon.SQS.Model.MessageAttributeValue> MessageAttributes { get; set; }
        }

        public class EventBusRequest
        {
            public string EventBusName { get; set; }
            public Dictionary<string, string> Tags { get; set; }
        }

        public class RuleRequest
        {
            public string RuleName { get; set; }
            public string EventPattern { get; set; }
            public string ScheduleExpression { get; set; }
            public bool Enabled { get; set; }
        }

        public class EventEntry
        {
            public string Source { get; set; }
            public string DetailType { get; set; }
            public object Detail { get; set; }
        }
    }
}
