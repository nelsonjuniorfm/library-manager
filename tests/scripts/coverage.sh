#!/bin/bash
# coverage.sh
set -e

echo "🧹 Limpando relatórios anteriores..."
rm -rf ./coverage

echo "🧪 Rodando Unit Tests..."
dotnet test ../LibraryManager.Unit.Tests \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/unit \
  --no-build 2>/dev/null || dotnet test ../LibraryManager.Unit.Tests \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/unit

echo "🧪 Rodando Integration Tests..."
dotnet test ../LibraryManager.Integration.Tests \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/integration

echo "🧪 Rodando E2E Tests..."
dotnet test ../LibraryManager.E2E.Tests \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/e2e

echo "📊 Gerando relatório..."
reportgenerator \
  -reports:"./coverage/**/coverage.cobertura.xml" \
  -targetdir:"./coverage/report" \
  -reporttypes:"Html;Cobertura;Badges" \
  -assemblyfilters:"+LibraryManager.Domain;+LibraryManager.Application;+LibraryManager.Infrastructure;+LibraryManager.Api" \
  -classfilters:"-*Tests*"

echo "✅ Relatório gerado em ./coverage/report/index.html"
xdg-open ./coverage/report/index.html