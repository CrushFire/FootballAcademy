export const API_BASE = '/api'

export const ROLES = {
  ADMIN: 'admin',
  TRAINER: 'trainer',
  MEDICAL: 'medical'
} as const

export const POSITIONS = [
  { value: 'GK',  label: 'Вратарь', group: 'Вратарь' },

  { value: 'CB',  label: 'Центральный защитник', group: 'Защитники' },
  { value: 'LB',  label: 'Левый защитник', group: 'Защитники' },
  { value: 'RB',  label: 'Правый защитник', group: 'Защитники' },
  { value: 'LWB', label: 'Левый защитник (фулбек)', group: 'Защитники' },
  { value: 'RWB', label: 'Правый защитник (фулбек)', group: 'Защитники' },

  { value: 'CDM', label: 'Опорный полузащитник', group: 'Полузащитники' },
  { value: 'CM',  label: 'Центральный полузащитник', group: 'Полузащитники' },
  { value: 'CAM', label: 'Атакующий полузащитник', group: 'Полузащитники' },

  { value: 'LW',  label: 'Левый вингер', group: 'Атакующие полузащитники / фланги' },
  { value: 'RW',  label: 'Правый вингер', group: 'Атакующие полузащитники / фланги' },

  { value: 'ST',  label: 'Центральный нападающий', group: 'Нападающие' },
  { value: 'CF',  label: 'Свободный нападающий', group: 'Нападающие' },
  { value: 'SS',  label: 'Второй нападающий', group: 'Нападающие' }
] as const

// Быстрая мапа код → русское название (для рендера в карточках/чипах)
export const POSITION_LABEL: Record<string, string> = Object.fromEntries(
  POSITIONS.map(p => [p.value, p.label])
)

export const ATTENDANCE_STATUS = {
  Present: 'Присутствовал',
  Absent:  'Отсутствовал',
  Late:    'Опоздал'
} as const

export const MATCH_STATUS = {
  Scheduled:  'Запланирован',
  InProgress: 'Идет',
  Finished:   'Завершен'
} as const

export const MATCH_TYPE: Record<string, string> = {
  Friendly:   'Товарищеский',
  League:     'Лига',
  Cup:        'Кубок',
  Tournament: 'Турнир'
}

export const MATCH_RESULT: Record<string, string> = {
  Win:  'Победа',
  Draw: 'Ничья',
  Loss: 'Поражение'
}
