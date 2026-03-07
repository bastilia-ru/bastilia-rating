# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Команды

```bash
# Восстановить зависимости
dotnet restore

# Собрать
dotnet build
dotnet build -c Release -r linux-x64 ./Bastilia.Rating.Portal/Bastilia.Rating.Portal.csproj

# Запустить все тесты
dotnet test

# Запустить один тест
dotnet test --filter "FullyQualifiedName~JsonRoundTripTests"

# Проверить форматирование (CI требует отсутствия изменений с --severity error)
dotnet format --no-restore --verify-no-changes --severity error

# Применить форматирование
dotnet format

# Запустить портал локально
dotnet run --project ./Bastilia.Rating.Portal/Bastilia.Rating.Portal.csproj

# Добавить миграцию EF Core
dotnet ef migrations add <НазваниеМиграции> --project ./Bastilia.Rating.Database/Bastilia.Rating.Database.csproj --startup-project ./Bastilia.Rating.Portal/Bastilia.Rating.Portal.csproj
```

## Локальная база данных

PostgreSQL запускается через Docker Compose (порт 6432):

```bash
docker-compose up -d
```

Ключ строки подключения — `BastiliaRating`. Настраивается через `appsettings.json` или user secrets (UserSecretsId: `763c5644-5459-415e-bc3a-37ce53120f08` в проекте Portal).

## Архитектура

Решение — Blazor Web App (гибрид SSR + WASM) на .NET 10, база данных PostgreSQL через Entity Framework Core. Система ведёт учёт рейтинга и достижений членов LARP-клуба «Бастилия».

### Проекты

| Проект | Роль |
|---|---|
| `Bastilia.Rating.Portal` | Хост ASP.NET Core: серверные Blazor-страницы, минимальные API-эндпоинты, регистрация DI |
| `Bastilia.Rating.Portal.Client` | WebAssembly-проект: общие Razor-компоненты, рендерящиеся на клиенте |
| `Bastilia.Rating.Domain` | Доменные модели (records), интерфейсы репозиториев и сервисов, доменная логика |
| `Bastilia.Rating.Database` | EF Core `AppDbContext`, сущности, реализации репозиториев и сервисов, миграции |
| `Bastilia.Rating.Migrator` | Консольное приложение для применения миграций при деплое |
| `JoinRpg.Client` | HTTP-клиент для внешнего API JoinRPG (импорт пользователей) |
| `JoinRpg.XGameApi.Contract` | DTO-контракты для XGame API JoinRPG |
| `Bastilia.Rating.Tests` | xUnit-тесты с Shouldly; охватывают JSON round-trip для доменных моделей |

### Слои

- **Domain** (`Bastilia.Rating.Domain`) определяет интерфейсы (`IBastiliaMemberRepository`, `IBastiliaProjectRepository` и др.) и чистые доменные записи (`BastiliaMember`, `BastiliaProject` и др.) с вычисляемыми свойствами (расчёт рейтинга, вычисление статуса). Нет зависимостей на EF или инфраструктуру.
- **Database** (`Bastilia.Rating.Database`) реализует эти интерфейсы. Сущности — в `Entities/`, репозитории — в отдельных файлах, write-сервисы — в `DbServices/`. Регистрируются через `Registration.AddRatingDal()`.
- **Portal** потребляет интерфейсы домена и регистрирует всё в DI. Серверные компоненты — в `Components/Pages/` (SSR); общие UI-компоненты — в `Bastilia.Rating.Portal.Client/Components/` (WASM).

### Внешние интеграции

- **JoinRPG** — данные пользователей/персонажей импортируются через `JoinUserInfoClient` (настройка: `JoinConnectOptions`).
- **КогдаИгра** — данные игрового календаря через `JoinRpg.Common.KogdaIgraClient` (настройка: секция `KogdaIgra`).
- **iCal** — экспорт календаря: `GET /api/calendar/ical`.

### Деплой

Контейнеры публикуются в Yandex Container Registry (`cr.yandex`). CI (GitHub Actions) собирает при пуше в `master` или по тегам, вычисляет semver через PowerShell-скрипт, запускает `dotnet publish /t:PublishContainer`, затем деплоит через `deploy-prod-auto.yml`.
