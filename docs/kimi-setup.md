# Настройка kimi-code для работы с этим репозиторием

kimi-code читает этот репозиторий почти без настройки: инструкции — из корневого `AGENTS.md`,
субагент ревью — из `.agents/agents/`, MCP-сервер — из корневого `.mcp.json`. Ниже — то, что
нужно сделать руками один раз.

## Permission-правила

Автоматических разрешений из репозитория у kimi-code нет (в отличие от `.claude/settings.json`
для Claude Code). Добавь в `~/.kimi-code/config.toml` блок `[[permission.rules]]` — аналог
текущего `.claude/settings.json`:

```toml
[[permission.rules]]
decision = "allow"
pattern = "Bash(dotnet build*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(dotnet run*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(dotnet test*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(dotnet format*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(dotnet restore*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(dotnet publish*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(dotnet ef*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(dotnet tool restore*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(gh pr:*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(gh issue:*)"

[[permission.rules]]
decision = "allow"
pattern = "Bash(gh run view:*)"

[[permission.rules]]
decision = "allow"
pattern = "mcp__cwm-roslyn-navigator__*"

[[permission.rules]]
decision = "allow"
pattern = "WebSearch"

[[permission.rules]]
decision = "allow"
pattern = "FetchURL"
```

Отличия от claude-версии: `WebFetch` в kimi-code называется `FetchURL`, правила пишутся в TOML,
а не в JSON.

## Симлинки `.claude/` → `.agents/`

`.claude/agents` в репозитории — симлинк на `.agents/agents`. На Windows он материализуется
только если:

1. включён **режим разработчика** (Параметры → Система → Для разработчиков → Режим разработчика),
2. git клонировал с `core.symlinks=true` (проверь `git config core.symlinks` в репозитории;
   при клоне — `git config --global core.symlinks true`).

Если после клонирования `.claude/agents` — текстовый файл-заглушка вместо каталога,
восстанови ссылку:

```bash
rm .claude/agents
MSYS=winsymlinks:nativestrict ln -s ../.agents/agents .claude/agents
```

Внимание: в Git Bash без `MSYS=winsymlinks:nativestrict` команда `ln -s` на каталог молча
создаёт **копию каталога**, а не симлинк — результат проверяй через `readlink .claude/agents`.

## MCP-сервер cwm-roslyn-navigator

Сервер прописан в корневом `.mcp.json` и запускается локальным dotnet-инструментом
`cwm.roslynnavigator` из `.config/dotnet-tools.json`. На свежем клоне нужен `dotnet tool restore`
— без него сервер не поднимется. Решение (`Bastilia.Rating.slnx`) навигатор находит сам,
аргументы не нужны.

Новый MCP-сервер и новые агенты подхватываются при старте сессии — после изменений запускай
новую сессию (`/new`) или перезапускай CLI.

## Первый запуск

```bash
docker-compose up -d   # PostgreSQL (порт 6432)
dotnet tool restore    # dotnet-ef + MCP-сервер cwm-roslyn-navigator
```

## Локальные настройки

Файл `.kimi-code/local.toml` (личные правила и переопределения) добавлен в `.gitignore` —
клади туда всё, что не должно попасть в репозиторий.
