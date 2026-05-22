import { ref, computed } from 'vue'
import api from '@/services/api'
import type { Training, Match } from '@/types'

export function useAthleteDashboardData() {
  const sportsmanId      = ref(0)
  const sportsmanInfo    = ref<any>(null)
  const nextClass        = ref<any>(null)
  const lastTraining     = ref<Training | null>(null)
  const lastTrainingMetrics = ref<any>(null)
  const lastMatch        = ref<Match | null>(null)
  const normStats        = ref<{ excellent: number; good: number; satisfactory: number } | null>(null)
  const attendanceStats  = ref({ present: 0, absent: 0, late: 0 })
  const pentagonValues   = ref([0, 0, 0, 0, 0])
  const playerProfiles          = ref<string[]>([])
  const playerPotentialProfiles = ref<string[]>([])

  const attendancePct = computed(() => {
    const total = attendanceStats.value.present + attendanceStats.value.absent + attendanceStats.value.late
    if (!total) return { present: 0, absent: 0, late: 0 }
    return {
      present: Math.round(attendanceStats.value.present / total * 100),
      absent:  Math.round(attendanceStats.value.absent  / total * 100),
      late:    Math.round(attendanceStats.value.late    / total * 100),
    }
  })

  async function load(id: number) {
    sportsmanId.value = id

    const [trainPastRes, trainAllRes, matchRes, pentagonRes, attendanceRes, normRes, classesRes, groupsRes, profileRes] =
      await Promise.allSettled([
        api.get(`/training/sportsman/${id}`),
        api.get('/training'),
        api.get('/match'),
        api.get(`/analytic/${id}/pentagon`),
        api.get(`/schedule/attendance/sportsman/${id}`),
        api.get(`/normative/gto/results/sportsman/${id}`),
        api.get('/schedule'),
        api.get(`/group/sportsman/${id}`),
        api.get(`/analytic/${id}/profile`),
      ])

    // Тренировки
    if (trainPastRes.status === 'fulfilled' || trainAllRes.status === 'fulfilled') {
      const pastList: Training[] = trainPastRes.status === 'fulfilled' ? (trainPastRes.value.data.data ?? []) : []
      const sortedPast = [...pastList].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
      lastTraining.value = sortedPast[0] ?? null

      if (lastTraining.value) {
        const metricsRes = await api.get(`/analytic/${id}/metrics/${lastTraining.value.id}`).catch(() => null)
        if (metricsRes) {
          const raw = metricsRes.data.data?.metrics ?? null
          if (raw) {
            raw.rsa         = raw.rSA         ?? raw.RSA         ?? 0
            raw.hrStability = raw.hRStability ?? raw.HRStability ?? 0
            raw.hrRedPct    = raw.hRRedPercent ?? raw.hrRedPercent ?? 0
            raw.hiLoRatio   = raw.hI_LO_Ratio  ?? raw.hiLoRatio   ?? 0
          }
          lastTrainingMetrics.value = raw
        }
      }


    }

    // Матч
    if (matchRes.status === 'fulfilled') {
      const list: Match[] = matchRes.value.data.data ?? []
      lastMatch.value = [...list].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())[0] ?? null
    }

    // Игровой профиль
    if (profileRes.status === 'fulfilled') {
      const data = profileRes.value.data.data
      const profiles = data?.profiles ?? data?.Profiles ?? null
      const active    = profiles?.profiles          ?? profiles?.Profiles          ?? []
      const potential = profiles?.potentialProfiles ?? profiles?.PotentialProfiles ?? []
      const norm = (p: any) => typeof p === 'string' ? p : (p.profile ?? p.Profile ?? '')
      playerProfiles.value          = active.map(norm)
      playerPotentialProfiles.value = potential.map(norm)
    }

    // Пятиугольник
    if (pentagonRes.status === 'fulfilled') {
      const p = pentagonRes.value.data.data?.pentagon ?? pentagonRes.value.data.data?.Pentagon
      if (p) pentagonValues.value = [
        p.Speed ?? p.speed ?? 0,
        p.Power ?? p.power ?? 0,
        p.Endurance ?? p.endurance ?? 0,
        p.Sprints ?? p.sprints ?? 0,
        p.Explosive ?? p.explosive ?? 0,
      ]
    }

    // Ближайшее занятие
    if (classesRes.status === 'fulfilled') {
      const allClasses: any[] = classesRes.value.data.data ?? []
      const sportsmanGroupIds: number[] = groupsRes.status === 'fulfilled'
        ? (groupsRes.value.data.data ?? groupsRes.value.data ?? []).map((g: any) => g.Id ?? g.id)
        : []
      const classList = sportsmanGroupIds.length
        ? allClasses.filter(c => sportsmanGroupIds.includes(c.GroupId ?? c.groupId))
        : allClasses
      const dowMap: Record<string, number> = { Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 }
      const now = new Date()
      const todayDow = now.getDay()
      const todayTime = now.getHours() * 60 + now.getMinutes()
      const withNext = classList.map(c => {
        const rawDow = c.DayOfWeek ?? c.dayOfWeek
        const dow = typeof rawDow === 'number' ? rawDow : (dowMap[rawDow] ?? -1)
        const rawTime = c.BeginTime ?? c.beginTime ?? '00:00:00'
        const [h, m] = rawTime.split(':').map(Number)
        let daysAhead = (dow - todayDow + 7) % 7
        if (daysAhead === 0 && h * 60 + m <= todayTime) daysAhead = 7
        const nextDate = new Date(now)
        nextDate.setDate(now.getDate() + daysAhead)
        nextDate.setHours(h, m, 0, 0)
        return { ...c, nextDate }
      })
      withNext.sort((a, b) => a.nextDate.getTime() - b.nextDate.getTime())
      nextClass.value = withNext[0] ?? null
    }

    // Посещаемость — на дашборде считаем за последние 3 месяца
    if (attendanceRes.status === 'fulfilled') {
      const raw = attendanceRes.value.data
      const workouts: any[] = raw?.data ?? raw ?? []
      const threeMonthsAgo = new Date()
      threeMonthsAgo.setMonth(threeMonthsAgo.getMonth() - 3)
      const recent = workouts.filter((w: any) => {
        const d = w.trainingDate ?? w.TrainingDate ?? w.createdAt ?? w.CreatedAt
        return d ? new Date(d) >= threeMonthsAgo : true
      })
      attendanceStats.value = {
        present: recent.filter((w: any) => (w.Status ?? w.status) === 'Present').length,
        absent:  recent.filter((w: any) => (w.Status ?? w.status) === 'Absent').length,
        late:    recent.filter((w: any) => (w.Status ?? w.status) === 'Late').length,
      }
    }

    // Нормативы
    if (normRes.status === 'fulfilled') {
      const allResults = normRes.value.data.data ?? []
      const sixMonthsAgo = new Date()
      sixMonthsAgo.setMonth(sixMonthsAgo.getMonth() - 6)
      const results = allResults.filter((r: any) => new Date(r.createdAt) >= sixMonthsAgo)
      const normsRes = await api.get('/normative/gto').catch(() => null)
      const norms: any[] = normsRes?.data?.data ?? []
      let excellent = 0, good = 0, satisfactory = 0
      for (const r of results) {
        const norm = norms.find((n: any) => n.id === r.normativeId)
        if (!norm) continue
        const isLowerBetter = norm.gradeExcellent < norm.gradeSatisfactory
        if (isLowerBetter) {
          if (r.result <= norm.gradeExcellent) excellent++
          else if (r.result <= norm.gradeGood) good++
          else if (r.result <= norm.gradeSatisfactory) satisfactory++
        } else {
          if (r.result >= norm.gradeExcellent) excellent++
          else if (r.result >= norm.gradeGood) good++
          else if (r.result >= norm.gradeSatisfactory) satisfactory++
        }
      }
      if (excellent + good + satisfactory > 0)
        normStats.value = { excellent, good, satisfactory }
    }
  }

  return {
    sportsmanId, sportsmanInfo, nextClass, lastTraining, lastTrainingMetrics,
    lastMatch, normStats, attendanceStats, attendancePct, pentagonValues,
    playerProfiles, playerPotentialProfiles, load,
  }
}
