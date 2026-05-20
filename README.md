# LibraryManager

[![Coverage](https://nelsonjuniorfm.github.io/library-manager/coverage/badge_linecoverage.svg)](https://nelsonjuniorfm.github.io/library-manager/coverage/)

> **Projeto de referência** para estudo e adoção das principais estratégias de teste em aplicações .NET — testes unitários, de integração, E2E com BDD e análise de cobertura de código — aplicadas em um domínio real com arquitetura em camadas.

Sistema de gestão de biblioteca construído com **.NET 10** seguindo os princípios de **Domain-Driven Design**, com cobertura completa de testes e pipeline CI/CD automatizado.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core Minimal API |
| Banco de dados | MongoDB |
| Testes unitários | xUnit · NSubstitute · Shouldly · Bogus |
| Testes de integração | Testcontainers · WebApplicationFactory |
| Testes E2E / BDD | Reqnroll · Gherkin |
| Cobertura | Coverlet · ReportGenerator |
| CI/CD | GitHub Actions |

---

## Arquitetura

O projeto segue uma arquitetura em camadas inspirada em Clean Architecture, onde as dependências apontam sempre para dentro — do framework para o domínio, nunca o contrário.

```
┌─────────────────────────────────────────────────┐
│                      Api                        │
│         Minimal API · Endpoints · Middleware     │
└────────────────────┬────────────────────────────┘
                     │ depende de
┌────────────────────▼────────────────────────────┐
│                  Application                    │
│         Use Cases · Commands · Handlers         │
└────────────────────┬────────────────────────────┘
                     │ depende de
┌────────────────────▼────────────────────────────┐
│                    Domain                       │
│    Entities · Value Objects · Interfaces        │
│         Exceptions · Enums · Regras             │
└─────────────────────────────────────────────────┘
                     ▲
                     │ implementa
┌────────────────────┴────────────────────────────┐
│                Infrastructure                   │
│     MongoDB Repositories · Mappers · Documents  │
└─────────────────────────────────────────────────┘
```

> O `Domain` não possui nenhuma dependência externa — apenas C# puro.

---

## Domínio

O sistema gerencia três agregados principais e suas interações:

```
┌─────────────────┐         ┌─────────────────────┐
│      Book       │         │       Member        │
│─────────────────│         │─────────────────────│
│ ISBN            │         │ Name                │
│ Title           │         │ Email               │
│ Author          │         │ ActiveLoans         │
│ TotalCopies     │         │ Status              │
│ AvailableCopies │         │─────────────────────│
│─────────────────│         │ CanBorrow()         │
│ Reserve()       │         │ IncrementLoans()    │
│ Release()       │         │ DecrementLoans()    │
└────────┬────────┘         └──────────┬──────────┘
         │                             │
         └──────────┬──────────────────┘
                    │
         ┌──────────▼──────────┐
         │        Loan         │
         │─────────────────────│
         │ BookId              │
         │ MemberId            │
         │ BorrowedAt          │
         │ DueDate             │
         │ ReturnedAt?         │
         │ Status              │
         │─────────────────────│
         │ Return()            │
         │ IsOverdue()         │
         │ MarkAsOverdue()     │
         └─────────────────────┘
```

### Regras de negócio

- Membro não pode ter mais de **3 empréstimos ativos**
- Livro sem cópias disponíveis **não pode ser emprestado**
- Membro com status **Suspended** não pode pegar livros
- Devolução após `DueDate` marca o empréstimo como **Overdue**

---

## Fluxo de empréstimo

```
Cliente           API              Application           Domain          MongoDB
   │                │                    │                  │               │
   │  POST /loans   │                    │                  │               │
   │───────────────►│                    │                  │               │
   │                │  BorrowBookCommand │                  │               │
   │                │───────────────────►│                  │               │
   │                │                    │  GetByIdAsync    │               │
   │                │                    │─────────────────────────────────►│
   │                │                    │◄─────────────────────────────────│
   │                │                    │  member.CanBorrow()              │
   │                │                    │─────────────────►│               │
   │                │                    │  book.Reserve()  │               │
   │                │                    │─────────────────►│               │
   │                │                    │  new Loan(...)   │               │
   │                │                    │─────────────────►│               │
   │                │                    │  AddAsync(loan)  │               │
   │                │                    │─────────────────────────────────►│
   │  201 { loanId }│                    │                  │               │
   │◄───────────────│                    │                  │               │
```

---

## Estrutura do projeto

```
LibraryManager/
├── src/
│   ├── LibraryManager.Domain/
│   │   ├── Entities/          Book · Member · Loan
│   │   ├── ValueObjects/      ISBN · Email
│   │   ├── Enums/             LoanStatus · MemberStatus
│   │   ├── Exceptions/        DomainException e especializações
│   │   └── Interfaces/        IBookRepository · IMemberRepository · ILoanRepository
│   ├── LibraryManager.Application/
│   │   ├── Common/            IHandler<TRequest, TResponse>
│   │   └── UseCases/
│   │       ├── BorrowBook/    BorrowBookCommand · BorrowBookHandler
│   │       ├── ReturnBook/    ReturnBookCommand · ReturnBookHandler
│   │       └── SearchBooks/   SearchBooksQuery · SearchBooksHandler · BookResult
│   ├── LibraryManager.Infrastructure/
│   │   └── Persistence/
│   │       ├── Documents/     BookDocument · MemberDocument · LoanDocument
│   │       ├── Mappers/       BookMapper · MemberMapper · LoanMapper
│   │       └── Repositories/  MongoBookRepository · MongoMemberRepository · MongoLoanRepository
│   └── LibraryManager.Api/
│       ├── Endpoints/         BookEndpoints · MemberEndpoints · LoanEndpoints
│       ├── Middleware/        ExceptionMiddleware
│       ├── Requests/          CreateBookRequest · BorrowBookRequest · ReturnBookRequest
│       └── Program.cs
└── tests/
    ├── LibraryManager.TestHelpers/        Fakers compartilhados entre suítes
    ├── LibraryManager.Unit.Tests/         Domínio + Application
    ├── LibraryManager.Integration.Tests/  Repositórios + API
    └── LibraryManager.E2E.Tests/          Fluxos completos com Reqnroll
```

---

## Endpoints da API

| Método | Rota | Body | Resposta |
|---|---|---|---|
| `POST` | `/books` | `{ isbn, title, author, totalCopies }` | `201 { id }` |
| `GET` | `/books?term=` | — | `200 [ BookResult ]` |
| `POST` | `/members` | `{ name, email }` | `201 { id }` |
| `POST` | `/members/{id}/suspend` | — | `200` |
| `POST` | `/loans` | `{ bookId, memberId }` | `201 { loanId }` |
| `POST` | `/loans/return` | `{ loanId }` | `200` |

### Códigos de erro

| Status | Quando |
|---|---|
| `400` | Argumento inválido (ISBN malformado, email inválido) |
| `404` | Livro ou membro não encontrado |
| `422` | Violação de regra de domínio (sem cópias, membro suspenso, limite atingido) |
| `500` | Erro inesperado |

---

## Estratégia de testes

```
        ┌─────────────────────────────────┐
        │          E2E Tests              │  poucos · lentos · fluxos completos
        │    Reqnroll + HttpClient        │
        └──────────────┬──────────────────┘
                       │
        ┌──────────────▼──────────────────┐
        │       Integration Tests         │  médios · MongoDB real
        │  WebApplicationFactory          │  Testcontainers
        │  MongoRepository Tests          │
        └──────────────┬──────────────────┘
                       │
        ┌──────────────▼──────────────────┐
        │          Unit Tests             │  muitos · rápidos · isolados
        │  xUnit · NSubstitute · Shouldly │  sem I/O · sem banco
        └─────────────────────────────────┘
```

### Cobertura por camada

| Camada | Line | Branch | Meta |
|---|---|---|---|
| Domain | ~94% | ~88% | 90%+ |
| Application | 100% | 100% | 85%+ |
| Infrastructure | ~96% | — | 70%+ |
| Api | ~72% | — | 70%+ |

---

## Pipeline CI/CD

A cada `push` ou `pull_request` nas branches `main` e `develop`:

```
push / pull_request
        │
        ▼
┌───────────────┐
│     Build     │  dotnet restore + dotnet build
└───────┬───────┘
        │ needs: build
        ▼
┌───────────────┐
│  Unit Tests   │  66 testes · sem Docker · ~30s
└───────┬───────┘
        │ needs: unit-tests
        ▼
┌────────────────────────┐
│ Integration + E2E Tests│  Testcontainers · MongoDB real · ~2min
└───────────┬────────────┘
            │ needs: integration-tests
            ▼
┌───────────────────────┐
│   Coverage Report     │  ReportGenerator · HTML · artefato 30 dias
└───────────────────────┘
```

PRs na `main` e `develop` só podem ser mergeados com todos os jobs verdes.

---

## Como rodar localmente

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) — necessário para testes de integração e E2E
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator) — para relatório de cobertura local

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

