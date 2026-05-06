using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManager.Infrastructure.Persistence.Documents;

public class LoanDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid BookId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid MemberId { get; set; }

    public DateTime BorrowedAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}