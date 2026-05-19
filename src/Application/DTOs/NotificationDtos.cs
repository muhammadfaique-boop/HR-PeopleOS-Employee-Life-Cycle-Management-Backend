namespace PeopleOS.Api.Application.DTOs;

public record EmployeeNotificationResponseDto(
    int Id,
    int EmployeeId,
    string Title,
    string Body,
    string Tone,
    bool IsRead,
    DateTime CreatedAt);
