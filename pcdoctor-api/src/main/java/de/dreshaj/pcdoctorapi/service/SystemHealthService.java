package de.dreshaj.pcdoctorapi.service;

import de.dreshaj.pcdoctorapi.dto.SystemHealthResponseDto;
import de.dreshaj.pcdoctorapi.dto.SystemStatsResponseDto;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

@Service
public class SystemHealthService {

    private final SystemStatsService systemStatsService;

    public SystemHealthService(SystemStatsService systemStatsService) {
        this.systemStatsService = systemStatsService;
    }

    public SystemHealthResponseDto getLatestHealth(Long deviceId) {
        SystemStatsResponseDto latestStats = systemStatsService.getLatestStats(deviceId);

        int score = 100;
        List<String> reasons = new ArrayList<>();
        List<String> recommendations = new ArrayList<>();

        double cpuUsage = latestStats.getCpuUsage();
        double memoryUsagePercent = calculateMemoryUsagePercent(
                latestStats.getUsedMemoryMb(),
                latestStats.getTotalMemoryMb()
        );

        if (cpuUsage >= 85) {
            score -= 20;
            reasons.add("High CPU usage detected: " + String.format(Locale.US,"%.1f", cpuUsage) + "%");
            recommendations.add("Close CPU-heavy applications or check background processes.");
        }

        if (memoryUsagePercent >= 85) {
            score -= 20;
            reasons.add("High memory usage detected: " + String.format(Locale.US,"%.1f", memoryUsagePercent) + "%");
            recommendations.add("Close memory-heavy applications or restart unused services.");
        }

        if (reasons.isEmpty()) {
            reasons.add("System performance looks normal.");
            recommendations.add("No immediate action required.");
        }

        score = Math.max(score, 0);

        String status = determineStatus(score);

        return new SystemHealthResponseDto(
                score,
                status,
                reasons,
                recommendations
        );
    }

    private double calculateMemoryUsagePercent(double usedMemoryMb, double totalMemoryMb) {
        if (totalMemoryMb <= 0) {
            return 0;
        }

        return (usedMemoryMb / totalMemoryMb) * 100;
    }

    private String determineStatus(int score) {
        if (score >= 90) {
            return "HEALTHY";
        }

        if (score >= 60) {
            return "WARNING";
        }

        return "CRITICAL";
    }
}