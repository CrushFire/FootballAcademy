import { ref, watch } from 'vue'

const STORAGE_KEY = 'app-theme'
type Theme = 'light' | 'dark'

const saved = (typeof localStorage !== 'undefined' && localStorage.getItem(STORAGE_KEY)) as Theme | null
const theme = ref<Theme>(saved === 'dark' ? 'dark' : 'light')

function apply(t: Theme) {
  const root = document.documentElement
  if (t === 'dark') root.classList.add('dark')
  else root.classList.remove('dark')
}

if (typeof document !== 'undefined') apply(theme.value)

watch(theme, (t) => {
  apply(t)
  try { localStorage.setItem(STORAGE_KEY, t) } catch {}
})

export function useTheme() {
  function toggle() {
    theme.value = theme.value === 'dark' ? 'light' : 'dark'
  }
  function set(t: Theme) {
    theme.value = t
  }
  return { theme, toggle, set }
}
