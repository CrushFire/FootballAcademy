<template>
  <div class="p-3 h-full">
    <TrainerPageCard color="blue" class="h-full flex flex-col">
      <div class="flex items-start justify-between mb-5 pb-4 border-b border-neutral-100">
        <div>
          <h1 class="text-xl font-bold text-neutral-900">{{ isEdit ? 'Редактирование тренировки' : 'Создание тренировки' }}</h1>
          <p class="text-sm text-neutral-500 mt-0.5">
            {{ isEdit
              ? 'Можно редактировать в течение 24 часов с момента создания'
              : 'Заполните поля, выберите состав и проставьте посещаемость' }}
          </p>
        </div>
        <button
          @click="goBack"
          class="flex items-center gap-1.5 px-3 py-2 rounded-xl border border-blue-200 bg-blue-50 text-blue-600 text-xs font-semibold hover:bg-blue-100 transition-colors shrink-0"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
          </svg>
          Назад
        </button>
      </div>

      <div v-if="loading" class="flex-1 flex items-center justify-center text-sm text-neutral-400">Загрузка...</div>
      <div v-else-if="loadError" class="flex-1 flex items-center justify-center text-sm text-red-500">{{ loadError }}</div>
      <div v-else class="flex-1 overflow-y-auto flex flex-col gap-5">
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">

        <!-- Левая колонка — параметры -->
        <div class="flex flex-col gap-4">
          <div>
            <label class="block text-xs font-semibold text-neutral-500 mb-1.5 uppercase tracking-wide">Группа <span class="text-blue-500">*</span></label>
            <SearchableSelect
              v-model="form.groupId"
              :items="groups"
              label-field="name"
              placeholder="Выберите группу..."
              search-placeholder="Поиск группы..."
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-neutral-500 mb-1.5 uppercase tracking-wide">Тип тренировки <span class="text-blue-500">*</span></label>
            <SearchableSelect
              v-model="form.type"
              :items="typeItems"
              label-field="name"
              placeholder="Выберите тип..."
              search-placeholder="Поиск типа..."
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-neutral-500 mb-1.5 uppercase tracking-wide">План тренировки</label>
            <SearchableSelect
              v-model="form.planTrainingId"
              :groups="planGroups"
              label-field="name"
              :search-keys="['name', 'description', 'trainerFIO']"
              placeholder="Без плана"
              search-placeholder="Поиск плана..."
              :allow-empty="true"
              empty-label="Без плана"
            >
              <template #option="{ item }">
                <div class="flex items-center justify-between gap-2">
                  <span class="truncate">{{ item.name }}</span>
                  <span v-if="item.trainerId !== auth.personalId" class="text-[10px] text-neutral-400 shrink-0">{{ item.trainerFIO }}</span>
                </div>
              </template>
            </SearchableSelect>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-semibold text-neutral-500 mb-1.5 uppercase tracking-wide">Дата <span class="text-blue-500">*</span></label>
              <input
                v-model="form.date"
                type="date"
                class="w-full text-sm rounded-xl border border-neutral-200 bg-white px-3 py-2.5 focus:outline-none focus:border-blue-400"
              />
            </div>
            <div>
              <label class="block text-xs font-semibold text-neutral-500 mb-1.5 uppercase tracking-wide">Время <span class="text-blue-500">*</span></label>
              <input
                v-model="form.time"
                type="time"
                class="w-full text-sm rounded-xl border border-neutral-200 bg-white px-3 py-2.5 focus:outline-none focus:border-blue-400"
              />
            </div>
          </div>

          <div class="flex-1 flex flex-col">
            <label class="block text-xs font-semibold text-neutral-500 mb-1.5 uppercase tracking-wide">Описание</label>
            <textarea
              v-model="form.otherInformation"
              rows="3"
              placeholder="Краткое описание тренировки, цели, нагрузка..."
              class="w-full text-sm rounded-xl border border-neutral-200 bg-white px-3 py-2.5 focus:outline-none focus:border-blue-400 resize-none"
            />
          </div>
        </div>

        <!-- Правая колонка — посещаемость -->
        <div class="flex flex-col gap-3">
          <div>
            <label class="block text-xs font-semibold text-neutral-500 uppercase tracking-wide">Посещаемость</label>
            <div v-if="groupSportsmen.length" class="text-[11px] text-neutral-300 mt-0.5">
              Если спортсмен не отмечен, по умолчанию будет проставлено «Не был»
            </div>
          </div>
          <AttendanceEditor
            v-model="attendance"
            :sportsmen="groupSportsmen"
            :group-id="form.groupId"
          />
        </div>
        </div>

        <!-- Общий ряд: ошибка + кнопки на полную ширину -->
        <div class="flex flex-col gap-3">
          <div v-if="formError" class="text-xs text-red-600 bg-red-50 border border-red-200 rounded-xl px-3 py-2">
            {{ formError }}
          </div>
          <div class="flex items-center justify-center gap-3 pt-3 border-t border-neutral-100">
            <button
              @click="goBack"
              class="min-w-[180px] px-6 py-2.5 rounded-xl border border-neutral-300 bg-white text-sm font-semibold text-neutral-600 hover:bg-neutral-50 hover:border-neutral-400 transition-colors"
            >Отмена</button>
            <button
              @click="onSave"
              :disabled="!canSave || saving"
              class="min-w-[220px] px-6 py-2.5 rounded-xl text-sm font-semibold inline-flex items-center justify-center transition-colors border"
              :class="!canSave || saving ? 'bg-neutral-200 text-neutral-400 border-neutral-200 cursor-not-allowed' : 'bg-green-600 border-green-400 text-white hover:bg-green-700 hover:border-green-500'"
            >
              {{ saving ? 'Сохранение...' : (isEdit ? 'Сохранить' : 'Создать') }}
            </button>
          </div>
        </div>
      </div>
    </TrainerPageCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/services/api'
