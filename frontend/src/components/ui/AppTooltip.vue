<template>
  <span class="tip" @mouseenter="show" @mouseleave="hide">
    <span class="q-icon" :class="{ active: visible }">?</span>
    <Teleport to="body">
      <span v-if="visible" class="bubble" :style="style">{{ text }}</span>
    </Teleport>
  </span>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'

defineProps<{ text: string }>()

const visible = ref(false)
const style = reactive({ top: '0px', left: '0px', maxWidth: '300px' })

function show(e: MouseEvent) {
  const rect = (e.currentTarget as HTMLElement).getBoundingClientRect()
  // На узких экранах сужаем bubble чтобы влез
  const maxW = Math.min(300, window.innerWidth - 24)
  const bubbleW = maxW
  const bubbleH = 200 // приблизительно

  // Пытаемся справа от иконки
  let left = rect.right + 8
  // Не влез справа — пробуем слева
  if (left + bubbleW > window.innerWidth - 8) {
    left = rect.left - bubbleW - 8
  }
  // Если и слева не влезло (узкий экран) — прижимаем к правому краю с отступом
  if (left < 8) {
    left = Math.max(8, window.innerWidth - bubbleW - 8)
  }

  let top = rect.top
  // Не выходить за нижний край
  if (top + bubbleH > window.innerHeight - 8) {
    top = window.innerHeight - bubbleH - 8
  }
  // И за верхний (если bubbleH больше высоты экрана)
  if (top < 8) top = 8

  style.left = `${left}px`
  style.top = `${top}px`
  style.maxWidth = `${maxW}px`
  visible.value = true
}

function hide() {
  visible.value = false
}
</script>

<style scoped>
.tip {
  position: relative;
  display: inline-flex;
  align-items: center;
}
.q-icon {
  width: 14px; height: 14px;
  border-radius: 50%;
  background: #e2e8f0;
  color: #94a3b8;
  font-size: 9px;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: default;
  flex-shrink: 0;
}
.q-icon.active { background: #3b82f6; color: #fff; }

/* Тёмная тема — приглушённый знак вопроса, чтобы не отвлекал от контента */
:global(html.dark) .q-icon {
  background: #0B1426;
  color: #2A3A5C;
}
:global(html.dark) .q-icon.active {
  background: #3B82F6;
  color: #fff;
}
</style>

<style>
.bubble {
  position: fixed;
  background: #f1f5f9;
  border: 1px solid #e2e8f0;
  color: #475569;
  font-size: 11px;
  font-weight: 400;
  padding: 8px 12px;
  border-radius: 8px;
  box-shadow: 0 4px 16px rgba(0,0,0,0.12);
  z-index: 9999;
  pointer-events: none;
  white-space: pre-line;
  text-align: left;
  line-height: 1.4;
  /* width задаётся через maxWidth из JS — учитывает узкие экраны */
}

/* Тёмная тема — облачко подсказки */
html.dark .bubble {
  background: #0F1A2E;
  border-color: #1F2D4D;
  color: #C7D0E8;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.7);
}
</style>
