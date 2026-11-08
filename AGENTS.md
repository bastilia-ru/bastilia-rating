# AGENTS.md

Этот файл — инструкции для AI-агентов (kimi-code, Claude Code, opencode и других),
работающих с кодом этого репозитория. Он — источник истины; [CLAUDE.md](CLAUDE.md) —
только указатель на этот файл.

## Команды

```bash
# Восстановить зависимости
dotnet restore

# Локальные инструменты из .config/dotnet-tools.json (первый запуск; нужны для dotnet ef и MCP-сервера)
dotnet tool restore

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

## Рабочее дерево — только своё

Агент работает **только в том worktree/клоне, в котором запущен**. Соседние клоны этого
репозитория **не трогать**: не править в них файлы, не переключать в нём ветки, не коммитить,
не запускать там сборки и тесты.

Если нужно открыть другую ветку или параллельную задачу — создать своё worktree
и работать в нём:

```bash
git fetch upstream
git worktree add ../bastilia-rating.<задача> -b <ветка> upstream/master
```

Это же правило передавать субагентам (`Agent`/swarm): брифинговать их с абсолютным путём
worktree, в котором они работают (текущего или выделенного им), и явно запрещать выходить
за его пределы.

## Инструменты анализа кода

Для понимания кода предпочитай **cwm-roslyn-navigator** (MCP) перед чтением файлов. Roslyn понимает
семантику C#: типы, иерархии наследования, реальные использования символов. Используй
`find_symbol`, `find_references`, `find_callers`, `find_implementations`, `get_type_hierarchy`
и другие инструменты навигатора для навигации по коду. Чтение файлов — только когда нужен
полный контекст, которого навигатор не даёт.

Сервер прописан в `.mcp.json` и запускается локальным инструментом из `.config/dotnet-tools.json`,
поэтому на свежем клоне нужен `dotnet tool restore` — без него сервер не поднимется. Решение
(`Bastilia.Rating.slnx`) навигатор находит сам, аргументы не нужны. Новый MCP-сервер подхватывается
при старте сессии: если он только что появился, текущая сессия его не увидит.

Почему это важно на практике: грепом по именам **не видно мёртвых перегрузок** — одноимённый
живой метод прячет мёртвый. `find_references` такое находит сразу.

## Правила кода

- **Program.cs должен быть компактным.** Не добавляй многострочный код в `Program.cs` — оборачивай в extension-методы и выноси в отдельные файлы (пример: `Auth/AuthRegistration.cs`, `Common/HealthChecks.cs`).
- **Всегда используй типизированные опции.** Не читай конфигурацию через `IConfiguration.GetSection(...)["Key"]` — создавай класс опций и привязывай через `AddOptions<T>().BindConfiguration(...)` или `IConfiguration.GetSection(...).Get<T>()`.
- **Не зашивай URL и адреса внешних сервисов в код.** Выноси в конфигурацию (`appsettings.json` / user secrets) и читай через типизированные опции.
- **Пользовательские сообщения покрывай Verify snapshot-тестом.** Любой текст, который уходит пользователю (Telegram-рассылки, уведомления и т.п.), должен иметь Verify snapshot-тест на итоговый вывод (образец — `NewsMessageSnapshotTests` в `Bastilia.Rating.Tests`). Вывод теста должен быть детерминированным: даты и прочие изменчивые значения фиксируй константами.

## Работа с GitHub

`origin` — личный форк (`leotsarev/bastilia-rating`), основной репозиторий — `upstream`
(`bastilia-ru/bastilia-rating`). Туда же идут PR. Ребейзить и сверяться с базовой веткой
всегда через `upstream` (`git fetch upstream && git rebase upstream/master`): `origin/master`
в форке отстаёт, и на «ребейзни» от него можно получить ложный ответ «уже актуально».

### Создание PR после завершения задачи

**ОБЯЗАТЕЛЬНО**: после завершения задачи, результат которой оформляется PR, — сразу создавать
PR через `gh pr create`. Не предоставлять ссылку «Create PR» — создавать PR самостоятельно.
Не ждать отдельной просьбы.

`--title` и `--body` передавать всегда, явно. Без них `gh` пытается спросить заголовок
интерактивно и открыть редактор для тела — в сессии агента терминала нет, команда падает
с ошибкой вида «could not prompt: no terminal detected», и PR не создаётся. То же касается
`gh issue create`, `gh pr comment`, `gh issue comment` и `gh pr review`: любой `--body`
только через флаг.

```bash
gh pr create --title "feat: описание" --body "$(cat <<'EOF'
## Что сделано
- ...

