<template>
  <div class="login-root">
    <canvas ref="canvasRef" class="bg-canvas" />
    <div class="frame" />

    <div class="card">
      <div class="logo-wrap">
        <div class="logo-box">
          <img :src="logoUrl" alt="Академия футбола" />
        </div>
        <h1>FootballAcademy</h1>
        <span class="tag">⚽ Твой путь к большому спорту!</span>
      </div>

      <form @submit.prevent="handleLogin">
        <div class="field-group">
          <div class="mode-switch">
            <button type="button"
              class="mode-btn"
              :class="{ active: loginMode === 'email' }"
              @click="loginMode = 'email'"
            >Email</button>
            <button type="button"
              class="mode-btn"
              :class="{ active: loginMode === 'login' }"
              @click="loginMode = 'login'"
            >Логин</button>
          </div>
          <label>{{ loginMode === 'email' ? 'Электронная почта' : 'Логин' }}</label>
          <input
            v-model="identifier"
            :type="loginMode === 'email' ? 'email' : 'text'"
            :placeholder="loginMode === 'email' ? 'example@sport.ru' : 'ivanov'"
            :autocomplete="loginMode === 'email' ? 'email' : 'username'"
            required
          />
        </div>
        <div class="field-group">
          <label>Пароль</label>
          <div class="input-wrap">
            <span class="input-icon" @click="togglePass" v-html="showPass ? EYE_OFF : EYE_OPEN" />
            <input v-model="password" :type="showPass ? 'text' : 'password'" placeholder="••••••••" required />
          </div>
        </div>

        <div v-if="error" class="error-msg">{{ error }}</div>

        <div class="divider">войти в систему</div>
        <button class="btn" type="submit" :disabled="loading">
          {{ loading ? 'Входим...' : 'Войти' }}
        </button>
      </form>

      <div class="footer-tags">
        <span>
          <svg class="ico-goal" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
            <circle cx="12" cy="12" r="10"/>
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 2v20M2 12h20M5.6 5.6l12.8 12.8M5.6 18.4L18.4 5.6"/>
          </svg>
          Забивай голы
        </span>
        <span>
          <svg class="ico-growth" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3 17l6-6 4 4 8-8"/>
            <path stroke-linecap="round" stroke-linejoin="round" d="M14 7h7v7"/>
          </svg>
          Расти с нами
        </span>
        <span>
          <svg class="ico-star" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
            <path stroke-linecap="round" stroke-linejoin="round" d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.196-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118L2.05 10.1c-.783-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.673z"/>
          </svg>
          Стань лучшим
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useAuth } from '@/composables/useAuth'
import logoUrl from '@/assets/images/Академия_футбола.png'

const { login, loading, error } = useAuth()
const identifier = ref('')
const password = ref('')
const showPass = ref(false)
const loginMode = ref<'login' | 'email'>('email')
const canvasRef = ref<HTMLCanvasElement | null>(null)

function togglePass() { showPass.value = !showPass.value }
async function handleLogin() {
  // Email регистронезависимый (toLowerCase), логин — как ввели.
  // Режим передаётся на бэк, чтобы тот искал строго по выбранному полю
  // и возвращал понятную ошибку ("логин не найден" / "email не найден").
  const raw = identifier.value.trim()
  const value = loginMode.value === 'email' ? raw.toLowerCase() : raw
  await login(value, password.value, loginMode.value)
}

const EYE_OPEN = `<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.477 0 8.268 2.943 9.542 7-1.274 4.057-5.065 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>`
const EYE_OFF  = `<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.477 0-8.268-2.943-9.542-7a9.97 9.97 0 012.163-3.592m3.06-2.633A9.956 9.956 0 0112 5c4.477 0 8.268 2.943 9.542 7a9.97 9.97 0 01-1.308 2.558M15 12a3 3 0 11-4.243-4.243M3 3l18 18"/></svg>`

// На странице авторизации всегда светлая тема — снимаем .dark пока тут
let darkWasOn = false

let W = 0, H = 0, t = 0, rafId = 0
let ctx: CanvasRenderingContext2D | null = null

const spots = [
  { ox: 0.05, oy: 0.05, speed: 0.4,  phase: 0,             r: 0.22 },
  { ox: 0.95, oy: 0.08, speed: 0.28, phase: Math.PI * 0.7, r: 0.26 },
  { ox: 0.5,  oy: 0.02, speed: 0.18, phase: Math.PI * 1.4, r: 0.20 },
]

function resize() {
  const canvas = canvasRef.value!
  W = canvas.width = window.innerWidth
  H = canvas.height = window.innerHeight
}

