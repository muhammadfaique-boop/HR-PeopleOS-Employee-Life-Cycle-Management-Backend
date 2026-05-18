using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PeopleOS.Api.Tests;

public class PeopleOsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PeopleOsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
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
    public async Task Attendance_ReturnsRecordsAndCorrectionQueue()
    {
        var json = await _client.GetFromJsonAsync<JsonObject>("/api/peopleos/attendance?employeeId=2");

        Assert.True(json?["records"]?.AsArray().Count >= 3);
        Assert.Equal("Update check-in to 09:15", json?["corrections"]?[0]?["requestedChange"]?.GetValue<string>());
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
