# CLAUDE.md

Все инструкции для агентов живут в [AGENTS.md](AGENTS.md). Прочитай его полностью и следуй ему.

## Агенты — симлинки

`.claude/agents` — симлинк на общий для нескольких инструментов каталог
[`.agents/agents/`](.agents/agents/). Если ты **не видишь** агента `pr-reviewer`
(каталог отсутствует или вместо него текстовый файл-заглушка с путём) — клонирование,
вероятно, прошло без `core.symlinks` (на Windows — без режима разработчика). Ссылку нужно
восстановить:

```bash
rm .claude/agents
MSYS=winsymlinks:nativestrict ln -s ../.agents/agents .claude/agents
```

либо переклонировать репозиторий (`git config --global core.symlinks true` + режим
разработчика на Windows). Если симлинк на месте, а `pr-reviewer` всё равно отсутствует —
проверь, что `.agents/` закоммичен (`git ls-files .agents/`): без него ссылка будет
пустой. Подробности — [docs/kimi-setup.md](docs/kimi-setup.md).
