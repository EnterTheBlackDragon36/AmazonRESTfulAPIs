using Amazon.GroundStation;
using Amazon.GroundStation.Model;
using Microsoft.AspNetCore.Mvc;

namespace AmazonRESTfulAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SatelliteController : ControllerBase
    {
        private readonly IAmazonGroundStation _groundStationClient;

        public SatelliteController(IAmazonGroundStation groundStationClient)
        {
            _groundStationClient = groundStationClient;
        }

        [HttpGet("satellites")]
        public async Task<IActionResult> ListSatellites()
        {
            try
            {
                var request = new ListSatellitesRequest();
                var response = await _groundStationClient.ListSatellitesAsync(request);
                return Ok(response.Satellites);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> ListContacts([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            try
            {
                var request = new ListContactsRequest
                {
                    StartTime = startTime,
                    EndTime = endTime
                };
                var response = await _groundStationClient.ListContactsAsync(request);
                return Ok(response.ContactList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("contact-schedule")]
        public async Task<IActionResult> ReserveContact([FromBody] ReserveContactRequest request)
        {
            try
            {
                var response = await _groundStationClient.ReserveContactAsync(request);
                return Ok(response.ContactId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("contact-schedule/{contactId}")]
        public async Task<IActionResult> CancelContact(string contactId)
        {
            try
            {
                var request = new CancelContactRequest
                {
                    ContactId = contactId
                };
                await _groundStationClient.CancelContactAsync(request);
                return Ok($"Contact {contactId} cancelled successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ground-stations")]
        public async Task<IActionResult> ListGroundStations()
        {
            try
            {
                var request = new ListGroundStationsRequest();
                var response = await _groundStationClient.ListGroundStationsAsync(request);
                return Ok(response.GroundStationList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("config")]
        public async Task<IActionResult> ListConfigs()
        {
            try
            {
                var request = new ListConfigsRequest();
                var response = await _groundStationClient.ListConfigsAsync(request);
                return Ok(response.ConfigList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Model for satellite data validation
        public class SatelliteContactRequest
        {
            public string SatelliteId { get; set; }
            public string GroundStationId { get; set; }
            public DateTime MissionStartTime { get; set; }
            public DateTime MissionEndTime { get; set; }
            public string ConfigId { get; set; }
        }
    }
}
