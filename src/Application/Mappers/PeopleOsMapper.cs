using PeopleOS.Api.Application.DTOs;
using PeopleOS.Api.Domain.Entities;

namespace PeopleOS.Api.Application.Mappers;

public static class PeopleOsMapper
{
    public static EmployeeResponseDto ToResponse(this Employee entity) =>
        new(entity.Id, entity.EmployeeCode, entity.FullName, entity.Email, entity.Department, entity.Position, entity.Manager, entity.ManagerEmployeeId, entity.LifecycleStatus, entity.JoiningDate, entity.ProfileCompletion, entity.WorkLocation, entity.PreferredLanguage, entity.ProfileImageUrl);

    public static LifecycleStageResponseDto ToResponse(this LifecycleStage entity) =>
        new(entity.Id, entity.EmployeeId, entity.Stage, entity.Owner, entity.Status, entity.DueDate, entity.Summary);

    public static ApprovalTaskResponseDto ToResponse(this ApprovalTask entity) =>
        new(entity.Id, entity.Type, entity.Subject, entity.Requester, entity.ApproverRole, entity.Status, entity.DueDate);

    public static AttendanceRecordResponseDto ToResponse(this AttendanceRecord entity) =>
        new(entity.Id, entity.EmployeeId, entity.WorkDate, entity.CheckIn, entity.CheckOut, entity.Status, entity.Source);

    public static AttendanceCorrectionResponseDto ToResponse(this AttendanceCorrection entity) =>
        new(entity.Id, entity.EmployeeId, entity.WorkDate, entity.RequestedChange, entity.Reason, entity.Status, entity.Approver);

    public static LeaveBalanceResponseDto ToResponse(this LeaveBalance entity) =>
        new(entity.Id, entity.EmployeeId, entity.LeaveType, entity.AnnualEntitlement, entity.AvailableBalance);

    public static LeaveRequestResponseDto ToResponse(this LeaveRequest entity) =>
        new(entity.Id, entity.EmployeeId, entity.LeaveType, entity.FromDate, entity.ToDate, entity.TotalDays, entity.Reason, entity.ContactDuringLeave, entity.AttachmentFileName, entity.AttachmentDataUrl, entity.Status);

    public static BenefitPlanResponseDto ToResponse(this BenefitPlan entity) =>
        new(entity.Id, entity.Name, entity.Category, entity.Coverage, entity.Status, entity.Description);

    public static EmployeeDocumentResponseDto ToResponse(this EmployeeDocument entity) =>
        new(entity.Id, entity.EmployeeId, entity.Name, entity.Category, entity.Status, entity.UpdatedOn);

    public static PolicyDocumentResponseDto ToResponse(this PolicyDocument entity) =>
        new(entity.Id, entity.Title, entity.Category, entity.Version, entity.PublishedOn);

    public static ExpenseClaimResponseDto ToResponse(this ExpenseClaim entity) =>
        new(entity.Id, entity.EmployeeId, entity.ClaimType, entity.Category, entity.Amount, entity.ExpenseDate, entity.Description, entity.ReceiptFileName, entity.ReceiptDataUrl, entity.Status, entity.LineManager);

    public static ResignationResponseDto ToResponse(this ResignationRequest entity) =>
        new(entity.Id, entity.EmployeeId, entity.ResignationDate, entity.LastWorkingDate, entity.Reason, entity.Status, entity.LineManager);

    public static HolidayResponseDto ToResponse(this Holiday entity) =>
        new(entity.Id, entity.Name, entity.Date, entity.Type);

    public static AnnouncementResponseDto ToResponse(this Announcement entity) =>
        new(entity.Id, entity.Title, entity.Body, entity.PublishedOn, entity.Audience);

    public static QuickActionResponseDto ToResponse(this QuickAction entity) =>
        new(entity.Id, entity.Label, entity.Target, entity.DisplayOrder);

    public static LifecycleSignalResponseDto ToResponse(this LifecycleSignal entity) =>
        new(entity.Id, entity.Label, entity.Value, entity.Status, entity.DisplayOrder);
}
