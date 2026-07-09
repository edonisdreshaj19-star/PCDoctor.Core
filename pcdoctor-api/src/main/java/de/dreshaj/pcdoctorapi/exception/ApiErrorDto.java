package de.dreshaj.pcdoctorapi.exception;

import java.time.LocalDateTime;

public class ApiErrorDto {
    private String message;
    private LocalDateTime timestamp;

    public ApiErrorDto(String message) {
        this.message = message;
        this.timestamp = LocalDateTime.now();
    }

    public String getMessage() {
        return message;
    }

    public LocalDateTime getTimestamp() {
        return timestamp;
    }
}
