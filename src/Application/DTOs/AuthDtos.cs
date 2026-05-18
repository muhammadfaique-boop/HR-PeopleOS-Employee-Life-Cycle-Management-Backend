namespace PeopleOS.Api.Application.DTOs;

public record LoginRequestDto(string Email, string Password);
public record ChangePasswordRequestDto(string Email, string CurrentPassword, string NewPassword);
public record LoginResponseDto(string Token, string Email, string Role, EmployeeResponseDto? Employee);
public record MessageResponseDto(string Message);
