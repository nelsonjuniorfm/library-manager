Feature: Empréstimo e devolução de livros
  Como membro da biblioteca
  Quero emprestar e devolver livros
  Para que outros membros também possam acessá-los

  Scenario: Membro realiza empréstimo com sucesso
    Given que o livro "Clean Code" com ISBN "978-3-16-148410-0" está cadastrado com 2 cópias
    And que o membro "Robert Martin" com email "robert@example.com" está cadastrado
    When o membro solicita o empréstimo do livro
    Then o empréstimo é criado com sucesso
    And o livro passa a ter 1 cópia disponível

  Scenario: Membro devolve livro emprestado
    Given que existe um empréstimo ativo entre um livro e um membro
    When o membro devolve o livro
    Then o empréstimo fica com status "Returned"
    And o livro recupera a cópia disponível

  Scenario: Empréstimo falha quando livro não tem cópias
    Given que o livro "Domain-Driven Design" com ISBN "978-0-32-112521-7" está cadastrado com 1 cópia
    And que a única cópia já está emprestada
    And que o membro "Eric Evans" com email "eric@example.com" está cadastrado
    When o membro solicita o empréstimo do livro
    Then o sistema rejeita o empréstimo com erro de disponibilidade