import { useAuthStore } from '@/store/auth'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'
import SearchableSelect from '@/components/trainer/SearchableSelect.vue'
import AttendanceEditor from '@/components/trainer/AttendanceEditor.vue'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const TRAINING_TYPES = ['Общая', 'Физическая', 'Техническая', 'Тактическая', 'Матч', 'Восстановительная', 'Контрольная']
const typeItems = TRAINING_TYPES.map(t => ({ id: t, name: t }))

const isEdit = computed(() => !!route.params.id)
const trainingId = computed(() => route.params.id ? Number(route.params.id) : null)

const loading = ref(true)
const loadError = ref('')
const saving = ref(false)
const formError = ref('')

const form = ref<{
  groupId: number | null
  type: string | null
  planTrainingId: number | null
  date: string
  time: string
  otherInformation: string
}>({
  groupId: null,
  type: null,
  planTrainingId: null,
  date: '',
  time: '',
  otherInformation: '',
})

const groups = ref<any[]>([])
const myPlans = ref<any[]>([])
const otherPlans = ref<any[]>([])
const groupSportsmen = ref<any[]>([])
const attendance = ref<{ sportsmanId: number; status: 'Present'|'Late'|'Absent'|'ExcusedAbsent' }[]>([])

const planGroups = computed(() => [
  { key: 'mine',  label: 'Мои планы',           items: myPlans.value },
  { key: 'other', label: 'Других тренеров',     items: otherPlans.value },
])

const canSave = computed(() =>
  !!form.value.groupId &&
  !!form.value.type &&
  !!form.value.date &&
  !!form.value.time
)

// При смене группы — подгружаем спортсменов
watch(() => form.value.groupId, async (gid, prev) => {
  if (!gid) {
    groupSportsmen.value = []
    return
  }
  if (gid === prev) return
  const res = await api.get(`/sportsman/group/${gid}`).catch(() => null)
  groupSportsmen.value = res?.data?.data ?? []
  // Сбрасываем посещаемость для отсутствующих в новой группе
  const ids = new Set(groupSportsmen.value.map((s: any) => s.id))
  attendance.value = attendance.value.filter(a => ids.has(a.sportsmanId))
})

async function loadGroupsAndPlans() {
  const tid = auth.personalId
  const filters = tid ? { params: { filters: { trainerId: [tid] } } } : undefined

  // Группы — только свои
  const groupsRes = await api.get('/group', filters).catch(() => null)
  groups.value = groupsRes?.data?.data ?? []

  // Свои планы
  const myRes = tid
    ? await api.get(`/personal/${tid}/plans`).catch(() => null)
    : null
  myPlans.value = myRes?.data?.data ?? []

  // Чужие планы — собрать всех personal, исключить себя, вытащить планы каждого
  const personalsRes = await api.get('/personal').catch(() => null)
  const personals: any[] = personalsRes?.data?.data ?? []
  const others = personals.filter(p => p.id !== tid)
  if (others.length) {
    const results = await Promise.allSettled(
      others.map(p => api.get(`/personal/${p.id}/plans`).catch(() => null))
    )
    const plans: any[] = []
    results.forEach(r => {
      if (r.status === 'fulfilled' && r.value) {
        plans.push(...(r.value.data?.data ?? []))
      }
    })
    otherPlans.value = plans
  } else {
    otherPlans.value = []
  }
}

