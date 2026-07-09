using System.Net.Http.Json;
using PCDoctor.Core.Models;
using Serilog;

namespace PCDoctor.Core.Services;

public class DeviceRegistrationService
{
    private readonly HttpClient httpClient;
    private readonly DeviceTokenStore deviceTokenStore;

    public DeviceRegistrationService(HttpClient httpClient, DeviceTokenStore deviceTokenStore)
    {
        this.httpClient = httpClient;
        this.deviceTokenStore = deviceTokenStore;
    }

    public async Task<DeviceRegistrationResponse> GetOrRegisterDeviceAsync()
    {
        DeviceRegistrationResponse? existingDevice =
            await deviceTokenStore.LoadDeviceAsync();

        if (existingDevice != null)
        {
            return existingDevice;
        }

        RegisterDeviceRequest request = new()
        {
            DeviceName = Environment.MachineName
        };

        HttpResponseMessage response =
            await httpClient.PostAsJsonAsync("/api/devices/register", request);

        response.EnsureSuccessStatusCode();

        DeviceRegistrationResponse? registeredDevice =
            await response.Content.ReadFromJsonAsync<DeviceRegistrationResponse>();

        if (registeredDevice == null ||
            registeredDevice.Id <= 0 ||
            string.IsNullOrWhiteSpace(registeredDevice.DeviceToken))
        {
            throw new InvalidOperationException(
                "Device registration failed. The backend did not return a valid device id and token.");
        }

        await deviceTokenStore.SaveDeviceAsync(registeredDevice);

        Log.Information(
            "Registered device {DeviceName} with id {DeviceId}.",
            registeredDevice.DeviceName,
            registeredDevice.Id);

        return registeredDevice;
    }
}