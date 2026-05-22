<template>
  <div class="p-3 h-full relative">
    <AppCard no-border class="h-full flex flex-col">

      <!-- Вкладки -->
      <div class="flex items-center gap-1 mb-4 pb-4 border-b border-neutral-100 flex-shrink-0">
        <button
          v-for="tab in tabs" :key="tab.key"
          @click="activeTab = tab.key"
          class="text-xs px-4 py-1.5 rounded-xl font-medium transition-colors"
          :class="activeTab === tab.key ? 'bg-blue-500 text-white' : 'text-neutral-500 hover:bg-neutral-100'"
        >{{ tab.label }}</button>
      </div>

      <!-- Вкладка: Тренировки -->
      <template v-if="activeTab === 'trainings'">
        <div class="flex items-center gap-2 mb-3 flex-shrink-0">
          <div class="relative flex-1">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
              class="w-4 h-4 text-neutral-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
              <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-4.35-4.35M17 11A6 6 0 115 11a6 6 0 0112 0z"/>
            </svg>
            <input v-model="search" type="text" placeholder="Поиск..."
              class="w-full pl-9 pr-3 py-2 text-sm rounded-xl border border-neutral-200 bg-neutral-50 focus:outline-none focus:border-blue-400 placeholder-neutral-300" />
          </div>
          <select v-model="filterGroup" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
            <option value="">Все группы</option>
            <option v-for="g in groups" :key="g.id" :value="g.id">{{ g.name }}</option>
          </select>
          <select v-model="filterTrainer" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
            <option value="">Все тренеры</option>
            <option v-for="t in trainers" :key="t.id" :value="t.id">{{ t.fio }}</option>
          </select>
          <select v-model="sortBy" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
            <option v-for="o in sortOptions" :key="o.value" :value="o.value">{{ o.label }}</option>
          </select>
          <button @click="sortDir = sortDir === 'asc' ? 'desc' : 'asc'"
            class="w-9 h-9 rounded-xl border border-neutral-200 bg-neutral-50 flex items-center justify-center text-neutral-500 hover:bg-neutral-100 transition-colors"
            :title="sortDir === 'desc' ? 'По убыванию' : 'По возрастанию'">
            <svg v-if="sortDir === 'desc'" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 14l-7 7m0 0l-7-7m7 7V3"/>
            </svg>
            <svg v-else xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M5 10l7-7m0 0l7 7m-7-7v18"/>
            </svg>
          </button>
        </div>

        <div class="flex-1 overflow-y-auto flex flex-col gap-1.5">
          <div v-if="loading" class="text-sm text-neutral-400 py-4 text-center">Загрузка...</div>
          <div v-else-if="!filteredTrainings.length" class="text-sm text-neutral-400 py-4 text-center">Нет записей</div>
          <div v-for="t in pagedTrainings" :key="t.id"
            class="bg-white rounded-xl border border-neutral-200 p-3 flex items-center gap-3 hover:border-blue-200 transition-colors">
            <div class="flex-1 min-w-0">
              <div class="text-sm font-semibold text-neutral-800">{{ t.type || 'Тренировка' }}</div>
              <div class="text-xs text-neutral-400 mt-0.5">
                {{ formatDate(t.date) }}<template v-if="t.groupName"> · {{ t.groupName }}</template><template v-if="t.trainerName"> · {{ t.trainerName }}</template><template v-if="t.planTrainingName"> · {{ t.planTrainingName }}</template>
              </div>
            </div>
            <button @click="openDelete(t)"
              class="w-8 h-8 rounded-lg border border-blue-200 bg-blue-50 hover:bg-blue-100 flex items-center justify-center transition-colors"
              title="Удалить">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-blue-500">
                <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
              </svg>
            </button>
          </div>
        </div>

        <!-- Пагинация -->
        <div v-if="totalPages > 1" class="flex items-center justify-center gap-2 mt-3 flex-shrink-0">
          <button @click="page--" :disabled="page <= 1"
            class="px-3 py-1 text-xs rounded-lg border border-neutral-200 disabled:opacity-30 hover:bg-neutral-50 transition-colors">←</button>
          <span class="text-xs text-neutral-500">{{ page }} / {{ totalPages }}</span>
          <button @click="page++" :disabled="page >= totalPages"
            class="px-3 py-1 text-xs rounded-lg border border-neutral-200 disabled:opacity-30 hover:bg-neutral-50 transition-colors">→</button>
        </div>
      </template>

      <!-- Вкладка: Импорт -->
      <template v-if="activeTab === 'import'">

        <!-- Заголовок + действия (стиль как у Мониторинга) -->
        <div class="flex items-center justify-between mb-4 flex-shrink-0">
          <div>
            <div class="text-sm font-bold text-neutral-800">Импорт метрик</div>
            <div class="text-xs text-neutral-400 mt-0.5">Загрузка данных о тренировках с носимых устройств</div>
          </div>
          <div class="flex items-center gap-2">
            <button @click="runAutoImportNow" :disabled="runningNow"
              class="text-xs px-3 py-1.5 rounded-xl bg-blue-500 text-white hover:bg-blue-600 disabled:opacity-50 transition-colors flex items-center gap-1">
              <svg v-if="runningNow" class="w-3 h-3 animate-spin" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z"/>
              </svg>
              Запустить сейчас
            </button>
            <button @click="toggleAutoImport"
              class="text-xs px-3 py-1.5 rounded-xl border transition-colors"
              :class="autoImportEnabled ? 'border-blue-200 bg-blue-50 text-blue-600 hover:bg-blue-100' : 'border-neutral-200 text-neutral-500 hover:bg-neutral-50'">
              Автоимпорт: {{ autoImportEnabled ? 'Вкл' : 'Выкл' }}
            </button>
          </div>
        </div>

        <!-- 2 KPI карточки (стиль как у Мониторинга) -->
        <div class="grid grid-cols-2 gap-3 mb-4 flex-shrink-0">

          <!-- Автоимпорт статус -->
          <div class="p-4 rounded-2xl border flex items-start gap-3"
            :class="autoImportEnabled ? 'border-blue-200 bg-blue-50/50' : 'border-neutral-200 bg-neutral-50'">
            <div class="w-8 h-8 rounded-lg border flex items-center justify-center flex-shrink-0"
              :class="autoImportEnabled ? 'border-blue-200 bg-blue-100' : 'border-neutral-200 bg-neutral-100'">
              <svg class="w-4 h-4" :class="autoImportEnabled ? 'text-blue-500' : 'text-neutral-400'"
                fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-xs text-neutral-500">Автоимпорт</div>
              <div class="text-sm font-bold mt-0.5" :class="autoImportEnabled ? 'text-blue-700' : 'text-neutral-600'">
                {{ autoImportEnabled ? 'Активен' : 'Выключен' }}
              </div>
              <div class="text-[10px] text-neutral-400 mt-0.5">Запуск в 00:00, проверка хранилища с метриками</div>
            </div>
          </div>

          <!-- Тренировок в БД -->
          <div class="p-4 rounded-2xl border border-neutral-200 bg-neutral-100 flex items-start gap-3">
            <div class="w-8 h-8 rounded-lg border border-neutral-200 bg-neutral-100 flex items-center justify-center flex-shrink-0">
              <svg class="w-4 h-4 text-neutral-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 17v-2a2 2 0 012-2h2a2 2 0 012 2v2m-6 0h6m-9 0H4a2 2 0 01-2-2V5a2 2 0 012-2h16a2 2 0 012 2v10a2 2 0 01-2 2h-2"/>
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-xs text-neutral-500">Тренировок в системе</div>
              <div class="text-sm font-bold text-neutral-800 mt-0.5">{{ trainings.length }}</div>
              <div class="text-[10px] text-neutral-400 mt-0.5">За последний месяц: {{ trainingsLastMonth }} трен. / {{ metricsLastMonth }} метрик</div>
            </div>
          </div>
        </div>

        <!-- Статус последнего запуска -->
        <div v-if="runNowMessage" class="p-3 rounded-xl mb-4 flex-shrink-0 text-xs"
          :class="runNowEmpty
            ? 'bg-neutral-50 border border-neutral-200 text-neutral-500'
            : runNowSuccess
              ? 'bg-blue-50 border border-blue-200 text-blue-700'
              : 'bg-red-50 border border-red-200 text-red-700'">
          {{ runNowMessage }}
        </div>

        <!-- Ручная загрузка -->
        <div class="flex-1 overflow-y-auto flex flex-col gap-3">
          <div class="text-xs font-bold text-neutral-600 uppercase tracking-wide">Загрузка файлов</div>

          <label class="flex items-center justify-center gap-2 px-4 py-3 rounded-xl border-2 border-dashed border-blue-300 bg-blue-50 hover:bg-blue-100 hover:border-blue-400 transition-colors cursor-pointer">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-blue-600">
              <path stroke-linecap="round" stroke-linejoin="round" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"/>
            </svg>
            <span class="text-sm font-medium text-blue-700">Выбрать файлы (CSV / Excel)</span>
            <input type="file" multiple class="hidden" @change="onFileSelect" accept=".csv,.xlsx,.xls" />
          </label>

          <div v-if="selectedFiles.length" class="space-y-1">
            <div v-for="(f, i) in selectedFiles" :key="i"
              class="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-white border border-neutral-200">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-400 shrink-0">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
              </svg>
              <span class="text-xs text-neutral-600 flex-1 truncate">{{ f.name }}</span>
              <span class="text-[10px] text-neutral-400">{{ (f.size / 1024).toFixed(1) }} KB</span>
              <button @click="removeFile(i)" class="text-neutral-400 hover:text-red-500 transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
            </div>

            <button @click="uploadFiles" :disabled="uploading"
              class="text-xs px-4 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 disabled:opacity-40 transition-colors flex items-center gap-1.5 mt-2">
              <svg v-if="uploading" class="w-3 h-3 animate-spin" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z"/>
              </svg>
              {{ uploading ? 'Загрузка...' : `Загрузить (${selectedFiles.length})` }}
            </button>
          </div>

          <div v-if="uploadMessage" class="text-xs p-2 rounded-lg"
            :class="uploadSuccess ? 'bg-blue-50 text-blue-700 border border-blue-200' : 'bg-red-50 text-red-700 border border-red-200'">
            {{ uploadMessage }}
          </div>
        </div>
      </template>

    </AppCard>

    <!-- Модалка удаления -->
    <Teleport to="body">
      <div v-if="deleteItem" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="bg-white rounded-2xl shadow-xl w-full max-w-md p-6 flex flex-col gap-4">
          <div class="text-base font-bold text-neutral-900">Удалить тренировку?</div>
          <div class="text-sm text-neutral-500">
            {{ deleteItem.type || 'Тренировка' }} от {{ formatDate(deleteItem.date) }}
            <template v-if="deleteItem.groupName"> · {{ deleteItem.groupName }}</template>.
            Будут удалены все связанные метрики и записи посещаемости.
          </div>
          <div class="flex gap-3 mt-1">
            <button @click="deleteItem = null"
              class="flex-1 py-2 rounded-xl border border-neutral-300 bg-neutral-100 hover:bg-neutral-200 transition-colors text-sm font-semibold text-neutral-700">
              Отмена
            </button>
            <button @click="confirmDelete" :disabled="deleting"
              class="flex-1 py-2 rounded-xl bg-red-500 hover:bg-red-600 disabled:opacity-50 transition-colors text-sm font-semibold text-white">
              {{ deleting ? 'Удаление...' : 'Удалить' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import AppCard from '@/components/ui/AppCard.vue'

type TabKey = 'trainings' | 'import'
const tabs: { key: TabKey; label: string }[] = [
  { key: 'trainings', label: 'Тренировки' },
  { key: 'import',    label: 'Импорт' },
]

const activeTab = ref<TabKey>('trainings')

// ===== Тренировки =====
const PER_PAGE = 20
const loading = ref(true)
const trainings = ref<any[]>([])
const groups = ref<any[]>([])
const trainers = ref<any[]>([])
const search = ref('')
const filterGroup = ref('')
const filterTrainer = ref('')
const sortBy = ref<'date'|'type'|'group'|'trainer'>('date')
const sortDir = ref<'asc'|'desc'>('desc')
const page = ref(1)

const sortOptions = [
  { value: 'date',    label: 'По дате' },
  { value: 'type',    label: 'По типу' },
  { value: 'group',   label: 'По группе' },
  { value: 'trainer', label: 'По тренеру' },
]

const filteredTrainings = computed(() => {
  let list = trainings.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(t =>
      t.type?.toLowerCase().includes(q) ||
      t.groupName?.toLowerCase().includes(q) ||
      t.trainerName?.toLowerCase().includes(q) ||
      t.planTrainingName?.toLowerCase().includes(q)
    )
  }
  if (filterGroup.value)   list = list.filter(t => t.groupId === Number(filterGroup.value))
  if (filterTrainer.value) list = list.filter(t => t.trainerId === Number(filterTrainer.value))

  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'date')         res = new Date(b.date).getTime() - new Date(a.date).getTime()
    else if (sortBy.value === 'type')    res = (a.type ?? '').localeCompare(b.type ?? '', 'ru')
    else if (sortBy.value === 'group')   res = (a.groupName ?? '').localeCompare(b.groupName ?? '', 'ru')
    else if (sortBy.value === 'trainer') res = (a.trainerName ?? '').localeCompare(b.trainerName ?? '', 'ru')
    return sortDir.value === 'asc' ? -res : res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredTrainings.value.length / PER_PAGE)))
