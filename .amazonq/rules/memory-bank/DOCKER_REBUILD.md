# Docker — пересборка без кэша

## Проблема

`snapshot does not exist: not found` — баг Docker Desktop на Windows.
Кэш BuildKit повреждается после Ctrl+C, перезагрузки или обновления Docker Desktop.

## Решение

```bash
docker-compose build --no-cache backend
docker-compose up -d
```

## Ядерный вариант (если всё равно падает)

```bash
docker builder prune -f
docker-compose up --build -d
```

## Правило

- `docker-compose up --build` — использует кэш, иногда ломается
- `--no-cache` — медленнее, но надёжнее
