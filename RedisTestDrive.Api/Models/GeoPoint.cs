using System.ComponentModel.DataAnnotations;

namespace RedisTestDrive.Api.Models;

public class GeoPoint
{
    [Required]
    public double Latitude { get; set; }
    [Required]
    public double Longitude { get; set; }
    [Required]
    public string Label { get; set; }
}
