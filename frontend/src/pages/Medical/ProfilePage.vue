<template>
  <div class="p-3 h-full">
    <div class="bg-white rounded-2xl border border-neutral-200 shadow-sm h-full flex overflow-hidden">

      <div v-if="loading" class="flex-1 flex items-center justify-center text-sm text-neutral-400">Загрузка...</div>

      <template v-else-if="profile">
        <!-- Левая колонка: аватар + имя + выход -->
        <div class="w-60 shrink-0 flex flex-col items-center pt-10 px-6 pb-6">
          <div class="flex flex-col items-center gap-4 flex-1">
            <div class="relative group cursor-pointer" @click="avatarInput?.click()">
              <div class="w-48 h-48 rounded-full overflow-hidden shrink-0 bg-emerald-600 flex items-center justify-center">
                <img v-if="avatarUrl" :src="avatarUrl" class="w-full h-full object-cover object-top" alt="Аватар" />
                <span v-else class="text-white font-bold text-5xl">{{ initials(profile.fio) }}</span>
              </div>
              <div class="absolute inset-0 rounded-full bg-black/40 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-8 h-8 text-white">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5m-13.5-9L12 3m0 0l4.5 4.5M12 3v13.5"/>
                </svg>
              </div>
              <input ref="avatarInput" type="file" accept="image/*" class="hidden" @change="uploadAvatar" />
            </div>
            <div class="text-center">
              <div class="text-base font-bold text-neutral-900 leading-snug">{{ profile.fio }}</div>
              <div class="text-xs text-neutral-500 mt-1">Медицинский персонал</div>
            </div>
          </div>
          <button
            @click="showLogoutConfirm = true"
            class="w-full mt-auto py-2 px-4 rounded-xl border border-neutral-300 bg-neutral-100 hover:bg-neutral-200 transition-colors text-sm font-semibold text-neutral-700 flex items-center justify-center gap-2"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
            </svg>
            Выйти
          </button>
        </div>

        <Teleport to="body">
          <div v-if="showLogoutConfirm" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
            <div class="bg-white rounded-2xl shadow-xl p-6 w-80 flex flex-col gap-4">
              <div class="text-base font-bold text-neutral-900 text-center">Выйти из аккаунта?</div>
              <div class="text-sm text-neutral-500 text-center">Вы будете перенаправлены на страницу входа</div>
              <div class="flex gap-3 mt-1">
                <button @click="showLogoutConfirm = false" class="flex-1 py-2 rounded-xl border border-neutral-300 bg-neutral-100 hover:bg-neutral-200 transition-colors text-sm font-semibold text-neutral-700">Отмена</button>
                <button @click="auth.logout()" class="flex-1 py-2 rounded-xl bg-red-500 hover:bg-red-600 transition-colors text-sm font-semibold text-white">Выйти</button>
              </div>
            </div>
          </div>
        </Teleport>

        <!-- Вертикальная линия -->
        <div class="w-px bg-neutral-900 shrink-0 my-6" />

        <!-- Правая часть -->
        <div class="flex-1 overflow-y-auto px-8 py-8 flex flex-col gap-0">

          <div class="space-y-3">
            <div class="text-base font-bold text-neutral-700 text-center mb-5">Основные данные</div>
            <InfoRow label="Должность" :value="profile.position" />
            <InfoRow label="Дата регистрации" :value="formatDate(profile.createdAt)" />
            <template v-if="profile.description">
              <div class="flex gap-3 pt-1">
                <span class="text-xs text-neutral-600 w-40 shrink-0 pt-0.5">О себе</span>
                <span class="text-sm text-neutral-800 flex-1 whitespace-pre-wrap">{{ profile.description }}</span>
              </div>
            </template>
          </div>

          <div class="border-t border-neutral-900 my-6" />

          <div class="space-y-3">
            <div class="text-base font-bold text-neutral-700 text-center mb-5">Контакт</div>
            <InfoRow label="Email" :value="userInfo?.email ?? '—'" />
          </div>

          <div class="space-y-3 mt-3">
            <button
              @click="showPasswordForm = !showPasswordForm"
              class="flex items-center gap-1.5 text-xs font-semibold text-blue-800 hover:text-blue-900 transition-colors"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 5.25a3 3 0 013 3m3 0a6 6 0 01-7.029 5.912c-.563-.097-1.159.026-1.563.43L10.5 17.25H8.25v2.25H6v2.25H2.25v-2.818c0-.597.237-1.17.659-1.591l6.499-6.499c.404-.404.527-1 .43-1.563A6 6 0 1121.75 8.25z"/>
              </svg>
              {{ showPasswordForm ? 'Скрыть' : 'Сменить пароль' }}
            </button>

            <div v-if="showPasswordForm" class="mt-4 space-y-3 max-w-sm">
              <div>
                <label class="block text-xs text-neutral-500 mb-1">Старый пароль</label>
                <input v-model="oldPassword" type="password" placeholder="Введите старый пароль"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50" />
              </div>
              <div>
                <label class="block text-xs text-neutral-500 mb-1">Новый пароль</label>
                <input v-model="newPassword" type="password" placeholder="Введите новый пароль"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50" />
              </div>
              <div>
                <label class="block text-xs text-neutral-500 mb-1">Повторите новый пароль</label>
                <input v-model="confirmPassword" type="password" placeholder="Повторите новый пароль"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50" />
              </div>
              <div v-if="passwordError" class="text-xs text-red-500">{{ passwordError }}</div>
              <button @click="changePassword" :disabled="savingPassword"
                class="w-full py-2 rounded-xl bg-blue-500 text-white text-sm font-semibold hover:bg-blue-600 disabled:opacity-50 transition-colors">
                {{ savingPassword ? 'Сохранение...' : 'Сменить пароль' }}
              </button>
              <div v-if="passwordSuccess" class="text-xs text-green-600 text-center">Пароль успешно изменён</div>
            </div>
          </div>

        </div>
      </template>

      <div v-else class="flex-1 flex items-center justify-center text-sm text-neutral-400">Профиль не найден</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, defineComponent, h } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/store/auth'
