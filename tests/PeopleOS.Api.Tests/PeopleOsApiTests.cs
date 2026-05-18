using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PeopleOS.Api.Tests;

public class PeopleOsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PeopleOsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();
    }

    [Fact]
    public async Task Login_WithDemoEmployeeCredentials_ReturnsSession()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "employee@peopleos.dev",
            password = "Employee@123"
        });

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();

        Assert.Equal("Employee", json?["role"]?.GetValue<string>());
        Assert.Equal("Muhammad Faique", json?["employee"]?["fullName"]?.GetValue<string>());
        Assert.StartsWith("demo-token-", json?["token"]?.GetValue<string>());
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "employee@peopleos.dev",
            password = "wrong"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithCurrentPassword_UpdatesDemoPassword()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/change-password", new
        {
            email = "employee@peopleos.dev",
            currentPassword = "Employee@123",
            newPassword = "Employee@1234"
        });

        response.EnsureSuccessStatusCode();

        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "employee@peopleos.dev",
            password = "Employee@1234"
        });

        login.EnsureSuccessStatusCode();

        var reset = await _client.PostAsJsonAsync("/api/auth/change-password", new
        {
            email = "employee@peopleos.dev",
            currentPassword = "Employee@1234",
            newPassword = "Employee@123"
        });

        reset.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Dashboard_ReturnsLifecycleMetricsAndExcludedScopeData()
    {
        var json = await _client.GetFromJsonAsync<JsonObject>("/api/peopleos/dashboard");

        Assert.Equal("Active employees", json?["metrics"]?[0]?["label"]?.GetValue<string>());
        Assert.Equal("2", json?["metrics"]?[0]?["value"]?.GetValue<string>());
        Assert.Contains(RequiredArray(json, "lifecycle"), item =>
            item?["stage"]?.GetValue<string>() == "Probation review");
        Assert.Contains(RequiredArray(json, "approvals"), item =>
            item?["type"]?.GetValue<string>() == "Document");
    }

    [Fact]
    public async Task Dashboard_ReturnsDatabaseBackedHrHubData()
    {
        var json = await _client.GetFromJsonAsync<JsonObject>("/api/peopleos/dashboard");

        Assert.Contains(RequiredArray(json, "holidays"), item =>
            item?["name"]?.GetValue<string>() == "Eid Holiday");
        Assert.Contains(RequiredArray(json, "announcements"), item =>
            item?["title"]?.GetValue<string>() == "Policy refresh");
        Assert.Contains(RequiredArray(json, "quickActions"), item =>
            item?["target"]?.GetValue<string>() == "leave");
        Assert.Contains(RequiredArray(json, "lifecycleSignals"), item =>
            item?["label"]?.GetValue<string>() == "Documents pending");
        Assert.Contains(RequiredArray(json, "recentActivity"), item =>
            item?.GetValue<string>() == "Attendance sync completed for today");
    }

    [Fact]
    public async Task Attendance_ReturnsRecordsAndCorrectionQueue()
    {
        var json = await _client.GetFromJsonAsync<JsonObject>("/api/peopleos/attendance?employeeId=2");

        Assert.True(json?["records"]?.AsArray().Count >= 3);
        Assert.Equal("Update check-in to 09:15", json?["corrections"]?[0]?["requestedChange"]?.GetValue<string>());
    }

    [Fact]
    public async Task AttendanceDownloadPdf_ReturnsValidPdfWithEmployeeFileName()
    {
        var response = await _client.GetAsync("/api/peopleos/attendance/download/pdf?employeeId=2");

        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var header = System.Text.Encoding.ASCII.GetString(bytes.Take(8).ToArray());

        Assert.StartsWith("%PDF", header);
        Assert.Contains("Login_UserId_2.Attendance log.pdf", response.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task Leave_ReturnsBalancesAndRequestHistory()
    {
        var json = await _client.GetFromJsonAsync<JsonObject>("/api/peopleos/leave?employeeId=2");

        Assert.Contains(RequiredArray(json, "balances"), item =>
            item?["leaveType"]?.GetValue<string>() == "Annual Leave");
        Assert.Contains(RequiredArray(json, "requests"), item =>
            item?["status"]?.GetValue<string>() == "Approval Required");
    }

    [Fact]
    public async Task EmployeeProfile_ReturnsDocumentsAndLifecycle()
    {
        var json = await _client.GetFromJsonAsync<JsonObject>("/api/peopleos/employees/2");

        Assert.Equal("Muhammad Faique", json?["employee"]?["fullName"]?.GetValue<string>());
        Assert.Contains(RequiredArray(json, "documents"), item =>
            item?["category"]?.GetValue<string>() == "Letters");
    }

    private static JsonArray RequiredArray(JsonObject? json, string propertyName)
    {
        var array = json?[propertyName]?.AsArray();
        Assert.NotNull(array);
        return array;
    }
}