function drawField() {
  const c = ctx!
  const sw = W / 10
  for (let i = 0; i < 10; i++) {
    c.fillStyle = i % 2 === 0 ? '#1a4fa3' : '#1b57b8'
    c.fillRect(i * sw, 0, sw, H)
  }
  c.strokeStyle = 'rgba(255,255,255,0.55)'
  c.lineWidth = 2
  const p = Math.min(W, H) * 0.06
  const fw = W - p * 2, fh = H - p * 2
  c.strokeRect(p, p, fw, fh)
  c.beginPath(); c.moveTo(W / 2, p); c.lineTo(W / 2, p + fh); c.stroke()
  const cr = Math.min(fw, fh) * 0.12
  c.beginPath(); c.arc(W / 2, H / 2, cr, 0, Math.PI * 2); c.stroke()
  c.fillStyle = 'rgba(255,255,255,0.55)'
  c.beginPath(); c.arc(W / 2, H / 2, 4, 0, Math.PI * 2); c.fill()
  const bw = fw * 0.12, bh = fh * 0.38
  c.strokeRect(p, H / 2 - bh / 2, bw, bh)
  c.strokeRect(p + fw - bw, H / 2 - bh / 2, bw, bh)
  const gw = fw * 0.025, gh = fh * 0.16
  c.strokeRect(p - gw, H / 2 - gh / 2, gw, gh)
  c.strokeRect(p + fw, H / 2 - gh / 2, gw, gh)
  const ar = Math.min(fw, fh) * 0.03
  ;[[p, p, 0], [p + fw, p, Math.PI / 2], [p, p + fh, -Math.PI / 2], [p + fw, p + fh, Math.PI]].forEach(([cx, cy, sa]) => {
    c.beginPath(); c.arc(cx, cy, ar, sa, sa + Math.PI / 2); c.stroke()
  })
  c.beginPath(); c.arc(p + fw * 0.09, H / 2, 4, 0, Math.PI * 2); c.fill()
  c.beginPath(); c.arc(p + fw * 0.91, H / 2, 4, 0, Math.PI * 2); c.fill()
}

function drawSpotlights() {
  const c = ctx!
  for (const s of spots) {
    const tx = W * (0.2 + 0.6 * (0.5 + 0.5 * Math.sin(t * s.speed + s.phase)))
    const ty = H * (0.2 + 0.6 * (0.5 + 0.5 * Math.cos(t * s.speed * 0.7 + s.phase)))
    const ox = s.ox * W, oy = s.oy * H
    const radius = s.r * Math.min(W, H)
    const angle = Math.atan2(ty - oy, tx - ox)
    const dist = Math.sqrt((tx - ox) ** 2 + (ty - oy) ** 2)

    const coneGrad = c.createRadialGradient(ox, oy, 0, ox, oy, dist)
    coneGrad.addColorStop(0, 'rgba(255,255,220,0.12)')
    coneGrad.addColorStop(1, 'rgba(255,255,220,0.0)')
    c.save()
    c.beginPath()
    c.moveTo(ox, oy)
    c.arc(ox, oy, dist, angle - 0.18, angle + 0.18)
    c.closePath()
    c.fillStyle = coneGrad
    c.fill()
    c.restore()

    const spotGrad = c.createRadialGradient(tx, ty, 0, tx, ty, radius)
    spotGrad.addColorStop(0, 'rgba(255,255,200,0.22)')
    spotGrad.addColorStop(0.4, 'rgba(255,255,180,0.10)')
    spotGrad.addColorStop(1, 'rgba(255,255,180,0)')
    c.beginPath(); c.arc(tx, ty, radius, 0, Math.PI * 2)
    c.fillStyle = spotGrad; c.fill()
  }
}

function loop() {
  t += 0.012
  ctx!.clearRect(0, 0, W, H)
  drawField()
  drawSpotlights()
  rafId = requestAnimationFrame(loop)
}

onMounted(() => {
  darkWasOn = document.documentElement.classList.contains('dark')
  if (darkWasOn) document.documentElement.classList.remove('dark')

  ctx = canvasRef.value!.getContext('2d')!
  resize()
  window.addEventListener('resize', resize)
  loop()
})

onUnmounted(() => {
  if (darkWasOn) document.documentElement.classList.add('dark')

  cancelAnimationFrame(rafId)
  window.removeEventListener('resize', resize)
})
</script>

<style scoped>
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

.login-root {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: 'Segoe UI', system-ui, sans-serif;
  overflow: hidden;
}

.bg-canvas { position: fixed; inset: 0; z-index: 0; }

.frame {
  position: fixed; inset: 0;
  border: 20px solid #0c2461;
  z-index: 200; pointer-events: none; border-radius: 4px;
}
.frame::after {
  content: ''; position: absolute; inset: 3px;
  border: 1.5px solid rgba(99,157,255,0.4);
  border-radius: 2px; pointer-events: none;
}

.card {
  position: relative; z-index: 10;
  width: 100%; max-width: 510px; margin: 24px;
  background: rgba(255,255,255,0.93);
  backdrop-filter: blur(20px); -webkit-backdrop-filter: blur(20px);
  border-radius: 28px; border: 1px solid rgba(255,255,255,0.8);
  padding: 48px 48px 38px;
  box-shadow: 0 32px 80px rgba(10,36,99,0.28), 0 8px 24px rgba(10,36,99,0.14), 0 0 0 1px rgba(59,130,246,0.1);
}

