<template>
  <div class="p-3 flex flex-col gap-3 h-full overflow-y-auto">

    <!-- Шапка тренера -->
    <CoachAthleteHeader
      v-if="mode === 'coach-view'"
      :info="info"
      @back="router.push('/trainer/sportsmen')"
      @edit="openEdit"
    />

    <!-- Dashboard тренера -->
    <CoachAthleteDashboard
      v-if="mode === 'coach-view'"
      :info="info"
      :pentagon-values="pentagonValues"
      :player-profiles="playerProfiles"
      :player-potential-profiles="playerPotentialProfiles"
      :sportsman-position="info.position ?? null"
      :last-match="lastMatch"
      :attendance-stats="attendanceStats"
      :attendance-pct="attendancePct"
      :norm-stats="normStats"
      @go-schedule="goSchedule"
      @go-matches="goMatches"
      @go-normatives="goNormatives"
      @go-trainings="goTrainings"
      @go-attendance="goAttendance"
    />

    <!-- Dashboard спортсмена -->
    <SelfAthleteDashboard
      v-else
      :pentagon-values="pentagonValues"
      :player-profiles="playerProfiles"
      :player-potential-profiles="playerPotentialProfiles"
      :sportsman-position="info.position ?? null"
      :next-class="nextClass"
      :last-training="lastTraining"
      :last-training-metrics="lastTrainingMetrics"
      :last-match="lastMatch"
      :attendance-stats="attendanceStats"
      :attendance-pct="attendancePct"
      :norm-stats="normStats"
      @go-schedule="goSchedule"
      @go-matches="goMatches"
      @go-normatives="goNormatives"
      @go-trainings="goTrainings"
      @go-attendance="goAttendance"
    />

    <!-- Модалка редактирования -->
    <EditModal v-if="editOpen" title="Редактировать параметры" @save="saveEdit" @cancel="editOpen = false">
      <div class="grid grid-cols-2 gap-3">
        <FormField label="Рост (см)">
          <input v-model.number="editForm.height" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
        </FormField>
        <FormField label="Вес (кг)">
          <input v-model.number="editForm.weight" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
        </FormField>
      </div>
      <FormField label="Позиция">
        <PositionSelect v-model="editForm.position" />
      </FormField>
    </EditModal>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { useAthleteDashboardData } from '@/composables/useAthleteDashboardData'
import CoachAthleteHeader from './CoachAthleteHeader.vue'
import CoachAthleteDashboard from './CoachAthleteDashboard.vue'
import SelfAthleteDashboard from './SelfAthleteDashboard.vue'
import EditModal from '@/components/ui/EditModal.vue'
import FormField from '@/components/ui/FormField.vue'
import PositionSelect from '@/components/ui/PositionSelect.vue'

const props = defineProps<{
  mode: 'self' | 'coach-view'
  athleteId?: number
}>()

const router = useRouter()

const { nextClass, lastTraining, lastTrainingMetrics, lastMatch, normStats, attendanceStats, attendancePct, pentagonValues, playerProfiles, playerPotentialProfiles, load } = useAthleteDashboardData()

const info        = ref<any>({})
const effectiveId = ref(0)
const editOpen    = ref(false)
const editForm    = ref<any>({})

const goSchedule     = () => router.push(props.mode === 'coach-view' ? `/trainer/sportsman/${effectiveId.value}/schedule`   : '/schedule')
const goMatches      = () => router.push(props.mode === 'coach-view' ? `/trainer/sportsman/${effectiveId.value}/matches`    : '/matches')
const goNormatives   = () => router.push(props.mode === 'coach-view' ? `/trainer/sportsman/${effectiveId.value}/normatives` : '/normatives')
const goTrainings    = () => router.push(props.mode === 'coach-view' ? `/trainer/sportsman/${effectiveId.value}/trainings`  : '/trainings')
const goAttendance   = () => router.push(props.mode === 'coach-view' ? `/trainer/sportsman/${effectiveId.value}/attendance` : '/attendance')

function openEdit() {
  editForm.value = { height: info.value.height, weight: info.value.weight, position: info.value.position ?? '' }
  editOpen.value = true
}

async function saveEdit() {
  await api.put(`/sportsman/${effectiveId.value}`, {
    fio: info.value.fio,
    birthDate: info.value.birthDate,
    gender: info.value.gender,
    specialization: info.value.specialization,
    height: editForm.value.height,
    weight: editForm.value.weight,
    position: editForm.value.position || null,
  })
  editOpen.value = false
  const res = await api.get(`/sportsman/${effectiveId.value}`).catch(() => null)
  if (res?.data?.data) info.value = { ...info.value, ...res.data.data }
}

onMounted(async () => {
  let id = 0
  if (props.mode === 'coach-view' && props.athleteId) {
    id = props.athleteId
    const res = await api.get(`/sportsman/${id}`).catch(() => null)
    info.value = res?.data?.data ?? {}
    const grpRes = await api.get(`/group/sportsman/${id}`).catch(() => null)
    const groups: any[] = grpRes?.data?.data ?? grpRes?.data ?? []
    if (groups.length) info.value.groupName = groups[0].name ?? groups[0].Name ?? ''
  } else {
    const meRes = await api.get('/sportsman/me').catch(() => null)
    id = meRes?.data?.data?.id ?? 0
    info.value = meRes?.data?.data ?? {}
  }
  effectiveId.value = id
  if (id) await load(id)
})
</script>
