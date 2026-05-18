namespace PeopleOS.Api.Application.DTOs;

public record ResignationResponseDto(int Id, int EmployeeId, DateOnly ResignationDate, DateOnly LastWorkingDate, string Reason, string Status, string LineManager);
public record CreateResignationRequestDto(int EmployeeId, DateOnly LastWorkingDate, string Reason);
