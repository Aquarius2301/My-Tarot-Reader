using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;

namespace MyTarotReader.Infrastructure.Services;

public class HistoryService : IHistoryService
{
    private readonly IAppDbContext _context;

    public HistoryService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<GetHistoryResponse> GetHistoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var history = await _context
            .ReadHistories.Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new HistoryResult(r.Id, r.CardCode, r.IsReversed, r.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetHistoryResponse(history);
    }

    public async Task DeleteHistoryAsync(
        Guid userId,
        Guid historyId,
        CancellationToken cancellationToken = default
    )
    {
        var record =
            await _context.ReadHistories.FirstOrDefaultAsync(
                r => r.Id == historyId && r.UserId == userId,
                cancellationToken
            ) ?? throw new NotFoundException(ErrorMessageCode.History.NotFound);

        record.DeletedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<AIReadHistoryResult>> GetAllAiReadHistoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AIReadHistories.Where(h => h.UserId == userId && h.DeletedAt == null)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new AIReadHistoryResult(
                h.Id,
                h.CardCount,
                h.QuestionType,
                h.Answer,
                h.Cards,
                h.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAiReadHistoryAsync(
        Guid userId,
        Guid historyId,
        CancellationToken cancellationToken = default
    )
    {
        var entity =
            await _context.AIReadHistories.FirstOrDefaultAsync(
                h => h.Id == historyId && h.UserId == userId,
                cancellationToken
            ) ?? throw new NotFoundException(ErrorMessageCode.History.NotFound);

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
