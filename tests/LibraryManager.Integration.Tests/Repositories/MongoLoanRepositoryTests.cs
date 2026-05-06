using LibraryManager.Domain.Enums;
using LibraryManager.Infrastructure.Persistence;
using LibraryManager.Integration.Tests.Fixtures;
using LibraryManager.TestHelpers.Fakers;
using Shouldly;

namespace LibraryManager.Integration.Tests.Repositories;

[Collection("MongoDB")]
public class MongoLoanRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbFixture _fixture;
    private readonly MongoLoanRepository _sut;

    public MongoLoanRepositoryTests(MongoDbFixture fixture)
    {
        _fixture = fixture;
        _sut = new MongoLoanRepository(fixture.Database);
    }

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync()    => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_WhenCalled_PersistsLoan()
    {
        var loan = LoanFaker.Active();

        await _sut.AddAsync(loan);
        var found = await _sut.GetByIdAsync(loan.Id);

        found.ShouldNotBeNull();
        found.BookId.ShouldBe(loan.BookId);
        found.Status.ShouldBe(LoanStatus.Active);
    }

    [Fact]
    public async Task UpdateAsync_AfterReturn_PersistsReturnedStatus()
    {
        var loan = LoanFaker.Active();
        await _sut.AddAsync(loan);

        loan.Return(DateTime.UtcNow);
        await _sut.UpdateAsync(loan);

        var found = await _sut.GetByIdAsync(loan.Id);
        found!.Status.ShouldBe(LoanStatus.Returned);
        found.ReturnedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetActiveLoansByMemberAsync_ReturnsOnlyActiveLoans()
    {
        var memberId  = Guid.NewGuid();
        var activeLoan  = LoanFaker.Active(memberId: memberId);
        var returnedLoan = LoanFaker.Active(memberId: memberId);

        await _sut.AddAsync(activeLoan);
        await _sut.AddAsync(returnedLoan);

        returnedLoan.Return(DateTime.UtcNow);
        await _sut.UpdateAsync(returnedLoan);

        var active = await _sut.GetActiveLoansByMemberAsync(memberId);

        active.ShouldHaveSingleItem();
        active.First().Id.ShouldBe(activeLoan.Id);
    }
}