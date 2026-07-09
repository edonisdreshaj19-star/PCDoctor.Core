using System.Text.Json;
using PCDoctor.Core.Models;

namespace PCDoctor.Core.Services;

public class DeviceTokenStore
{
    private readonly string filePath;

    public DeviceTokenStore()
    {
        string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PCDoctor");

        Directory.CreateDirectory(folderPath);

        filePath = Path.Combine(folderPath, "device.json");
    }

    public async Task<DeviceRegistrationResponse?> LoadDeviceAsync()
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        string json = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        DeviceRegistrationResponse? device =
            JsonSerializer.Deserialize<DeviceRegistrationResponse>(json);

        if (device == null ||
            device.Id <= 0 ||
            string.IsNullOrWhiteSpace(device.DeviceToken))
        {
            return null;
        }

        return device;
    }

    public async Task SaveDeviceAsync(DeviceRegistrationResponse device)
    {
        string json = JsonSerializer.Serialize(device, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json);
    }
}