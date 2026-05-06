using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManager.Infrastructure.Persistence.Documents;

public class MemberDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ActiveLoans { get; set; }
    public string Status { get; set; } = string.Empty;
}