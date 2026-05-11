using Scalar.AspNetCore;
using LibraryManager.Api.Endpoints;
using LibraryManager.Api.Middleware;
using LibraryManager.Application.UseCases.BorrowBook;
using LibraryManager.Application.UseCases.ReturnBook;
using LibraryManager.Application.UseCases.SearchBooks;
using LibraryManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

// Application — handlers
builder.Services.AddScoped<BorrowBookHandler>();
builder.Services.AddScoped<ReturnBookHandler>();
builder.Services.AddScoped<SearchBooksHandler>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Endpoints
app.MapBookEndpoints();
app.MapMemberEndpoints();
app.MapLoanEndpoints();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("", options =>
    {
        options.Title = "Library Manager API";
        options.Theme = ScalarTheme.BluePlanet;
    });
}

app.UseHttpsRedirection();

app.Run();

// Necessário para o WebApplicationFactory nos testes de integração
public partial class Program { }