<template>
  <!-- Вертикальный вариант FootballField. Логика и API один в один:
       принимает positions[] с (x, y), 0..100 на каждой оси.
       Отличие — поле перевёрнуто: длина по Y (ворота сверху и снизу),
       центральная линия горизонтальная.
       Координаты игроков в `positions` ОЖИДАЮТСЯ те же что и в горизонтальном —
       координата x → поперёк поля, y → от ворот к воротам.
       Подходит для мобильного отображения завершённого матча. -->
  <div>
    <div class="field-container">
      <svg class="field-lines" viewBox="0 0 100 100" preserveAspectRatio="xMidYMid slice">
        <!-- Горизонтальные полосы газона (поперёк короткой стороны) -->
        <rect x="0" y="0"  width="100" height="10" fill="#1a4fa3" opacity="0.6"/>
        <rect x="0" y="10" width="100" height="10" fill="#1b57b8" opacity="0.6"/>
        <rect x="0" y="20" width="100" height="10" fill="#1a4fa3" opacity="0.6"/>
        <rect x="0" y="30" width="100" height="10" fill="#1b57b8" opacity="0.6"/>
        <rect x="0" y="40" width="100" height="10" fill="#1a4fa3" opacity="0.6"/>
        <rect x="0" y="50" width="100" height="10" fill="#1b57b8" opacity="0.6"/>
        <rect x="0" y="60" width="100" height="10" fill="#1a4fa3" opacity="0.6"/>
        <rect x="0" y="70" width="100" height="10" fill="#1b57b8" opacity="0.6"/>
        <rect x="0" y="80" width="100" height="10" fill="#1a4fa3" opacity="0.6"/>
        <rect x="0" y="90" width="100" height="10" fill="#1b57b8" opacity="0.6"/>

        <!-- Внешняя рамка -->
        <rect x="3" y="3" width="94" height="94" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>

        <!-- Центральная линия (горизонтальная) -->
        <line x1="3" y1="50" x2="97" y2="50" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>

        <!-- Центральный круг -->
        <circle cx="50" cy="50" r="12" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>
        <circle cx="50" cy="50" r="0.8" fill="rgba(255,255,255,0.7)"/>

        <!-- Штрафная верхняя -->
        <rect x="31" y="3" width="38" height="16" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>
        <!-- Вратарская верхняя -->
        <rect x="40" y="3" width="20" height="6" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
        <!-- Ворота верхние -->
        <rect x="43" y="0.5" width="14" height="2.5" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
        <!-- Точка пенальти верхняя -->
        <circle cx="50" cy="14" r="0.7" fill="rgba(255,255,255,0.7)"/>

        <!-- Штрафная нижняя -->
        <rect x="31" y="81" width="38" height="16" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>
        <!-- Вратарская нижняя -->
        <rect x="40" y="91" width="20" height="6" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
        <!-- Ворота нижние -->
        <rect x="43" y="97" width="14" height="2.5" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
        <!-- Точка пенальти нижняя -->
        <circle cx="50" cy="86" r="0.7" fill="rgba(255,255,255,0.7)"/>

        <!-- Угловые дуги -->
        <path d="M3,6 A3,3 0 0,1 6,3" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
        <path d="M94,3 A3,3 0 0,1 97,6" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
        <path d="M3,94 A3,3 0 0,0 6,97" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
        <path d="M97,94 A3,3 0 0,0 94,97" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
      </svg>

      <!-- Игроки и пустые слоты -->
      <div class="players-layer">
        <div
          v-for="(pos, idx) in positions"
          :key="idx"
          class="player-wrapper"
          :style="{ left: pos.x + '%', top: pos.y + '%' }"
        >
          <!-- Пустой слот: пунктирный кружок с подписью позиции -->
          <template v-if="pos.empty">
            <div class="player-main">
              <div class="player-circle player-empty">
                <span class="empty-mark">{{ pos.position || '?' }}</span>
              </div>
              <div class="empty-tooltip">
                <div class="empty-tooltip-title">Слот не занят</div>
                <div v-if="pos.preferred && pos.preferred.length" class="empty-tooltip-sub">
                  Подходящие позиции:
                </div>
                <div v-if="pos.preferred && pos.preferred.length" class="empty-tooltip-list">
                  <span
                    v-for="(code, i) in pos.preferred" :key="code"
                    class="empty-tooltip-chip"
                    :class="i === 0 ? 'empty-tooltip-chip-primary' : ''"
                  >{{ code }} <span class="empty-tooltip-chip-label">({{ positionLabel[code] ?? code }})</span></span>
                </div>
              </div>
            </div>
          </template>
          <!-- Обычный игрок -->
          <template v-else>
            <div class="player-main">
              <div
                class="player-circle"
                :class="{ 'has-substitute': pos.substitute, 'is-late': pos.late, 'is-reserve': pos.type === 'Reserve' }"
                @click="pos.substitute && toggle(idx)"
              >
                {{ initials(pos.name) }}
              </div>
              <div class="player-position">{{ pos.position || 'ST' }} ({{ positionLabel[pos.position || 'ST'] }})</div>
              <div class="player-tooltip">{{ pos.name }}</div>
            </div>

            <Transition name="sub-fade">
              <div v-if="pos.substitute && activeIdx === idx" class="player-substitute">
                <div class="substitute-arrow">↓</div>
                <div class="substitute-circle" @click="toggle(idx)">
                  {{ initials(pos.substitute.name) }}
                </div>
                <div class="substitute-name">{{ pos.substitute.name }}</div>
              </div>
            </Transition>
          </template>
        </div>
      </div>
    </div>
    <div class="text-xs text-neutral-500 mt-2 flex items-center justify-between gap-3">
      <div>
        <span class="text-[#10b981] font-semibold">Зелёная рамка</span> — есть замена
      </div>
      <slot name="footer-right" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { POSITION_LABEL } from '@/constants'

