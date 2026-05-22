<template>
  <Teleport to="body">
    <div class="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4" @click.self="$emit('close')">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl w-full max-w-2xl flex flex-col max-h-[85vh]">
        <div class="flex items-center gap-3 p-5 border-b border-neutral-100">
          <div class="w-10 h-10 rounded-xl bg-sky-50 flex items-center justify-center shrink-0 text-sky-600">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"/>
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-bold text-neutral-900 truncate">Подопечные «{{ personalName }}»</div>
            <div class="text-xs text-neutral-400 mt-0.5">Группы и команды, которыми руководит</div>
          </div>
          <button @click="$emit('close')" class="w-8 h-8 rounded-lg flex items-center justify-center text-neutral-400 hover:bg-neutral-100 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <!-- Табы -->
        <div class="px-5 pt-3 border-b border-neutral-100">
          <div class="flex gap-1">
            <button @click="tab = 'groups'"
              class="text-xs font-semibold px-4 py-2 rounded-t-xl transition-colors border-b-2"
              :class="tab === 'groups' ? 'text-blue-600 border-blue-500' : 'text-neutral-500 border-transparent hover:text-neutral-700'">
              Группы · {{ ownGroups.length }}
            </button>
            <button @click="tab = 'teams'"
              class="text-xs font-semibold px-4 py-2 rounded-t-xl transition-colors border-b-2"
              :class="tab === 'teams' ? 'text-blue-600 border-blue-500' : 'text-neutral-500 border-transparent hover:text-neutral-700'">
              Команды · {{ ownTeams.length }}
            </button>
          </div>
        </div>

        <!-- Контент: 2 колонки -->
        <div class="flex-1 overflow-hidden grid grid-cols-2 gap-4 p-5">
          <div class="flex flex-col min-h-0">
            <div class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2">Руководит · {{ insideList.length }}</div>
            <input v-model="searchIn" placeholder="Поиск..."
              class="w-full text-xs rounded-lg border border-neutral-200 bg-white px-3 py-2 focus:outline-none focus:border-blue-400 mb-2" />
            <div class="flex-1 overflow-y-auto rounded-xl border border-neutral-200 divide-y divide-neutral-50 bg-white">
              <div v-if="!insideFiltered.length" class="px-3 py-4 text-xs text-neutral-400 italic text-center">
                {{ insideList.length ? 'Ничего не найдено' : 'Пусто' }}
              </div>
              <div v-for="it in insideFiltered" :key="it.id"
                class="flex items-center justify-between gap-2 px-3 py-2 hover:bg-neutral-50 transition-colors">
                <div class="flex-1 min-w-0">
                  <div class="text-sm font-semibold text-neutral-800 truncate">{{ it.name }}</div>
                  <div class="text-[11px] text-neutral-400 truncate">{{ subForInside(it) }}</div>
                </div>
                <button @click="onRemove(it.id)"
                  class="w-7 h-7 rounded-lg border border-blue-200 bg-blue-50 text-blue-500 hover:bg-blue-100 transition-colors inline-flex items-center justify-center shrink-0"
                  title="Убрать">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>
          </div>

          <div class="flex flex-col min-h-0">
            <div class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2">Можно назначить · {{ outsideList.length }}</div>
            <input v-model="searchOut" placeholder="Поиск..."
              class="w-full text-xs rounded-lg border border-neutral-200 bg-white px-3 py-2 focus:outline-none focus:border-blue-400 mb-2" />
            <div class="flex-1 overflow-y-auto rounded-xl border border-neutral-200 divide-y divide-neutral-50 bg-white">
              <div v-if="!outsideFiltered.length" class="px-3 py-4 text-xs text-neutral-400 italic text-center">
                {{ outsideList.length ? 'Ничего не найдено' : 'Нет доступных' }}
              </div>
              <div v-for="it in outsideFiltered" :key="it.id"
                class="flex items-center justify-between gap-2 px-3 py-2 hover:bg-neutral-50 transition-colors">
                <div class="flex-1 min-w-0">
                  <div class="text-sm font-semibold text-neutral-800 truncate">{{ it.name }}</div>
                  <div class="text-[11px] text-neutral-400 truncate mt-0.5">{{ subForOutside(it) }}</div>
                </div>
                <button @click="tryAdd(it)"
                  class="w-7 h-7 rounded-lg border border-blue-200 bg-blue-50 text-blue-600 hover:bg-blue-100 transition-colors inline-flex items-center justify-center shrink-0"
                  title="Назначить">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="px-5 pb-5 flex justify-end">
          <button @click="$emit('close')"
            class="px-5 py-2 rounded-xl bg-blue-600 text-white text-xs font-semibold hover:bg-blue-700 transition-colors">
            Готово
          </button>
        </div>
      </div>
    </div>

    <!-- Подтверждение перепривязки -->
    <div v-if="confirmItem" class="fixed inset-0 bg-black/50 z-[60] flex items-center justify-center p-4" @click.self="confirmItem = null">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl w-full max-w-md p-5">
        <div class="flex items-center gap-3 mb-4">
          <div class="w-10 h-10 rounded-xl bg-amber-50 flex items-center justify-center shrink-0 text-amber-600">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
          </div>
          <div class="text-sm font-bold text-neutral-900">Переназначить?</div>
        </div>
        <div class="text-sm text-neutral-700 leading-relaxed">
          {{ tab === 'teams' ? 'Командой' : 'Группой' }}
          «<span class="font-semibold">{{ confirmItem.name }}</span>» сейчас руководит
          <span class="font-semibold">{{ trainerNameOf(confirmItem) }}</span>.
          <br><br>
          У него она исчезнет, а будет назначена «<span class="font-semibold">{{ personalName }}</span>». Продолжить?
        </div>
        <div class="flex gap-2 justify-end mt-5">
          <button @click="confirmItem = null"
            class="px-4 py-2 rounded-xl border border-neutral-200 text-xs font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">Отмена</button>
          <button @click="confirmReassign"
            class="px-4 py-2 rounded-xl bg-amber-500 text-white text-xs font-semibold hover:bg-amber-600 transition-colors">
            Переназначить
          </button>
        </div>
      </div>
    </div>

    <!-- Передача другому тренеру (при попытке снять) -->
    <div v-if="transferItem" class="fixed inset-0 bg-black/50 z-[60] flex items-center justify-center p-4" @click.self="transferItem = null">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl w-full max-w-md flex flex-col max-h-[80vh]">
        <div class="flex items-center gap-3 p-5 border-b border-neutral-100">
          <div class="w-10 h-10 rounded-xl bg-amber-50 flex items-center justify-center shrink-0 text-amber-600">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4"/>
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-bold text-neutral-900">Кому передать?</div>
            <div class="text-xs text-neutral-400 mt-0.5">{{ tab === 'teams' ? 'Команда' : 'Группа' }} «{{ transferItem.name }}» не может остаться без тренера</div>
          </div>
        </div>

        <div class="p-4">
          <input v-model="transferSearch" placeholder="Поиск тренера..."
            class="w-full text-xs rounded-lg border border-neutral-200 bg-white px-3 py-2 focus:outline-none focus:border-blue-400 mb-2" />
          <div class="rounded-xl border border-neutral-200 divide-y divide-neutral-50 bg-white max-h-[300px] overflow-y-auto">
            <div v-if="!filteredTrainers.length" class="px-3 py-4 text-xs text-neutral-400 italic text-center">
              {{ availableTrainers.length ? 'Ничего не найдено' : 'Нет других тренеров' }}
            </div>
            <button v-for="t in filteredTrainers" :key="t.id"
              @click="chooseTransferTarget(t.id)"
              class="w-full flex items-center gap-3 px-3 py-2.5 hover:bg-blue-50 transition-colors text-left">
              <div class="w-8 h-8 rounded-full bg-blue-100 text-blue-700 inline-flex items-center justify-center text-xs font-bold shrink-0">
                {{ t.fio.trim().split(' ').slice(0, 2).map(w => w[0]?.toUpperCase() ?? '').join('') }}
              </div>
              <span class="text-sm font-semibold text-neutral-800 flex-1 truncate">{{ t.fio }}</span>
            </button>
          </div>
        </div>

        <div class="px-5 pb-5 flex justify-end">
          <button @click="transferItem = null"
            class="px-4 py-2 rounded-xl border border-neutral-200 text-xs font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">
            Отмена
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

