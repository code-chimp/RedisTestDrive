using Microsoft.AspNetCore.Mvc;
using RedisTestDrive.Api.Models;
using RedisTestDrive.Common;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace RedisTestDrive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
public class StringsController : ControllerBase
{
    private readonly IRedisClient _redis;
    private readonly ILogger<StringsController> _logger;

    public StringsController(IRedisClient redis, ILogger<StringsController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// List of keys currently stored in the Strings db.
    /// </summary>
    /// <returns>Array of keys</returns>
    /// <response code="200">Array of keys</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetKeys()
    {
        try
        {
            var db = _redis.GetDb(CacheConstants.StringValuesDb);
            var keys = await db.SearchKeysAsync("*");
            return Ok(keys);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error retrieving keys: {e.Message}");
        }
    }

    /// <summary>
    /// Retrieves a string value cached at the specified key.
    /// NOTE: If this is an object you will receive the serialized JSON string created by Newtonsoft.
    /// </summary>
    /// <param name="key">String representing a keys</param>
    /// <returns>String value cached at the specified key</returns>
    /// <response code="200">string value cached at specified key</response>
    /// <response code="404">Key not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{key}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetValue(string key)
    {
        try
        {
            var db = _redis.GetDb(CacheConstants.StringValuesDb);
            var value = await db.Database.StringGetAsync(key);
            return !string.IsNullOrEmpty(value) ? Ok((string)value) : NotFound($"Key {key} not found in cache");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error retrieving key: {e.Message}");
        }
    }

    private async Task<bool> Upsert(SetStringRequest request)
    {
        var db = _redis.GetDb(CacheConstants.StringValuesDb);
        var success = await db.AddAsync(request.Key, request.Value);

        return success;
    }

    /// <summary>
    /// Caches/updates a string value at the specified keys.
    /// </summary>
    /// <param name="request">Key/Value pair to be cached</param>
    /// <returns>String indicating disposition</returns>
    /// <response code="201">Successfully cached value</response>
    /// <response code="400">Value not cached</response>
    /// <response code="500">Unexpected error</response>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Set(SetStringRequest request)
    {
        try
        {
            var success = await Upsert(request);

            return success
                ? CreatedAtAction(nameof(GetValue), new { key = request.Key }, request.Value)
                : BadRequest($"There was an error adding {request.Key} to the cache");
            ;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error caching string value: {e.Message}");
        }
    }

    /// <summary>
    /// Updates a string value at the specified key in the Redis database set aside for strings.
    /// </summary>
    /// <param name="key">string representing key to store value at</param>
    /// <param name="value">string representing value to be stored at specified key</param>
    /// <returns></returns>
    /// <response code="202">Successfully updated cached value</response>
    /// <response code="400">Value not cached</response>
    /// <response code="500">Unexpected error</response>
    [HttpPut("{key}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Set(string key, string value)
    {
        try
        {
            var success = await Upsert(new SetStringRequest { Key = key, Value = value });

            return success ? Accepted(value) : BadRequest($"There was an error updating {key} to {value} in the cache");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error updating cached string value: {e.Message}");
        }
    }

    /// <summary>
    /// Delete a string or object value at the specified key.
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
            var db = _redis.GetDb(CacheConstants.StringValuesDb);
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
            return Problem($"Unexpected error removing cached string value: {e.Message}");
        }
    }

    /// <summary>
    /// Clear all cached String data.
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
            var db = _redis.GetDb(CacheConstants.StringValuesDb);
            await db.FlushDbAsync();
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error flushing database: {e.Message}");
        }
    }

    /// <summary>
    /// Caches/updates an object value serialized as a JSON string at the specified key.
    /// </summary>
    /// <param name="request">Key/Value pair to be cached</param>
    /// <returns>Object cached</returns>
    /// <response code="201">Successfully cached value</response>
    /// <response code="400">Value not cached</response>
    /// <response code="500">Server error</response>
    [HttpPost]
    [Route("Object")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SimpleObject), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetObject(SetStringObjectRequest request)
    {
        try
        {
            var db = _redis.GetDb(CacheConstants.StringValuesDb);
            var success = await db.AddAsync(request.Key, request.Value);

            return success
                ? CreatedAtAction(nameof(GetObject), new { key = request.Key }, request.Value)
                : BadRequest($"There was an error serializing and adding {request.Key} to the cache");
            ;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error caching object value: {e.Message}");
        }
    }

    /// <summary>
    /// Retrieves an object value cached at the specified key.
    /// NOTE: If this is a string you will receive an error.
    /// </summary>
    /// <param name="key">String representing a keys</param>
    /// <returns>Object value cached at the specified key</returns>
    /// <response code="200">Deserialized object value cached at specified key</response>
    /// <response code="404">Key not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("Object/{key}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SimpleObject), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetObject(string key)
    {
        try
        {
            var db = _redis.GetDb(CacheConstants.StringValuesDb);
            var value = await db.GetAsync<SimpleObject>(key);
            return value is not null ? Ok(value) : NotFound($"Key {key} not found in cache");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Problem($"Unexpected error retrieving key: {e.Message}");
        }
    }
}
