using LibraryManager.Api.Requests;
using LibraryManager.Application.UseCases.SearchBooks;
using LibraryManager.Domain.Entities;
using LibraryManager.Domain.Interfaces;
using LibraryManager.Domain.ValueObjects;

namespace LibraryManager.Api.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/books");

        group.MapPost("/", async (CreateBookRequest req, IBookRepository repo, CancellationToken ct) =>
        {
            var book = new Book(
                new ISBN(req.ISBN),
                req.Title,
                req.Author,
                req.TotalCopies);

            await repo.AddAsync(book, ct);

            return Results.Created($"/books/{book.Id}", new { id = book.Id });
        });

        group.MapGet("/", async (string term, SearchBooksHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new SearchBooksQuery(term), ct);
            return Results.Ok(result);
        });

        return app;
    }
}