using Microsoft.AspNetCore.Mvc;
using RedisTestDrive.Api.Models;
using RedisTestDrive.Common;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace RedisTestDrive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
public class GeoController : ControllerBase
{
    private readonly IRedisClient _redis;
    private readonly ILogger<GeoController> _logger;

    private readonly List<GeoPoint> _geoPoints = new()
    {
        new GeoPoint
        {
            Latitude = 41.226567703202,
            Longitude = -96.07906965916344,
            Label = "Tim's Domicile"
        },
        new GeoPoint
        {
            Latitude = 41.23665494384777,
            Longitude = -96.1230578591632,
            Label = "Dave & Busters"
        },
        new GeoPoint
        {
            Latitude = 41.26451910012323,
            Longitude = -96.06945525916254,
            Label = "Cheesecake Factory"
        },
        new GeoPoint
        {
            Latitude = 41.22493028558474,
            Longitude = -95.92865423218247,
            Label = "#1 Zoo in the World"
        },
        new GeoPoint
        {
            Latitude = 39.17685704075071,
            Longitude = -94.48629091688504,
            Label = "Worlds of Fun"
        }
    };

    public GeoController(IRedisClient redis, ILogger<GeoController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Seed a set of geographic points.
    /// </summary>
    /// <returns></returns>
    /// <response code="204">Successfully seeded geo cache</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [Route("Seed")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SeedDb()
    {
        try
        {
            var db = _redis.GetDb(CacheConstants.GeoValuesDb);
            const string key = "points.of.interest";

            foreach (var geoPoint in _geoPoints)
            {
                await db.Database.GeoAddAsync(key, geoPoint.Longitude, geoPoint.Latitude, geoPoint.Label);
            }

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error seeding geo database: {e.Message}");
        }
    }

    /// <summary>
    /// Delete a set of Geo points.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <response code="204">Successfully removed cached value</response>
    /// <response code="404">Key not found in database</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{key}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string key)
    {
        try
        {
            var db = _redis.GetDb(CacheConstants.GeoValuesDb);
            var exists = await db.ExistsAsync(key);

            if (!exists)
            {
                return NotFound($"Key {key} not found in cache");
            }

            await db.RemoveAsync(key);

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error removing cached Geo set: {e.Message}");
        }
    }

    /// <summary>
    /// Clear all cached Geo data.
    /// </summary>
    /// <returns></returns>
    /// <response code="204">Successfully flushed the database</response>
    /// <response code="500">Server error</response>
    [HttpDelete]
    [Route("Flush")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FlushDb()
    {
        try
        {
            var db = _redis.GetDb(CacheConstants.GeoValuesDb);
            await db.FlushDbAsync();
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error flushing database: {e.Message}");
        }
    }
}
