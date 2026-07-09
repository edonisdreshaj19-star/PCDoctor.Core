using System.Text.Json.Serialization;

namespace PCDoctor.Core.Models;

public class DeviceRegistrationResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("deviceName")]
    public string DeviceName { get; set; } = string.Empty;

    [JsonPropertyName("deviceToken")]
    public string DeviceToken { get; set; } = string.Empty;
}