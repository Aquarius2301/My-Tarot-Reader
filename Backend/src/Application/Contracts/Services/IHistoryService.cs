using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Application.Contracts.Services;

public interface IHistoryService
{
    /// <summary>
    /// Retrieves a list of active (not deleted) tarot card read history items for a specific user,
    /// ordered by creation date.
    /// </summary>
    /// <param name="userId">The ID of the user whose history to retrieve.</param>
    /// <returns>A list of <see cref="GetHistoryResponse"/> objects.</returns>
    Task<GetHistoryResponse> GetHistoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Soft-deletes a read history record by setting DeletedAt to the current UTC time.
    /// </summary>
    /// <param name="userId">The ID of the user who owns the history record.</param>
    /// <param name="historyId">The ID of the history record to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteHistoryAsync(
        Guid userId,
        Guid historyId,
        CancellationToken cancellationToken = default
    );
}
