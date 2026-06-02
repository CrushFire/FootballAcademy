<template>
  <div>
    <div class="field-container">
      <!-- Разметка поля -->
      <!-- Горизонтальная половина: ворота слева, атака вправо.
           viewBox 100×75 (соотношение 4:3). Если mirror=true — ворота справа, атака влево
           (для второй нашей команды в матче "обе наши"). -->
      <svg class="field-lines" viewBox="0 0 100 75" preserveAspectRatio="xMidYMid slice">
        <!-- Полосы газона -->
        <rect x="0"  y="0" width="10" height="75" fill="#1a4fa3" opacity="0.6"/>
        <rect x="10" y="0" width="10" height="75" fill="#1b57b8" opacity="0.6"/>
        <rect x="20" y="0" width="10" height="75" fill="#1a4fa3" opacity="0.6"/>
        <rect x="30" y="0" width="10" height="75" fill="#1b57b8" opacity="0.6"/>
        <rect x="40" y="0" width="10" height="75" fill="#1a4fa3" opacity="0.6"/>
        <rect x="50" y="0" width="10" height="75" fill="#1b57b8" opacity="0.6"/>
        <rect x="60" y="0" width="10" height="75" fill="#1a4fa3" opacity="0.6"/>
        <rect x="70" y="0" width="10" height="75" fill="#1b57b8" opacity="0.6"/>
        <rect x="80" y="0" width="10" height="75" fill="#1a4fa3" opacity="0.6"/>
        <rect x="90" y="0" width="10" height="75" fill="#1b57b8" opacity="0.6"/>

        <g :transform="mirror ? 'translate(100,0) scale(-1,1)' : ''">
          <!-- Внешняя рамка -->
          <rect x="3" y="3" width="94" height="69" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>

          <!-- Центральная линия (справа — граница с чужой половиной) -->
          <line x1="97" y1="3" x2="97" y2="72" stroke="rgba(255,255,255,0.6)" stroke-width="0.7"/>

          <!-- Полукруг от центра поля (выходит влево от центральной линии) -->
          <path d="M 97,20 A 16,16 0 0,0 97,55" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>
          <circle cx="97" cy="37.5" r="0.8" fill="rgba(255,255,255,0.7)"/>

          <!-- Штрафная (слева — наши ворота) -->
          <rect x="3" y="17" width="20" height="40" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>
          <!-- Вратарская -->
          <rect x="3" y="27" width="8" height="20" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
          <!-- Ворота -->
          <rect x="0.5" y="32" width="2.5" height="11" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
          <!-- Точка пенальти -->
          <circle cx="17" cy="37.5" r="0.7" fill="rgba(255,255,255,0.7)"/>
          <!-- Дуга штрафной -->
          <path d="M 23,30 A 6,6 0 0,1 23,45" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>

          <!-- Угловые дуги -->
          <path d="M3,6 A3,3 0 0,0 6,3" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
          <path d="M3,69 A3,3 0 0,1 6,72" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
        </g>
      </svg>

      <!-- Игроки. Если mirror=true — координата x зеркалится (100 - x). -->
      <div class="players-layer">
        <div
          v-for="(pos, idx) in positions"
          :key="idx"
          class="player-wrapper"
          :style="{ left: (mirror ? 100 - pos.x : pos.x) + '%', top: pos.y + '%' }"
        >
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
        </div>
      </div>
    </div>
    <div class="text-xs text-neutral-500 mt-2">
      <span class="text-[#10b981] font-semibold">Зелёная рамка</span> — есть замена
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
    substitute: { number: number; name: string; position?: string; type?: string } | null
  }[]
  // Если true — поле и игроки отзеркалены: ворота справа, атака влево.
  // Используется для второй команды в матче "обе наши".
  mirror?: boolean
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
  aspect-ratio: 4 / 3;
  border-radius: 16px;
  overflow: visible;
  background: linear-gradient(135deg, #1a4fa3 0%, #1b57b8 50%, #1d4ed8 100%);
  box-shadow: 0 8px 32px rgba(29, 78, 216, 0.3), inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.field-lines {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
  border-radius: 16px;
  overflow: hidden;
}

.players-layer {
  position: absolute;
  inset: 0;
}

.player-wrapper {
  position: absolute;
  transform: translate(-50%, -50%);
}

.player-main {
  position: relative;
}

.player-circle {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: linear-gradient(145deg, #ffffff, #f0f4ff);
  border: 2px solid #3b82f6;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  color: #1d4ed8;
  box-shadow: 0 4px 16px rgba(59, 130, 246, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.8);
  transition: transform 0.2s, box-shadow 0.2s;
}

.player-circle.is-late {
  border-color: #93c5fd;
  color: #1d4ed8;
  background: linear-gradient(145deg, #eff6ff, #dbeafe);
  box-shadow: 0 4px 16px rgba(147, 197, 253, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.8);
}

.player-circle.is-reserve {
  border-color: #10b981;
  color: #047857;
  background: linear-gradient(145deg, #d1fae5, #a7f3d0);
  box-shadow: 0 4px 16px rgba(16, 185, 129, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.8);
}

.player-circle.has-substitute {
  cursor: pointer;
  border-color: #10b981;
  color: #047857;
  box-shadow: 0 4px 16px rgba(16, 185, 129, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.8);
}

.player-circle.has-substitute:hover {
  transform: scale(1.15);
  box-shadow: 0 6px 24px rgba(16, 185, 129, 0.5), inset 0 1px 0 rgba(255, 255, 255, 0.9);
}

.player-tooltip {
  position: absolute;
  bottom: calc(100% + 6px);
  left: 50%;
  transform: translateX(-50%);
  padding: 5px 9px;
  background: rgba(15, 23, 42, 0.95);
  backdrop-filter: blur(8px);
  color: white;
  font-size: 11px;
  font-weight: 600;
  border-radius: 8px;
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.2s;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  z-index: 10;
}

.player-wrapper:hover .player-tooltip {
  opacity: 1;
}

.player-position {
  position: absolute;
  top: calc(100% + 4px);
  left: 50%;
  transform: translateX(-50%);
  font-size: 10px;
  font-weight: 700;
  color: #6b7280;
  background: rgba(255, 255, 255, 0.9);
  padding: 2px 6px;
  border-radius: 4px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border: 1px solid #d1d5db;
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.2s;
  z-index: 10;
}

.player-wrapper:hover .player-position {
  opacity: 1;
}

.player-substitute {
  position: absolute;
  top: calc(100% + 4px);
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  z-index: 20;
}

.substitute-arrow {
  font-size: 10px;
  color: #10b981;
  font-weight: 700;
  line-height: 1;
}

.substitute-circle {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: linear-gradient(145deg, #d1fae5, #a7f3d0);
  border: 2px solid #10b981;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  color: #047857;
  box-shadow: 0 3px 12px rgba(16, 185, 129, 0.5);
  cursor: pointer;
  transition: transform 0.2s;
}

.substitute-circle:hover {
  transform: scale(1.1);
}

.substitute-name {
  padding: 3px 7px;
  background: rgba(15, 23, 42, 0.9);
  color: #6ee7b7;
  font-size: 10px;
  font-weight: 600;
  border-radius: 6px;
  white-space: nowrap;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.sub-fade-enter-active,
.sub-fade-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}
.sub-fade-enter-from,
.sub-fade-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(-6px);
}
.sub-fade-enter-to,
.sub-fade-leave-from {
  opacity: 1;
  transform: translateX(-50%) translateY(0);
}
</style>
