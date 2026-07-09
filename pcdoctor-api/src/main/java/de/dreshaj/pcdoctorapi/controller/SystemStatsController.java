package de.dreshaj.pcdoctorapi.controller;

import de.dreshaj.pcdoctorapi.dto.DiagnosticMessageDto;
import de.dreshaj.pcdoctorapi.dto.SystemStatsDto;
import de.dreshaj.pcdoctorapi.dto.SystemStatsResponseDto;
import de.dreshaj.pcdoctorapi.service.DiagnosticService;
import de.dreshaj.pcdoctorapi.service.SystemStatsService;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
public class SystemStatsController {

    private final SystemStatsService systemStatsService;
    private final DiagnosticService diagnosticService;

    public SystemStatsController(SystemStatsService systemStatsService, DiagnosticService diagnosticService) {
        this.systemStatsService = systemStatsService;
        this.diagnosticService = diagnosticService;
    }

    @PostMapping("/api/system-stats")
    public String receiveStats(
            @RequestHeader(value = "X-Device-Token", required = false) String deviceToken,
            @RequestBody SystemStatsDto stats
    ) {
        systemStatsService.saveStats(deviceToken, stats);
        return "System stats received.";
    }

    @GetMapping("/api/devices/{deviceId}/system-stats/latest")
    public SystemStatsResponseDto getLatestStats(@PathVariable Long deviceId) {
        return systemStatsService.getLatestStats(deviceId);
    }

    @GetMapping("/api/devices/{deviceId}/system-stats/history")
    public List<SystemStatsResponseDto> getHistory(@PathVariable Long deviceId) {
        return systemStatsService.getHistory(deviceId);
    }

    @GetMapping("/api/devices/{deviceId}/system-stats/diagnostics")
    public List<DiagnosticMessageDto> getDiagnostics(@PathVariable Long deviceId) {
        SystemStatsResponseDto latestStats = systemStatsService.getLatestStats(deviceId);

        SystemStatsDto dto = new SystemStatsDto();
        dto.setCpuUsage(latestStats.getCpuUsage());
        dto.setUsedMemoryMb(latestStats.getUsedMemoryMb());
        dto.setTotalMemoryMb(latestStats.getTotalMemoryMb());

        return diagnosticService.analyze(dto);
    }
}