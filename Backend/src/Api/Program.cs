using MyTarotReader.Api.Extensions;
using MyTarotReader.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Must run before AddAllServices: JWT, Redis, the connection string and CORS are read
// eagerly while the services are registered.
var secretFilePath = builder.Configuration.GetSecretFilePath(builder.Environment);
var secretFileLoaded =
    secretFilePath is not null
    && builder.Configuration.AddSecretFileIfExists(secretFilePath);

builder.Services.AddAllServices(builder.Configuration, builder.Environment);
builder.Services.AddHealthChecks();

var app = builder.Build();

if (secretFilePath is not null)
{
    app.Logger.LogInformation(
        secretFileLoaded
            ? "Loaded secret settings from {Path}."
            : "No secret settings file at {Path}; using appsettings.json and environment variables.",
        secretFilePath
    );
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