async function loadExisting() {
  if (!trainingId.value) return
  const res = await api.get(`/training/${trainingId.value}`).catch((e) => {
    loadError.value = e?.response?.data?.error ?? 'Не удалось загрузить тренировку'
    return null
  })
  if (!res) return
  const t = res.data?.data
  if (!t) {
    loadError.value = 'Тренировка не найдена'
    return
  }
  // Проверяем что это тренировка текущего тренера
  if (t.trainerId !== auth.personalId) {
    loadError.value = 'Нет доступа к этой тренировке'
    return
  }
  // Проверяем срок редактирования
  if (t.createdAt) {
    const ageMs = Date.now() - new Date(t.createdAt).getTime()
    if (ageMs > 24 * 60 * 60 * 1000) {
      loadError.value = 'Срок редактирования истёк (более 24 часов с момента создания)'
      return
    }
  }

  // Заполняем форму
  const dt = new Date(t.date)
  const pad = (n: number) => String(n).padStart(2, '0')
  form.value.groupId = t.groupId ?? null
  form.value.type = t.type ?? null
  form.value.planTrainingId = t.planTrainingId ?? null
  form.value.date = `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}`
  form.value.time = `${pad(dt.getHours())}:${pad(dt.getMinutes())}`
  form.value.otherInformation = t.otherInformation ?? ''

  // Подгружаем посещаемость
  const attRes = await api.get(`/schedule/attendance/${trainingId.value}`).catch(() => null)
  const items: any[] = attRes?.data?.data ?? []
  attendance.value = items.map(a => ({ sportsmanId: a.sportsmanId, status: a.status }))

  // Подгружаем спортсменов группы (watch это сделает, но мы хотим без задержки)
  if (form.value.groupId) {
    const sRes = await api.get(`/sportsman/group/${form.value.groupId}`).catch(() => null)
    groupSportsmen.value = sRes?.data?.data ?? []
  }
}

function combinedDateTimeIso(): string {
  // Локальный datetime → ISO без сдвига зоны (как datetime-local)
  return `${form.value.date}T${form.value.time}:00`
}

async function onSave() {
  if (!canSave.value) {
    formError.value = 'Заполните обязательные поля'
    return
  }
  formError.value = ''
  saving.value = true
  try {
    const dateIso = combinedDateTimeIso()
    let id = trainingId.value

    if (isEdit.value && id) {
      const payload = {
        trainerId: auth.personalId,
        planTrainingId: form.value.planTrainingId,
        groupId: form.value.groupId,
        type: form.value.type,
        date: dateIso,
        otherInformation: form.value.otherInformation || null,
      }
      await api.put(`/training/${id}`, payload)
    } else {
      const payload = {
        trainerId: auth.personalId,
        planTrainingId: form.value.planTrainingId,
        groupId: form.value.groupId,
        type: form.value.type,
        date: dateIso,
        otherInformation: form.value.otherInformation || null,
      }
      const res = await api.post('/training', payload)
      id = res.data?.data?.id
    }

    // Отправляем посещаемость (всегда — батч, перезатирает старое)
    // Для всех спортсменов группы: если не отмечен — Absent по умолчанию
    if (id && groupSportsmen.value.length) {
      const marked = new Map(attendance.value.map(a => [a.sportsmanId, a.status]))
      const full = groupSportsmen.value.map((s: any) => ({
        sportsmanId: s.id,
        status: marked.get(s.id) ?? 'Absent',
      }))
      await api.post(`/schedule/attendance/${id}`, full)
    }

    router.push('/trainer/trainings')
  } catch (e: any) {
    formError.value = e?.response?.data?.error ?? 'Ошибка при сохранении'
  } finally {
    saving.value = false
  }
}

function goBack() {
  router.push('/trainer/trainings')
}

onMounted(async () => {
  loading.value = true
  try {
    await loadGroupsAndPlans()
    if (isEdit.value) {
      await loadExisting()
    } else {
      // Дефолт: сегодня, текущее время
      const now = new Date()
      const pad = (n: number) => String(n).padStart(2, '0')
      form.value.date = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`
      form.value.time = `${pad(now.getHours())}:${pad(now.getMinutes())}`
    }
  } catch (e: any) {
    loadError.value = e?.response?.data?.error ?? 'Ошибка загрузки'
  } finally {
    loading.value = false
  }
})
</script>
