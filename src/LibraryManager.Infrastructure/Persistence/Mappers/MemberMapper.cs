using LibraryManager.Domain.Entities;
using LibraryManager.Domain.Enums;
using LibraryManager.Domain.ValueObjects;
using LibraryManager.Infrastructure.Persistence.Documents;

namespace LibraryManager.Infrastructure.Persistence.Mappers;

internal static class MemberMapper
{
    public static MemberDocument ToDocument(Member member) => new()
    {
        Id          = member.Id,
        Name        = member.Name,
        Email       = member.Email.Value,
        ActiveLoans = member.ActiveLoans,
        Status      = member.Status.ToString()
    };

    public static Member ToEntity(MemberDocument doc) =>
        Member.Reconstitute(
            doc.Id,
            doc.Name,
            new Email(doc.Email),
            doc.ActiveLoans,
            Enum.Parse<MemberStatus>(doc.Status));
}