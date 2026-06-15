<template>
  <div class="p-3 md:p-6 h-full flex flex-col gap-4 min-h-0">

    <!-- ШАГ 1: Выбор / создание матча -->
    <template v-if="step === 'select'">
      <div class="shrink-0">
        <LiveMatchHeader>
          <div class="flex-1">
            <h1 class="text-xl font-bold text-neutral-900">Live-матч</h1>
            <p class="text-sm text-neutral-500 mt-0.5">Выберите запланированный матч или создайте новый</p>
          </div>
        </LiveMatchHeader>
      </div>

      <div class="bg-white rounded-2xl border border-neutral-200 shadow-sm overflow-hidden flex-1 flex flex-col min-h-0">
        <div class="p-3 md:p-5 pb-6 flex-1 flex flex-col gap-4 overflow-y-auto">

          <!-- Запланированные матчи -->
          <div class="border border-neutral-100 rounded-xl overflow-hidden">
            <div class="px-4 py-3 border-b border-neutral-100 bg-neutral-50">
              <div class="text-xs font-bold text-neutral-500 uppercase tracking-wide text-center">Запланированные матчи</div>
            </div>
            <div v-if="loadingScheduled" class="px-5 py-8 text-center text-xs text-neutral-400">Загрузка...</div>
            <div v-else-if="!scheduledMatches.length" class="px-5 py-8 text-center text-xs text-neutral-400">Нет запланированных матчей</div>
            <div v-else class="divide-y divide-neutral-100">
              <button
                v-for="m in scheduledMatches" :key="m.id"
                @click="selectMatch(m)"
                class="w-full px-4 py-3 flex items-center gap-3 hover:bg-neutral-50 transition-colors text-left"
              >
                <div class="flex-1 min-w-0">
                  <div class="text-sm font-semibold text-neutral-900 truncate">{{ m.homeTeamName }} vs {{ m.opponentTeamName ?? 'Соперник' }}</div>
                  <div class="text-xs text-neutral-400 mt-0.5">{{ formatDate(m.date) }} · {{ typeLabel[m.type] ?? m.type }}</div>
                </div>
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-300 shrink-0">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/>
                </svg>
              </button>
            </div>
          </div>

          <!-- Создать новый матч.
               БЕЗ overflow-hidden — иначе кнопка "Продолжить → Состав" может быть обрезана
               на коротких экранах (мобила в landscape, маленький viewport). -->
          <div class="border border-neutral-100 rounded-xl">
            <button
              @click="showCreateForm = !showCreateForm"
              class="w-full px-4 py-3 flex items-center gap-3 hover:bg-neutral-50 transition-colors text-left"
            >
              <div class="w-7 h-7 rounded-lg bg-green-50 flex items-center justify-center shrink-0">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5 text-green-600">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
                </svg>
              </div>
              <span class="text-sm font-semibold text-neutral-800 flex-1">Создать новый матч</span>
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-400 transition-transform" :class="showCreateForm ? 'rotate-180' : ''">
                <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7"/>
              </svg>
            </button>
            <div v-if="showCreateForm" class="px-4 pb-4 space-y-3 border-t border-neutral-100 pt-3">
              <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div>
                  <label class="block text-xs font-semibold text-neutral-500 mb-1">Наша команда *</label>
                  <select v-model="createForm.homeTeamId" class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 focus:outline-none focus:border-blue-400">
                    <option value="">Выберите команду</option>
                    <option v-for="t in myTeams" :key="t.id" :value="t.id">{{ t.name }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-neutral-500 mb-1">Тип матча *</label>
                  <select v-model="createForm.type" @change="onTypeChange" class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 focus:outline-none focus:border-blue-400">
                    <option value="Friendly">Товарищеский</option>
                    <option value="League">Лига</option>
                    <option value="Cup">Кубок</option>
                    <option value="Tournament">Турнир</option>
                    <option value="Home">Домашний</option>
                  </select>
                </div>
                <div v-if="createForm.type === 'Home'">
                  <label class="block text-xs font-semibold text-neutral-500 mb-1">Команда соперника *</label>
                  <select v-model="createForm.opponentTeamId" class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 focus:outline-none focus:border-blue-400">
                    <option value="">Выберите команду</option>
                    <option v-for="t in availableOpponentTeams" :key="t.id" :value="t.id">{{ t.name }}</option>
                  </select>
                </div>
                <div v-else>
                  <label class="block text-xs font-semibold text-neutral-500 mb-1">Соперник</label>
                  <input v-model="createForm.opponentTeamName" type="text" placeholder="Название команды соперника"
                    class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 focus:outline-none focus:border-blue-400" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-neutral-500 mb-1">Дата и время *</label>
                  <input v-model="createForm.date" type="datetime-local"
                    class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 focus:outline-none focus:border-blue-400" />
                </div>
              </div>
              <button
                @click="goToLineup"
                :disabled="!canCreateMatch"
                class="w-full py-2.5 rounded-xl bg-green-600 text-white text-sm font-semibold hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >Продолжить → Состав</button>
            </div>
          </div>

        </div>
      </div>
    </template>

    <!-- ШАГ 1.5: Утверждение состава -->
    <template v-if="step === 'lineup'">
      <LiveMatchHeader>
        <div class="flex-1 min-w-0">
          <div class="text-base font-bold text-neutral-900">Утверждение состава</div>
          <div class="text-xs text-neutral-500 mt-0.5">
            {{ myTeams.find(t => t.id === Number(createForm.homeTeamId))?.name ?? '' }}
            <template v-if="createForm.type === 'Home' && createForm.opponentTeamId">
              vs {{ myTeams.find(t => t.id === Number(createForm.opponentTeamId))?.name ?? '' }}
            </template>
            <template v-else-if="createForm.opponentTeamName"> vs {{ createForm.opponentTeamName }}</template>
          </div>
        </div>
        <button @click="step = 'select'" class="text-xs text-neutral-400 hover:text-neutral-600 shrink-0">← Назад</button>
      </LiveMatchHeader>

      <div class="bg-white rounded-2xl border border-neutral-200 shadow-sm overflow-hidden flex-1 flex flex-col">
        <div class="p-5 flex-1 flex flex-col gap-4 overflow-y-auto">

          <!-- Жёлтый баннер -->
          <div class="flex items-start gap-3 px-4 py-3 rounded-xl border border-yellow-300 bg-yellow-50">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-yellow-500 shrink-0 mt-0.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"/>
            </svg>
            <div>
              <div class="text-sm font-bold text-yellow-800">Утвердите состав перед началом матча</div>
              <div class="text-xs text-yellow-700 mt-0.5">Проверьте игроков и нажмите «Начать матч». После старта состав нельзя изменить.</div>
            </div>
          </div>

          <!-- Переключатель команд для Home-матча -->
          <div v-if="createForm.type === 'Home'" class="flex gap-2">
            <button
              @click="editTeamSide = 'home'"
              class="flex-1 py-2 rounded-xl border text-xs font-semibold transition-colors"
              :class="editTeamSide === 'home' ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-500'">
              {{ myTeams.find(t => t.id === Number(createForm.homeTeamId))?.name ?? 'Наша команда' }}
            </button>
            <button
              @click="editTeamSide = 'away'"
              class="flex-1 py-2 rounded-xl border text-xs font-semibold transition-colors"
              :class="editTeamSide === 'away' ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-500'">
              {{ myTeams.find(t => t.id === Number(createForm.opponentTeamId))?.name ?? 'Соперник' }}
            </button>
          </div>

          <!-- Список игроков состава — только основа (Main). Запасные подбираются через бейдж "Замены".
               БЕЗ внутреннего max-h/overflow — иначе кнопка "Начать матч" уходит под обрез на коротких экранах
               (мобила), а внутренний скролл крутит только игроков. Скролл весь блок один — внешний overflow-y-auto на p-5. -->
          <div class="space-y-2">
            <div v-for="{ entry, idx } in lineupMainEntries" :key="idx"
              class="rounded-xl border border-neutral-100 bg-neutral-50">
              <div class="flex items-center gap-2 p-2">
                <!-- ФИО игрока -->
                <div class="flex-1 min-w-0 text-xs font-semibold text-neutral-700 px-1 truncate">
                  {{ sportsmenMap[entry.sportsmanId] ?? (entry.sportsmanId ? `#${entry.sportsmanId}` : '—') }}
                </div>
                <!-- Бейдж замены — слева от позиции, всегда показывает счётчик -->
                <button v-if="getPossibleReplacements(entry.position).length > 0"
                  type="button"
                  @click="toggleSubstitutes(idx)"
                  class="shrink-0 px-2 py-1 rounded-lg border border-green-400 bg-green-50 text-green-700 text-[10px] font-bold hover:bg-green-100 transition-colors">
                  Замены: {{ getPossibleReplacements(entry.position).length }}
                </button>
                <span v-else class="shrink-0 px-2 py-1 rounded-lg border border-neutral-200 bg-neutral-100 text-neutral-400 text-[10px] font-semibold">
                  Нет замен
                </span>
                <!-- Позиция -->
                <div class="shrink-0">
                  <div class="text-xs font-bold text-neutral-600 px-2 py-1 rounded border border-neutral-300 bg-white min-w-[40px] text-center">
                    {{ entry.position }}
                  </div>
                </div>
              </div>
              <!-- Раскрытый список потенциальных замен — выбираем конкретного -->
              <div v-if="openSubstitutesFor === idx"
                class="px-3 pb-2 pt-1 border-t border-neutral-200 bg-white rounded-b-xl">
                <div class="text-[10px] text-neutral-400 mb-1.5">Выберите конкретного запасного для {{ entry.position }}:</div>
                <div class="space-y-1">
                  <button type="button"
                    @click="assignSubstitute(idx, undefined); openSubstitutesFor = null"
                    class="w-full flex items-center justify-between text-xs px-2 py-1.5 rounded transition-colors"
                    :class="!entry.substituteId ? 'bg-blue-50 text-blue-700 font-semibold' : 'hover:bg-neutral-50 text-neutral-400'">
                    <span>Не назначена</span>
                  </button>
                  <button v-for="s in getPossibleReplacements(entry.position)" :key="s.id"
                    type="button"
                    @click="assignSubstitute(idx, s.id); openSubstitutesFor = null"
                    class="w-full flex items-center justify-between text-xs px-2 py-1.5 rounded transition-colors"
                    :class="entry.substituteId === s.id ? 'bg-green-50 text-green-700 font-semibold' : 'hover:bg-neutral-50 text-neutral-700'">
                    <span class="truncate">{{ s.fio }}</span>
                    <span class="text-[10px] font-bold text-neutral-500 ml-2 shrink-0">{{ s.position }}</span>
                  </button>
                </div>
              </div>
            </div>
            <div v-if="!lineupMainEntries.length" class="text-center text-xs text-neutral-400 py-4">Нет игроков в составе</div>
          </div>

          <!-- Кнопка начать -->
          <button
            @click="showStartConfirm = true"
            :disabled="startingMatch"
            class="w-full py-3 rounded-xl bg-green-600 text-white text-sm font-bold hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >{{ startingMatch ? 'Запуск матча...' : 'Начать матч' }}</button>

        </div>
      </div>
    </template>

    <!-- ШАГ 2: Live матч -->
    <template v-if="step === 'live' && match">

      <!-- Хедер -->
      <LiveMatchHeader>
        <div class="flex-1 min-w-0">
          <div class="text-base font-bold text-neutral-900 truncate">{{ match.homeTeamName }} vs {{ match.opponentTeamName ?? 'Соперник' }}</div>
          <div class="text-xs text-neutral-500 mt-0.5">{{ typeLabel[match.type] ?? match.type }} · {{ formatDate(match.date) }}</div>
        </div>
        <span class="text-xs font-semibold px-2 py-0.5 rounded-full bg-green-100 text-green-700 shrink-0 animate-pulse">● Идёт</span>
      </LiveMatchHeader>

      <!-- Глобальная карта -->
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-sm overflow-hidden flex-1 flex flex-col">
        <div class="p-3 md:p-5 space-y-5 flex-1 overflow-y-auto">

          <!-- Счёт + таймер -->
          <div class="relative flex items-center gap-2 md:gap-4 pt-1">
            <!-- Кнопка паузы — справа сверху, заблокирована во время перерыва между таймами -->
            <button
              @click="togglePause"
              :disabled="matchPhase === 'break'"
              class="absolute top-0 right-0 w-8 h-8 rounded-xl border flex items-center justify-center transition-colors"
              :class="matchPhase === 'break' ? 'border-neutral-100 bg-neutral-50 cursor-not-allowed opacity-40' : 'border-neutral-200 bg-neutral-50 hover:bg-neutral-100'"
              :title="isPaused ? 'Возобновить' : 'Пауза'"
            >
              <!-- Пауза активна — иконка песочных часов -->
              <svg v-if="isPaused" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5 text-yellow-500">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              <!-- Идёт игра — иконка пауза -->
              <svg v-else xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5 text-neutral-500">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 5.25v13.5m-7.5-13.5v13.5"/>
              </svg>
            </button>

            <!-- Эмблема домашней -->
            <div class="flex-1 min-w-0 flex flex-col items-center gap-1.5">
              <img v-if="homeTeamImage" :src="homeTeamImage" class="w-20 h-20 md:w-36 md:h-36 object-contain" alt="" />
              <div v-else class="w-20 h-20 md:w-36 md:h-36 rounded-full border-2 border-neutral-200 bg-neutral-50 flex items-center justify-center">
                <span class="text-lg font-bold text-blue-500 select-none">{{ initials(match.homeTeamName ?? '') }}</span>
              </div>
              <div class="text-xs md:text-sm font-semibold text-neutral-700 text-center max-w-[140px] leading-tight line-clamp-2">{{ match.homeTeamName }}</div>
            </div>

            <!-- Центр -->
            <div class="flex flex-col items-center gap-2 shrink-0 min-w-[96px] md:min-w-[120px]">
              <div class="text-3xl md:text-4xl font-extrabold text-neutral-900 tabular-nums leading-none">{{ homeGoals }} : {{ awayGoals }}</div>

              <!-- Таймер + статус -->
              <div class="flex items-center gap-1.5 px-3 py-1 rounded-full border transition-colors"
                :class="isPaused ? 'bg-yellow-50 border-yellow-200' : 'bg-green-50 border-green-200'">
                <span class="w-1.5 h-1.5 rounded-full shrink-0"
                  :class="isPaused ? 'bg-yellow-400' : 'bg-green-500 animate-pulse'"></span>
                <span class="text-sm font-bold tabular-nums"
                  :class="isPaused ? 'text-yellow-700' : 'text-green-700'">{{ timerDisplay }}</span>
              </div>
              <div class="text-xs font-semibold text-neutral-400">
                {{ matchPhase === 'break' ? 'Перерыв' : (isPaused ? 'Пауза' : 'Идёт') }}
              </div>

              <!-- Кнопки тайма -->
              <div class="flex gap-1.5">
                <button
                  @click="endFirstHalf"
                  :disabled="matchPhase !== 'first'"
                  :title="matchPhase === 'first' ? 'Нажмите чтобы закончить первый тайм' : undefined"
                  class="px-2.5 py-1 rounded-lg text-[10px] font-bold border transition-colors"
                  :class="matchPhase === 'first' ? 'border-blue-400 bg-blue-50 text-blue-600 hover:bg-blue-100' : 'border-neutral-200 bg-neutral-50 text-neutral-400 cursor-not-allowed'"
                >1-й тайм</button>
                <button
                  @click="startSecondHalf"
                  :disabled="matchPhase !== 'break'"
                  :title="matchPhase === 'break' ? 'Нажмите чтобы начать второй тайм' : undefined"
                  class="px-2.5 py-1 rounded-lg text-[10px] font-bold border transition-colors"
                  :class="matchPhase === 'break' ? 'border-blue-400 bg-blue-50 text-blue-600 hover:bg-blue-100' : 'border-neutral-200 bg-neutral-50 text-neutral-400 cursor-not-allowed'"
                >2-й тайм</button>
              </div>
            </div>

            <!-- Эмблема гостей -->
            <div class="flex-1 min-w-0 flex flex-col items-center gap-1.5">
              <img v-if="awayTeamImage" :src="awayTeamImage" class="w-20 h-20 md:w-36 md:h-36 object-contain" alt="" />
              <div v-else class="w-20 h-20 md:w-36 md:h-36 rounded-full border-2 border-neutral-200 bg-neutral-50 flex items-center justify-center">
                <span class="text-lg font-bold text-neutral-400 select-none">{{ initials(match.opponentTeamName ?? '?') }}</span>
              </div>
              <div class="text-xs md:text-sm font-semibold text-neutral-700 text-center max-w-[140px] leading-tight line-clamp-2">{{ match.opponentTeamName ?? 'Соперник' }}</div>
            </div>
          </div>

          <hr class="border-neutral-100" />

          <!-- Поле с составом — переключатель команд для Home-матча -->
          <div>
            <div class="text-xs font-bold text-neutral-500 uppercase tracking-wide text-center mb-3">Текущий состав</div>

            <!-- Переключатель команды для Home-матча -->
            <div v-if="isHomeMatch" class="flex gap-2 mb-3">
              <button
                @click="fieldTeamSide = 'home'"
                class="flex-1 py-2 rounded-xl border text-xs font-semibold transition-colors"
                :class="fieldTeamSide === 'home' ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-500 hover:border-neutral-300'"
              >{{ match.homeTeamName }}</button>
              <button
                @click="fieldTeamSide = 'away'"
                class="flex-1 py-2 rounded-xl border text-xs font-semibold transition-colors"
                :class="fieldTeamSide === 'away' ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-500 hover:border-neutral-300'"
              >{{ match.opponentTeamName }}</button>
            </div>

            <!-- Десктоп/планшет (≥768px): горизонтальная половина с выбором тактики -->
            <div class="hidden md:block">
              <MatchLineupDisplay
                :lineup="currentFieldLineup"
                :sportsmenMap="sportsmenMap"
                :events="fieldTeamSide === 'home' ? liveEvents.filter(e => e.isHomeTeam) : liveEvents.filter(e => !e.isHomeTeam)"
                :mirror="isInternalMatch && fieldTeamSide === 'away'"
                :match-id="match?.id ? `${match.id}:${fieldTeamSide}` : undefined"
              />
            </div>
            <!-- Мобила (<768px): вертикальное поле с той же логикой тактик -->
            <div class="md:hidden">
              <FootballFieldMobile
                :lineup="currentFieldLineup"
                :sportsmen-map="sportsmenMap"
                :events="fieldTeamSide === 'home' ? liveEvents.filter(e => e.isHomeTeam) : liveEvents.filter(e => !e.isHomeTeam)"
                :mirror="isInternalMatch && fieldTeamSide === 'away'"
                :match-id="match?.id ? `${match.id}:${fieldTeamSide}` : undefined"
              />
            </div>

            <!-- Кнопка просмотра состава -->
            <button @click="openLineupEditor"
              class="w-full mt-3 py-2 rounded-xl border border-blue-200 bg-blue-50/60 text-xs font-semibold text-blue-600 hover:bg-blue-100 transition-colors flex items-center justify-center gap-1.5">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z"/><path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
              </svg>
              {{ showFieldEdit ? 'Скрыть состав' : 'Просмотреть состав' }}
            </button>

            <!-- Состав (read-only после старта матча, обновляется при заменах) -->
            <div v-if="showFieldEdit" class="mt-3 space-y-2">
              <!-- Переключатель команды для Home-матча -->
              <div v-if="isHomeMatch" class="flex gap-2 mb-2">
                <button
                  @click="editTeamSide = 'home'"
                  class="flex-1 py-1.5 rounded-xl border text-xs font-semibold transition-colors"
                  :class="editTeamSide === 'home' ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-500'"
                >{{ match.homeTeamName }}</button>
                <button
                  @click="editTeamSide = 'away'"
                  class="flex-1 py-1.5 rounded-xl border text-xs font-semibold transition-colors"
                  :class="editTeamSide === 'away' ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-500'"
                >{{ match.opponentTeamName }}</button>
              </div>

              <div class="space-y-2 max-h-64 overflow-y-auto pr-1">
                <div v-for="row in liveLineupView" :key="row.sportsmanId"
                  class="flex items-center gap-2 p-2 rounded-xl border bg-neutral-50"
                  :class="row.cameOnFromBench ? 'border-purple-200 bg-purple-50' : 'border-neutral-100'">
                  <!-- ФИО и позиция текстом -->
                  <div class="flex-1 min-w-0 text-xs px-1 truncate font-semibold text-neutral-700">
                    {{ row.fio }}
                    <span class="text-neutral-400 font-normal">· {{ row.position }} ({{ positionRu[row.position] ?? row.position }})</span>
                    <span v-if="row.cameOnFromBench" class="ml-1 text-[10px] font-bold text-purple-600">↑ {{ row.subMinute }}′</span>
                  </div>
                  <!-- Позиция (короткий код) -->
                  <div class="shrink-0">
                    <div class="text-xs font-bold text-neutral-600 px-2 py-1 rounded border border-neutral-300 bg-white min-w-[48px] text-center">
                      {{ row.position }}
                    </div>
                  </div>
                  <!-- Тип -->
                  <div class="shrink-0">
                    <div class="text-xs font-bold px-2 py-1 rounded border min-w-[48px] text-center"
                      :class="row.type === 'Main' ? 'border-blue-400 bg-blue-50 text-blue-600' : 'border-green-400 bg-green-50 text-green-600'">
                      {{ row.type === 'Main' ? 'Осн.' : 'Зап.' }}
                    </div>
                  </div>
                </div>
                <div v-if="!liveLineupView.length" class="text-center text-xs text-neutral-400 py-3">
                  Нет игроков в составе
                </div>
              </div>
            </div>
          </div>

          <hr class="border-neutral-100" />

          <!-- Добавить событие -->
          <div>
            <div class="text-xs font-bold text-neutral-500 uppercase tracking-wide text-center mb-3">Добавить событие</div>
            <div class="grid grid-cols-4 gap-2">
              <button v-for="et in eventTypes" :key="et.type"
                @click="openAddEvent(et.type)"
                class="flex flex-col items-center gap-1.5 py-2.5 rounded-xl border border-neutral-200 hover:border-blue-300 hover:bg-blue-50 transition-colors group">
                <div class="w-8 h-8 rounded-lg flex items-center justify-center shrink-0" :class="et.bg">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8" class="w-4 h-4" :class="et.color" v-html="et.path"></svg>
                </div>
                <span class="text-[10px] font-semibold text-neutral-500 group-hover:text-blue-700 text-center leading-tight px-1">{{ et.label }}</span>
              </button>
            </div>
          </div>

          <hr class="border-neutral-100" />

          <!-- События матча -->
          <div>
            <div class="text-xs font-bold text-neutral-500 uppercase tracking-wide text-center mb-3">
              События матча
              <span class="ml-1 text-neutral-400 font-normal normal-case">({{ liveEvents.length }})</span>
            </div>
            <div v-if="!liveEvents.length" class="text-center text-xs text-neutral-400 py-4">Событий ещё нет</div>
            <div v-else class="divide-y divide-neutral-100">
              <div v-for="ev in sortedLiveEvents" :key="ev.id"
                class="relative flex items-center gap-2 py-2.5"
                :class="ev.isHomeTeam ? 'flex-row' : 'flex-row-reverse'"
              >
                <div class="flex items-center gap-2 flex-1 min-w-0" :class="ev.isHomeTeam ? 'justify-start' : 'justify-end'">
                  <div class="w-7 h-7 rounded-lg flex items-center justify-center shrink-0" :class="getEventStyle(ev.type).bg">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8" class="w-3.5 h-3.5" :class="getEventStyle(ev.type).color" v-html="getEventStyle(ev.type).path"></svg>
                  </div>
                  <div class="min-w-0" :class="ev.isHomeTeam ? 'text-left' : 'text-right'">
                    <div class="text-xs font-semibold text-neutral-800">{{ eventTypeLabel[ev.type] ?? ev.type }}</div>
                    <div v-if="ev.sportsmanId" class="text-xs text-neutral-500 truncate">
                      {{ sportsmenMap[ev.sportsmanId] ?? `#${ev.sportsmanId}` }}
                      <template v-if="ev.substituteSportsmanId"> → {{ sportsmenMap[ev.substituteSportsmanId] ?? `#${ev.substituteSportsmanId}` }}</template>
                    </div>
                    <div v-if="ev.comment" class="text-xs text-neutral-400 italic truncate">{{ ev.comment }}</div>
                  </div>
                </div>
                <div class="absolute left-1/2 -translate-x-1/2 top-1/2 -translate-y-1/2 text-base font-bold text-neutral-600 tabular-nums pointer-events-none">
                  {{ ev.minute }}'
                </div>
                <div class="flex items-center gap-0.5 shrink-0">
                  <button @click="openEditEvent(ev)"
                    class="w-7 h-7 rounded-lg flex items-center justify-center text-neutral-400 hover:text-blue-600 hover:bg-blue-50 transition-colors">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3 h-3">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931z"/>
                    </svg>
                  </button>
                  <button @click="deleteEvent(ev.id)"
                    class="w-7 h-7 rounded-lg flex items-center justify-center text-neutral-400 hover:text-red-500 hover:bg-red-50 transition-colors">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3 h-3">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0"/>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <hr class="border-neutral-100" />

          <!-- Завершение матча -->
          <div>
            <div class="text-xs font-bold text-neutral-500 uppercase tracking-wide text-center mb-3">Завершение матча</div>
            <div class="grid grid-cols-1 gap-3 sm:grid-cols-2 mb-3">
              <div>
                <label class="block text-xs font-semibold text-neutral-500 mb-1">Результат</label>
                <select v-model="finishForm.result" class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 focus:outline-none focus:border-blue-400">
                  <option value="">Выберите результат</option>
                  <template v-if="isInternalMatch">
                    <!-- Обе команды наши: подписываем чьей именно командой выиграно.
                         Win = победа домашней; Loss = победа гостевой (тоже нашей). -->
                    <option value="Win">Победа {{ match.homeTeamName }}</option>
                    <option value="Loss">Победа {{ match.opponentTeamName }}</option>
                    <option value="Draw">Ничья</option>
                  </template>
                  <template v-else>
                    <option value="Win">Победа</option>
                    <option value="Draw">Ничья</option>
                    <option value="Loss">Поражение</option>
                  </template>
                </select>
              </div>
              <div>
                <label class="block text-xs font-semibold text-neutral-500 mb-1">Комментарий тренера</label>
                <input v-model="finishForm.trainerComment" type="text" placeholder="Итог игры..."
                  class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 focus:outline-none focus:border-blue-400" />
              </div>
            </div>
            <button
              @click="finishMatch"
              :disabled="!finishForm.result || finishingMatch"
              class="w-full py-3 rounded-xl bg-red-600 text-white text-sm font-bold hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >{{ finishingMatch ? 'Завершение...' : 'Завершить матч' }}</button>
            <button
              @click="showCancelConfirm = true"
              class="w-full mt-2 py-2.5 rounded-xl bg-neutral-100 text-neutral-500 text-sm font-semibold hover:bg-neutral-200 transition-colors"
            >Отменить матч</button>
          </div>

        </div>
      </div>
    </template>

    <!-- Модалка подтверждения старта -->
    <ConfirmStartModal
      v-if="showStartConfirm"
      message="Состав будет зафиксирован, таймер запустится."
      @confirm="showStartConfirm = false; confirmAndStart()"
      @cancel="showStartConfirm = false"
    />

    <!-- Модалка отмены матча -->
    <ConfirmDeleteModal
      v-if="showCancelConfirm"
      message="Матч будет удалён без сохранения результата. Это действие нельзя отменить."
      @confirm="cancelMatch"
      @cancel="showCancelConfirm = false"
    />

    <!-- Модалка события -->
    <Teleport to="body">
      <div v-if="eventModal.open" class="fixed inset-0 z-50 flex items-end sm:items-center justify-center p-4 bg-black/40 backdrop-blur-sm" @click.self="eventModal.open = false">
        <div class="w-full max-w-sm bg-white rounded-2xl shadow-2xl overflow-hidden">
          <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400" />
          <div class="px-5 py-4 border-b border-neutral-100 flex items-center justify-between">
            <div class="font-bold text-neutral-900 text-sm">{{ eventModal.editId ? 'Редактировать событие' : 'Добавить событие' }}</div>
            <button @click="eventModal.open = false" class="w-8 h-8 rounded-xl flex items-center justify-center text-neutral-400 hover:bg-neutral-100 transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>
          <div class="px-5 py-4 space-y-3 max-h-[70vh] overflow-y-auto">

            <!-- Тип события -->
            <div>
              <div class="text-xs font-semibold text-neutral-500 mb-2">Тип события</div>
              <div class="grid grid-cols-4 gap-1.5">
                <button v-for="et in eventTypes" :key="et.type"
                  @click="eventModal.form.type = et.type"
                  class="flex flex-col items-center gap-1 px-1 py-2 rounded-xl border transition-colors"
                  :class="eventModal.form.type === et.type ? 'border-blue-400 bg-blue-50' : 'border-neutral-200 hover:border-neutral-300'">
                  <div class="w-7 h-7 rounded-lg flex items-center justify-center" :class="et.bg">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8" class="w-3.5 h-3.5" :class="et.color" v-html="et.path"></svg>
                  </div>
                  <span class="text-[10px] font-semibold text-neutral-600 leading-tight text-center">{{ et.label }}</span>
                </button>
              </div>
            </div>

            <!-- Команда — для Home-матча показываем обе наши, иначе только кнопку переключения -->
            <div>
              <div class="text-xs font-semibold text-neutral-500 mb-2">Команда</div>
              <div class="flex gap-2">
                <button
                  @click="setEventTeam(true)"
                  class="flex-1 py-2 rounded-xl border text-xs font-semibold transition-colors"
                  :class="eventModal.form.isHomeTeam ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-600 hover:border-neutral-300'">
                  {{ match?.homeTeamName ?? 'Наша команда' }}
                </button>
                <button
                  @click="setEventTeam(false)"
                  class="flex-1 py-2 rounded-xl border text-xs font-semibold transition-colors"
                  :class="!eventModal.form.isHomeTeam ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-600 hover:border-neutral-300'">
                  {{ match?.opponentTeamName ?? 'Соперник' }}
                </button>
              </div>
            </div>

            <!-- Минута -->
            <div>
              <div class="text-xs font-semibold text-neutral-500 mb-2">Минута (0–130)</div>
              <input v-model.number="eventModal.form.minute" type="number" min="0" max="130"
                class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 focus:outline-none focus:border-blue-400" />
            </div>

            <!-- Игрок -->
            <template v-if="currentModalSportsmen.length > 0">
              <div>
                <div class="text-xs font-semibold text-neutral-500 mb-2">
                  {{ eventModal.form.type === 'Substitution' ? 'Уходит с поля' : 'Игрок' }}
                </div>
                <!-- Кнопка-заголовок: показывает выбранного, клик раскрывает -->
                <button type="button"
                  @click="playerSelectOpen = !playerSelectOpen"
                  class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 bg-white text-left flex items-center justify-between hover:border-neutral-300 transition-colors">
                  <span :class="eventModal.form.sportsmanId ? 'text-neutral-900' : 'text-neutral-400'">
                    {{ selectedSportsmanLabel || 'Не указан' }}
                  </span>
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
                    class="w-3.5 h-3.5 text-neutral-400 transition-transform"
                    :class="playerSelectOpen ? 'rotate-180' : ''">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 8.25l-7.5 7.5-7.5-7.5"/>
                  </svg>
                </button>
                <div v-if="playerSelectOpen" class="mt-2">
                  <input v-model="playerSearch" placeholder="Поиск игрока..." class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2 mb-2 focus:outline-none focus:border-blue-400" />
                  <div class="max-h-40 overflow-y-auto rounded-xl border border-neutral-200 divide-y divide-neutral-50">
                    <button type="button"
                      @click="eventModal.form.sportsmanId = ''; playerSelectOpen = false"
                      class="w-full text-left px-3 py-2 text-sm transition-colors"
                      :class="eventModal.form.sportsmanId === '' ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-neutral-400 hover:bg-neutral-50'">
                      Не указан
                    </button>
                    <button type="button"
                      v-for="s in filteredModalSportsmen" :key="s.id"
                      @click="eventModal.form.sportsmanId = s.id; playerSelectOpen = false"
                      class="w-full text-left px-3 py-2 text-sm transition-colors"
                      :class="eventModal.form.sportsmanId === s.id ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-neutral-700 hover:bg-neutral-50'">
                      {{ s.fio }}<span v-if="s.position" class="text-neutral-400 text-xs ml-1">· {{ positionRu[s.position] ?? s.position }}</span>
                    </button>
                  </div>
                </div>
              </div>
              <div v-if="eventModal.form.type === 'Substitution'">
                <div class="text-xs font-semibold text-neutral-500 mb-2">Выходит на поле</div>
                <!-- Кнопка-заголовок второго селектора -->
                <button type="button"
                  @click="substituteSelectOpen = !substituteSelectOpen"
                  class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 bg-white text-left flex items-center justify-between hover:border-neutral-300 transition-colors">
                  <span :class="eventModal.form.substituteSportsmanId ? 'text-neutral-900' : 'text-neutral-400'">
                    {{ selectedSubstituteLabel || 'Не указан' }}
                  </span>
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
                    class="w-3.5 h-3.5 text-neutral-400 transition-transform"
                    :class="substituteSelectOpen ? 'rotate-180' : ''">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 8.25l-7.5 7.5-7.5-7.5"/>
                  </svg>
                </button>
                <div v-if="substituteSelectOpen" class="mt-2">
                  <input v-model="substituteSearch" placeholder="Поиск замены..." class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2 mb-2 focus:outline-none focus:border-blue-400" />
                  <div class="max-h-40 overflow-y-auto rounded-xl border border-neutral-200 divide-y divide-neutral-50">
                    <button type="button"
                      @click="eventModal.form.substituteSportsmanId = ''; substituteSelectOpen = false"
                      class="w-full text-left px-3 py-2 text-sm transition-colors"
                      :class="eventModal.form.substituteSportsmanId === '' ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-neutral-400 hover:bg-neutral-50'">
                      Не указан
                    </button>
                    <button type="button"
                      v-for="s in filteredModalReserves" :key="s.id"
                      @click="eventModal.form.substituteSportsmanId = s.id; substituteSelectOpen = false"
                      class="w-full text-left px-3 py-2 text-sm transition-colors"
                      :class="eventModal.form.substituteSportsmanId === s.id ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-neutral-700 hover:bg-neutral-50'">
                      {{ s.fio }}<span v-if="s.position" class="text-neutral-400 text-xs ml-1">· {{ positionRu[s.position] ?? s.position }}</span>
                    </button>
                  </div>
                </div>
              </div>
            </template>

            <!-- Комментарий -->
            <div>
              <div class="text-xs font-semibold text-neutral-500 mb-2">Комментарий</div>
              <input v-model="eventModal.form.comment" type="text" placeholder="Необязательно..."
                class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2.5 focus:outline-none focus:border-blue-400" />
            </div>
          </div>
          <div class="px-5 pb-5 pt-3 flex gap-2 border-t border-neutral-100">
            <button @click="eventModal.open = false"
              class="flex-1 py-2.5 rounded-xl border border-neutral-200 text-sm font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">Отмена</button>
            <button @click="submitEvent" :disabled="submittingEvent"
              class="flex-1 py-2.5 rounded-xl bg-blue-600 text-white text-sm font-semibold hover:bg-blue-700 disabled:opacity-50 transition-colors">
              {{ submittingEvent ? 'Сохранение...' : (eventModal.editId ? 'Сохранить' : 'Добавить') }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { POSITION_AND_GROUP_LABEL, MATCH_TYPE, POSITION_REPLACEMENTS } from '@/constants'
import { formatDate } from '@/utils/formatDate'
import { imageUrl } from '@/utils/imageUrl'
import { useAuthStore } from '@/store/auth'
import LiveMatchHeader from '@/components/trainer/LiveMatchHeader.vue'
import MatchLineupDisplay from '@/components/trainer/MatchLineupDisplay.vue'
import FootballFieldMobile from '@/components/trainer/FootballFieldMobile.vue'
import ConfirmDeleteModal from '@/components/ui/ConfirmDeleteModal.vue'
import ConfirmStartModal from '@/components/ui/ConfirmStartModal.vue'

const router = useRouter()
const auth = useAuthStore()



const typeLabel = MATCH_TYPE

// Лейблы позиций — единый источник из @/constants, плюс альтернативные коды (CB2/CM2/CM3) для составов.
const positionRu: Record<string, string> = {
  ...POSITION_AND_GROUP_LABEL,
  CB2: POSITION_AND_GROUP_LABEL.CB,
  CM2: POSITION_AND_GROUP_LABEL.CM,
  CM3: POSITION_AND_GROUP_LABEL.CM,
}
const eventTypeLabel: Record<string, string> = {
  Goal: 'Гол', YellowCard: 'Жёлтая карточка', RedCard: 'Красная карточка',
  Corner: 'Угловой', Penalty: 'Пенальти', Foul: 'Фол', Substitution: 'Замена',
  HalfTimeEnd: 'Конец 1-го тайма', SecondHalfStart: 'Начало 2-го тайма',
  Pause: 'Матч приостановлен', Resume: 'Матч продолжен',
}

const EVENT_STYLES: Record<string, { bg: string; color: string; path: string }> = {
  Goal:            { bg: 'bg-green-50',  color: 'text-green-600',  path: '<circle cx="12" cy="12" r="10"/><path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4l3 3"/>' },
  YellowCard:      { bg: 'bg-yellow-50', color: 'text-yellow-500', path: '<rect x="6" y="3" width="12" height="18" rx="2"/>' },
  RedCard:         { bg: 'bg-red-50',    color: 'text-red-500',    path: '<rect x="6" y="3" width="12" height="18" rx="2"/>' },
  Corner:          { bg: 'bg-blue-50',   color: 'text-blue-600',   path: '<path stroke-linecap="round" stroke-linejoin="round" d="M3 3v18h18"/><path stroke-linecap="round" stroke-linejoin="round" d="M3 21c5-1 10-5 12-9"/>' },
  Foul:            { bg: 'bg-orange-50', color: 'text-orange-500', path: '<path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3m0 0v3m0-3h3m-3 0H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z"/>' },
  Penalty:         { bg: 'bg-purple-50', color: 'text-purple-600', path: '<path stroke-linecap="round" stroke-linejoin="round" d="M15 10l4.553-2.069A1 1 0 0121 8.82v6.36a1 1 0 01-1.447.894L15 14M3 8a2 2 0 012-2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2V8z"/>' },
  Substitution:    { bg: 'bg-teal-50',   color: 'text-teal-600',   path: '<path stroke-linecap="round" stroke-linejoin="round" d="M7 16V4m0 0L3 8m4-4l4 4M17 8v12m0 0l4-4m-4 4l-4-4"/>' },
  HalfTimeEnd:     { bg: 'bg-neutral-100', color: 'text-neutral-500', path: '<path stroke-linecap="round" stroke-linejoin="round" d="M15.75 5.25v13.5m-7.5-13.5v13.5"/>' },
  SecondHalfStart: { bg: 'bg-neutral-100', color: 'text-neutral-500', path: '<path stroke-linecap="round" stroke-linejoin="round" d="M5.25 5.653c0-.856.917-1.398 1.667-.986l11.54 6.347a1.125 1.125 0 010 1.972l-11.54 6.347a1.125 1.125 0 01-1.667-.986V5.653z"/>' },
  Pause:           { bg: 'bg-yellow-50', color: 'text-yellow-500', path: '<path stroke-linecap="round" stroke-linejoin="round" d="M15.75 5.25v13.5m-7.5-13.5v13.5"/>' },
  Resume:          { bg: 'bg-green-50',  color: 'text-green-600',  path: '<path stroke-linecap="round" stroke-linejoin="round" d="M5.25 5.653c0-.856.917-1.398 1.667-.986l11.54 6.347a1.125 1.125 0 010 1.972l-11.54 6.347a1.125 1.125 0 01-1.667-.986V5.653z"/>' },
}

// Только те типы которые тренер добавляет вручную (без системных)
const MANUAL_EVENT_TYPES = ['Goal', 'YellowCard', 'RedCard', 'Corner', 'Foul', 'Penalty', 'Substitution']

const eventTypes = MANUAL_EVENT_TYPES.map(type => ({
  type, label: eventTypeLabel[type], ...EVENT_STYLES[type],
}))

function getEventStyle(type: string) {
  return EVENT_STYLES[type] ?? { bg: 'bg-neutral-50', color: 'text-neutral-500', path: '<circle cx="12" cy="12" r="4"/>' }
}

// State
const step = ref<'select' | 'lineup' | 'live'>('select')
const match = ref<any>(null)
const loadingScheduled = ref(true)
const scheduledMatches = ref<any[]>([])
const myTeams = ref<any[]>([])
// Все команды академии — для выбора соперника (включая чужих тренеров).
const allTeams = ref<any[]>([])
const myGroups = ref<any[]>([])

// Эмблемы команд — отдельные ref для прямого биндинга
const homeTeamImage = ref<string>('')
const awayTeamImage = ref<string>('')

const sportsmenMap = reactive<Record<number, string>>({})
// Спортсмены домашней команды
const homeSportsmen = ref<{ id: number; fio: string; position: string | null }[]>([])
// Спортсмены гостевой команды (для Home-матча)
const awaySportsmen = ref<{ id: number; fio: string; position: string | null }[]>([])


const liveEvents = ref<any[]>([])
const showCreateForm = ref(false)
const showFieldEdit = ref(false)
const showCancelConfirm = ref(false)
const lineupConfirmed = ref(false)
const startingMatch = ref(false)
const pendingMatchId = ref<number | null>(null)
const showStartConfirm = ref(false)

// Тайм: 'first' | 'break' | 'second' | 'done'
const matchPhase = ref<'first' | 'break' | 'second' | 'done'>('first')
const isPaused = ref(false)
const finishingMatch = ref(false)
const submittingEvent = ref(false)

// Сторона для отображения поля (переключатель Home-матча)
const fieldTeamSide = ref<'home' | 'away'>('home')
// Сторона для редактора состава
const editTeamSide = ref<'home' | 'away'>('home')

// lineup для редактирования (по текущей стороне editTeamSide)
const homeLineupEdit = ref<{ sportsmanId: any; position: string; type: string; substituteId?: any }[]>([])
const awayLineupEdit = ref<{ sportsmanId: any; position: string; type: string; substituteId?: any }[]>([])

// Алиас для текущего редактируемого состава
const lineupEdit = computed({
  get: () => editTeamSide.value === 'home' ? homeLineupEdit.value : awayLineupEdit.value,
  set: (v) => { if (editTeamSide.value === 'home') homeLineupEdit.value = v; else awayLineupEdit.value = v },
})

// Текущий lineup для отображения поля.
// Дополняем substituteId из локального homeLineupEdit/awayLineupEdit — выбор тренера
// на шаге утверждения хранится только во фронте (не уходит на бэк), но не должен теряться
// между шагом утверждения и live. Связка нужна для парной отрисовки на карте (Main + замена).
const currentFieldLineup = computed(() => {
  if (!match.value) return []
  const isHomeSide = !isHomeMatch.value || fieldTeamSide.value === 'home'
  const baseLineup = isHomeSide
    ? (match.value.lineup ?? [])
    : (match.value.awayLineup ?? [])
  const localEdit = isHomeSide ? homeLineupEdit.value : awayLineupEdit.value
  // Мерджим substituteId по sportsmanId
  return baseLineup.map((e: any) => {
    const local = localEdit.find(l => Number(l.sportsmanId) === Number(e.sportsmanId))
    return { ...e, substituteId: local?.substituteId ?? undefined }
  })
})

// Спортсмены для текущего редактора состава
const currentEditSportsmen = computed(() =>
  editTeamSide.value === 'home' ? homeSportsmen.value : awaySportsmen.value
)

// На шаге утверждения показываем ТОЛЬКО основу (Main).
// Запасные не нужны как отдельные строки — они подбираются через бейдж "Замены".
// Сохраняем оригинальный индекс в lineupEdit для двухсторонней связи через v-model.
const lineupMainEntries = computed(() =>
  lineupEdit.value
    .map((e, i) => ({ entry: e, idx: i }))
    .filter(x => x.entry.type === 'Main')
)

// Возвращает список Reserve-игроков, подходящих по позиции для подмены данной.
// Если в таблице нет позиции — допускаем только точное совпадение.
function getPossibleReplacements(mainPosition: string) {
  const allowed = POSITION_REPLACEMENTS[mainPosition] ?? [mainPosition]
  const reserveEntries = lineupEdit.value.filter(e => e.type === 'Reserve')
  return reserveEntries
    .filter(r => allowed.includes(r.position))
    .map(r => {
      const s = currentEditSportsmen.value.find(x => x.id === Number(r.sportsmanId))
      return { id: Number(r.sportsmanId), fio: s?.fio ?? `#${r.sportsmanId}`, position: r.position }
    })
}

// Индекс строки с раскрытым списком замен (для бейджа на шаге утверждения)
const openSubstitutesFor = ref<number | null>(null)
function toggleSubstitutes(idx: number) {
  openSubstitutesFor.value = openSubstitutesFor.value === idx ? null : idx
}

// Назначение замены: свопаем роли Main ↔ Reserve.
// Тренер хочет видеть выбранного запасного на позиции основного, а основной уходит в резерв.
// Связь "кого заменил" сохраняем через substituteId — нужно для парного отображения в бейдже и на карте.
function assignSubstitute(mainIdx: number, substituteId: number | undefined) {
  const mainEntry = lineupEdit.value[mainIdx]
  if (!mainEntry) return

  // Если был ранее назначен другой запасной — сначала восстанавливаем его как Reserve
  const previousSubId = mainEntry.substituteId ? Number(mainEntry.substituteId) : null

  if (!substituteId) {
    // Сброс — если кто-то был Main по чужой позиции, надо его вернуть на Reserve
    // и mainEntry вернуть как Main на свою позицию (если был свопнут).
    // Проще: если mainEntry.type === 'Reserve' (его свопнули) — вернуть в Main
    if (mainEntry.type === 'Reserve' && previousSubId) {
      // Находим того кто сейчас Main на позиции mainEntry.position и был свопнут — вернуть в Reserve
      const onPosMainIdx = lineupEdit.value.findIndex((e, i) => i !== mainIdx && e.type === 'Main' && Number(e.sportsmanId) === previousSubId)
      if (onPosMainIdx !== -1) {
        lineupEdit.value[onPosMainIdx].type = 'Reserve'
        // Восстанавливаем оригинальную позицию запасного (по умолчанию его исходная, но мы её не помним —
        // оставим как есть, тренер сам разрулит при необходимости)
      }
      mainEntry.type = 'Main'
    }
    mainEntry.substituteId = undefined
    return
  }

  // Назначаем нового
  const subIdx = lineupEdit.value.findIndex(e => Number(e.sportsmanId) === Number(substituteId) && e.type === 'Reserve')
  if (subIdx === -1) {
    // Запасной не найден — просто запоминаем substituteId без свопа
    mainEntry.substituteId = substituteId
    return
  }
  // Если был выбран другой ранее — вернуть его в Reserve
  if (previousSubId && previousSubId !== Number(substituteId)) {
    const prevIdx = lineupEdit.value.findIndex(e => Number(e.sportsmanId) === previousSubId && e.type === 'Main')
    if (prevIdx !== -1) lineupEdit.value[prevIdx].type = 'Reserve'
  }
  // Свопаем роли
  const subEntry = lineupEdit.value[subIdx]
  subEntry.type = 'Main'
  subEntry.position = mainEntry.position
  // mainEntry уходит в Reserve, но substituteId остаётся для отображения пары
  mainEntry.type = 'Reserve'
  mainEntry.substituteId = substituteId
  // Связку: у нового Main substituteId указывает на бывшего (чтобы пара отображалась)
  subEntry.substituteId = mainEntry.sportsmanId
}

// Спортсмены для модалки события (основные + запасные, для поля «уходит»)
const currentModalSportsmen = computed(() => {
  // Home-матч (внутри академии) — нормальное переключение между home/away составами
  if (isHomeMatch.value) {
    return eventModal.value.form.isHomeTeam ? homeSportsmen.value : awaySportsmen.value
  }
  // Внешний соперник — наши игроки только для своей команды, иначе пустой список
  return eventModal.value.form.isHomeTeam ? homeSportsmen.value : []
})

const playerSearch = ref('')
const substituteSearch = ref('')

// Состояние раскрытия дропдаунов в модалке события
const playerSelectOpen = ref(false)
const substituteSelectOpen = ref(false)

const selectedSportsmanLabel = computed(() => {
  const id = Number(eventModal.value.form.sportsmanId)
  if (!id) return ''
  const s = currentModalSportsmen.value.find(x => x.id === id)
  if (!s) return sportsmenMap[id] ?? `#${id}`
  return s.position ? `${s.fio} · ${positionRu[s.position] ?? s.position}` : s.fio
})

const selectedSubstituteLabel = computed(() => {
  const id = Number(eventModal.value.form.substituteSportsmanId)
  if (!id) return ''
  const s = currentModalSportsmen.value.find(x => x.id === id)
  if (!s) return sportsmenMap[id] ?? `#${id}`
  return s.position ? `${s.fio} · ${positionRu[s.position] ?? s.position}` : s.fio
})

// Для Substitution — пул "Уходит с поля" = только те, кто сейчас на поле
// (Main + Reserve которые уже вышли заменой, минус те кого уже заменили).
// Для остальных событий — все игроки команды.
const currentlyOnField = computed(() => {
  const lineup = eventModal.value.form.isHomeTeam
    ? (match.value?.lineup ?? [])
    : (match.value?.awayLineup ?? [])
  const onField = new Set<number>()
  lineup.forEach((e: any) => {
    if (e.type !== 'Reserve') onField.add(Number(e.sportsmanId))
  })
  const subs = liveEvents.value
    .filter(e => e.type === 'Substitution' && e.isHomeTeam === eventModal.value.form.isHomeTeam && e.sportsmanId && e.substituteSportsmanId)
    .sort((a, b) => (a.minute ?? 0) - (b.minute ?? 0))
  subs.forEach(sub => {
    onField.delete(Number(sub.sportsmanId))
    onField.add(Number(sub.substituteSportsmanId))
  })
  return currentModalSportsmen.value.filter(s => onField.has(s.id))
})

const filteredModalSportsmen = computed(() => {
  const isSub = eventModal.value.form.type === 'Substitution'
  const pool = isSub ? currentlyOnField.value : currentModalSportsmen.value
  const q = playerSearch.value.trim().toLowerCase()
  if (!q) return pool
  return pool.filter(s => s.fio?.toLowerCase().includes(q))
})

// Read-only представление текущего состава для блока «Просмотреть состав» на live-странице.
// Берёт match.lineup (или awayLineup), применяет события Substitution в хронологии:
//   - ушедший с поля становится 'OffField' (был Main, теперь не играет)
//   - запасной, вышедший заменой, становится 'Main' (на позиции ушедшего)
// Reserve без выхода — остаются 'Reserve'.
const liveLineupView = computed(() => {
  if (!match.value || step.value !== 'live') return []
  const isHome = !isHomeMatch.value || editTeamSide.value === 'home'
  const lineup = isHome
    ? (match.value.lineup ?? [])
    : (match.value.awayLineup ?? [])
  if (!lineup.length) return []
  const sportsmenPool = isHome ? homeSportsmen.value : awaySportsmen.value

  type Row = { sportsmanId: number; fio: string; position: string; type: string; cameOnFromBench?: boolean; offField?: boolean; subMinute?: number }
  const rows: Row[] = lineup.map((e: any) => ({
    sportsmanId: Number(e.sportsmanId),
    fio: sportsmenPool.find(s => s.id === Number(e.sportsmanId))?.fio ?? sportsmenMap[Number(e.sportsmanId)] ?? `#${e.sportsmanId}`,
    position: e.position,
    type: e.type ?? 'Main',
  }))

  const subs = liveEvents.value
    .filter(e => e.type === 'Substitution' && e.isHomeTeam === isHome && e.sportsmanId && e.substituteSportsmanId)
    .sort((a, b) => (a.minute ?? 0) - (b.minute ?? 0))

  // Применяем замены: ушедший удаляется из видимого списка, вместо него встаёт пришедший
  // на его позицию. Пришедший помечается флагом cameOnFromBench для визуальной пометки в списке.
  // История замен видна в блоке "События" и в тултипах на карте.
  const removeIds = new Set<number>()
  subs.forEach(sub => {
    const outId = Number(sub.sportsmanId)
    const inId  = Number(sub.substituteSportsmanId)
    const out = rows.find(r => r.sportsmanId === outId && r.type === 'Main')
    const inP = rows.find(r => r.sportsmanId === inId && r.type === 'Reserve')
    if (!out || !inP) return
    removeIds.add(outId)
    inP.type = 'Main'
    inP.position = out.position
    inP.cameOnFromBench = true
    inP.subMinute = sub.minute
  })

  // Сортируем: Main на поле → вышедшие заменой → Reserve на скамейке
  return rows
    .filter(r => !removeIds.has(r.sportsmanId))
    .sort((a, b) => {
      const order = (r: Row) =>
        r.type === 'Main' && !r.cameOnFromBench ? 0 :
        r.cameOnFromBench ? 1 : 2
      return order(a) - order(b)
    })
})

// Запасные (type=Reserve) для поля «Выходит на замену»
// Кто сейчас на скамейке (с учётом всех уже сделанных замен).
// Скамейка = исходные Reserve, которые ещё не выходили + ушедшие с поля Main.
// Эти игроки могут быть выбраны для "Выходит на поле" в новой замене.
const currentModalBench = computed(() => {
  const lineup = eventModal.value.form.isHomeTeam
    ? (match.value?.lineup ?? [])
    : (match.value?.awayLineup ?? [])
  // Стартовая скамейка — все Reserve
  const bench = new Set<number>()
  lineup.forEach((e: any) => {
    if (e.type === 'Reserve') bench.add(Number(e.sportsmanId))
  })
  // Применяем все Substitution в хронологии: ушедший возвращается на скамейку, вошедший уходит
  const subs = liveEvents.value
    .filter(e => e.type === 'Substitution' && e.isHomeTeam === eventModal.value.form.isHomeTeam && e.sportsmanId && e.substituteSportsmanId)
    .sort((a, b) => (a.minute ?? 0) - (b.minute ?? 0))
  subs.forEach(sub => {
    bench.delete(Number(sub.substituteSportsmanId))
    bench.add(Number(sub.sportsmanId))
  })
  return currentModalSportsmen.value.filter(s => bench.has(s.id))
})

const currentModalReserves = computed(() => {
  const pool = currentModalBench.value
  // Если уже выбран "уходящий" — фильтруем по совместимости позиции (с учётом текущей позиции)
  const outId = Number(eventModal.value.form.sportsmanId) || 0
  if (!outId) return pool
  // Текущая позиция уходящего: берём из liveLineupView (он учитывает замены)
  const outRow = liveLineupView.value.find(r => r.sportsmanId === outId)
  const outPos = outRow?.position
  if (!outPos) return pool
  const allowed = POSITION_REPLACEMENTS[outPos] ?? [outPos]
  return pool.filter(s => allowed.includes(s.position ?? ''))
})

const filteredModalReserves = computed(() => {
  const q = substituteSearch.value.trim().toLowerCase()
  if (!q) return currentModalReserves.value
  return currentModalReserves.value.filter(s => s.fio?.toLowerCase().includes(q))
})

const createForm = ref({ homeTeamId: '' as any, opponentTeamName: '', opponentTeamId: '' as any, type: 'Friendly', date: getLocalDateTimeString() })
const finishForm = ref({ result: '', trainerComment: '' })

// Матч между двумя нашими командами: тип Home + есть opponentTeamId.
// Тогда селектор результата показывает имена команд вместо абстрактных Win/Loss.
const isInternalMatch = computed(() => {
  const m = match.value
  return !!m && m.type === 'Home' && !!m.opponentTeamId
})

const eventModal = ref({
  open: false,
  editId: null as number | null,
  form: { type: 'Goal', isHomeTeam: true, minute: 1, sportsmanId: '' as any, substituteSportsmanId: '' as any, comment: '' },
})

// Это Home-матч (обе команды наши)?
const isHomeMatch = computed(() => match.value?.type === 'Home')

// Timer
const timerSeconds = ref(0)
let timerInterval: ReturnType<typeof setInterval> | null = null

const timerDisplay = computed(() => {
  const m = Math.floor(timerSeconds.value / 60)
  const s = timerSeconds.value % 60
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
})
const currentMinute = computed(() => Math.floor(timerSeconds.value / 60) + 1)

const canCreateMatch = computed(() => {
  if (!createForm.value.homeTeamId || !createForm.value.date) return false
  if (createForm.value.type === 'Home') {
    return !!createForm.value.opponentTeamId
  }
  return !!createForm.value.opponentTeamName
})

// Соперник — любая команда академии (не обязательно тренера), кроме самой "домашней".
const availableOpponentTeams = computed(() => {
  if (!createForm.value.homeTeamId) return allTeams.value
  return allTeams.value.filter((t: any) => t.id !== Number(createForm.value.homeTeamId))
})

function getLocalDateTimeString() {
  const now = new Date()
  const year = now.getFullYear()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  const hours = String(now.getHours()).padStart(2, '0')
  const minutes = String(now.getMinutes()).padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}`
}

// Бэк возвращает UTC DateTime без суффикса Z — добавляем его для корректного парсинга
function parseUtcDate(s: string): Date {
  if (!s) return new Date()
  if (/[Zz]$/.test(s) || /[+-]\d{2}:?\d{2}$/.test(s)) return new Date(s)
  return new Date(s + 'Z')
}

function startTimer(fromUtc?: string) {
  if (timerInterval) return
  if (fromUtc) {
    const elapsed = Math.floor((Date.now() - parseUtcDate(fromUtc).getTime()) / 1000)
    timerSeconds.value = Math.max(0, elapsed)
  } else {
    timerSeconds.value = 0
  }
  timerInterval = setInterval(() => { timerSeconds.value++ }, 1000)
}

function stopTimer() {
  if (timerInterval) { clearInterval(timerInterval); timerInterval = null }
}

const homeGoals = computed(() => liveEvents.value.filter(e => e.type === 'Goal' && e.isHomeTeam).length)
const awayGoals = computed(() => liveEvents.value.filter(e => e.type === 'Goal' && !e.isHomeTeam).length)
const sortedLiveEvents = computed(() => [...liveEvents.value].sort((a, b) => b.minute - a.minute))

function initials(name: string) {
  return name.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase()
}

function onTypeChange() {
  if (createForm.value.type === 'Home') {
    createForm.value.opponentTeamName = ''
  } else {
    createForm.value.opponentTeamId = ''
  }
}

// Смена команды в модалке
function setEventTeam(isHome: boolean) {
  eventModal.value.form.isHomeTeam = isHome
  eventModal.value.form.sportsmanId = ''
  eventModal.value.form.substituteSportsmanId = ''
  playerSearch.value = ''
  substituteSearch.value = ''
  playerSelectOpen.value = false
  substituteSelectOpen.value = false
}

// Синхронизация lineupEdit при смене вкладки редактора
function syncLineupEdit() {
  if (editTeamSide.value === 'home') {
    homeLineupEdit.value = (match.value?.lineup ?? []).map((e: any) => ({
      sportsmanId: e.sportsmanId,
      position: e.position,
      type: e.type ?? 'Main',
    }))
  } else {
    awayLineupEdit.value = (match.value?.awayLineup ?? []).map((e: any) => ({
      sportsmanId: e.sportsmanId,
      position: e.position,
      type: e.type ?? 'Main',
    }))
  }
}

function openLineupEditor() {
  showFieldEdit.value = !showFieldEdit.value
  if (showFieldEdit.value) {
    editTeamSide.value = 'home'
    syncLineupEdit()
  }
}

// Кэш всех спортсменов тренера
const allSportsmensCache = ref<any[]>([])

async function applyTeamSportsmen(homeTeamId: number, awayTeamId?: number | null) {
  const loadTeam = async (teamId: number): Promise<any[]> => {
    const res = await api.get('/sportsman', { params: { filters: { teamId: [teamId] } } }).catch(() => null)
    return res?.data?.data ?? []
  }

  const homeList = await loadTeam(homeTeamId)
  homeSportsmen.value = homeList.map((s: any) => ({ id: s.id, fio: s.fio, position: s.position ?? null }))
  homeList.forEach((s: any) => { if (s.id && s.fio) sportsmenMap[s.id] = s.fio })

  if (awayTeamId) {
    const awayList = await loadTeam(awayTeamId)
    awaySportsmen.value = awayList.map((s: any) => ({ id: s.id, fio: s.fio, position: s.position ?? null }))
    awayList.forEach((s: any) => { if (s.id && s.fio) sportsmenMap[s.id] = s.fio })
  }
}

async function loadScheduled() {
  loadingScheduled.value = true
  const tid = auth.personalId
  const matchFilter = tid
    ? { filters: { trainerId: [tid] } }
    : undefined
  const [mRes, tRes, sRes, gRes, allTRes] = await Promise.allSettled([
    api.get('/match', { params: matchFilter }),
    api.get('/team', { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
    api.get('/sportsman'),
    api.get('/group', { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
    // Все команды академии — для списка соперников при создании матча.
    api.get('/team'),
  ])

  if (tRes.status === 'fulfilled') myTeams.value = tRes.value.data.data ?? []
  if (allTRes.status === 'fulfilled') allTeams.value = allTRes.value.data.data ?? []
  if (gRes.status === 'fulfilled') myGroups.value = gRes.value.data.data ?? []
  if (sRes.status === 'fulfilled') {
    allSportsmensCache.value = sRes.value.data.data ?? []
    allSportsmensCache.value.forEach((s: any) => {
      if (s.id && s.fio) sportsmenMap[s.id] = s.fio
    })
  }

  if (mRes.status === 'fulfilled') {
    const all = mRes.value.data.data ?? []
    // Если есть идущий матч — сразу переходим в live
    const inProgress = all.find((m: any) => m.status === 'InProgress')
    if (inProgress) {
      loadingScheduled.value = false
      await selectMatch(inProgress)
      return
    }
    scheduledMatches.value = all.filter((m: any) => m.status === 'Scheduled')
  }

  loadingScheduled.value = false
}

async function loadTeamImages(homeId: number, awayId?: number | null) {
  const loadOne = async (id: number, target: 'home' | 'away') => {
    const t = await api.get(`/team/${id}`).catch(() => null)
    const url = imageUrl(t?.data?.data?.images)
    if (url) {
      if (target === 'home') homeTeamImage.value = url
      else awayTeamImage.value = url
    }
  }
  const tasks = [loadOne(homeId, 'home')]
  if (awayId) tasks.push(loadOne(awayId, 'away'))
  await Promise.allSettled(tasks)
}

async function loadEventSportsmenNames(events: any[]) {
  const ids = [...new Set(
    events.flatMap((e: any) => [e.sportsmanId, e.substituteSportsmanId].filter(Boolean))
  )] as number[]
  const unknown = ids.filter(id => !sportsmenMap[id])
  await Promise.allSettled(unknown.map(async (sid: number) => {
    const s = await api.get(`/sportsman/${sid}`).catch(() => null)
    const fio = s?.data?.data?.fio
    if (fio) sportsmenMap[sid] = fio
  }))
}

// Строим awayLineupEdit из спортсменов гостевой команды (только для Home-матча, локально — нет endpoint)
function initAwayLineupEdit() {
  if (!awaySportsmen.value.length) { awayLineupEdit.value = []; return }
  const fallbackPositions = ['GK', 'CB', 'CB2', 'LB', 'RB', 'CDM', 'CM', 'CM2', 'LW', 'RW', 'ST']
  awayLineupEdit.value = awaySportsmen.value.map((s, i) => ({
    sportsmanId: s.id,
    position: s.position ?? fallbackPositions[i % fallbackPositions.length],
    type: i < 11 ? 'Main' : 'Reserve',
  }))
  // Синхронизируем в match.value для отображения поля
  if (match.value) {
    match.value = { ...match.value, awayLineup: awayLineupEdit.value }
  }
}


async function selectMatch(m: any) {
  homeTeamImage.value = ''
  awayTeamImage.value = ''
  stopTimer()
  timerSeconds.value = 0

  const full = await api.get(`/match/${m.id}`).catch(() => null)
  const fullMatch = full?.data?.data ?? m

  const awayTeamId = fullMatch.type === 'Home' ? fullMatch.opponentTeamId : null
  await applyTeamSportsmen(fullMatch.homeTeamId, awayTeamId)
  await loadTeamImages(fullMatch.homeTeamId, fullMatch.opponentTeamId)

  if (fullMatch.status === 'InProgress') {
    // Уже идёт — сразу в live
    match.value = { ...fullMatch }
    liveEvents.value = [...(fullMatch.events ?? [])]
    await loadEventSportsmenNames(liveEvents.value)

    homeLineupEdit.value = (fullMatch.lineup ?? []).map((e: any) => ({
      sportsmanId: e.sportsmanId, position: e.position, type: e.type ?? 'Main',
    }))
    if (fullMatch.type === 'Home') initAwayLineupEdit()

    lineupConfirmed.value = true
    startTimer(fullMatch.startedAt ?? fullMatch.date)
    step.value = 'live'
  } else {
    // Scheduled — идём через экран состава
    match.value = { ...fullMatch }
    liveEvents.value = []

    // Заполняем состав: если уже есть на беке — берём его, иначе строим локально
    if (fullMatch.lineup?.length) {
      homeLineupEdit.value = fullMatch.lineup.map((e: any) => ({
        sportsmanId: e.sportsmanId, position: e.position, type: e.type ?? 'Main',
      }))
    } else {
      buildLocalLineup(fullMatch.homeTeamId, awayTeamId)
    }
    if (fullMatch.type === 'Home') initAwayLineupEdit()

    lineupConfirmed.value = false
    // Подменяем createForm чтобы confirmAndStart знал что делать
    createForm.value = {
      homeTeamId: fullMatch.homeTeamId,
      opponentTeamName: fullMatch.opponentTeamName ?? '',
      opponentTeamId: fullMatch.opponentTeamId ?? '',
      type: fullMatch.type,
      date: fullMatch.date,
    }
    // Для Scheduled сохраняем id чтобы не создавать новый матч
    pendingMatchId.value = fullMatch.id
    step.value = 'lineup'
  }
}

// Переход на шаг выбора состава (без запросов к беку)
async function goToLineup() {
  if (!createForm.value.homeTeamId || !createForm.value.date) return
  homeTeamImage.value = ''
  awayTeamImage.value = ''
  // Подгружаем спортсменов выбранной команды локально
  const homeTeamId = Number(createForm.value.homeTeamId)
  const awayTeamId = createForm.value.type === 'Home' ? Number(createForm.value.opponentTeamId) || null : null
  await applyTeamSportsmen(homeTeamId, awayTeamId)
  loadTeamImages(homeTeamId, awayTeamId)
  // Строим стартовый состав локально
  buildLocalLineup(homeTeamId, awayTeamId)
  lineupConfirmed.value = false
  step.value = 'lineup'
}

function buildLocalLineup(_homeTeamId: number, awayTeamId?: number | null) {
  // Первые 11 — основа, остальные — запас. Позиция: из БД, если нет — fallback по индексу.
  const fallback = ['GK', 'CB', 'CB2', 'LB', 'RB', 'CDM', 'CM', 'CM2', 'LW', 'RW', 'ST']

  homeLineupEdit.value = homeSportsmen.value.map((s: any, i: number) => ({
    sportsmanId: s.id,
    position: s.position ?? fallback[i % fallback.length],
    type: i < 11 ? 'Main' : 'Reserve',
  }))

  if (awayTeamId) {
    awayLineupEdit.value = awaySportsmen.value.map((s: any, i: number) => ({
      sportsmanId: s.id,
      position: s.position ?? fallback[i % fallback.length],
      type: i < 11 ? 'Main' : 'Reserve',
    }))
  } else {
    awayLineupEdit.value = []
  }
}

// Создаёт матч на беке (или берёт существующий), сохраняет состав и стартует
async function confirmAndStart() {
  startingMatch.value = true
  try {
    let matchId = pendingMatchId.value

    if (!matchId) {
      // Новый матч — создаём
      const payload: any = {
        homeTeamId: Number(createForm.value.homeTeamId),
        type: createForm.value.type,
        date: new Date(createForm.value.date).toISOString(),
      }
      if (createForm.value.type === 'Home') {
        if (createForm.value.opponentTeamId) payload.opponentTeamId = Number(createForm.value.opponentTeamId)
      } else {
        if (createForm.value.opponentTeamName) payload.opponentTeamName = createForm.value.opponentTeamName
      }
      // Бэк автоматически создаст Training (Type="Матч") для каждой команды матча (привязка через TeamId)
      const res = await api.post('/match', payload)
      const created = res.data.data
      if (!created) return
      matchId = created.id
      match.value = { ...created, events: [], lineup: [] }
    }

    // Сохраняем состав домашней команды
    const lineupPayload = homeLineupEdit.value
      .filter(e => e.sportsmanId)
      .map(e => ({ sportsmanId: Number(e.sportsmanId), position: e.position, type: e.type }))
    await api.put(`/match/${matchId}/lineup`, lineupPayload).catch(() => null)

    // Стартуем матч
    const startRes = await api.put(`/match/${matchId}/start`)
    const startedData = startRes?.data?.data

    match.value = {
      ...match.value,
      status: 'InProgress',
      startedAt: startedData?.startedAt,
      lineup: lineupPayload,
    }
    if (createForm.value.type === 'Home') {
      match.value = { ...match.value, awayLineup: awayLineupEdit.value }
    }

    liveEvents.value = []
    lineupConfirmed.value = true
    pendingMatchId.value = null

    stopTimer()
    startTimer(startedData?.startedAt ?? undefined)
    step.value = 'live'
  } catch (err: any) {
    alert('Ошибка запуска матча: ' + (err.response?.data?.message || err.message))
  } finally {
    startingMatch.value = false
  }
}


function openAddEvent(type: string) {
  playerSearch.value = ''
  substituteSearch.value = ''
  playerSelectOpen.value = false
  substituteSelectOpen.value = false
  eventModal.value = {
    open: true,
    editId: null,
    form: { type, isHomeTeam: true, minute: currentMinute.value, sportsmanId: '', substituteSportsmanId: '', comment: '' },
  }
}

function openEditEvent(ev: any) {
  playerSearch.value = ''
  substituteSearch.value = ''
  playerSelectOpen.value = false
  substituteSelectOpen.value = false
  eventModal.value = {
    open: true,
    editId: ev.id,
    form: {
      type: ev.type,
      isHomeTeam: ev.isHomeTeam,
      minute: ev.minute,
      sportsmanId: ev.sportsmanId ?? '',
      substituteSportsmanId: ev.substituteSportsmanId ?? '',
      comment: ev.comment ?? '',
    },
  }
}

async function submitEvent() {
  if (!match.value) return
  submittingEvent.value = true
  try {
    const f = eventModal.value.form
    const payload: any = { type: f.type, isHomeTeam: f.isHomeTeam, minute: Number(f.minute) }
    if (f.comment) payload.comment = f.comment
    if (f.sportsmanId) payload.sportsmanId = Number(f.sportsmanId)
    if (f.substituteSportsmanId && f.type === 'Substitution') payload.substituteSportsmanId = Number(f.substituteSportsmanId)

    if (eventModal.value.editId) {
      await api.delete(`/match/event/${eventModal.value.editId}`)
    }
    await api.post(`/match/${match.value.id}/event`, payload)

    const res = await api.get(`/match/${match.value.id}/events`).catch(() => null)
    if (res?.data?.data) {
      liveEvents.value = res.data.data
      await loadEventSportsmenNames(liveEvents.value)
    }
    eventModal.value.open = false
  } finally {
    submittingEvent.value = false
  }
}

async function togglePause() {
  if (!match.value) return
  if (isPaused.value) {
    isPaused.value = false
    if (timerInterval) return
    timerInterval = setInterval(() => { timerSeconds.value++ }, 1000)
    await api.post(`/match/${match.value.id}/event`, {
      type: 'Resume', isHomeTeam: true, minute: currentMinute.value,
    }).catch(() => null)
    const res = await api.get(`/match/${match.value.id}/events`).catch(() => null)
    if (res?.data?.data) { liveEvents.value = res.data.data; await loadEventSportsmenNames(liveEvents.value) }
  } else {
    isPaused.value = true
    if (timerInterval) { clearInterval(timerInterval); timerInterval = null }
    await api.post(`/match/${match.value.id}/event`, {
      type: 'Pause', isHomeTeam: true, minute: currentMinute.value,
    }).catch(() => null)
    const res = await api.get(`/match/${match.value.id}/events`).catch(() => null)
    if (res?.data?.data) { liveEvents.value = res.data.data; await loadEventSportsmenNames(liveEvents.value) }
  }
}

async function endFirstHalf() {
  if (!match.value || matchPhase.value !== 'first') return
  matchPhase.value = 'break'
  isPaused.value = true
  if (timerInterval) { clearInterval(timerInterval); timerInterval = null }
  await api.post(`/match/${match.value.id}/event`, {
    type: 'HalfTimeEnd', isHomeTeam: true, minute: currentMinute.value,
  }).catch(() => null)
  const res = await api.get(`/match/${match.value.id}/events`).catch(() => null)
  if (res?.data?.data) { liveEvents.value = res.data.data; await loadEventSportsmenNames(liveEvents.value) }
}

async function startSecondHalf() {
  if (!match.value || matchPhase.value !== 'break') return
  matchPhase.value = 'second'
  isPaused.value = false
  if (!timerInterval) timerInterval = setInterval(() => { timerSeconds.value++ }, 1000)
  await api.post(`/match/${match.value.id}/event`, {
    type: 'SecondHalfStart', isHomeTeam: true, minute: currentMinute.value,
  }).catch(() => null)
  const res = await api.get(`/match/${match.value.id}/events`).catch(() => null)
  if (res?.data?.data) { liveEvents.value = res.data.data; await loadEventSportsmenNames(liveEvents.value) }
}

async function deleteEvent(eventId: number) {
  await api.delete(`/match/event/${eventId}`)
  liveEvents.value = liveEvents.value.filter(e => e.id !== eventId)
}

async function cancelMatch() {
  if (!match.value) return
  await api.delete(`/match/${match.value.id}`).catch(() => null)
  stopTimer()
  showCancelConfirm.value = false
  router.push('/trainer/matches')
}

async function finishMatch() {
  if (!match.value || !finishForm.value.result) return
  finishingMatch.value = true
  try {
    const payload: any = { result: finishForm.value.result }
    if (finishForm.value.trainerComment) payload.trainerComment = finishForm.value.trainerComment
    await api.put(`/match/${match.value.id}/finish`, payload)
    stopTimer()
    router.push(`/trainer/matches/${match.value.id}`)
  } finally {
    finishingMatch.value = false
  }
}

onMounted(loadScheduled)
onUnmounted(stopTimer)
</script>
