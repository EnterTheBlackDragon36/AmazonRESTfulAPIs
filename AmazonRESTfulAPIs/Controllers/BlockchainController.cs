using Amazon.ManagedBlockchain;
using Amazon.ManagedBlockchain.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockchainController : ControllerBase
    {
        private readonly IAmazonManagedBlockchain _blockchainClient;

        public BlockchainController(IAmazonManagedBlockchain blockchainClient)
        {
            _blockchainClient = blockchainClient;
        }

        // Network Operations
        [HttpPost("networks")]
        public async Task<IActionResult> CreateNetwork([FromBody] NetworkRequest request)
        {
            try
            {
                var createRequest = new CreateNetworkRequest
                {
                    Name = request.NetworkName,
                    Description = request.Description,
                    Framework = request.Framework,
                    FrameworkVersion = request.FrameworkVersion,
                    
                    //NetworkConfiguration = new NetworkConfiguration
                    //{
                    //    VotingPolicy = new VotingPolicy
                    //    {
                    //        ApprovalThresholdPolicy = new ApprovalThresholdPolicy
                    //        {
                    //            ThresholdPercentage = request.ThresholdPercentage,
                    //            ProposalDurationInHours = request.ProposalDurationInHours,
                    //            ThresholdComparator = request.ThresholdComparator
                    //        }
                    //    }
                    //},
                    MemberConfiguration = new MemberConfiguration
                    {
                        Name = request.MemberName,
                        Description = request.MemberDescription,
                        //AdminUsername = request.AdminUsername,
                        //AdminPassword = request.AdminPassword
                    }
                };

                var response = await _blockchainClient.CreateNetworkAsync(createRequest);
                return Ok(new
                {
                    NetworkId = response.NetworkId,
                    MemberId = response.MemberId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("networks")]
        public async Task<IActionResult> ListNetworks()
        {
            try
            {
                var request = new ListNetworksRequest();
                var response = await _blockchainClient.ListNetworksAsync(request);
                return Ok(response.Networks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Member Operations
        [HttpPost("networks/{networkId}/members")]
        public async Task<IActionResult> CreateMember(string networkId, [FromBody] MemberRequest request)
        {
            try
            {
                var createRequest = new CreateMemberRequest
                {
                    NetworkId = networkId,
                    MemberConfiguration = new MemberConfiguration
                    {
                        Name = request.MemberName,
                        Description = request.Description,
                        //AdminUsername = request.AdminUsername,
                        //AdminPassword = request.AdminPassword
                    }
                };

                var response = await _blockchainClient.CreateMemberAsync(createRequest);
                return Ok(new { MemberId = response.MemberId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("networks/{networkId}/members")]
        public async Task<IActionResult> ListMembers(string networkId)
        {
            try
            {
                var request = new ListMembersRequest
                {
                    NetworkId = networkId
                };
                var response = await _blockchainClient.ListMembersAsync(request);
                return Ok(response.Members);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Node Operations
        [HttpPost("networks/{networkId}/members/{memberId}/nodes")]
        public async Task<IActionResult> CreateNode(
            string networkId,
            string memberId,
            [FromBody] NodeRequest request)
        {
            try
            {
                var createRequest = new CreateNodeRequest
                {
                    NetworkId = networkId,
                    MemberId = memberId,
                    NodeConfiguration = new NodeConfiguration
                    {
                        InstanceType = request.InstanceType,
                        AvailabilityZone = request.AvailabilityZone,
                        //LogPublishingConfiguration = new LogPublishingConfiguration
                        //{
                        //    Fabric = new Amazon.ManagedBlockchain.Model.LogConfiguration
                        //    {
                        //        CloudWatchLogGroupName = request.LogGroupName
                        //    }
                        //}
                    }
                };

                var response = await _blockchainClient.CreateNodeAsync(createRequest);
                return Ok(new { NodeId = response.NodeId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("networks/{networkId}/members/{memberId}/nodes")]
        public async Task<IActionResult> ListNodes(string networkId, string memberId)
        {
            try
            {
                var request = new Amazon.ManagedBlockchain.Model.ListNodesRequest
                {
                    NetworkId = networkId,
                    MemberId = memberId
                };
                var response = await _blockchainClient.ListNodesAsync(request);
                return Ok(response.Nodes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Proposal Operations
        [HttpPost("networks/{networkId}/proposals")]
        public async Task<IActionResult> CreateProposal(
            string networkId,
            [FromBody] ProposalRequest request)
        {
            try
            {
                var createRequest = new CreateProposalRequest
                {
                    NetworkId = networkId,
                    MemberId = request.MemberId,
                    //Actions = request.Actions
                };

                var response = await _blockchainClient.CreateProposalAsync(createRequest);
                return Ok(new { ProposalId = response.ProposalId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("networks/{networkId}/proposals/{proposalId}/vote")]
        public async Task<IActionResult> VoteOnProposal(
            string networkId,
            string proposalId,
            [FromBody] VoteRequest request)
        {
            try
            {
                var voteRequest = new VoteOnProposalRequest
                {
                    NetworkId = networkId,
                    ProposalId = proposalId,
                    VoterMemberId = request.VoterMemberId,
                    Vote = request.Vote
                };

                await _blockchainClient.VoteOnProposalAsync(voteRequest);
                return Ok("Vote recorded successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Custom Models
        public class NetworkRequest
        {
            public string NetworkName { get; set; }
            public string Description { get; set; }
            public string Framework { get; set; }
            public string FrameworkVersion { get; set; }
            public int ThresholdPercentage { get; set; }
            public int ProposalDurationInHours { get; set; }
            public string ThresholdComparator { get; set; }
            public string MemberName { get; set; }
            public string MemberDescription { get; set; }
            public string AdminUsername { get; set; }
            public string AdminPassword { get; set; }
        }

        public class MemberRequest
        {
            public string MemberName { get; set; }
            public string Description { get; set; }
            public string AdminUsername { get; set; }
            public string AdminPassword { get; set; }
        }

        public class NodeRequest
        {
            public string InstanceType { get; set; }
            public string AvailabilityZone { get; set; }
            public string LogGroupName { get; set; }
        }

        public class ProposalRequest
        {
            public string MemberId { get; set; }
            public List<Action> Actions { get; set; }
        }

        public class VoteRequest
        {
            public string VoterMemberId { get; set; }
            public string Vote { get; set; }
        }
    }
}
