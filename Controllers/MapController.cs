using Microsoft.AspNetCore.Mvc;
using FullstackHA.Models;
using FullstackHA.Services;

namespace FullstackHA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly IMapService _mapService;
        private readonly ILogger<MapController> _logger;

        public MapController(IMapService mapService, ILogger<MapController> logger)
        {
            _mapService = mapService;
            _logger = logger;
        }

        [HttpPost("SetMap")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult SetMap([FromBody] Graph graph)
        {
            try
            {
                // Validate input
                if (graph == null)
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Map data is required"
                    });
                }

                if (graph.Nodes == null || graph.Nodes.Count == 0)
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Map must contain at least one node"
                    });
                }

                if (graph.Edges == null)
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Map must contain edges"
                    });
                }

                // Store the map
                _mapService.SetMap(graph);
                _logger.LogInformation("Map successfully stored with {NodeCount} nodes and {EdgeCount} edges",
                    graph.Nodes.Count, graph.Edges.Count);

                return Ok(new
                {
                    message = "Map successfully stored",
                    nodeCount = graph.Nodes.Count,
                    edgeCount = graph.Edges.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing map");
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = ex.Message
                });
            }
        }

        [HttpGet("GetMap")]
        [ProducesResponseType(typeof(Graph), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMap()
        {
            try
            {
                if (!_mapService.HasMap())
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Map has not been set. Please call SetMap first."
                    });
                }

                var map = _mapService.GetMap();
                return Ok(map);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving map");
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = ex.Message
                });
            }
        }

        [HttpGet("ShortestRoute")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ShortestRoute([FromQuery] string from, [FromQuery] string to)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrWhiteSpace(from))
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Parameter 'from' is required"
                    });
                }

                if (string.IsNullOrWhiteSpace(to))
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Parameter 'to' is required"
                    });
                }

                if (!_mapService.HasMap())
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Map has not been set. Please call SetMap first."
                    });
                }

                // Calculate shortest route
                var route = _mapService.GetShortestRoute(from, to);
                _logger.LogInformation("Shortest route from {From} to {To}: {Route}", from, to, route);

                return Ok(route);
            }
            catch (ArgumentException ex)
            {
                // Node not found
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                // No path exists or map not set
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shortest route");
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get the shortest distance between two nodes
        /// </summary>
        /// <param name="from">Starting node ID (e.g., "G")</param>
        /// <param name="to">Destination node ID (e.g., "E")</param>
        /// <returns>Distance as an integer</returns>
        /// <response code="200">Distance calculated successfully</response>
        /// <response code="400">Invalid parameters or nodes not found</response>
        /// <response code="401">Unauthorized - requires FS_Read API key</response>
        [HttpGet("ShortestDistance")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ShortestDistance([FromQuery] string from, [FromQuery] string to)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrWhiteSpace(from))
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Parameter 'from' is required"
                    });
                }

                if (string.IsNullOrWhiteSpace(to))
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Parameter 'to' is required"
                    });
                }

                if (!_mapService.HasMap())
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "Map has not been set. Please call SetMap first."
                    });
                }

                // Calculate shortest distance
                var distance = _mapService.GetShortestDistance(from, to);
                _logger.LogInformation("Shortest distance from {From} to {To}: {Distance}", from, to, distance);

                return Ok(distance);
            }
            catch (ArgumentException ex)
            {
                // Node not found
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                // No path exists or map not set
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shortest distance");
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = ex.Message
                });
            }
        }
    }
}