import { imageUrl } from '@/utils/imageUrl'

const auth = useAuthStore()
const showLogoutConfirm = ref(false)

const InfoRow = defineComponent({
  props: { label: String, value: String },
  setup(props) {
    return () => h('div', { class: 'flex items-center gap-3' }, [
      h('span', { class: 'text-xs text-neutral-600 w-40 shrink-0' }, props.label),
      h('span', { class: 'text-sm text-neutral-800 flex-1' }, props.value || '—'),
    ])
  }
})

const loading = ref(true)
const profile = ref<any>(null)
const userInfo = ref<any>(null)
const avatarUrl = ref<string>('')
const avatarInput = ref<HTMLInputElement | null>(null)

const showPasswordForm = ref(false)
const oldPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const passwordError = ref('')
const passwordSuccess = ref(false)
const savingPassword = ref(false)

function initials(fio: string): string {
  if (!fio) return '?'
  const parts = fio.trim().split(' ')
  return parts.length >= 2
    ? (parts[0][0] + parts[1][0]).toUpperCase()
    : fio.slice(0, 2).toUpperCase()
}

function formatDate(iso: string): string {
  if (!iso) return '—'
  return new Date(iso).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })
}

async function uploadAvatar(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  const form = new FormData()
  form.append('newImages', file)
  try {
    const res = await api.post('/personal/me/images', form, { headers: { 'Content-Type': 'multipart/form-data' } })
    const images = res?.data?.data?.images ?? res?.data?.data ?? []
    const last = Array.isArray(images) ? images[images.length - 1] : null
    if (last?.path) avatarUrl.value = imageUrl(last.path) ?? ''
  } catch { /* ignore */ }
}

async function load() {
  loading.value = true
  try {
    const [pm, user] = await Promise.allSettled([
      api.get('/personal/me'),
      api.get('/users/me'),
    ])
    profile.value = (pm as any).value?.data?.data ?? null
    userInfo.value = (user as any).value?.data?.data ?? null
    const images = profile.value?.images ?? []
    if (images.length > 0) avatarUrl.value = imageUrl(images) ?? ''
  } finally {
    loading.value = false
  }
}

async function changePassword() {
  passwordError.value = ''
  passwordSuccess.value = false
  if (!oldPassword.value) { passwordError.value = 'Введите старый пароль'; return }
  if (!newPassword.value) { passwordError.value = 'Введите новый пароль'; return }
  if (newPassword.value !== confirmPassword.value) { passwordError.value = 'Пароли не совпадают'; return }
  if (newPassword.value.length < 6) { passwordError.value = 'Минимум 6 символов'; return }
  savingPassword.value = true
  try {
    await api.put('/users/update/me', {
      login: userInfo.value?.login,
      email: userInfo.value?.email,
      password: newPassword.value,
    })
    passwordSuccess.value = true
    oldPassword.value = ''
    newPassword.value = ''
    confirmPassword.value = ''
    setTimeout(() => { passwordSuccess.value = false; showPasswordForm.value = false }, 2000)
  } catch {
    passwordError.value = 'Ошибка при смене пароля. Проверьте старый пароль.'
  } finally {
    savingPassword.value = false
  }
}

onMounted(load)
</script>

