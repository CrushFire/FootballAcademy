# Миграции EF Core + PostgreSQL — что делать при изменениях

## Когда нужно пересоздавать миграцию

- Добавил/удалил поле в сущности
- Изменил тип поля (например `string` → `enum`)
- Добавил новую сущность
- Изменил связи между сущностями (FK, навигационные свойства)

Если не пересоздать — бэкенд упадёт при старте с ошибкой:
```
PendingModelChangesWarning: The model for context 'ApplicationDbContext' has pending changes.
```

---

## Сценарий 1 — изменил модель, БД пустая (или не жалко дропнуть)

```bash
# 1. Дропнуть БД через docker
docker exec FootballAcademy_postgres psql -U postgres -c "DROP DATABASE IF EXISTS \"FootballAcademy_DB\";"

# 2. Удалить старые файлы миграции вручную
# backend/DataAccess/Migrations/ — удалить все .cs файлы

# 3. Создать новую миграцию
cd backend
dotnet ef migrations add InitialCreate --project DataAccess --startup-project FootballAcademy

# 4. Перезапустить бэкенд — он сам применит миграцию
docker-compose up backend -d
```

---

## Сценарий 2 — изменил модель, БД с данными (нельзя дропать)

```bash
# 1. Создать новую миграцию с именем изменения
cd backend
dotnet ef migrations add AddPositionEnum --project DataAccess --startup-project FootballAcademy

# 2. Перезапустить бэкенд — он применит только новую миграцию
docker-compose restart backend
```

---

## Сценарий 3 — бэкенд падает с "relation already exists"

Значит таблицы есть в БД, но миграция пытается создать их заново.
Причина: удалили миграцию вручную, но БД осталась.

```bash
# Дропнуть БД и перезапустить
docker exec FootballAcademy_postgres psql -U postgres -c "DROP DATABASE IF EXISTS \"FootballAcademy_DB\";"
docker-compose up backend -d
```

---

## Сценарий 4 — залить тестовые данные заново

```bash
# Очистить БД (через Swagger или curl)
curl -X DELETE http://localhost:5000/seed/clear

# Залить тестовые данные
curl -X POST http://localhost:5000/seed
```

Или через Swagger: `http://localhost:5000/swagger`
- `DELETE /seed/clear` — очистить
- `POST /seed` — заполнить

---

## Полный сброс (ядерный вариант)

```bash
# Остановить всё и удалить volumes (данные БД)
docker-compose down -v

# Поднять заново — БД создастся с нуля
docker-compose up -d
```

---

## Важные заметки

- `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` — стоит в `Program.cs`, без этого `DateTime` без `Kind=Utc` падает
- Миграции хранятся в `backend/DataAccess/Migrations/`
- Бэкенд применяет миграции **автоматически при старте** через `context.Database.Migrate()`
- Credentials БД в файле `.env` в корне проекта
