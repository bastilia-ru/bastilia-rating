---
name: prod-logs
description: Читать и фильтровать логи продакшна rating.bastilia.ru из сервиса Логи Selectel (k8s-логи Portal и миграций БД) через aws CLI. Используй, когда нужно разобрать ошибку на проде, посмотреть последние запросы, найти конкретный трейс/RequestId или действия пользователя.
---

# Логи продакшна (Selectel Logs)

stdout подов собирает fluent-bit (DaemonSet в namespace `infra`, чарт — `platform/fluent-bit` в
[joinrpg/joinrpg-helms](https://github.com/joinrpg/joinrpg-helms/tree/main/platform/fluent-bit)) и шлёт в сервис
[Логи Selectel](https://docs.selectel.ru/en/logs/), пул ru-6. API сервиса совместим с AWS CloudWatch Logs,
читаем его aws CLI. Хранение — 30 дней. Бастилия живёт в том же кластере Selectel, что и JoinRpg
(деплой — через `joinrpg/deploy-actions`, манифесты — `manifests/` в этом репозитории).

**Группа `k8s/bastilia-rating-prod` на 2026-11-14 ещё не создана**: fluent-bit собирает только явно
перечисленные в чарте namespace'ы, а `bastilia-rating-prod` в их числе пока нет. Проверка:
`aws logs describe-log-groups --profile selectel-logs --query "logGroups[].logGroupName"`.
Если группы нет — namespace надо добавить в чарт `platform/fluent-bit`; до этого логи доступны
только через `kubectl logs -n bastilia-rating-prod deploy/bastilia-rating --tail=200`
(нужен настроенный kubectl-доступ к кластеру).

## Доступ

aws CLI с профилем `selectel-logs` (настраивает пользователь, один раз):

- ключ — «S3-ключ» сервисного пользователя Selectel с ролью `reader` или `logs.viewer`, проект JoinRpgCloud.
  Другого способа авторизации API логов нет: IAM-токен он не принимает.
  Ключ fluent-bit (`logs.writer`) читать не может;
- `aws configure --profile selectel-logs` (регион `ru-6`), затем
  `aws configure set endpoint_url https://ru-6.logs.selcloud.ru --profile selectel-logs`.

Ключи — в `~/.aws/credentials` пользователя, не в репозитории и не в чате. Проверка:
`aws logs describe-log-groups --profile selectel-logs --query "logGroups[].logGroupName"`.

Писать `--profile selectel-logs` в команде буквально, не через переменную или splat (`@P`): правило
разрешения сверяется с текстом команды. Формат правила зависит от инструмента: в Claude Code это
`aws logs * --profile selectel-logs*` в `~/.claude/settings.json`, в kimi-code —
`Bash(aws logs * --profile selectel-logs*)` в `[[permission.rules]]` `~/.kimi-code/config.toml`.

## Группы и потоки

- Группа на namespace: `k8s/bastilia-rating-prod` — так отличается прод. Dev-окружения нет
  (в `manifests/` только overlay `bastilia-rating-prod`).
- Поток на контейнер: `bastilia-rating` — портал (единственный контейнер Deployment),
  `bastilia-rating-migrator` — Job миграций БД (живёт несколько минут при деплое).
- Общий поток `_all` — все контейнеры namespace. **Каждая запись лежит дважды**: в потоке контейнера и в `_all`.
  Поэтому всегда указывать `--log-stream-names`: `_all` для всего namespace или имя контейнера для одного
  приложения. Без него выдача удваивается.
- Имени пода в имени потока нет (раньше было и упёрлось в лимит Selectel — 100 потоков на группу).
  Под — в поле `kubernetes.pod_name` записи.

## Базовая команда

```powershell
$now  = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
$from = $now - 3600000   # час назад
aws logs filter-log-events --profile selectel-logs --log-group-name k8s/bastilia-rating-prod --log-stream-names _all `
  --start-time $from [--end-time $to] [--filter-pattern '<термы>'] --max-items 1000 --output json > logs.json
```

- `--start-time`/`--end-time` — миллисекунды Unix epoch (UTC). Время из лога в ms:
  `[DateTimeOffset]::Parse('2026-11-14T17:30:00Z').ToUnixTimeMilliseconds()`.
- **Без `--start-time` выдача начинается с самого старого события в потоке**, а не с последних.
- События идут **от старых к новым**, и `--max-items` отрезает **новые**. Если вернулось ровно `--max-items`
  записей и есть `NextToken` — выдача обрезана: сузить окно или поднять лимит. Чтобы увидеть последние
  события, сужать `--start-time`, а не уменьшать лимит.
- CLI сам листает страницы до `--max-items`; 1000 записей — около 3–5 секунд.
- Выдачу сохранять в файл и разбирать им, а не читать в контекст целиком: одна запись — около 2 КБ.

## Фильтры

Работают только **текстовые** filter-pattern (ограничение сервиса, как в скилле prod-logs joinrpg-net):

| Шаблон | Что делает |
|---|---|
| `ERROR` | терм — подстрока в записи |
| `bastilia-rating-migrator ERROR` | все термы сразу (AND) |
| `?ERROR ?Fatal` | любой из термов (OR) |
| `ERROR -health` | исключить терм |
| `"atana@bastilia.ru"` | точная фраза |

**JSON-фильтры (`{ $.Level = "Error" }`, `{ $.StatusCode >= 500 }`) не поддерживаются** — молча возвращают
пустую выдачу, даже на условии, которому заведомо есть совпадения. Пустой результат с `{ $... }` не значит
«ничего не было».

Поэтому фильтровать грубо термом на сервере, а точно — на клиенте, разобрав `message`:

```powershell
$j = Get-Content logs.json -Raw | ConvertFrom-Json
$j.events | ForEach-Object { $_.message | ConvertFrom-Json } |
  Where-Object { $_.kubernetes.container_name -eq 'bastilia-rating' -and $_.StatusCode -ge 500 } |
  Select-Object '@timestamp', Level, StatusCode, RequestPath, TraceId, message
```

Удобные термы:

- уровень — `ERROR` / `WARN`: на практике матчат записи этих уровней (поле `YcLevel`), но терм может
  попасть и в текст сообщения — уровень перепроверять полем;
- `TraceId`, `RequestId`, `SourceContext`, путь запроса (`/api/...`) — уникальны, как термы работают точно.

## Структура записи

`events[].message` — **JSON строкой** (запись fluent-bit), его надо разбирать ещё раз. Поля Serilog лежат
в корне этой записи (`Merge_Log On`), рядом — метаданные `kubernetes`. Логирование — общее с JoinRpg
(`UseJoinSerilog`/`UseJoinRequestLogging` из `JoinRpg.Common.WebInfrastructure`, см. `Bastilia.Rating.Portal/Program.cs`),
поэтому структура та же:

```jsonc
{
  "@timestamp": "2026-11-14T18:23:47.062Z",
  "Level": "Information",          // Serilog: Verbose/Debug/Information/Warning/Error/Fatal
  "YcLevel": "INFO",               // то же для облака: INFO/WARN/ERROR — по нему удобнее фильтровать
  "message": "HTTP \"GET\" \"/api/...\" responded 200 in 45.4 ms",
  "AppName": "Bastilia.Rating.Portal",
  "SourceContext": "Serilog.AspNetCore.RequestLoggingMiddleware",
  "TraceId": "...", "SpanId": "...", "RequestId": "0HNPDQID3O373:00000026",
  "RequestMethod": "GET", "RequestPath": "/api/...", "StatusCode": 200, "Elapsed": 45.4,
  "EndpointName": "...", "QueryString": "...",
  "MachineName": "bastilia-rating-55548f6c85-68dhc",
  "exceptions": [{ "ClassName": "...", "Message": "...", "StackTraceString": "...", "Depth": 0 }],
  "fields": { "http.request.method": "GET", "url.path": "...", "user_agent.original": "...", "Referer": "..." },
  "kubernetes": { "pod_name": "...", "container_name": "bastilia-rating", "namespace_name": "bastilia-rating-prod",
                  "container_hash": "cr.selcloud.ru/joinrpg/bastilia-rating@sha256:..." },
  "stream": "stdout", "time": "..."
}
```

**Поля `LoggedUser` в логах Бастилии нет**: enricher из общей библиотеки читает email-claim
(`ClaimTypes.Email`), а `BastiliaUserLoginHandler` кладёт в claims `UserName` и роли, email не кладёт.
Искать действия конкретного пользователя текстом по логину, если он попадает в сообщения (например,
логи Telegram-рассылок), либо по `TraceId`/`RequestId` известного запроса.

`RemoteIpAddress` в записях — внутренний адрес пода ingress, а не адрес пользователя:
фильтр по IP бесполезен.

## Сценарии

- **Ошибки на проде за последний час:**
  ```powershell
  $now = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
  aws logs filter-log-events --profile selectel-logs --log-group-name k8s/bastilia-rating-prod --log-stream-names _all `
    --start-time ($now - 3600000) --filter-pattern ERROR --max-items 2000 --output json > err.json
  ```
  Свести в сводку, а не читать построчно:
  ```powershell
  (Get-Content err.json -Raw | ConvertFrom-Json).events | ForEach-Object { $_.message | ConvertFrom-Json } |
    Where-Object YcLevel -eq 'ERROR' |
    Group-Object { $_.kubernetes.container_name }, SourceContext, { (($_.message -split "`n")[0] -replace '\d+', 'N') } |
    Sort-Object Count -Descending | Select-Object -First 15 Count, Name
  ```
- **Одно приложение** — поток контейнера вместо `_all`: `--log-stream-names bastilia-rating`.
  Логи миграций БД — `--log-stream-names bastilia-rating-migrator`, окно — время деплоя (Job живёт минуты).
- **По TraceId/RequestId** из отчёта пользователя или страницы ошибки: `--filter-pattern '<traceid>'`,
  окно — сутки, если время неизвестно.

### Соседние действия того же пользователя

Найдя конкретный инцидент, почти всегда стоит посмотреть, что происходило до и после: это объясняет
контекст (сам обновил страницу и получил успех, ошибка воспроизводится на каждом запросе и т. п.).
Окно — ±5–15 минут вокруг найденной записи. Поля пользователя в записи нет (см. выше), поэтому:
по логину — текстовым термом, если логин пишется в сообщениях; иначе — окно без терма,
сузив поток до `bastilia-rating`.

## Ограничения и осторожность

- Это боевые данные пользователей (содержимое запросов, логины) — не публиковать вовне, не вставлять
  в публичные issue/PR.
- Широкое окно без терма тянет много данных — сужать окно, поток и терм, прежде чем брать всё подряд.
- Логи потерянных записей не догоняют: если fluent-bit не смог отправить пачку, записи пропадают.
  Подозрительная тишина — повод заглянуть в `kubectl logs -n infra ds/fluent-bit --tail=50`.
