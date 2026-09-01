using MyTarotReader.Api.Extensions;
using MyTarotReader.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add all service (Database, Dependency Injection, etc.)
builder.Services.AddAllExtensions(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors(CorsExtension.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
