export const API_BASE = '/api'

export const ROLES = {
  ADMIN: 'admin',
  TRAINER: 'trainer',
  MEDICAL: 'medical'
} as const

// Единый источник лейблов позиций. Вариант 1 — короткие лейблы (Центр., Атак., Оттян.).
export const POSITIONS = [
  { value: 'GK',  label: 'Вратарь', group: 'Вратарь' },

  { value: 'CB',  label: 'Центр. защитник', group: 'Защитники' },
  { value: 'LB',  label: 'Левый защитник', group: 'Защитники' },
  { value: 'RB',  label: 'Правый защитник', group: 'Защитники' },
  { value: 'LWB', label: 'Левый латераль', group: 'Защитники' },
  { value: 'RWB', label: 'Правый латераль', group: 'Защитники' },

  { value: 'CDM', label: 'Опорный полузащитник', group: 'Полузащитники' },
  { value: 'CM',  label: 'Центр. полузащитник', group: 'Полузащитники' },
  { value: 'CAM', label: 'Атак. полузащитник', group: 'Полузащитники' },

  { value: 'LW',  label: 'Левый вингер', group: 'Вингеры' },
  { value: 'RW',  label: 'Правый вингер', group: 'Вингеры' },

  { value: 'ST',  label: 'Центр. нападающий', group: 'Нападающие' },
  { value: 'CF',  label: 'Второй нападающий', group: 'Нападающие' },
  { value: 'SS',  label: 'Оттян. нападающий', group: 'Нападающие' }
] as const

// Быстрая мапа код → русское название (для рендера в карточках/чипах)
export const POSITION_LABEL: Record<string, string> = Object.fromEntries(
  POSITIONS.map(p => [p.value, p.label])
)

// Маппинг PositionGroup с бэка (Goalkeeper/Defender/Midfielder/Winger/Forward)
// в русские лейблы — для случаев когда бэк отдаёт строку группы вместо кода позиции.
export const POSITION_GROUP_LABEL: Record<string, string> = {
  Goalkeeper: 'Вратарь',
  Defender:   'Защитник',
  Midfielder: 'Полузащитник',
  Winger:     'Вингер',
  Forward:    'Нападающий',
}

// Объединённый словарь — лейблы позиций + лейблы групп.
// Используется в местах где приходит и код (GK/CB), и строка группы (Goalkeeper).
export const POSITION_AND_GROUP_LABEL: Record<string, string> = {
  ...POSITION_LABEL,
  ...POSITION_GROUP_LABEL,
}

// Таблица совместимости позиций при замене: какие позиции запасных могут
// подменять основного на данной позиции. Логика: близкие роли (CM ↔ CDM/CAM),
// зеркальные фланги (LB ↔ LWB), универсалы (CAM ↔ SS/LW/RW).
// Используется при выборе замен на шаге утверждения состава и в live-режиме.
export const POSITION_REPLACEMENTS: Record<string, string[]> = {
  GK:  ['GK'],
  CB:  ['CB'],
  LB:  ['LB', 'LWB'],
  RB:  ['RB', 'RWB'],
  LWB: ['LWB', 'LB'],
  RWB: ['RWB', 'RB'],
  CDM: ['CDM', 'CM'],
  CM:  ['CM', 'CDM', 'CAM'],
  CAM: ['CAM', 'CM', 'SS'],
  LW:  ['LW', 'CAM'],
  RW:  ['RW', 'CAM'],
  ST:  ['ST', 'CF', 'SS'],
  CF:  ['CF', 'ST'],
  SS:  ['SS', 'CAM', 'ST'],
}

export const ATTENDANCE_STATUS: Record<string, string> = {
  Present: 'Присутствовал',
  Absent:  'Отсутствовал',
  Late:    'Опоздал'
}

export const MATCH_STATUS: Record<string, string> = {
  Scheduled:  'Запланирован',
  InProgress: 'Идет',
  Finished:   'Завершен'
}

// Типы матчей по GameType enum на бэке. Home — внутриакадемический матч
// (две своих команды играют друг с другом), не «домашняя игра».
export const MATCH_TYPE: Record<string, string> = {
  Friendly:   'Товарищеский',
  League:     'Лига',
  Cup:        'Кубок',
  Tournament: 'Турнир',
  Home:       'Домашний',
}

export const MATCH_RESULT: Record<string, string> = {
  Win:  'Победа',
  Draw: 'Ничья',
  Loss: 'Поражение'
}
