using System.ComponentModel.DataAnnotations;

namespace RedisTestDrive.Api.Models;

public class SetStringObjectRequest
{
    [Required]
    public string Key { get; set; }
    [Required]
    public SimpleObject Value { get; set; }
}
