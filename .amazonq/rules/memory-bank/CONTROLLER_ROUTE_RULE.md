# ⚠️ КРИТИЧЕСКОЕ ПРАВИЛО — Route в контроллерах

## Проблема

Vite proxy настроен с rewrite:
```ts
'/api': {
  target: 'http://localhost:5000',
  rewrite: (path) => path.replace(/^\/api/, '')
}
```

Это значит: фронт делает `/api/schedule` → бэкенд получает `/schedule`.

Если контроллер прописан как `[Route("api/schedule")]` — он получит 404, потому что бэкенд ищет `/api/schedule`, а пришло `/schedule`.

## Правило

**НИКОГДА не писать `api/` в Route контроллера.**

```csharp
// ❌ НЕПРАВИЛЬНО — 404 на фронте
[Route("api/schedule")]

// ✅ ПРАВИЛЬНО
[Route("schedule")]
```

## Как устроено в проекте

- `BaseController` имеет `[Route("api/[controller]")]` — это только для Swagger/документации, реально не используется если контроллер переопределяет Route
- Все контроллеры переопределяют Route коротким именем без `api/`:
  - `[Route("schedule")]`
  - `[Route("sportsman")]`
  - `[Route("group")]`
  - и т.д.

## Как проверить

```bash
findstr /r "Route(" Controllers\*.cs
```

Ни одна строка не должна содержать `"api/` (кроме BaseController).

## История

`ScheduleController` был прописан как `[Route("api/schedule")]` — все запросы к расписанию и посещаемости возвращали 404. Исправлено на `[Route("schedule")]`.
