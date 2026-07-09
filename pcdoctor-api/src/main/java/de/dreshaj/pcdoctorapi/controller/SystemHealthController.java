package de.dreshaj.pcdoctorapi.controller;

import de.dreshaj.pcdoctorapi.dto.SystemHealthResponseDto;
import de.dreshaj.pcdoctorapi.service.SystemHealthService;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/devices/{deviceId}/system-health")
public class SystemHealthController {

    private final SystemHealthService systemHealthService;

    public SystemHealthController(SystemHealthService systemHealthService) {
        this.systemHealthService = systemHealthService;
    }

    @GetMapping("/latest")
    public SystemHealthResponseDto getLatestHealth(@PathVariable Long deviceId) {
        return systemHealthService.getLatestHealth(deviceId);
    }
}