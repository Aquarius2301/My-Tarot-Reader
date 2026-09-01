namespace MyTarotReader.Application.Contracts.Dtos;

public class ReadHistoryDto
{
    public string CardCode { get; set; } = null!;
    public bool IsReversed { get; set; }
    public DateTime CreatedAt { get; set; }
}