.logo-wrap { display: flex; flex-direction: column; align-items: center; margin-bottom: 32px; gap: 12px; }
.logo-box {
  width: 100px; height: 100px; border-radius: 26px;
  background: linear-gradient(145deg, #e0eeff, #c7d9f8);
  display: flex; align-items: center; justify-content: center;
  border: 1px solid rgba(147,197,253,0.6);
  box-shadow: 0 8px 24px rgba(37,99,235,0.18), inset 0 1px 0 rgba(255,255,255,0.8);
  overflow: hidden;
}
.logo-box img { width: 100%; height: 100%; object-fit: contain; padding: 8px; }
h1 { font-size: 26px; font-weight: 800; color: #1e3a8a; letter-spacing: -0.5px; }
.tag {
  font-size: 11px; font-weight: 600; color: #3b82f6;
  background: #eff6ff; border: 1px solid #bfdbfe;
  padding: 3px 12px; border-radius: 20px; letter-spacing: 0.2px;
}

.field-group { margin-bottom: 18px; }

.mode-switch {
  display: flex;
  gap: 6px;
  margin-bottom: 12px;
  padding: 4px;
  background: #f1f5f9;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
}
.mode-btn {
  flex: 1;
  padding: 8px 14px;
  border: none;
  border-radius: 9px;
  background: #94a3b8;
  font-size: 13px;
  font-weight: 600;
  color: #f8fafc;
  cursor: pointer;
  font-family: inherit;
  transition: background .15s, color .15s, box-shadow .15s;
}
.mode-btn:hover:not(.active) { background: #64748b; color: #ffffff; }
.mode-btn.active {
  background: linear-gradient(135deg, #1d4ed8 0%, #3b82f6 100%);
  color: #fff;
  box-shadow: 0 2px 8px rgba(29, 78, 216, 0.35);
}
label { display: block; font-size: 12px; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 0.6px; margin-bottom: 8px; }

.input-wrap { position: relative; }
.input-wrap input { padding-right: 42px; }
.input-icon {
  position: absolute; right: 12px; top: 50%; transform: translateY(-50%);
  cursor: pointer; user-select: none; color: #94a3b8; display: flex; align-items: center;
}
.input-icon:hover { color: #3b82f6; }
.input-icon :deep(svg) { width: 18px; height: 18px; }

input[type=email], input[type=password], input[type=text] {
  width: 100%; padding: 13px 16px; border-radius: 14px;
  border: 1.5px solid #e2e8f0; background: #f8fafc;
  font-size: 15px; color: #0f172a; outline: none;
  transition: border-color .2s, box-shadow .2s, background .2s;
  font-family: inherit;
}
input:focus { border-color: #3b82f6; background: #fff; box-shadow: 0 0 0 4px rgba(59,130,246,0.12); }
input::placeholder { color: #94a3b8; }

.error-msg {
  font-size: 12px; color: #dc2626; background: #fef2f2;
  border: 1px solid #fecaca; border-radius: 10px;
  padding: 9px 13px; margin-bottom: 14px;
}

.divider {
  display: flex; align-items: center; gap: 10px;
  margin: 18px 0 16px; font-size: 12px; font-weight: 600; color: #64748b;
}
.divider::before, .divider::after { content: ''; flex: 1; height: 1px; background: #e2e8f0; }

.btn {
  width: 100%; padding: 15px; border-radius: 16px; border: none;
  cursor: pointer; font-size: 15px; font-weight: 700; color: #fff; letter-spacing: 0.3px;
  background: linear-gradient(135deg, #1d4ed8 0%, #3b82f6 100%);
  box-shadow: 0 4px 20px rgba(29,78,216,0.4), inset 0 1px 0 rgba(255,255,255,0.15);
  transition: transform .15s, box-shadow .15s; margin-top: 4px;
}
.btn:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 8px 28px rgba(29,78,216,0.45), inset 0 1px 0 rgba(255,255,255,0.15); }
.btn:active { transform: translateY(0); }
.btn:disabled { opacity: 0.7; cursor: not-allowed; }

.footer-tags {
  display: flex; justify-content: center; gap: 16px;
  margin-top: 20px; padding-top: 16px; border-top: 1px solid #f1f5f9;
}
.footer-tags span {
  font-size: 14px;
  font-weight: 600;
  color: #475569;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}
.footer-tags svg { width: 16px; height: 16px; flex-shrink: 0; }
.footer-tags .ico-goal,
.footer-tags .ico-growth,
.footer-tags .ico-star   { color: #334155 !important; stroke: #334155 !important; }
.footer-tags svg path,
.footer-tags svg circle,
.footer-tags svg rect { stroke: inherit; }
</style>