interface Item { id: number; name: string; trainerId?: number | null; ageGroup?: string; description?: string }
interface Trainer { id: number; fio: string }

const props = defineProps<{
  personalName: string
  personalId: number
  allGroups: Item[]
  allTeams: Item[]
  personalMap?: Record<number, string>
  allTrainers?: Trainer[]
}>()

const emit = defineEmits<{
  'assign-group':   [groupId: number]
  'unassign-group': [groupId: number]
  'assign-team':    [teamId: number]
  'unassign-team':  [teamId: number]
  'transfer-group': [groupId: number, newTrainerId: number]
  'transfer-team':  [teamId: number,  newTrainerId: number]
  close: []
}>()

// При снятии — передаём другому тренеру (нельзя оставить без тренера)
const transferItem = ref<Item | null>(null)
const transferSearch = ref('')

const availableTrainers = computed<Trainer[]>(() => {
  const all = props.allTrainers ?? []
  // Исключаем текущего владельца
  return all.filter(t => t.id !== props.personalId)
})
const filteredTrainers = computed(() =>
  availableTrainers.value.filter(t =>
    !transferSearch.value.trim() ||
    t.fio.toLowerCase().includes(transferSearch.value.trim().toLowerCase())
  )
)

const tab = ref<'groups' | 'teams'>('groups')
const searchIn = ref('')
const searchOut = ref('')
const confirmItem = ref<Item | null>(null)

