using MyTarotReader.Api.Extensions;
using MyTarotReader.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAllServices(builder.Configuration, builder.Environment);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (builder.Environment.IsProduction())
{
    var secretFilePath = "/etc/secrets/appsettings.Production.json";
    if (File.Exists(secretFilePath))
    {
        builder.Configuration.AddJsonFile(secretFilePath, optional: false, reloadOnChange: true);
    }
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors(CorsExtension.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").RequireCors(CorsExtension.HealthPolicyName);

app.Run();
