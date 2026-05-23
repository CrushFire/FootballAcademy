// Преобразует путь картинки из БД (например "images/team_158_X.png" или просто "team_158_X.png")
// в URL для отображения в браузере: /api/images/team_158_X.png.
// Принимает: string | { path }-объект | массив таких объектов (берёт последнюю) | null/undefined.
// Возвращает null если картинки нет — компонент должен показать fallback (буквы инициалов).
export function imageUrl(input: unknown): string | null {
  if (!input) return null

  // Массив (например images:[{path:...},...]) — берём последнюю
  if (Array.isArray(input)) {
    if (input.length === 0) return null
    return imageUrl(input[input.length - 1])
  }

  // Объект { path: "..." }
  if (typeof input === 'object' && input !== null && 'path' in input) {
    const p = (input as { path?: string }).path
    if (!p) return null
    return imageUrl(p)
  }

  // Строка с путём
  if (typeof input === 'string') {
    const clean = input.replace(/^images\//, '').replace(/^\/+/, '')
    if (!clean) return null
    return `/api/images/${clean}`
  }

  return null
}

// Инициалы из ФИО (для fallback когда нет картинки)
export function initials(fio?: string | null): string {
  if (!fio) return '?'
  return fio
    .trim()
    .split(/\s+/)
    .map(w => w[0])
    .join('')
    .slice(0, 2)
    .toUpperCase()
}
