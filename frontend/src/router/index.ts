import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/store/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('@/pages/LoginPage.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/',
      component: () => import('@/layouts/AppLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'Dashboard',
          component: () => import('@/pages/Sportsman/DashboardPage.vue')
        },
        {
          path: 'admin-dashboard',
          name: 'AdminDashboard',
          component: () => import('@/pages/Admin/DashboardPage.vue')
        },
        {
          path: 'admin/manage',
          name: 'AdminManage',
          component: () => import('@/pages/Admin/ManagePage.vue')
        },
        {
          path: 'admin/sportsmen',
          name: 'AdminSportsmen',
          component: () => import('@/pages/Admin/SportsmenPage.vue')
        },
        {
          path: 'admin/personal',
          name: 'AdminPersonal',
          component: () => import('@/pages/Admin/PersonalPage.vue')
        },
        {
          path: 'admin/trainings',
          name: 'AdminTrainings',
          component: () => import('@/pages/Admin/TrainingsPage.vue')
        },
        {
          path: 'admin/groups',
          name: 'AdminGroups',
          component: () => import('@/pages/Admin/GroupsPage.vue')
        },
        {
          path: 'admin/teams',
          name: 'AdminTeams',
          component: () => import('@/pages/Admin/TeamsPage.vue')
        },
        {
          path: 'admin/matches',
          name: 'AdminMatches',
          component: () => import('@/pages/Admin/MatchesPage.vue')
        },
        {
          path: 'admin/schedule',
          name: 'AdminSchedule',
          component: () => import('@/pages/Admin/SchedulePage.vue')
        },
        {
          path: 'admin/normatives',
          name: 'AdminNormatives',
          component: () => import('@/pages/Admin/NormativesPage.vue')
        },
        {
          path: 'admin/plans',
          name: 'AdminPlans',
          component: () => import('@/pages/Admin/PlansPage.vue')
        },
        {
          path: 'admin/workouts',
          name: 'AdminWorkouts',
          component: () => import('@/pages/Admin/WorkoutsPage.vue')
        },
        {
          path: 'admin/messages',
          name: 'AdminMessages',
          component: () => import('@/pages/Admin/MessagesPage.vue')
        },
        {
          path: 'admin/profile',
          name: 'AdminProfile',
          component: () => import('@/pages/Admin/ProfilePage.vue')
        },
        // Medical routes
        {
          path: 'medical',
          name: 'MedicalDashboard',
          component: () => import('@/pages/Medical/DashboardPage.vue')
        },
        {
          path: 'medical/sportsmen',
          name: 'MedicalSportsmen',
          component: () => import('@/pages/Medical/SportsmenPage.vue')
        },
        {
          path: 'medical/normatives',
          name: 'MedicalNormatives',
          component: () => import('@/pages/Medical/NormativesPage.vue')
        },
        {
          path: 'medical/checks',
          name: 'MedicalChecks',
          component: () => import('@/pages/Medical/ChecksPage.vue')
        },
        {
          path: 'medical/sportsman/:id',
          name: 'MedicalSportsmanProfile',
          component: () => import('@/pages/Medical/SportsmanPage.vue')
        },
        {
          path: 'medical/messages',
          name: 'MedicalMessages',
          component: () => import('@/pages/Medical/MessagesPage.vue')
        },
        {
          path: 'medical/profile',
          name: 'MedicalProfile',
          component: () => import('@/pages/Medical/ProfilePage.vue')
        },
        // Trainer routes
        {
          path: 'trainer',
          name: 'TrainerDashboard',
          component: () => import('@/pages/Trainer/DashboardPage.vue')
        },
        {
          path: 'trainer/sportsmen',
          name: 'TrainerSportsmen',
          component: () => import('@/pages/Trainer/SportsmenPage.vue')
        },
        {
          path: 'trainer/groups',
          name: 'TrainerGroups',
          component: () => import('@/pages/Trainer/GroupsPage.vue')
        },
        {
          path: 'trainer/trainings',
          name: 'TrainerTrainings',
          component: () => import('@/pages/Trainer/TrainingsPage.vue')
        },
        {
          path: 'trainer/training/create',
          name: 'TrainerTrainingCreate',
          component: () => import('@/pages/Trainer/TrainingFormPage.vue')
        },
        {
          path: 'trainer/training/edit/:id',
          name: 'TrainerTrainingEdit',
          component: () => import('@/pages/Trainer/TrainingFormPage.vue')
        },
        {
          path: 'trainer/trainings/:id',
          name: 'TrainerTrainingDetail',
          component: () => import('@/pages/Trainer/TrainingDetailPage.vue')
        },
        {
          path: 'trainer/schedule',
          name: 'TrainerSchedule',
          component: () => import('@/pages/Trainer/SchedulePage.vue')
        },
        {
          path: 'trainer/matches',
          name: 'TrainerMatches',
          component: () => import('@/pages/Trainer/MatchesPage.vue')
        },
        {
          path: 'trainer/matches/:id',
          name: 'TrainerMatchDetail',
          component: () => import('@/pages/Trainer/MatchDetailPage.vue')
        },
        {
          path: 'trainer/normatives',
          name: 'TrainerNormatives',
          component: () => import('@/pages/Trainer/NormativesPage.vue')
        },
        {
          path: 'trainer/messages',
          name: 'TrainerMessages',
          component: () => import('@/pages/Trainer/MessagesPage.vue')
        },
        {
          path: 'trainer/profile',
          name: 'TrainerProfile',
          component: () => import('@/pages/Trainer/ProfilePage.vue')
        },
        {
          path: 'trainer/compare',
          name: 'TrainerCompare',
          component: () => import('@/pages/Trainer/ComparePage.vue')
        },
        {
          path: 'trainer/ai-eval',
          name: 'TrainerAiEval',
          component: () => import('@/pages/Trainer/AiEvalPage.vue')
        },
        {
          path: 'trainer/plans',
          name: 'TrainerPlans',
          component: () => import('@/pages/Trainer/PlansPage.vue')
        },
        {
          path: 'trainer/workouts',
          name: 'TrainerWorkouts',
          component: () => import('@/pages/Trainer/WorkoutsPage.vue')
        },
        {
          path: 'trainer/match-live',
          name: 'TrainerMatchLive',
          component: () => import('@/pages/Trainer/MatchLivePage.vue')
        },
        {
          path: 'trainer/normative-record',
          name: 'TrainerNormativeRecord',
          component: () => import('@/pages/Trainer/NormativeRecordPage.vue')
        },
        {
          path: 'trainer/sportsman/:id',
          name: 'TrainerSportsmanProfile',
          component: () => import('@/pages/Trainer/SportsmanProfilePage.vue')
        },
        {
          path: 'trainer/sportsman/:id/schedule',
          name: 'TrainerSportsmanSchedule',
          component: () => import('@/pages/SchedulePage.vue')
        },
        {
          path: 'trainer/sportsman/:id/trainings',
          name: 'TrainerSportsmanTrainings',
          component: () => import('@/pages/Sportsman/TrainingsPage.vue')
        },
        {
          path: 'trainer/sportsman/:id/matches',
          name: 'TrainerSportsmanMatches',
          component: () => import('@/pages/MatchesPage.vue')
        },
        {
          path: 'trainer/sportsman/:id/normatives',
          name: 'TrainerSportsmanNormatives',
          component: () => import('@/pages/NormativesPage.vue')
        },
        {
          path: 'trainer/sportsman/:id/attendance',
          name: 'TrainerSportsmanAttendance',
          component: () => import('@/pages/Sportsman/AttendancePage.vue')
        },
        {
          path: 'sportsmen',
          name: 'Sportsmen',
          component: () => import('@/pages/SportsmenPage.vue')
        },
        {
          path: 'sportsmen/:id',
          name: 'SportsmanDetail',
          component: () => import('@/pages/SportsmanDetailPage.vue')
        },
        {
          path: 'groups',
          name: 'Groups',
          component: () => import('@/pages/GroupsPage.vue')
        },
        {
          path: 'teams',
          name: 'Teams',
          component: () => import('@/pages/TeamsPage.vue')
        },
        {
          path: 'trainings',
          name: 'Trainings',
          component: () => import('@/pages/Sportsman/TrainingsPage.vue')
        },
        {
          path: 'attendance',
          name: 'Attendance',
          component: () => import('@/pages/Sportsman/AttendancePage.vue')
        },
        {
          path: 'trainings/:id',
          name: 'TrainingDetail',
          component: () => import('@/pages/TrainingDetailPage.vue')
        },
        {
          path: 'matches',
          name: 'Matches',
          component: () => import('@/pages/MatchesPage.vue')
        },
        {
          path: 'matches/:id',
          name: 'MatchDetail',
          component: () => import('@/pages/MatchDetailPage.vue')
        },
        {
          path: 'workouts',
          name: 'Workouts',
          component: () => import('@/pages/Sportsman/WorkoutsPage.vue')
        },
        {
          path: 'normatives',
          name: 'Normatives',
          component: () => import('@/pages/NormativesPage.vue')
        },
        {
          path: 'schedule',
          name: 'Schedule',
          component: () => import('@/pages/SchedulePage.vue')
        },
        {
          path: 'messages',
          name: 'Messages',
          component: () => import('@/pages/MessagesPage.vue')
        },
        {
          path: 'analytics/:id',
          name: 'Analytics',
          component: () => import('@/pages/AnalyticsPage.vue')
        },

      ]
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/'
    }
  ]
})

// Навигационный гвард, проверка авторизации
router.beforeEach((to) => {
  const auth = useAuthStore()
  const requiresAuth = to.matched.some(r => r.meta.requiresAuth)
  if (requiresAuth && !auth.isAuthenticated) {
    return { name: 'Login' }
  }
  if (to.name === 'Login' && auth.isAuthenticated) {
    if (auth.userRole === 'admin') return { name: 'AdminDashboard' }
    if (auth.userRole === 'personal' && auth.personalType?.toLowerCase() === 'trainer') return { name: 'TrainerDashboard' }
    if (auth.userRole === 'personal' && auth.personalType?.toLowerCase() === 'medical') return { name: 'MedicalDashboard' }
    return { name: 'Dashboard' }
  }
})

export default router