const pagedTrainings = computed(() => filteredTrainings.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

// Сколько было импортировано за последний запуск «Запустить сейчас»
const lastImportTrainings = ref(0)
const lastImportMetrics = ref(0)

const trainingsLastMonth = computed(() => {
  const monthAgo = Date.now() - 30 * 24 * 60 * 60 * 1000
  return trainings.value.filter((t: any) => new Date(t.date).getTime() >= monthAgo).length
})

// Метрик за последний месяц (берём сумму metricsCount у тренировок за окно)
const metricsLastMonth = computed(() => {
  const monthAgo = Date.now() - 30 * 24 * 60 * 60 * 1000
  return trainings.value
    .filter((t: any) => new Date(t.date).getTime() >= monthAgo)
    .reduce((sum: number, t: any) => sum + (t.metricsCount ?? 0), 0)
})

// Удаление
const deleteItem = ref<any>(null)
const deleting = ref(false)

function openDelete(t: any) { deleteItem.value = t }

async function confirmDelete() {
  if (!deleteItem.value) return
  deleting.value = true
  try {
    await api.delete(`/training/${deleteItem.value.id}`)
    trainings.value = trainings.value.filter(t => t.id !== deleteItem.value.id)
    deleteItem.value = null
  } catch (e) {
    console.error(e)
  } finally {
    deleting.value = false
  }
}

async function loadTrainings() {
  loading.value = true
  try {
    const [trRes, grRes, prRes] = await Promise.all([
      api.get('/training').catch(() => null),
      api.get('/group').catch(() => null),
      api.get('/personal').catch(() => null),
    ])
    trainings.value = trRes?.data?.data ?? []
    groups.value    = grRes?.data?.data ?? []
    const personalList = prRes?.data?.data ?? []
    trainers.value = personalList.filter((p: any) => (p.type ?? p.Type) === 'Trainer')
  } catch (e) {
    console.error(e)
  }
  loading.value = false
}

// ===== Импорт =====
const autoImportEnabled = ref(true)
const runningNow = ref(false)
const runNowMessage = ref('')
const runNowSuccess = ref(false)
const runNowEmpty = ref(false)

const selectedFiles = ref<File[]>([])
const uploading = ref(false)
const uploadMessage = ref('')
const uploadSuccess = ref(false)

async function loadAutoImportStatus() {
  const res = await api.get('/metric/auto-import/status').catch(() => null)
  autoImportEnabled.value = !!(res?.data?.enabled)
}

async function toggleAutoImport() {
  const next = !autoImportEnabled.value
  const res = await api.post('/metric/auto-import/toggle', next).catch(() => null)
  if (res?.data?.enabled !== undefined) {
    autoImportEnabled.value = res.data.enabled
  }
}

async function runAutoImportNow() {
  runningNow.value = true
  runNowMessage.value = ''
  runNowEmpty.value = false
  try {
    const res = await api.post('/metric/auto-import/run-now')
    const data = res?.data?.data ?? res?.data
    const msg = data?.message ?? res?.data?.message ?? ''
    const processed = data?.processed ?? data?.imported ?? data?.count ?? 0
    const metricsImported = data?.metricsImported ?? 0
    const noFiles = (typeof processed === 'number' && processed === 0 && metricsImported === 0)
      || /пуст|нет (новых )?файл|нет (новых )?метрик|no files|empty/i.test(msg)

    // Запоминаем сколько импортировано за этот запуск (для KPI-карточек)
    lastImportTrainings.value = processed
    lastImportMetrics.value = metricsImported

    if (noFiles) {
      runNowEmpty.value = true
      runNowSuccess.value = true
      runNowMessage.value = 'Хранилище не содержит новых метрик'
    } else {
      runNowSuccess.value = true
      runNowMessage.value = msg || 'Импорт завершён'
    }
    await loadTrainings()
  } catch (e: any) {
    runNowSuccess.value = false
    runNowMessage.value = e?.response?.data?.message ?? 'Ошибка при запуске'
  } finally {
    runningNow.value = false
  }
}

function onFileSelect(e: Event) {
  const target = e.target as HTMLInputElement
  if (target.files) {
    selectedFiles.value = [...selectedFiles.value, ...Array.from(target.files)]
    target.value = ''
  }
}

function removeFile(idx: number) {
  selectedFiles.value.splice(idx, 1)
}

async function uploadFiles() {
  if (!selectedFiles.value.length) return
  uploading.value = true
  uploadMessage.value = ''
  try {
    const formData = new FormData()
    for (const f of selectedFiles.value) formData.append('files', f)
    const res = await api.post('/metric/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    uploadSuccess.value = true
    uploadMessage.value = res?.data?.message ?? res?.data?.data?.message ?? `Загружено: ${selectedFiles.value.length} файл(а/ов)`
    selectedFiles.value = []
    await loadTrainings()
  } catch (e: any) {
    uploadSuccess.value = false
    uploadMessage.value = e?.response?.data?.message ?? 'Ошибка при загрузке'
  } finally {
    uploading.value = false
  }
}

onMounted(async () => {
  await Promise.all([loadTrainings(), loadAutoImportStatus()])
})
</script>
