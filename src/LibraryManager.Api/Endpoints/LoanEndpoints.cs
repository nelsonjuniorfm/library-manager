using LibraryManager.Api.Requests;
using LibraryManager.Application.UseCases.BorrowBook;
using LibraryManager.Application.UseCases.ReturnBook;

namespace LibraryManager.Api.Endpoints;

public static class LoanEndpoints
{
    public static IEndpointRouteBuilder MapLoanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/loans");

        group.MapPost("/", async (BorrowBookRequest req, BorrowBookHandler handler, CancellationToken ct) =>
        {
            var loanId = await handler.Handle(
                new BorrowBookCommand(req.BookId, req.MemberId), ct);

            return Results.Created($"/loans/{loanId}", new { loanId });
        });

        group.MapPost("/return", async (ReturnBookRequest req, ReturnBookHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new ReturnBookCommand(req.LoanId), ct);

            return Results.Ok();
        });

        return app;
    }
}