defineProps<{
  positions: {
    x: number
    y: number
    number: number
    name: string
    position?: string
    type?: string
    late?: boolean
    empty?: boolean
    preferred?: string[]
    substitute: { number: number; name: string; position?: string; type?: string } | null
  }[]
}>()

const positionLabel = POSITION_LABEL

const activeIdx = ref<number | null>(null)

function toggle(idx: number) {
  activeIdx.value = activeIdx.value === idx ? null : idx
}

function initials(name: string) {
  const parts = name.trim().split(' ')
  return ((parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')).toUpperCase()
}
</script>

<style scoped>
.field-container {
  position: relative;
  aspect-ratio: 3 / 4;
  border-radius: 16px;
  overflow: visible;
  background: #1d4ed8;
  box-shadow: 0 4px 12px rgba(29, 78, 216, 0.25);
}
.field-lines {
  position: absolute;
  inset: 0;
  width: 100%; height: 100%;
  pointer-events: none;
  border-radius: 16px;
  overflow: hidden;
}
.players-layer { position: absolute; inset: 0; }
.player-wrapper { position: absolute; transform: translate(-50%, -50%); }
.player-main { position: relative; }

.player-circle {
  width: 36px; height: 36px;
  border-radius: 50%;
  background: #fff;
  border: 2px solid #3b82f6;
  display: flex; align-items: center; justify-content: center;
  font-size: 11px; font-weight: 700; color: #1d4ed8;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.25);
  transition: transform 0.15s;
}
.player-circle.is-late {
  border-color: #93c5fd;
  background: #eff6ff;
}
.player-circle.is-reserve {
  border-color: #10b981;
  color: #047857;
  background: #d1fae5;
}
.player-circle.has-substitute {
  cursor: pointer;
  border-color: #10b981;
  color: #047857;
}
.player-circle.has-substitute:hover { transform: scale(1.12); }

/* Пустой слот — пунктирный кружок с белой жирной подписью */
.player-circle.player-empty,
:global(.dark) .player-circle.player-empty {
  background: transparent !important;
  border: 2px dashed rgba(255, 255, 255, 0.55) !important;
  color: #ffffff !important;
  font-size: 10px;
  font-weight: 900;
  letter-spacing: 0.3px;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.6);
  box-shadow: none;
}
.player-circle.player-empty .empty-mark { pointer-events: none; color: #ffffff; }

/* Тултип имени игрока — сверху */
.player-tooltip {
  position: absolute;
  bottom: calc(100% + 6px);
  left: 50%;
  transform: translateX(-50%);
  padding: 5px 9px;
  background: rgba(15, 23, 42, 0.95);
  color: white;
  font-size: 11px; font-weight: 600;
  border-radius: 8px;
  white-space: nowrap;
  opacity: 0; pointer-events: none;
  transition: opacity 0.15s;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  z-index: 10;
}
.player-wrapper:hover .player-tooltip { opacity: 1; }

/* Лейбл позиции под кружком (на hover) */
.player-position {
  position: absolute;
  top: calc(100% + 4px);
  left: 50%;
  transform: translateX(-50%);
  font-size: 10px; font-weight: 700;
  color: #6b7280;
  background: rgba(255, 255, 255, 0.9);
  padding: 2px 6px;
  border-radius: 4px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border: 1px solid #d1d5db;
  white-space: nowrap;
  opacity: 0; pointer-events: none;
  transition: opacity 0.2s;
  z-index: 10;
}
.player-wrapper:hover .player-position { opacity: 1; }

/* Подсказка пустого слота (snizu, светлая в любой теме) */
.empty-tooltip {
  position: absolute;
  top: calc(100% + 6px);
  left: 50%;
  transform: translateX(-50%);
  min-width: 200px; max-width: 260px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 8px 10px;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.18);
  opacity: 0; pointer-events: none;
  transition: opacity 0.15s;
  z-index: 25;
}
.player-wrapper:hover .empty-tooltip { opacity: 1; }
.empty-tooltip-title {
  font-size: 11px; font-weight: 700; color: #0f172a;
  margin-bottom: 4px;
}
.empty-tooltip-sub {
  font-size: 9px; font-weight: 700; text-transform: uppercase;
  letter-spacing: 0.4px; color: #94a3b8; margin-bottom: 4px;
}
.empty-tooltip-list { display: flex; flex-wrap: wrap; gap: 3px; }
.empty-tooltip-chip {
  display: inline-flex; align-items: center; gap: 3px;
  padding: 2px 6px;
  border-radius: 6px;
  background: #f1f5f9; color: #475569;
  font-size: 10px; font-weight: 500;
  border: 1px solid #e2e8f0;
}
.empty-tooltip-chip-primary {
  background: #dbeafe; color: #1e40af; border-color: #93c5fd;
}
.empty-tooltip-chip-label {
  font-weight: 500; font-size: 9px;
  color: inherit; opacity: 0.85;
}

/* Замена */
.player-substitute {
  position: absolute; top: calc(100% + 4px); left: 50%;
  transform: translateX(-50%);
  display: flex; flex-direction: column; align-items: center; gap: 2px;
  z-index: 20;
}
.substitute-arrow { font-size: 10px; color: #10b981; font-weight: 700; line-height: 1; }
.substitute-circle {
  width: 30px; height: 30px;
  border-radius: 50%;
  background: #d1fae5;
  border: 2px solid #10b981;
  display: flex; align-items: center; justify-content: center;
  font-size: 11px; font-weight: 700; color: #047857;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  cursor: pointer;
  transition: transform 0.15s;
}
.substitute-circle:hover { transform: scale(1.1); }
.substitute-name {
  padding: 3px 7px;
  background: rgba(15, 23, 42, 0.9);
  color: #6ee7b7;
  font-size: 10px; font-weight: 600;
  border-radius: 6px;
  white-space: nowrap;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.sub-fade-enter-active, .sub-fade-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}
.sub-fade-enter-from, .sub-fade-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(-6px);
}
.sub-fade-enter-to, .sub-fade-leave-from {
  opacity: 1;
  transform: translateX(-50%) translateY(0);
}
</style>
