<template>
  <div class="p-3 h-full relative">
    <TrainerPageCard color="green" class="h-full flex flex-col">

    <!-- Step 1: выбор группы -->
    <AdminListLayout
      v-if="step === 'select'"
      title="Фиксация результатов нормативов"
      :search="groupSearch"
      @update:search="v => { groupSearch = v; groupPage = 1 }"
      :loading="loadingGroups"
      :items="pagedGroups"
      :page="groupPage"
      :total-pages="groupTotalPages"
      :total="filteredGroups.length"
      :sort-dir="groupSortDir"
      sort-label="По имени"
      no-sort-menu
      @toggle-dir="groupSortDir = groupSortDir === 'asc' ? 'desc' : 'asc'"
      @prev="groupPage--"
      @next="groupPage++"
    >
      <template #filter>
        <select v-model="groupSourceFilter" @change="groupPage = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-green-400 text-neutral-600">
          <option value="">Все</option>
          <option value="group">Группы</option>
          <option value="team">Команды</option>
        </select>
      </template>
      <template #items>
        <button
          v-for="g in pagedGroups"
          :key="g.id"
          @click="selectGroup(g)"
          class="w-full text-left bg-white rounded-2xl border border-neutral-200 p-3.5 hover:border-green-200 hover:bg-green-50/30 transition-all flex items-center gap-3 shadow-[0_6px_0_-2px_#BBF7D0] dark:shadow-[0_6px_0_-2px_#14532D] mb-1.5"
        >
          <div class="w-10 h-10 rounded-xl bg-green-100 flex items-center justify-center flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-green-600">
              <path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"/>
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-base font-semibold text-neutral-800">{{ g.name }}</div>
            <div class="text-sm text-neutral-400 mt-0.5">{{ g._type === 'team' ? 'Команда' : (g.description || 'Группа') }}</div>
          </div>
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-300 flex-shrink-0">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/>
          </svg>
        </button>
      </template>
    </AdminListLayout>

    <!-- Step 2: заполнение нормативов -->
    <div v-else class="h-full flex flex-col gap-3">
      <div class="flex flex-col gap-4 flex-1 min-h-0">

        <!-- Group header + back -->
        <div class="flex items-center gap-2 flex-shrink-0">
          <button @click="step = 'select'" class="p-1.5 rounded-lg border border-neutral-200 hover:bg-neutral-100 transition-colors flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
            </svg>
          </button>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-bold text-neutral-900 truncate">{{ selectedGroup?.name }}</div>
            <div class="text-xs text-neutral-400">{{ groupSportsmen.length }} учеников</div>
          </div>
          <div class="flex gap-1 bg-neutral-100 rounded-xl p-0.5">
            <button
              @click="normTab = 'gto'"
              class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
              :class="normTab === 'gto' ? 'bg-white text-neutral-800 shadow-sm' : 'text-neutral-500 hover:text-neutral-700'"
            >ГТО</button>
            <button
              @click="normTab = 'local'"
              class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
              :class="normTab === 'local' ? 'bg-white text-neutral-800 shadow-sm' : 'text-neutral-500 hover:text-neutral-700'"
            >Локальные</button>
          </div>
        </div>

        <!-- Normative selector -->
        <div class="flex-shrink-0">
          <div v-if="normTab === 'gto'" class="flex flex-col gap-1.5">
            <div class="flex items-center justify-between px-1">
              <div class="text-xs font-semibold text-neutral-500">Норматив ГТО</div>
              <label class="flex items-center gap-1.5 text-xs text-neutral-500 cursor-pointer select-none">
                <input v-model="showAllNorms" type="checkbox" class="w-3.5 h-3.5 accent-blue-500" />
                Показать все
              </label>
            </div>
            <select
              v-model="selectedGtoNormId"
              @change="onNormChange"
              class="w-full px-3 py-2 rounded-xl border border-neutral-200 bg-white text-sm text-neutral-800 outline-none focus:border-blue-300 transition-colors"
            >
              <option :value="null">— Выберите норматив —</option>
              <option v-for="n in filteredGtoNorms" :key="n.id" :value="n.id">{{ n.type }} ({{ n.unit }}){{ showAllNorms ? ` · U${n.ageGroup} ${n.gender}` : '' }}</option>
            </select>
            <div v-if="gtoFilterEmpty" class="text-[11px] text-amber-700 bg-amber-50 border border-amber-100 rounded-lg px-2 py-1">
              Для группы (U{{ groupAge }} {{ groupGender }}) нет нормативов — показаны все
            </div>
            <div v-if="selectedGtoNorm" class="flex flex-col gap-1 px-1">
              <span class="text-xs font-semibold text-neutral-500">Пороговые значения:</span>
              <div class="flex items-center gap-2 flex-wrap">
                <span class="text-[10px] px-2 py-0.5 rounded-full bg-green-100 text-green-700 font-semibold border border-green-200">Отл: {{ selectedGtoNorm.gradeExcellent }}</span>
                <span class="text-[10px] px-2 py-0.5 rounded-full bg-blue-100 text-blue-700 font-semibold border border-blue-200">Хор: {{ selectedGtoNorm.gradeGood }}</span>
                <span class="text-[10px] px-2 py-0.5 rounded-full bg-yellow-100 text-yellow-700 font-semibold border border-yellow-200">Удовл: {{ selectedGtoNorm.gradeSatisfactory }}</span>
                <span class="text-[10px] text-neutral-400">{{ selectedGtoNorm.unit }}</span>
              </div>
            </div>
          </div>
          <div v-else class="flex flex-col gap-1.5">
            <div class="flex items-center justify-between px-1">
              <div class="text-xs font-semibold text-neutral-500">Локальный норматив</div>
              <label class="flex items-center gap-1.5 text-xs text-neutral-500 cursor-pointer select-none">
                <input v-model="showAllNorms" type="checkbox" class="w-3.5 h-3.5 accent-blue-500" />
                Показать все
              </label>
            </div>
            <select
              v-model="selectedLocalNormId"
              @change="onNormChange"
              class="w-full px-3 py-2 rounded-xl border border-neutral-200 bg-white text-sm text-neutral-800 outline-none focus:border-blue-300 transition-colors"
            >
              <option :value="null">— Выберите норматив —</option>
              <option v-for="n in filteredLocalNorms" :key="n.id" :value="n.id">{{ n.type }} ({{ n.unit }}){{ showAllNorms && n.ageGroup ? ` · U${n.ageGroup} ${n.gender ?? ''}` : '' }}</option>
            </select>
            <div v-if="localFilterEmpty" class="text-[11px] text-amber-700 bg-amber-50 border border-amber-100 rounded-lg px-2 py-1">
              Для группы (U{{ groupAge }} {{ groupGender }}) нет нормативов — показаны все
            </div>
            <div v-if="selectedLocalNorm" class="flex flex-col gap-1 px-1">
              <span class="text-xs font-semibold text-neutral-500">Пороговые значения:</span>
              <div class="flex items-center gap-2 flex-wrap">
                <span class="text-[10px] px-2 py-0.5 rounded-full bg-teal-100 text-teal-700 font-semibold border border-teal-200">Порог: {{ selectedLocalNorm.value }} {{ selectedLocalNorm.unit }}</span>
                <span class="text-[10px] text-neutral-400">{{ selectedLocalNorm.isMoreBetter ? 'чем больше — лучше' : 'чем меньше — лучше' }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Sportsmen results -->
        <div v-if="loadingGroupSportsmen" class="py-12 text-center text-sm text-neutral-400">Загрузка учеников...</div>
        <div v-else-if="!groupSportsmen.length" class="py-12 text-center text-sm text-neutral-400">Нет учеников в группе</div>
        <div v-else class="flex-1 overflow-y-auto space-y-2">
          <div
            v-for="s in groupSportsmen"
            :key="s.id"
            class="bg-white rounded-2xl border p-3 flex items-center gap-3 transition-all"
            :class="rowBorderClass(s.id)"
          >
            <!-- Avatar -->
            <div class="w-8 h-8 rounded-xl flex items-center justify-center flex-shrink-0 text-xs font-bold"
              :class="absent[s.id] ? 'bg-neutral-100 text-neutral-400' : 'bg-neutral-100 text-neutral-500'">
              {{ initials(s.fio) }}
            </div>

            <!-- Name + status badge -->
            <div class="flex-1 min-w-0">
              <div class="text-sm font-semibold truncate" :class="absent[s.id] ? 'text-neutral-400' : 'text-neutral-800'">{{ s.fio }}</div>
              <div v-if="absent[s.id]" class="text-[10px] text-neutral-400 mt-0.5">Отсутствовал</div>
              <div v-else-if="recentResults[s.id] && !hasInput(s.id)" class="text-[10px] text-neutral-400 mt-0.5">
                Сдано {{ formatDate(recentResults[s.id].createdAt) }} · {{ recentResults[s.id].result }} {{ activeUnit }}
              </div>
            </div>

            <!-- Controls -->
            <div class="flex items-center gap-2 flex-shrink-0">

              <!-- Пересдать: только для тех у кого есть recent и кто не absent -->
              <button
                v-if="recentResults[s.id] && !absent[s.id]"
                @click="toggleRetake(s.id)"
                class="text-[10px] px-2 py-1 rounded-lg border transition-colors font-medium"
                :class="retake[s.id]
                  ? 'border-green-300 bg-green-50 text-green-700 hover:bg-green-100'
                  : 'border-neutral-200 text-neutral-400 hover:border-neutral-300 hover:bg-neutral-50'"
              >
                {{ retake[s.id] ? '✕ Отмена' : 'Пересдать' }}
              </button>

              <!-- Absent toggle -->
              <button
                @click="toggleAbsent(s.id)"
                class="text-[10px] px-2 py-1 rounded-lg border transition-colors font-medium"
                :class="absent[s.id]
                  ? 'border-neutral-300 bg-neutral-100 text-neutral-500 hover:bg-neutral-200'
                  : 'border-neutral-200 text-neutral-400 hover:border-neutral-300 hover:bg-neutral-50'"
              >
                {{ absent[s.id] ? '✕ Снять' : 'Отсутствовал' }}
              </button>

              <!-- Input: показывается если нет recent, или если recent но нажали Пересдать, или нет absent -->
              <input
                v-if="!absent[s.id] && (!recentResults[s.id] || retake[s.id])"
                v-model.number="results[s.id]"
                type="number"
                step="0.01"
                min="0"
                placeholder="—"
                class="w-20 px-2 py-1.5 rounded-xl border text-sm text-right font-semibold outline-none transition-colors"
                :class="inputBorderClass(s.id)"
              />

              <!-- Grade badge для нового ввода -->
              <span
                v-if="!absent[s.id] && hasInput(s.id)"
                class="text-[10px] font-bold px-2 py-0.5 rounded-full min-w-[52px] text-center"
                :class="resultBadgeClass(s.id)"
              >{{ resultLabel(s.id) }}</span>

              <!-- "Сдано" когда нет нового ввода и нет retake -->
              <span
                v-else-if="!absent[s.id] && recentResults[s.id] && !retake[s.id]"
                class="text-[10px] font-medium px-2 py-0.5 rounded-full min-w-[52px] text-center"
                :class="recentGradeBadge(s.id)"
              >Сдано · {{ recentGradeLabel(s.id) }}</span>
            </div>
          </div>
        </div>

        <!-- Save button -->
        <div class="flex-shrink-0">
          <button
            @click="saveResults"
            :disabled="saving || !canSave"
            class="w-full py-3 rounded-xl bg-green-600 text-white text-sm font-bold hover:bg-green-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            <span v-if="saving">Сохранение...</span>
            <span v-else-if="!activeNormId">Выберите норматив</span>
            <span v-else-if="!canSave">Заполните всех учеников без оценки</span>
            <span v-else>Сохранить результаты ({{ saveCount }})</span>
          </button>
        </div>
      </div>

    </div>

    </TrainerPageCard>

    <!-- Success toast -->
    <div
      v-if="savedToast"
      class="absolute top-6 left-1/2 -translate-x-1/2 bg-green-600 text-white text-xs font-semibold px-4 py-2.5 rounded-2xl shadow-lg z-10 pointer-events-none"
    >
      Результаты сохранены
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/store/auth'
import { formatDate } from '@/utils/formatDate'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'

const auth = useAuthStore()

type Step = 'select' | 'fill'
const step = ref<Step>('select')

const loadingGroups         = ref(true)
const loadingGroupSportsmen = ref(false)
const saving                = ref(false)
const savedToast            = ref(false)

const groups         = ref<any[]>([])
const teams          = ref<any[]>([])
const groupSourceFilter = ref('')
const groupSearch    = ref('')
const groupSortDir   = ref<'asc' | 'desc'>('asc')
const groupPage      = ref(1)
const groupPageSize  = 10

const allSources = computed(() => {
  const gs = groups.value.map(g => ({ ...g, _type: 'group' }))
  const ts = teams.value.map(t => ({ ...t, _type: 'team' }))
  return [...gs, ...ts]
})

const filteredGroups = computed(() => {
  const q = groupSearch.value.trim().toLowerCase()
  let list = allSources.value
  if (groupSourceFilter.value === 'group') list = list.filter(g => g._type === 'group')
  if (groupSourceFilter.value === 'team')  list = list.filter(g => g._type === 'team')
  if (q) list = list.filter(g => g.name.toLowerCase().includes(q))
  return [...list].sort((a, b) => {
    const r = a.name.localeCompare(b.name, 'ru')
    return groupSortDir.value === 'asc' ? r : -r
  })
})
const groupTotalPages = computed(() => Math.max(1, Math.ceil(filteredGroups.value.length / groupPageSize)))
const pagedGroups     = computed(() => filteredGroups.value.slice((groupPage.value - 1) * groupPageSize, groupPage.value * groupPageSize))
const selectedGroup  = ref<any>(null)
const groupSportsmen = ref<any[]>([])
const normTab        = ref<'gto' | 'local'>('gto')

const gtoNorms   = ref<any[]>([])
const localNorms = ref<any[]>([])

const selectedGtoNormId   = ref<number | null>(null)
const selectedLocalNormId = ref<number | null>(null)

const selectedGtoNorm   = computed(() => gtoNorms.value.find(n => n.id === selectedGtoNormId.value) ?? null)
const selectedLocalNorm = computed(() => localNorms.value.find(n => n.id === selectedLocalNormId.value) ?? null)
const activeNormId      = computed(() => normTab.value === 'gto' ? selectedGtoNormId.value : selectedLocalNormId.value)
const activeUnit        = computed(() => normTab.value === 'gto' ? (selectedGtoNorm.value?.unit ?? '') : (selectedLocalNorm.value?.unit ?? ''))

// Фильтр нормативов по возрасту и полу группы
const showAllNorms = ref(false)

// Возраст группы: сначала пробуем взять из AgeGroup команды ("U16" → 16),
// затем из названия группы/команды (regex /U(\d+)/), затем — fallback на возраст спортсмена.
// Пол — берётся с любого спортсмена (все в группе одного пола).
const groupAge = computed(() => {
  const sel = selectedGroup.value
  if (sel) {
    const fromEnum = typeof sel.ageGroup === 'string' ? parseInt(sel.ageGroup.replace(/\D/g, ''), 10) : null
    if (fromEnum && !isNaN(fromEnum)) return fromEnum
    const fromName = typeof sel.name === 'string' ? sel.name.match(/U(\d+)/i)?.[1] : null
    if (fromName) return parseInt(fromName, 10)
  }
  return groupSportsmen.value[0]?.age ?? null
})
const groupGender = computed(() => groupSportsmen.value[0]?.gender ?? null)

// Год обучения: смотрим Sportsman.CreatedAt — если у хотя бы одного спортсмена группы
// CreatedAt старше года — для группы считается "выше года обучения".
// (Старое поведение через первую метрику — отказались в пользу простого CreatedAt.)
const groupIsAboveYear = computed(() => {
  if (!groupSportsmen.value.length) return false
  const yearAgo = Date.now() - 365 * 24 * 60 * 60 * 1000
  return groupSportsmen.value.some(s => {
    if (!s.createdAt) return false
    return new Date(s.createdAt).getTime() < yearAgo
  })
})

// Подходит ли норматив под группу.
// ГТО (с ageGroup): фильтр по полу + возрасту ±1.
// Локальный (с isAboveYearOfStudy): фильтр по полу + флагу года обучения.
// Если для локального типа в БД есть две версии (true и false) — берём по году группы.
// Если только одна — она и подойдёт (флаг с другой стороны не отсеет, см. ниже filterLocal).
function matchesGroup(n: any): boolean {
  if (n.gender && groupGender.value && n.gender !== groupGender.value) return false
  if (n.ageGroup && groupAge.value) {
    const diff = Math.abs(n.ageGroup - groupAge.value)
    if (diff > 1) return false
  }
  // Год обучения проверяется отдельно после, см. filterLocalNormsByYear ниже
  return true
}

// Среди локальных нормативов: если есть две версии одного type+spec+gender — выбираем подходящую
// по году обучения; иначе берём единственную доступную.
function filterLocalNormsByYear(list: any[]): any[] {
  if (!list.length) return list
  // Группируем по уникальному ключу (type + specialization + gender)
  const groups = new Map<string, any[]>()
  for (const n of list) {
    const key = `${n.type}|${n.specialization}|${n.gender}`
    if (!groups.has(key)) groups.set(key, [])
    groups.get(key)!.push(n)
  }
  const result: any[] = []
  groups.forEach(variants => {
    if (variants.length === 1) {
      result.push(variants[0])
      return
    }
    // Несколько версий — выбираем подходящую по году обучения группы
    const match = variants.find(v => !!v.isAboveYearOfStudy === groupIsAboveYear.value)
    result.push(match ?? variants[0])
  })
  return result
}

const filteredGtoNorms = computed(() => {
  if (showAllNorms.value) return gtoNorms.value
  const filtered = gtoNorms.value.filter(matchesGroup)
  return filtered.length > 0 ? filtered : gtoNorms.value
})

const filteredLocalNorms = computed(() => {
  if (showAllNorms.value) return localNorms.value
  // 1) Фильтр по полу/возрасту 2) Выбор подходящего по году обучения если есть две версии
  const byBase = localNorms.value.filter(matchesGroup)
  const filtered = filterLocalNormsByYear(byBase)
  return filtered.length > 0 ? filtered : localNorms.value
})

const gtoFilterEmpty   = computed(() => !showAllNorms.value && gtoNorms.value.length > 0 && gtoNorms.value.filter(matchesGroup).length === 0)
const localFilterEmpty = computed(() => !showAllNorms.value && localNorms.value.length > 0 && localNorms.value.filter(matchesGroup).length === 0)

// results[id] — новое введённое значение
const results = ref<Record<number, number | string>>({})
// absent[id] — помечен как отсутствовавший
const absent  = ref<Record<number, boolean>>({})
// retake[id] — нажали "Пересдать" для тех у кого есть recent result
const retake  = ref<Record<number, boolean>>({})
// recentResults[id] — последний результат за 3 месяца
const recentResults = ref<Record<number, any>>({})
const loadingRecent = ref(false)

const THREE_MONTHS_MS = 90 * 24 * 60 * 60 * 1000

function hasInput(id: number): boolean {
  const v = results.value[id]
  return v !== undefined && v !== null && v !== ''
}

// Спортсмен без результата — тот у кого нет recentResults (и не нажали retake)
function hasNoResult(id: number): boolean {
  return !recentResults.value[id]
}

// Все спортсмены без результата должны быть либо absent либо иметь ввод
const canSave = computed(() =>
  activeNormId.value !== null &&
  groupSportsmen.value.every(s => {
    if (!hasNoResult(s.id)) return true       // уже есть оценка — пропускаем
    return absent.value[s.id] || hasInput(s.id) // должен быть absent или ввод
  }) &&
  groupSportsmen.value.some(s => !absent.value[s.id] && hasInput(s.id))
)

const saveCount = computed(() =>
  groupSportsmen.value.filter(s => !absent.value[s.id] && hasInput(s.id)).length
)

function gradeForId(id: number): string {
  const val = results.value[id]
  if (!hasInput(id)) return 'none'
  const num = Number(val)
  if (normTab.value === 'gto') {
    const norm = selectedGtoNorm.value
    if (!norm) return 'none'
    const lower = norm.gradeExcellent < norm.gradeSatisfactory
    if (lower) {
      if (num <= norm.gradeExcellent)    return 'excellent'
      if (num <= norm.gradeGood)         return 'good'
      if (num <= norm.gradeSatisfactory) return 'satisfactory'
    } else {
      if (num >= norm.gradeExcellent)    return 'excellent'
      if (num >= norm.gradeGood)         return 'good'
      if (num >= norm.gradeSatisfactory) return 'satisfactory'
    }
    return 'fail'
  } else {
    const norm = selectedLocalNorm.value
    if (!norm) return 'none'
    return norm.isMoreBetter ? (num >= norm.value ? 'pass' : 'fail') : (num <= norm.value ? 'pass' : 'fail')
  }
}

function resultLabel(id: number): string {
  const g = gradeForId(id)
  return ({ excellent: 'Отлично', good: 'Хорошо', satisfactory: 'Удовл.', pass: 'Сдано', fail: 'Не сдано', none: '—' } as any)[g] ?? '—'
}

function resultBadgeClass(id: number): string {
  const g = gradeForId(id)
  return ({ excellent: 'bg-green-100 text-green-700', good: 'bg-blue-100 text-blue-700', satisfactory: 'bg-yellow-100 text-yellow-700', pass: 'bg-teal-100 text-teal-700', fail: 'bg-red-100 text-red-600', none: 'bg-neutral-100 text-neutral-500' } as any)[g] ?? 'bg-neutral-100 text-neutral-500'
}

function inputBorderClass(id: number): string {
  const g = gradeForId(id)
  if (g === 'none') return 'border-neutral-200 focus:border-blue-300 bg-white'
  return ({ excellent: 'border-green-300 bg-green-50/40', good: 'border-blue-300 bg-blue-50/40', satisfactory: 'border-yellow-300 bg-yellow-50/40', pass: 'border-teal-300 bg-teal-50/40', fail: 'border-red-300 bg-red-50/30' } as any)[g] ?? 'border-neutral-200 focus:border-blue-300 bg-white'
}

function recentGradeForId(id: number): string {
  const r = recentResults.value[id]
  if (!r) return 'none'
  if (normTab.value === 'gto') {
    const norm = selectedGtoNorm.value
    if (!norm) return 'none'
    const num = r.result
    const lower = norm.gradeExcellent < norm.gradeSatisfactory
    if (lower) {
      if (num <= norm.gradeExcellent)    return 'excellent'
      if (num <= norm.gradeGood)         return 'good'
      if (num <= norm.gradeSatisfactory) return 'satisfactory'
    } else {
      if (num >= norm.gradeExcellent)    return 'excellent'
      if (num >= norm.gradeGood)         return 'good'
      if (num >= norm.gradeSatisfactory) return 'satisfactory'
    }
    return 'fail'
  } else {
    const norm = selectedLocalNorm.value
    if (!norm) return 'none'
    return norm.isMoreBetter ? (r.result >= norm.value ? 'pass' : 'fail') : (r.result <= norm.value ? 'pass' : 'fail')
  }
}

function recentGradeLabel(id: number): string {
  const g = recentGradeForId(id)
  return ({ excellent: 'Отлично', good: 'Хорошо', satisfactory: 'Удовл.', pass: 'Сдано', fail: 'Не сдано', none: '—' } as any)[g] ?? '—'
}

function recentGradeBadge(id: number): string {
  const g = recentGradeForId(id)
  return ({ excellent: 'bg-green-100 text-green-700', good: 'bg-blue-100 text-blue-700', satisfactory: 'bg-yellow-100 text-yellow-700', pass: 'bg-teal-100 text-teal-700', fail: 'bg-red-100 text-red-600', none: 'bg-neutral-100 text-neutral-400' } as any)[g] ?? 'bg-neutral-100 text-neutral-400'
}

function rowBorderClass(id: number): string {
  if (absent.value[id]) return 'border-neutral-100 bg-neutral-50/60 opacity-60'
  if (recentResults.value[id] && !hasInput(id)) return 'border-green-100 bg-green-50/20'
  return 'border-neutral-200'
}

function initials(fio: string) {
  return fio.trim().split(' ').slice(0, 2).map((w: string) => w[0]?.toUpperCase() ?? '').join('')
}

function toggleRetake(id: number) {
  retake.value[id] = !retake.value[id]
  if (retake.value[id]) {
    const recent = recentResults.value[id]
    if (recent) results.value[id] = recent.result
  } else {
    delete results.value[id]
  }
}

function toggleAbsent(id: number) {
  absent.value[id] = !absent.value[id]
  if (absent.value[id]) {
    // сбрасываем введённый результат при пометке отсутствия
    delete results.value[id]
  }
}

// Загрузить последние результаты за 3 месяца для выбранного норматива
async function loadRecentResults() {
  if (!activeNormId.value || !groupSportsmen.value.length) {
    recentResults.value = {}
    return
  }
  loadingRecent.value = true
  const cutoff = new Date(Date.now() - THREE_MONTHS_MS)
  const isGto = normTab.value === 'gto'

  const fetches = await Promise.allSettled(
    groupSportsmen.value.map(s =>
      api.get(isGto
        ? `/normative/gto/results/sportsman/${s.id}`
        : `/normative/local/results/sportsman/${s.id}`
      ).catch(() => null)
    )
  )

  const map: Record<number, any> = {}
  fetches.forEach((r, i) => {
    if (r.status !== 'fulfilled' || !r.value) return
    const allRes: any[] = r.value.data.data ?? []
    const normKey = isGto ? 'normativeId' : 'localNormativeId'
    // фильтруем по текущему нормативу и за 3 месяца
    const matching = allRes
      .filter(x => x[normKey] === activeNormId.value && new Date(x.createdAt) >= cutoff)
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    if (matching.length) map[groupSportsmen.value[i].id] = matching[0]
  })
  recentResults.value = map
  loadingRecent.value = false
}

function onNormChange() {
  results.value = {}
  absent.value  = {}
  retake.value  = {}
  loadRecentResults()
}

// При смене вкладки сбрасываем
watch(groupSearch, () => { groupPage.value = 1 })

watch(normTab, () => {
  results.value = {}
  absent.value  = {}
  retake.value  = {}
  recentResults.value = {}
})

async function selectGroup(g: any) {
  selectedGroup.value = g
  step.value = 'fill'
  results.value = {}
  absent.value  = {}
  retake.value  = {}
  recentResults.value = {}
  loadingGroupSportsmen.value = true
  const res = g._type === 'team'
    ? await api.get('/sportsman', { params: { filters: { teamId: [g.id] } } }).catch(() => null)
    : await api.get(`/sportsman/group/${g.id}`).catch(() => null)
  groupSportsmen.value = res?.data?.data ?? []
  loadingGroupSportsmen.value = false
  loadRecentResults()
  // Год обучения теперь определяется по Sportsman.CreatedAt (см. groupIsAboveYear computed выше).
}

async function saveResults() {
  if (!activeNormId.value || saving.value) return
  saving.value = true
  const isGto = normTab.value === 'gto'

  // Сохраняем только тех у кого есть новый ввод и кто не absent
  const entries = groupSportsmen.value
    .filter(s => !absent.value[s.id] && hasInput(s.id))
    .map(s => ({
      sportsmanId:      s.id,
      normativeId:      isGto ? activeNormId.value : undefined,
      localNormativeId: isGto ? undefined : activeNormId.value,
      result:           Number(results.value[s.id]),
    }))

  const endpoint = isGto ? '/normative/gto/results' : '/normative/local/results'
  await Promise.allSettled(entries.map(e => api.post(endpoint, e)))

  saving.value = false
  results.value = {}
  absent.value  = {}
  retake.value  = {}
  savedToast.value = true
  setTimeout(() => { savedToast.value = false }, 2500)
  loadRecentResults()
}

onMounted(async () => {
  const tid = auth.personalId
  const [groupsRes, teamsRes, gtoRes, localRes] = await Promise.allSettled([
    tid ? api.get('/group', { params: { filters: { trainerId: [tid] } } }) : Promise.resolve({ data: { data: [] } }),
    tid ? api.get('/team',  { params: { filters: { trainerId: [tid] } } }) : Promise.resolve({ data: { data: [] } }),
    api.get('/normative/gto'),
    api.get('/normative/local'),
  ])
  if (groupsRes.status === 'fulfilled') groups.value     = groupsRes.value.data.data ?? []
  if (teamsRes.status === 'fulfilled')  teams.value      = teamsRes.value.data.data  ?? []
  if (gtoRes.status === 'fulfilled')    gtoNorms.value   = gtoRes.value.data.data    ?? []
  if (localRes.status === 'fulfilled')  localNorms.value = localRes.value.data.data  ?? []
  loadingGroups.value = false
})
</script>

