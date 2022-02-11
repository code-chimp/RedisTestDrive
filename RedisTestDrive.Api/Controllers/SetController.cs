using Microsoft.AspNetCore.Mvc;
using RedisTestDrive.Common;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace RedisTestDrive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
public class SetController : ControllerBase
{
    private readonly IRedisClient _redis;
    private readonly ILogger<SetController> _logger;

    public SetController(IRedisClient redis, ILogger<SetController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        return Ok("Hello");
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
            var db = _redis.GetDb(CacheConstants.SetValuesDb);
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
            var db = _redis.GetDb(CacheConstants.SetValuesDb);
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
