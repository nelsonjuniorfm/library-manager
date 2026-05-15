#!/bin/bash
# tests/coverage.sh
# Execute sempre da raiz da solução: bash tests/coverage.sh
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# ↑ descobre o diretório absoluto onde o script está
#   funciona independente de onde você executa

ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
# ↑ sobe um nível — raiz da solução

COVERAGE_DIR="$ROOT_DIR/coverage"
# ↑ todos os relatórios ficam na raiz da solução

ASSEMBLIES="+LibraryManager.Domain;+LibraryManager.Application;+LibraryManager.Infrastructure;+LibraryManager.Api"

echo "🧹 Limpando relatórios anteriores..."
rm -rf "$COVERAGE_DIR"

echo "🧪 Rodando Unit Tests..."
dotnet test "$ROOT_DIR/tests/LibraryManager.Unit.Tests" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$COVERAGE_DIR/unit" \
  --verbosity quiet

echo "🧪 Rodando Integration Tests..."
dotnet test "$ROOT_DIR/tests/LibraryManager.Integration.Tests" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$COVERAGE_DIR/integration" \
  --verbosity quiet

echo "🧪 Rodando E2E Tests..."
dotnet test "$ROOT_DIR/tests/LibraryManager.E2E.Tests" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$COVERAGE_DIR/e2e" \
  --verbosity quiet

echo "📊 Gerando relatório..."
# reportgenerator \
#   -reports:"$COVERAGE_DIR/**/coverage.cobertura.xml" \
#   -targetdir:"$COVERAGE_DIR/report" \
#   -reporttypes:"Html;Cobertura;Badges;TextSummary" \
#   -assemblyfilters:"$ASSEMBLIES" \
#   -classfilters:"-*Tests*" \
#   -title:"LibraryManager Coverage Report" \
#   -tag:"$(git rev-parse --short HEAD 2>/dev/null || echo 'local')"

reportgenerator \
  -reports:"$COVERAGE_DIR/**/coverage.cobertura.xml" \
  -targetdir:"$COVERAGE_DIR/report" \
  -reporttypes:"Html;Cobertura;Badges;TextSummary" \
  -assemblyfilters:"+LibraryManager.Domain;+LibraryManager.Application;+LibraryManager.Infrastructure;+LibraryManager.Api" \
  -classfilters:"-*Tests*;-Microsoft.AspNetCore*;-System.*;-Program" \
  -title:"LibraryManager Coverage Report" \
  -tag:"$(git rev-parse --short HEAD 2>/dev/null || echo 'local')"

echo ""
echo "📋 Resumo:"
cat "$COVERAGE_DIR/report/Summary.txt"

echo ""
echo "✅ Relatório gerado em $COVERAGE_DIR/report/index.html"
xdg-open "$COVERAGE_DIR/report/index.html" 2>/dev/null || true