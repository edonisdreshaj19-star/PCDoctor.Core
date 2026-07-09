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
}