<template>
  <AthleteDashboard mode="coach-view" :athlete-id="athleteId" />
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import AthleteDashboard from '@/components/athlete-dashboard/AthleteDashboard.vue'

const route = useRoute()
const athleteId = Number(route.params.id)
</script>