const ownGroups = computed(() => props.allGroups.filter(g => g.trainerId === props.personalId))
const ownTeams  = computed(() => props.allTeams.filter(t => t.trainerId === props.personalId))

const insideList = computed(() => tab.value === 'groups' ? ownGroups.value : ownTeams.value)
const outsideList = computed(() => {
  const all = tab.value === 'groups' ? props.allGroups : props.allTeams
  return all.filter(x => x.trainerId !== props.personalId)
})

function match(it: Item, q: string): boolean {
  const query = q.trim().toLowerCase()
  if (!query) return true
  return (it.name ?? '').toLowerCase().includes(query)
}
const insideFiltered  = computed(() => insideList.value.filter(it => match(it, searchIn.value)))
const outsideFiltered = computed(() => outsideList.value.filter(it => match(it, searchOut.value)))

function isOccupied(it: Item): boolean {
  return it.trainerId != null
}

function trainerNameOf(it: Item): string {
  if (!it.trainerId) return '—'
  return props.personalMap?.[it.trainerId] ?? `#${it.trainerId}`
}

function subForInside(it: Item): string {
  // У "Руководит" всё внутри текущего тренера: только возрастная (для команд) или описание (для групп)
  if (tab.value === 'teams') return it.ageGroup ?? '—'
  return it.description || '—'
}

function subForOutside(it: Item): string {
  if (isOccupied(it)) return `Тренер: ${trainerNameOf(it)}`
  // Свободно: для команд — возрастная, для групп — описание/—
  if (tab.value === 'teams') return it.ageGroup ?? '—'
  return it.description || '—'
}

function tryAdd(it: Item) {
  if (isOccupied(it)) {
    confirmItem.value = it
  } else {
    doAssign(it.id)
  }
}

function confirmReassign() {
  if (confirmItem.value) doAssign(confirmItem.value.id)
  confirmItem.value = null
}

function doAssign(id: number) {
  if (tab.value === 'groups') emit('assign-group', id)
  else emit('assign-team', id)
}

function onRemove(id: number) {
  // При снятии — открываем модалку выбора нового тренера
  const list = tab.value === 'groups' ? props.allGroups : props.allTeams
  const item = list.find(x => x.id === id)
  if (item) transferItem.value = item
}

function chooseTransferTarget(newTrainerId: number) {
  if (!transferItem.value) return
  if (tab.value === 'groups') emit('transfer-group', transferItem.value.id, newTrainerId)
  else emit('transfer-team', transferItem.value.id, newTrainerId)
  transferItem.value = null
  transferSearch.value = ''
}
</script>
