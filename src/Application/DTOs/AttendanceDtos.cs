namespace PeopleOS.Api.Application.DTOs;

public record AttendanceRecordResponseDto(int Id, int EmployeeId, DateOnly WorkDate, TimeOnly? CheckIn, TimeOnly? CheckOut, string Status, string Source);
public record AttendanceCorrectionResponseDto(int Id, int EmployeeId, DateOnly WorkDate, string RequestedChange, string Reason, string Status, string Approver);
public record AttendanceResponseDto(IReadOnlyList<AttendanceRecordResponseDto> Records, IReadOnlyList<AttendanceCorrectionResponseDto> Corrections);
public record CreateAttendanceCorrectionRequestDto(int EmployeeId, DateOnly WorkDate, string RequestedChange, string Reason);
public record AttendanceDownloadResponseDto(string FileName, string ContentType, byte[] Content);
