# Marraia.POC

API .NET 10 com OpenTelemetry (traces, métricas e logs) exportando via OTLP para o Grafana Cloud.

## Estrutura

```
src/
  Marraia.POC.Api          -> Controllers, Program.cs e configuração do OpenTelemetry
  Marraia.POC.Application  -> Serviços, DTOs, ActivitySource/Meter customizados
  Marraia.POC.Domain       -> Entidades e contratos de repositório
```

## Configuração do Grafana Cloud

As configurações ficam na seção `OpenTelemetry` do `appsettings.json`. O header de autenticação
**não** é versionado: configure-o via User Secrets ou variável de ambiente.

```bash
cd src/Marraia.POC.Api
dotnet user-secrets set "OpenTelemetry:Headers" "Authorization=Basic <seu-token-base64>"
```

Ou, em container/servidor:

```bash
OpenTelemetry__Headers="Authorization=Basic <seu-token-base64>"
```

## Executando

```bash
dotnet run --project src/Marraia.POC.Api
```

Endpoints:

- `GET  /api/products`
- `GET  /api/products/{id}`
- `POST /api/products` — `{ "name": "Notebook", "price": 4500.90 }`
- `GET  /api/products/database` — lê produtos do PostgreSQL (`ConnectionStrings:ProductsDatabase`). Sem banco disponível, a conexão falha e retorna **500**
- `GET  /api/calculations/divide?dividend=10&divisor=0` — divisão por zero (`DivideByZeroException`) e retorna **500**
- `GET  /api/calculations/multiply?multiplicand=1.7976931348623157E308&multiplier=10` — multiplicação que resulta em número infinito (`OverflowException`) e retorna **500**

As rotas de erro existem para gerar alertas no Grafana. Exceções não tratadas são logadas, registradas
no span (status `Error`) e retornadas como `ProblemDetails` com status 500.

## Telemetria

- **Traces**: ASP.NET Core, HttpClient, Npgsql e spans customizados (`Marraia.POC.Application`)
- **Métricas**: ASP.NET Core, HttpClient, Runtime e métricas customizadas (`poc.products.created`, `poc.products.price`, `poc.errors`)
- **Logs**: `ILogger` exportado via OTLP com correlação de trace
