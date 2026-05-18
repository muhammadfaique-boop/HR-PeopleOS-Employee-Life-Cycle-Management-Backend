using Microsoft.EntityFrameworkCore;
using PeopleOS.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<PeopleOsDbContext>(options =>
    options.UseInMemoryDatabase("PeopleOS"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("PeopleOSFrontend", policy =>
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
                origin.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PeopleOsDbContext>();
    db.Database.EnsureCreated();
    PeopleOsSeed.Seed(db);
}

app.UseCors("PeopleOSFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
