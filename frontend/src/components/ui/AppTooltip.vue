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
const style = reactive({ top: '0px', left: '0px' })

function show(e: MouseEvent) {
  const rect = (e.currentTarget as HTMLElement).getBoundingClientRect()
  const bubbleW = 300
  const bubbleH = 200 // приблизительно

  let left = rect.right + 8
  let top = rect.top

  // не выходить за правый край
  if (left + bubbleW > window.innerWidth - 8) {
    left = rect.left - bubbleW - 8
  }
  // не выходить за нижний край
  if (top + bubbleH > window.innerHeight - 8) {
    top = window.innerHeight - bubbleH - 8
  }

  style.left = `${left}px`
  style.top = `${top}px`
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

/* Тёмная тема */
:global(html.dark) .q-icon {
  background: #1E3A5F;
  color: #5B7BB0;
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
  width: 300px;
  white-space: pre-line;
  text-align: left;
  line-height: 1.4;
}

/* Тёмная тема — облачко подсказки */
html.dark .bubble {
  background: #0F1A2E;
  border-color: #1F2D4D;
  color: #C7D0E8;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.7);
}
</style>
