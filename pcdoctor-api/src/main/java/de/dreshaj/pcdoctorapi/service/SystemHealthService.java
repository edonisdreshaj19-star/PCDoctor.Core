package de.dreshaj.pcdoctorapi.service;

import de.dreshaj.pcdoctorapi.dto.ProcessStatsDto;
import de.dreshaj.pcdoctorapi.dto.SystemHealthResponseDto;
import de.dreshaj.pcdoctorapi.dto.SystemStatsResponseDto;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.Locale;

@Service
public class SystemHealthService {

    private static final double CPU_WARNING_THRESHOLD = 75;
    private static final double CPU_CRITICAL_THRESHOLD = 90;

    private static final double MEMORY_WARNING_THRESHOLD = 80;
    private static final double MEMORY_CRITICAL_THRESHOLD = 90;

    private static final double DISK_WARNING_THRESHOLD = 80;
    private static final double DISK_CRITICAL_THRESHOLD = 90;

    private static final double HEAVY_PROCESS_MEMORY_MB = 1000;

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

        double memoryUsagePercent = calculateUsagePercent(
                latestStats.getUsedMemoryMb(),
                latestStats.getTotalMemoryMb()
        );

        double diskUsagePercent = calculateUsagePercent(
                latestStats.getUsedDiskGb(),
                latestStats.getTotalDiskGb()
        );

        score -= analyzeCpuUsage(cpuUsage, reasons, recommendations);
        score -= analyzeMemoryUsage(memoryUsagePercent, reasons, recommendations);
        score -= analyzeDiskUsage(
                diskUsagePercent,
                latestStats.getTotalDiskGb(),
                reasons,
                recommendations
        );
        score -= analyzeTopProcesses(
                latestStats.getTopProcesses(),
                memoryUsagePercent,
                reasons,
                recommendations
        );

        score = Math.max(score, 0);

        if (reasons.isEmpty()) {
            reasons.add("System performance looks normal.");
            recommendations.add("No immediate action required.");
        }

        String status = determineStatus(
                score,
                cpuUsage,
                memoryUsagePercent,
                diskUsagePercent
        );

        return new SystemHealthResponseDto(
                score,
                status,
                reasons,
                recommendations
        );
    }

    private int analyzeCpuUsage(
            double cpuUsage,
            List<String> reasons,
            List<String> recommendations
    ) {
        if (cpuUsage >= CPU_CRITICAL_THRESHOLD) {
            reasons.add("CPU usage is critically high at " + formatPercent(cpuUsage) + ".");
            recommendations.add("Check the top processes and close applications that are using too much CPU.");
            return 30;
        }

        if (cpuUsage >= CPU_WARNING_THRESHOLD) {
            reasons.add("CPU usage is elevated at " + formatPercent(cpuUsage) + ".");
            recommendations.add("Close unnecessary background applications if the system feels slow.");
            return 15;
        }

        return 0;
    }

    private int analyzeMemoryUsage(
            double memoryUsagePercent,
            List<String> reasons,
            List<String> recommendations
    ) {
        if (memoryUsagePercent >= MEMORY_CRITICAL_THRESHOLD) {
            reasons.add("Memory usage is critically high at " + formatPercent(memoryUsagePercent) + ".");
            recommendations.add("Close memory-heavy applications or restart unused services.");
            return 35;
        }

        if (memoryUsagePercent >= MEMORY_WARNING_THRESHOLD) {
            reasons.add("Memory usage is elevated at " + formatPercent(memoryUsagePercent) + ".");
            recommendations.add("Review memory-heavy applications and close anything you do not need.");
            return 15;
        }

        return 0;
    }

    private int analyzeDiskUsage(
            double diskUsagePercent,
            double totalDiskGb,
            List<String> reasons,
            List<String> recommendations
    ) {
        if (totalDiskGb <= 0) {
            return 0;
        }

        if (diskUsagePercent >= DISK_CRITICAL_THRESHOLD) {
            reasons.add("Disk usage is critically high at " + formatPercent(diskUsagePercent) + ".");
            recommendations.add("Free up disk space by removing temporary files, unused programs, or large downloads.");
            return 25;
        }

        if (diskUsagePercent >= DISK_WARNING_THRESHOLD) {
            reasons.add("Disk usage is elevated at " + formatPercent(diskUsagePercent) + ".");
            recommendations.add("Consider cleaning temporary files and moving large files to external storage.");
            return 10;
        }

        return 0;
    }

    private int analyzeTopProcesses(
            List<ProcessStatsDto> topProcesses,
            double memoryUsagePercent,
            List<String> reasons,
            List<String> recommendations
    ) {
        if (topProcesses == null || topProcesses.isEmpty()) {
            return 0;
        }

        List<ProcessStatsDto> memoryHeavyProcesses = topProcesses.stream()
                .filter(process -> process.getMemoryUsageMb() >= HEAVY_PROCESS_MEMORY_MB)
                .sorted(Comparator.comparingDouble(ProcessStatsDto::getMemoryUsageMb).reversed())
                .toList();

        if (memoryHeavyProcesses.isEmpty()) {
            return 0;
        }

        ProcessStatsDto heaviestProcess = memoryHeavyProcesses.get(0);

        reasons.add(
                "The process " + heaviestProcess.getProcessName()
                        + " is using " + formatMemory(heaviestProcess.getMemoryUsageMb())
                        + " of memory."
        );

        recommendations.add(
                "Review " + heaviestProcess.getProcessName()
                        + " and close or restart it if it is not needed."
        );

        int penalty = memoryUsagePercent >= MEMORY_WARNING_THRESHOLD ? 10 : 5;

        if (memoryHeavyProcesses.size() >= 2) {
            reasons.add(memoryHeavyProcesses.size() + " processes are using more than 1 GB of memory.");
            recommendations.add("Close unused memory-heavy applications to free up RAM.");
            penalty += 5;
        }

        if (memoryHeavyProcesses.size() >= 4) {
            penalty += 5;
        }

        return penalty;
    }

    private double calculateUsagePercent(double usedValue, double totalValue) {
        if (totalValue <= 0) {
            return 0;
        }

        return (usedValue / totalValue) * 100;
    }

    private String determineStatus(
            int score,
            double cpuUsage,
            double memoryUsagePercent,
            double diskUsagePercent
    ) {
        if (
                cpuUsage >= CPU_CRITICAL_THRESHOLD
                        || memoryUsagePercent >= MEMORY_CRITICAL_THRESHOLD
                        || diskUsagePercent >= DISK_CRITICAL_THRESHOLD
        ) {
            return "CRITICAL";
        }

        if (score >= 90) {
            return "HEALTHY";
        }

        if (score >= 70) {
            return "WARNING";
        }

        return "CRITICAL";
    }

    private String formatPercent(double value) {
        return String.format(Locale.US, "%.1f%%", value);
    }

    private String formatMemory(double value) {
        return String.format(Locale.US, "%.0f MB", value);
    }
}