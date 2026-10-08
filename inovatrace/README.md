# InovaTrace

Apoio à análise preliminar de enquadramento na Lei do Bem, sobre a massa fictícia do Hackathon STS 2026.

```
HACKATHON-STS-2026/
├── dados-originais/     cópia do pacote; não alterar
└── inovatrace/
    ├── backend/
    ├── frontend/
    ├── docs/
    └── docker-compose.yml
```

A leitura dos arquivos está em [docs/DATASET_ANALYSIS.md](docs/DATASET_ANALYSIS.md).

## Subir

```powershell
docker compose -f inovatrace/docker-compose.yml up -d
dotnet run --project inovatrace/backend/src/InovaTrace.Api --launch-profile http
cd inovatrace/frontend
npm install
npm run dev
```

A API fica em `http://localhost:5088` e a fila em `http://localhost:5173`. Na primeira subida os 40 projetos são importados. PostgreSQL usa a porta 5433.

A análise não recebe `historicos_classificados`. A página Calibração compara o resultado com o gabarito depois.

Para um endpoint de IA compatível com chat completions, preencha `LanguageModel` em `appsettings.json`. Sem isso, o motor determinístico segue em uso.