Closes #НОМЕР

🤖 <подпись фактическим инструментом — последней строкой, формат в разделе ниже>
EOF
)"
```

### Всегда подписываться как фактический инструмент

**ОБЯЗАТЕЛЬНО**: любое сообщение, отправляемое в GitHub от лица пользователя (комментарий
к issue или PR, описание PR, текст ревью, тело issue), должно быть подписано — из него
должно быть явно видно, что писал инструмент/модель, а не человек. Подпись ставится
последней строкой и указывает фактический генератор сообщения — имя инструмента, которым
сообщение реально написано, со ссылкой на его сайт. Она не фиксирована: каждый инструмент
подписывается собой. Например:

```
🤖 Generated with [Claude Code](https://claude.ai/code)
```

```
🤖 Generated with [Kimi Code](https://www.kimi.com/code)
```

а opencode подписался бы так:

```
🤖 Generated with [opencode](https://opencode.ai)
```

Это касается всех команд `gh` (`gh pr create`, `gh pr comment`, `gh issue create`,
`gh issue comment`, `gh pr review`) и любых других способов отправки текста в GitHub.
Учётная запись GitHub принадлежит человеку, поэтому без подписи сообщение выглядит
написанным им лично — это вводит собеседников в заблуждение.

### Ревью PR — отдельным субагентом

**ОБЯЗАТЕЛЬНО**: каждый PR (перед `gh pr create` и перед пушем доработок) ревьюить отдельным
субагентом (инструмент `Agent`), а не перечитывать самому. Порядок: коммит → ревью субагентом
→ разбор замечаний → `gh pr create`/пуш. «Сразу создавать PR» из раздела выше означает
«сразу после ревью».

Субагент — новый независимый агент-ревьюер (в kimi-code — тип `pr-reviewer`, файл
[`.agents/agents/pr-reviewer.md`](.agents/agents/pr-reviewer.md)), а не продолжение текущего
контекста: наследник контекста автора смотрит на свой текст глазами автора, и ревью теряет
смысл.
Дать ему команду для диффа (`git diff upstream/master...HEAD` — сравнивать с `upstream`,
не с `origin`) и задачу: найти ошибки, противоречия с кодом, нарушения правил из этого
файла. Замечания разобрать: исправить или явно отклонить с причиной, и только после этого
отправлять PR.

Почему: автор смотрит на свой текст глазами автора и пропускает то, что видит свежий контекст.

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

CI (GitHub Actions) собирает при пуше в `master` или по тегам, вычисляет semver через PowerShell-скрипт и собирает образы `dotnet publish /t:PublishContainer` в локальный docker (`joinrpg/<имя>:latest` и `:<версия>`, `ContainerRegistry` не задан). Дальше `docker push` публикует их в Selectel Container Registry (`cr.selcloud.ru/joinrpg`, целевой реестр) и временно в Yandex Container Registry (`cr.yandex`). Деплой запускается автоматически при пуше в `master` после успешного «Build and publish» (`deploy-prod-auto.yml`, триггер `workflow_run`; деплоится версия образа из артефакта сборки, а не `latest`) или вручную (`deploy-prod.yml`, workflow_dispatch с версией образа, по умолчанию `latest`) и берёт образы из Selectel; кластер тянет их через секрет `craas-auth`, который создаёт интеграция кластера с Container Registry. Прямой пуш из SDK в Selectel не работает: реестр отдаёт в Location внутренний адрес.
