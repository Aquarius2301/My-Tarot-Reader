using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;

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
            .ReadHistories.Where(r => r.UserId == userId && r.DeletedAt == null)
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
        var record = await _context.ReadHistories.FirstOrDefaultAsync(
            r => r.Id == historyId && r.UserId == userId && r.DeletedAt == null,
            cancellationToken
        );

        if (record is null)
            return;

        record.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
