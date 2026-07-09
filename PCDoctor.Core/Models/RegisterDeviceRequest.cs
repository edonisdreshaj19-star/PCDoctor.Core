using System.Text.Json.Serialization;

namespace PCDoctor.Core.Models;

public class RegisterDeviceRequest
{
    [JsonPropertyName("deviceName")]
    public string DeviceName { get; set; } = string.Empty;
}