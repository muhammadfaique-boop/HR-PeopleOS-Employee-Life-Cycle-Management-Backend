namespace PeopleOS.Api.Application.DTOs;

public record LoginRequestDto(string Email, string Password);
public record ChangePasswordRequestDto(string Email, string CurrentPassword, string NewPassword);
public record PermissionGrantResponseDto(string Key, string Scope);
public record LoginResponseDto(string Token, string Email, string Role, string Scope, IReadOnlyList<PermissionGrantResponseDto> Permissions, EmployeeResponseDto? Employee);
public record MessageResponseDto(string Message);
