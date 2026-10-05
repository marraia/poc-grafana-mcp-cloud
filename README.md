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

## Telemetria

- **Traces**: ASP.NET Core, HttpClient e spans customizados (`Marraia.POC.Application`)
- **Métricas**: ASP.NET Core, HttpClient, Runtime e métricas customizadas (`poc.products.created`, `poc.products.price`)
- **Logs**: `ILogger` exportado via OTLP com correlação de trace