### Configuração do Testcontainers

Os testes de integração e E2E usam Testcontainers para subir MongoDB em containers Docker isolados. Em alguns ambientes Linux é necessário desabilitar o Ryuk — processo de limpeza do Testcontainers — criando o arquivo abaixo no seu home:

```bash
cat >> ~/.testcontainers.properties << 'EOF'
ryuk.disabled=true
ryuk.container.privileged=false
EOF
```

> **Por que isso é necessário?** O Ryuk tenta subir um container auxiliar com privilégios elevados para gerenciar o ciclo de vida dos containers de teste. Em ambientes sem suporte a containers privilegiados — comum em máquinas de desenvolvimento Linux e runners de CI — ele falha na inicialização. Desabilitá-lo não afeta o funcionamento dos testes: os containers são destruídos normalmente pelo `DisposeAsync` das fixtures.

### Subir o ambiente de desenvolvimento

```bash
docker compose up -d
dotnet run --project src/LibraryManager.Api
```

A API estará disponível em `http://localhost:5000`.

### Rodar os testes

```bash
# todos os testes
dotnet test

# por suíte
dotnet test tests/LibraryManager.Unit.Tests
dotnet test tests/LibraryManager.Integration.Tests
dotnet test tests/LibraryManager.E2E.Tests
```

### Gerar relatório de cobertura local

```bash
bash tests/coverage.sh
```

O relatório será aberto automaticamente no browser em `coverage/report/index.html`.

---

## Configuração do MongoDB

### Desenvolvimento local

O `docker-compose.yml` na raiz do projeto sobe um MongoDB na porta `27017`. As configurações estão em `src/LibraryManager.Api/appsettings.Development.json`:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "librarymanager"
  }
}
```

### Testes

Os testes de integração e E2E sobem containers MongoDB isolados via Testcontainers — não interferem com o banco de desenvolvimento e são destruídos automaticamente ao final da execução.