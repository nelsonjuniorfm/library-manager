using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManager.Infrastructure.Persistence.Documents;

public class BookDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string ISBN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}