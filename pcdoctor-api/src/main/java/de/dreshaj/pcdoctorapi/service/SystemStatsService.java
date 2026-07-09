package de.dreshaj.pcdoctorapi.service;

import de.dreshaj.pcdoctorapi.dto.SystemStatsDto;
import de.dreshaj.pcdoctorapi.dto.SystemStatsResponseDto;
import de.dreshaj.pcdoctorapi.model.DeviceEntity;
import de.dreshaj.pcdoctorapi.model.SystemStatsEntity;
import de.dreshaj.pcdoctorapi.repository.SystemStatsRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.List;

@Service
public class SystemStatsService {

    private final SystemStatsRepository systemStatsRepository;
    private final DeviceService deviceService;

    public SystemStatsService(SystemStatsRepository systemStatsRepository, DeviceService deviceService) {
        this.systemStatsRepository = systemStatsRepository;
        this.deviceService = deviceService;
    }

    @Transactional
    public void saveStats(String deviceToken, SystemStatsDto stats) {
        validateStatsRequest(deviceToken, stats);

        DeviceEntity device = deviceService.getDeviceByToken(deviceToken);

        SystemStatsEntity entity = new SystemStatsEntity();
        entity.setDevice(device);
        entity.setCpuUsage(stats.getCpuUsage());
        entity.setUsedMemoryMb(stats.getUsedMemoryMb());
        entity.setTotalMemoryMb(stats.getTotalMemoryMb());

        device.setLastSeenAt(LocalDateTime.now());

        systemStatsRepository.save(entity);
    }

    public SystemStatsResponseDto getLatestStats(Long deviceId) {
        SystemStatsEntity latestStats = getLatestStatsEntity(deviceId);

        return mapToResponseDto(latestStats);
    }

    public List<SystemStatsResponseDto> getHistory(Long deviceId) {
        return systemStatsRepository
                .findTop50ByDeviceIdOrderByCreatedAtDesc(deviceId)
                .stream()
                .map(this::mapToResponseDto)
                .toList();
    }

    private SystemStatsEntity getLatestStatsEntity(Long deviceId) {
        return systemStatsRepository
                .findTop1ByDeviceIdOrderByCreatedAtDesc(deviceId)
                .orElseThrow(() -> new RuntimeException("No stats found for device with id: " + deviceId));
    }

    private SystemStatsResponseDto mapToResponseDto(SystemStatsEntity entity) {
        return new SystemStatsResponseDto(
                entity.getId(),
                entity.getCpuUsage(),
                entity.getUsedMemoryMb(),
                entity.getTotalMemoryMb(),
                entity.getCreatedAt()
        );
    }

    private void validateStatsRequest(String deviceToken, SystemStatsDto stats) {
        if (deviceToken == null || deviceToken.isBlank()) {
            throw new IllegalArgumentException("Device token is required.");
        }

        if (stats == null) {
            throw new IllegalArgumentException("System stats are required.");
        }

        if (stats.getCpuUsage() < 0 || stats.getCpuUsage() > 100) {
            throw new IllegalArgumentException("CPU usage must be between 0 and 100.");
        }

        if (stats.getUsedMemoryMb() < 0) {
            throw new IllegalArgumentException("Used memory must not be negative.");
        }

        if (stats.getTotalMemoryMb() <= 0) {
            throw new IllegalArgumentException("Total memory must be greater than 0.");
        }

        if (stats.getUsedMemoryMb() > stats.getTotalMemoryMb()) {
            throw new IllegalArgumentException("Used memory must not be greater than total memory.");
        }
    }
}