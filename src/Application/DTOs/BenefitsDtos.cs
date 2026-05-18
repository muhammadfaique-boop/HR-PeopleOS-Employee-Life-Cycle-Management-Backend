namespace PeopleOS.Api.Application.DTOs;

public record BenefitPlanResponseDto(int Id, string Name, string Category, string Coverage, string Status, string Description);
public record PolicyDocumentResponseDto(int Id, string Title, string Category, string Version, DateOnly PublishedOn);
