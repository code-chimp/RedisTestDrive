using System.ComponentModel.DataAnnotations;

namespace RedisTestDrive.Api.Models;

public class SetStringRequest
{
    [Required]
    public string Key { get; set; }
    [Required]
    public string Value { get; set; }
}
