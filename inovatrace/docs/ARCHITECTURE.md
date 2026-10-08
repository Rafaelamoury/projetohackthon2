# Arquitetura

Monólito modular. ASP.NET Core, React, TypeScript e PostgreSQL. Binários ficam na pasta do projeto; o banco guarda metadados e caminhos.

Princípio: a IA interpreta, o motor valida, o analista decide, o sistema registra.

## Camadas

- **Domain:** projeto, documento, atividade, evidência, medição, resultado, análise, critério, afirmação, contradição, lacuna, revisão e auditoria. `RecommendationPolicy` só combina estados de critério.
- **Application:** importação, leitura do dossiê, extração de afirmações, validação matemática, rubrica, contradições, motor, calibração e parecer.
- **Infrastructure:** CSV do hackathon, PDF da entrevista, Entity Framework, PostgreSQL e `CorporateLanguageModel`.
- **Api:** fila, análise, revisão, parecer e calibração.
- **frontend:** fila, análise com rastreio, revisão e parecer.

## Fluxo

Dataset → `HackathonDatasetImporter` → projeto, atividades, evidências e documentos → PostgreSQL.

`POST /api/projects/{id}/analyses` lê método, revisão e entrevista no disco, transcreve afirmações, recalcula medições, avalia os cinco critérios e grava a recomendação. O gabarito não entra nesse caminho.

`POST /api/calibration` roda PRJ01–PRJ20 e só então compara com `historicos_classificados`.

`POST /api/analyses/{id}/review` grava a decisão do analista ao lado da recomendação.

## Interfaces

- `IDatasetImporter`
- `IClaimExtractionService`
- `ILanguageModel`, implementada por `CorporateLanguageModel`
- `IAnalysisEngine`
- `IHistoricalCatalog`, usado só na calibração

## Endpoints

- `GET /api/projects`
- `GET /api/projects/{id}`
- `POST /api/projects/{id}/analyses`
- `POST /api/analyses/{id}/review`
- `GET /api/projects/{id}/report`
- `POST /api/calibration`

Upload genérico fica fora desta entrega. Quando entrar, o arquivo deve ir para área própria, com nome gerado pelo sistema e validação de tipo e tamanho.
