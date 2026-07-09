package de.dreshaj.pcdoctorapi.dto;

import java.util.List;

public class SystemHealthResponseDto {
    private int score;
    private String status;
    private List<String> reasons;
    private List<String> recommendations;

    public SystemHealthResponseDto(
            int score,
            String status,
            List<String> reasons,
            List<String> recommendations
    ) {
        this.score = score;
        this.status = status;
        this.reasons = reasons;
        this.recommendations = recommendations;
    }

    public int getScore() {
        return score;
    }

    public String getStatus() {
        return status;
    }

    public List<String> getReasons() {
        return reasons;
    }

    public List<String> getRecommendations() {
        return recommendations;
    }
}