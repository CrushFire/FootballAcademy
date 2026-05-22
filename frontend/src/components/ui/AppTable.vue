<template>
  <div class="overflow-x-auto rounded-xl border border-gray-200">
    <table class="w-full text-sm">
      <thead class="bg-gray-50 border-b border-gray-200">
        <tr>
          <th
            v-for="col in columns"
            :key="col.key"
            class="px-4 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wide"
          >
            {{ col.label }}
          </th>
        </tr>
      </thead>
      <tbody class="divide-y divide-gray-100 bg-white">
        <tr v-if="loading">
          <td :colspan="columns.length" class="px-4 py-8 text-center text-gray-400">Загрузка...</td>
        </tr>
        <tr v-else-if="!rows.length">
          <td :colspan="columns.length" class="px-4 py-8 text-center text-gray-400">Нет данных</td>
        </tr>
        <tr
          v-else
          v-for="(row, i) in rows"
          :key="i"
          class="hover:bg-gray-50 transition-colors cursor-pointer"
          @click="$emit('rowClick', row)"
        >
          <td v-for="col in columns" :key="col.key" class="px-4 py-3 text-gray-700">
            <slot :name="col.key" :row="row">{{ row[col.key] }}</slot>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  columns: { key: string; label: string }[]
  rows: Record<string, unknown>[]
  loading?: boolean
}>()
defineEmits<{ rowClick: [row: Record<string, unknown>] }>()
</script>
