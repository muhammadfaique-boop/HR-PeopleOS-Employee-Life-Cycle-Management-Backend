namespace PeopleOS.Api.Application.DTOs;

public record EmployeeResponseDto(
    int Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string Department,
    string Position,
    string Manager,
    int? ManagerEmployeeId,
    string LifecycleStatus,
    DateOnly JoiningDate,
    int ProfileCompletion,
    string WorkLocation,
    string PreferredLanguage,
    string ProfileImageUrl);

public record UpdateEmployeeProfileRequestDto(string PreferredLanguage, string ProfileImageUrl);

public record EmployeeDocumentResponseDto(int Id, int EmployeeId, string Name, string Category, string Status, DateOnly UpdatedOn);
public record EmployeeProfileResponseDto(
    EmployeeResponseDto Employee,
    IReadOnlyList<EmployeeDocumentResponseDto> Documents,
    IReadOnlyList<LifecycleStageResponseDto> Lifecycle,
    IReadOnlyList<LeaveBalanceResponseDto> LeaveBalances);

public record LifecycleStageResponseDto(int Id, int EmployeeId, string Stage, string Owner, string Status, DateOnly DueDate, string Summary);
public record ApprovalTaskResponseDto(int Id, string Type, string Subject, string Requester, string ApproverRole, string Status, DateOnly DueDate);

public record DashboardMetricResponseDto(string Label, string Value, string Accent);
public record WhoIsOutResponseDto(string EmployeeName, string LeaveType, DateOnly FromDate, DateOnly ToDate, string Department);
public record HolidayResponseDto(int Id, string Name, DateOnly Date, string Type);
public record AnnouncementResponseDto(int Id, string Title, string Body, DateOnly PublishedOn, string Audience);
public record QuickActionResponseDto(int Id, string Label, string Target, int DisplayOrder);
public record LifecycleSignalResponseDto(int Id, string Label, string Value, string Status, int DisplayOrder);

public record DashboardResponseDto(
    IReadOnlyList<DashboardMetricResponseDto> Metrics,
    EmployeeResponseDto ActiveEmployee,
    IReadOnlyList<EmployeeResponseDto> Employees,
    IReadOnlyList<LifecycleStageResponseDto> Lifecycle,
    IReadOnlyList<ApprovalTaskResponseDto> Approvals,
    IReadOnlyList<WhoIsOutResponseDto> WhoIsOut,
    IReadOnlyList<HolidayResponseDto> Holidays,
    IReadOnlyList<AnnouncementResponseDto> Announcements,
    IReadOnlyList<QuickActionResponseDto> QuickActions,
    IReadOnlyList<LifecycleSignalResponseDto> LifecycleSignals,
    IReadOnlyList<string> RecentActivity);
