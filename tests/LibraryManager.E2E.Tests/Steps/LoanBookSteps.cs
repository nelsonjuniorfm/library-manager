using System.Net.Http.Json;
using LibraryManager.E2E.Tests.Support;
using Reqnroll;
using Shouldly;

namespace LibraryManager.E2E.Tests.Steps;

[Binding]
public sealed class LoanBookSteps
{
    // estado compartilhado entre steps do mesmo cenário
    private readonly ScenarioContext _scenario;
    private readonly HttpClient      _client;

    private Guid _bookId;
    private Guid _memberId;
    private Guid _loanId;
    private HttpResponseMessage? _lastResponse;

    public LoanBookSteps(ScenarioContext scenario, E2EFactory factory)
    {
        _scenario = scenario;
        _client   = factory.CreateClient();
    }

    // ── Given ────────────────────────────────────────────────────────────

    [Given("que o livro {string} com ISBN {string} está cadastrado com {int} cópias")]
    [Given("que o livro {string} com ISBN {string} está cadastrado com {int} cópia")]
    public async Task GivenLivroCadastrado(string titulo, string isbn, int copias)
    {
        var response = await _client.PostAsJsonAsync("/books", new
        {
            ISBN        = isbn,
            Title       = titulo,
            Author      = "Autor Teste",
            TotalCopies = copias
        });

        response.IsSuccessStatusCode.ShouldBeTrue(
            $"Falha ao criar livro: {await response.Content.ReadAsStringAsync()}");

        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        _bookId = body!.Id;
    }

    [Given("que o membro {string} com email {string} está cadastrado")]
    public async Task GivenMembroCadastrado(string nome, string email)
    {
        var response = await _client.PostAsJsonAsync("/members", new
        {
            Name  = nome,
            Email = email
        });

        response.IsSuccessStatusCode.ShouldBeTrue(
            $"Falha ao criar membro: {await response.Content.ReadAsStringAsync()}");

        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        _memberId = body!.Id;
    }

    [Given("que a única cópia já está emprestada")]
    public async Task GivenCopiaEmprestada()
    {
        // cria um membro temporário para ocupar a cópia
        var membroTemp = await _client.PostAsJsonAsync("/members", new
        {
            Name  = "Membro Temporário",
            Email = $"temp-{Guid.NewGuid()}@example.com"
        });

        var membroBody = await membroTemp.Content.ReadFromJsonAsync<IdResponse>();

        await _client.PostAsJsonAsync("/loans", new
        {
            BookId   = _bookId,
            MemberId = membroBody!.Id
        });
    }

    [Given("que existe um empréstimo ativo entre um livro e um membro")]
    public async Task GivenEmprestimoAtivo()
    {
        // cria livro
        var bookResp = await _client.PostAsJsonAsync("/books", new
        {
            ISBN        = "978-3-16-148410-0",
            Title       = "Livro E2E",
            Author      = "Autor E2E",
            TotalCopies = 1
        });
        var bookBody = await bookResp.Content.ReadFromJsonAsync<IdResponse>();
        _bookId = bookBody!.Id;

        // cria membro
        var memberResp = await _client.PostAsJsonAsync("/members", new
        {
            Name  = "Membro E2E",
            Email = $"e2e-{Guid.NewGuid()}@example.com"
        });
        var memberBody = await memberResp.Content.ReadFromJsonAsync<IdResponse>();
        _memberId = memberBody!.Id;

        // cria o empréstimo
        var loanResp = await _client.PostAsJsonAsync("/loans", new
        {
            BookId   = _bookId,
            MemberId = _memberId
        });
        var loanBody = await loanResp.Content.ReadFromJsonAsync<LoanResponse>();
        _loanId = loanBody!.LoanId;
    }

    // ── When ─────────────────────────────────────────────────────────────

    [When("o membro solicita o empréstimo do livro")]
    public async Task WhenSolicitaEmprestimo()
    {
        _lastResponse = await _client.PostAsJsonAsync("/loans", new
        {
            BookId   = _bookId,
            MemberId = _memberId
        });
    }

    [When("o membro devolve o livro")]
    public async Task WhenDevolveLivro()
    {
        _lastResponse = await _client.PostAsJsonAsync("/loans/return", new
        {
            LoanId = _loanId
        });
    }

    // ── Then ─────────────────────────────────────────────────────────────

    [Then("o empréstimo é criado com sucesso")]
    public async Task ThenEmprestimoCriado()
    {
        _lastResponse!.StatusCode
            .ShouldBe(System.Net.HttpStatusCode.Created,
                await _lastResponse.Content.ReadAsStringAsync());

        var body = await _lastResponse.Content.ReadFromJsonAsync<LoanResponse>();
        _loanId = body!.LoanId;
        _loanId.ShouldNotBe(Guid.Empty);
    }

    [Then("o livro passa a ter {int} cópia disponível")]
    public async Task ThenCopiaDisponivel(int copias)
    {
        var response = await _client.GetAsync($"/books?term=Clean Code");
        var books    = await response.Content.ReadFromJsonAsync<List<BookResponse>>();

        books!.First().AvailableCopies.ShouldBe(copias);
    }

    [Then("o empréstimo fica com status {string}")]
    public void ThenEmprestimoRetornado(string status)
    {
        _lastResponse!.StatusCode
            .ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Then("o livro recupera a cópia disponível")]
    public async Task ThenCopiaLiberada()
    {
        var response = await _client.GetAsync($"/books?term=Livro E2E");
        var books    = await response.Content.ReadFromJsonAsync<List<BookResponse>>();

        books!.First().AvailableCopies.ShouldBe(1);
    }

    [Then("o sistema rejeita o empréstimo com erro de disponibilidade")]
    public void ThenEmprestimoRejeitado()
    {
        _lastResponse!.StatusCode
            .ShouldBe(System.Net.HttpStatusCode.UnprocessableEntity);
    }

    // ── DTOs de resposta ──────────────────────────────────────────────────

    private record IdResponse(Guid Id);
    private record LoanResponse(Guid LoanId);
    private record BookResponse(Guid Id, string Title, string Author,
        string ISBN, int AvailableCopies, int TotalCopies);
}