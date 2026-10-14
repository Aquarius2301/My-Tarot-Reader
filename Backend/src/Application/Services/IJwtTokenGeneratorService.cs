using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Services;

public interface IJwtTokenGeneratorService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
