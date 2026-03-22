# TKVG Substitutions

Tools for accessing school substitutions from TKVG Edupage (`https://tkvg.edupage.org/substitution/`). Includes a Telegram bot for notifications and a CLI for terminal queries.

## Solution Structure

Solution file: `src/TkvgSubstitutionBot.slnx`

| Project | Type | Role |
|---------|------|------|
| `TkvgSubstitution` | Library | Core: HTTP client, HTML parsing, caching |
| `TkvgSubstitutionBot` | ASP.NET Core Web | Telegram bot with subscription notifications |
| `Tkvg.Cli` | Console | CLI tool using Spectre.Console.Cli |
| `UnitTests` | xUnit | Tests with NSubstitute mocks and embedded test data |

All projects target **net10.0** with nullable reference types and implicit usings enabled.

## Build, Test, Run

```bash
# Build
dotnet build src/TkvgSubstitutionBot.slnx

# Test
dotnet test src/UnitTests/UnitTests.csproj

# CLI
dotnet run --project src/Tkvg.Cli -- 23.03
dotnet run --project src/Tkvg.Cli -- 23.03 --class 3b

# Bot (requires BotToken in appsettings or env)
dotnet run --project src/TkvgSubstitutionBot
```

## Data Flow

```
TKVG Edupage API (POST, returns JSON with HTML)
  → TkvgHttpClient          (fetches raw response)
  → TkvgSubstitutionReader  (parses HTML via HtmlAgilityPack)
  → TkvgSubstitutionService (in-memory cache, used by bot)
  → Tkvg.Cli / TkvgSubstitutionBot (consumer apps)
```

The CLI skips `TkvgSubstitutionService` and uses `TkvgSubstitutionReader` directly (no caching needed for one-shot queries).

## Key Conventions

- **Config format**: YAML (`appsettings.yml`) via NetEscapades.Configuration.Yaml
- **User-facing language**: Estonian for substitution data, Russian for bot UI messages
- **Date formats**: API uses `yyyy-MM-dd`, CLI input uses `dd.MM`
- **Working day logic**: `Utils.GetNextWorkingDay()` — Friday→Monday, Saturday→Monday, otherwise next day
- **Test data**: Embedded JSON resources in `UnitTests/TestData/`, mock `TkvgHttpClient` (method is `virtual`)

## Deployment

- **Docker**: Multi-stage build in `src/TkvgSubstitutionBot/Dockerfile`, image `musukvl/tkvg-substitution-bot`
- **Docker Compose**: `src/docker-compose.yml`, port 8080, volume for subscription data. Use `docker compose` as much as possible.
- **.env**: The `.env` file is the source for secrets. Must not being commited or exposed to public. Should be added to app environment.
- **Health check**: `/health` endpoint
- **No CI/CD**: Manual builds via `src/build.sh`
