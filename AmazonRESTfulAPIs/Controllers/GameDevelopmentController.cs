using Amazon.GameLift;
using Amazon.GameLift.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.EC2.Model;
using Amazon.RoboMaker.Model;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameDevelopmentController : ControllerBase
    {
        private readonly IAmazonGameLift _gameLiftClient;

        public GameDevelopmentController(IAmazonGameLift gameLiftClient)
        {
            _gameLiftClient = gameLiftClient;
        }

        // Fleet Management
        [HttpPost("fleets")]
        public async Task<IActionResult> CreateFleet([FromBody] GameFleetRequest request)
        {
            try
            {
                var createRequest = new Amazon.GameLift.Model.CreateFleetRequest
                {
                    Name = request.FleetName,
                    Description = request.Description,
                    BuildId = request.BuildId,
                    ServerLaunchPath = request.ServerLaunchPath,
                    EC2InstanceType = request.InstanceType,
                    EC2InboundPermissions = request.InboundPermissions,
                    NewGameSessionProtectionPolicy = request.ProtectionPolicy,
                    RuntimeConfiguration = new RuntimeConfiguration
                    {
                        ServerProcesses = request.ServerProcesses
                    }
                };

                var response = await _gameLiftClient.CreateFleetAsync(createRequest);
                return Ok(new { FleetId = response.FleetAttributes.FleetId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("fleets")]
        public async Task<IActionResult> ListFleets()
        {
            try
            {
                var request = new Amazon.GameLift.Model.ListFleetsRequest();
                var response = await _gameLiftClient.ListFleetsAsync(request);
                return Ok(response.FleetIds);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Game Session Management
        [HttpPost("game-sessions")]
        public async Task<IActionResult> CreateGameSession([FromBody] GameSessionRequest request)
        {
            try
            {
                var createRequest = new CreateGameSessionRequest
                {
                    FleetId = request.FleetId,
                    MaximumPlayerSessionCount = request.MaxPlayers,
                    GameProperties = request.GameProperties?.Select(p =>
                        new GameProperty { Key = p.Key, Value = p.Value }).ToList(),
                    Name = request.SessionName
                };

                var response = await _gameLiftClient.CreateGameSessionAsync(createRequest);
                return Ok(response.GameSession);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("game-sessions/{fleetId}")]
        public async Task<IActionResult> ListGameSessions(string fleetId)
        {
            try
            {
                var request = new DescribeGameSessionsRequest
                {
                    FleetId = fleetId
                };
                var response = await _gameLiftClient.DescribeGameSessionsAsync(request);
                return Ok(response.GameSessions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Player Session Management
        [HttpPost("player-sessions")]
        public async Task<IActionResult> CreatePlayerSession([FromBody] PlayerSessionRequest request)
        {
            try
            {
                var createRequest = new CreatePlayerSessionRequest
                {
                    GameSessionId = request.GameSessionId,
                    PlayerId = request.PlayerId
                };

                var response = await _gameLiftClient.CreatePlayerSessionAsync(createRequest);
                return Ok(response.PlayerSession);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Matchmaking Configuration
        [HttpPost("matchmaking/configurations")]
        public async Task<IActionResult> CreateMatchmakingConfiguration([FromBody] MatchmakingConfigRequest request)
        {
            try
            {
                var createRequest = new CreateMatchmakingConfigurationRequest
                {
                    Name = request.Name,
                    Description = request.Description,
                    GameSessionQueueArns = request.GameSessionQueueArns,
                    RequestTimeoutSeconds = request.RequestTimeoutSeconds,
                    RuleSetName = request.RuleSetName
                };

                var response = await _gameLiftClient.CreateMatchmakingConfigurationAsync(createRequest);
                return Ok(response.Configuration);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Matchmaking
        [HttpPost("matchmaking")]
        public async Task<IActionResult> StartMatchmaking([FromBody] StartMatchmakingRequest request)
        {
            try
            {
                var response = await _gameLiftClient.StartMatchmakingAsync(request);
                return Ok(response.MatchmakingTicket);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("matchmaking/{ticketId}")]
        public async Task<IActionResult> GetMatchmakingTicket(string ticketId)
        {
            try
            {
                var request = new DescribeMatchmakingRequest
                {
                    TicketIds = new List<string> { ticketId }
                };
                var response = await _gameLiftClient.DescribeMatchmakingAsync(request);
                return Ok(response.TicketList.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Scaling Policy
        [HttpPost("scaling-policy")]
        public async Task<IActionResult> PutScalingPolicy([FromBody] ScalingPolicyRequest request)
        {
            try
            {
                var putRequest = new PutScalingPolicyRequest
                {
                    FleetId = request.FleetId,
                    Name = request.PolicyName,
                    ScalingAdjustment = request.ScalingAdjustment,
                    ScalingAdjustmentType = request.AdjustmentType,
                    Threshold = request.Threshold,
                    ComparisonOperator = request.ComparisonOperator,
                    EvaluationPeriods = request.EvaluationPeriods,
                    MetricName = request.MetricName,
                    PolicyType = request.PolicyType
                };

                var response = await _gameLiftClient.PutScalingPolicyAsync(putRequest);
                return Ok(new { PolicyName = response.Name });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class GameFleetRequest
        {
            public string FleetName { get; set; }
            public string Description { get; set; }
            public string BuildId { get; set; }
            public string ServerLaunchPath { get; set; }
            public string InstanceType { get; set; }
            public List<Amazon.GameLift.Model.IpPermission> InboundPermissions { get; set; }
            public string ProtectionPolicy { get; set; }
            public List<ServerProcess> ServerProcesses { get; set; }
        }

        public class GameSessionRequest
        {
            public string FleetId { get; set; }
            public int MaxPlayers { get; set; }
            public Dictionary<string, string> GameProperties { get; set; }
            public string SessionName { get; set; }
        }

        public class PlayerSessionRequest
        {
            public string GameSessionId { get; set; }
            public string PlayerId { get; set; }
        }

        public class MatchmakingConfigRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<string> GameSessionQueueArns { get; set; }
            public int RequestTimeoutSeconds { get; set; }
            public string RuleSetName { get; set; }
        }

        public class ScalingPolicyRequest
        {
            public string FleetId { get; set; }
            public string PolicyName { get; set; }
            public int ScalingAdjustment { get; set; }
            public string AdjustmentType { get; set; }
            public double Threshold { get; set; }
            public string ComparisonOperator { get; set; }
            public int EvaluationPeriods { get; set; }
            public string MetricName { get; set; }
            public string PolicyType { get; set; }
        }
    }
}
