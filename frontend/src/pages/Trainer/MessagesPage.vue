<template>
  <div class="p-3 h-full">
  <TrainerPageCard color="sky" class="h-full flex flex-col p-0 overflow-hidden">
  <div class="flex h-full overflow-hidden bg-neutral-50 rounded-2xl">

    <!-- Левая панель -->
    <aside class="w-72 bg-white border-r border-neutral-200 flex flex-col shrink-0">

      <!-- Вкладки -->
      <div class="flex border-b border-neutral-200">
        <button
          v-for="tab in tabs" :key="tab.value"
          @click="activeTab = tab.value"
          class="flex-1 py-3 text-xs font-semibold transition-colors"
          :class="activeTab === tab.value
            ? 'text-blue-600 border-b-2 border-blue-500 bg-blue-50/50'
            : 'text-neutral-500 hover:text-neutral-700'"
        >{{ tab.label }}</button>
      </div>

      <!-- Поиск + кнопка создать -->
      <div class="p-3 flex gap-2 border-b border-neutral-100">
        <input
          v-model="search"
          placeholder="Поиск..."
          class="flex-1 px-3 py-1.5 text-xs rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50"
        />
        <button
          v-if="activeTab === 'chats'"
          @click="openNewChat"
          class="w-8 h-8 rounded-xl bg-blue-500 flex items-center justify-center hover:bg-blue-600 transition-colors shrink-0"
          title="Новый чат"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4 text-white">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
          </svg>
        </button>
        <button
          v-else
          @click="openNewBroadcast"
          class="w-8 h-8 rounded-xl bg-green-500 flex items-center justify-center hover:bg-green-600 transition-colors shrink-0"
          title="Новая рассылка"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4 text-white">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
          </svg>
        </button>
      </div>

      <!-- Фильтр Мои/Все (только для рассылок) -->
      <div v-if="activeTab === 'broadcasts'" class="px-3 py-2 border-b border-neutral-100 flex gap-1">
        <button
          v-for="f in broadcastFilters" :key="f.value"
          @click="broadcastFilter = f.value"
          class="flex-1 py-1 text-xs font-semibold rounded-lg transition-colors"
          :class="broadcastFilter === f.value ? 'bg-blue-500 text-white' : 'text-neutral-500 hover:bg-neutral-100'"
        >{{ f.label }}</button>
      </div>

      <!-- Сортировка -->
      <div class="px-3 py-2 border-b border-neutral-100 flex items-center gap-2">
        <span class="text-[10px] text-neutral-400 uppercase tracking-wide">Сортировка</span>
        <select v-model="sortBy" class="flex-1 text-xs rounded-lg border border-neutral-200 px-2 py-1 focus:outline-none bg-neutral-50">
          <option value="date">По дате</option>
          <option value="name">По имени</option>
          <option value="unread">По непрочитанным</option>
        </select>
        <button @click="sortDir = sortDir === 'asc' ? 'desc' : 'asc'" class="text-neutral-400 hover:text-neutral-600">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5" :class="sortDir === 'desc' ? 'rotate-180' : ''">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3 4h13M3 8h9M3 12h5"/>
          </svg>
        </button>
      </div>

      <!-- Список чатов -->
      <div v-if="activeTab === 'chats'" class="flex-1 overflow-y-auto">
        <div v-if="usersLoading" class="p-6 text-center text-xs text-neutral-400">Загрузка...</div>
        <div v-else-if="filteredUsers.length === 0" class="p-6 text-center text-xs text-neutral-400">Нет диалогов</div>
        <div
          v-for="user in filteredUsers" :key="user.id"
          @click="selectUser(user)"
          class="flex items-center gap-3 px-4 py-3 cursor-pointer hover:bg-neutral-50 transition-colors border-b border-neutral-50"
          :class="selectedUser?.id === user.id ? 'bg-blue-50 border-r-2 border-r-blue-500' : ''"
        >
          <div class="w-9 h-9 rounded-full flex items-center justify-center text-white font-bold text-sm shrink-0"
            :class="roleColor(user.role)">
            {{ initials(user.login) }}
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-semibold text-neutral-800 truncate">{{ user.login }}</div>
            <div class="text-xs text-neutral-400 truncate">{{ roleLabel(user.role) }}</div>
          </div>
          <span v-if="unread[user.id]" class="bg-blue-500 text-white text-[10px] font-bold rounded-full min-w-[18px] h-[18px] flex items-center justify-center px-1 shrink-0">
            {{ unread[user.id] > 99 ? '99+' : unread[user.id] }}
          </span>
        </div>
      </div>

      <!-- Список рассылок -->
      <div v-else class="flex-1 overflow-y-auto">
        <div v-if="broadcastsLoading" class="p-6 text-center text-xs text-neutral-400">Загрузка...</div>
        <div v-else-if="filteredBroadcasts.length === 0" class="p-6 text-center text-xs text-neutral-400">Нет рассылок</div>
        <div
          v-for="b in filteredBroadcasts" :key="b.id"
          @click="selectBroadcast(b)"
          class="flex items-start gap-3 px-4 py-3 cursor-pointer hover:bg-neutral-50 transition-colors border-b border-neutral-50"
          :class="selectedBroadcast?.id === b.id ? 'bg-green-50 border-r-2 border-r-green-500' : ''"
        >
          <div class="w-9 h-9 rounded-xl bg-green-100 flex items-center justify-center shrink-0 mt-0.5">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-green-600">
              <path stroke-linecap="round" stroke-linejoin="round" d="M11 5.882V19.24a1.76 1.76 0 01-3.417.592l-2.147-6.15M18 13a3 3 0 100-6M5.436 13.683A4.001 4.001 0 017 6h1.832c4.1 0 7.625-1.234 9.168-3v14c-1.543-1.766-5.067-3-9.168-3H7a3.988 3.988 0 01-1.564-.317z"/>
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-1">
              <span class="text-sm font-semibold text-neutral-800 truncate flex-1">{{ b.title }}</span>
              <span v-if="b.createdById === auth.userId" class="text-[10px] text-blue-600 font-semibold bg-blue-50 px-1.5 py-0.5 rounded-full shrink-0">Моя</span>
            </div>
            <div class="text-xs text-neutral-400 truncate">{{ b.text.slice(0, 60) }}</div>
            <div class="text-[10px] text-neutral-300 mt-0.5">{{ formatRelative(b.createdAt) }} · {{ b.recipientsCount }} получ.</div>
          </div>
        </div>
      </div>
    </aside>

    <!-- Основная область -->
    <div class="flex-1 flex flex-col overflow-hidden">

      <!-- Чат с пользователем -->
      <div v-if="activeTab === 'chats' && selectedUser" class="flex flex-col flex-1 min-h-0">
        <div class="px-5 py-3 bg-white border-b border-neutral-200 flex items-center gap-3 shrink-0">
          <div class="w-9 h-9 rounded-full flex items-center justify-center text-white font-bold text-sm shrink-0"
            :class="roleColor(selectedUser.role)">
            {{ initials(selectedUser.login) }}
          </div>
          <div>
            <div class="font-semibold text-neutral-900 text-sm">{{ selectedUser.login }}</div>
            <div class="text-xs text-neutral-400">{{ roleLabel(selectedUser.role) }}</div>
          </div>
        </div>

        <div ref="messagesEl" class="flex-1 overflow-y-auto p-4 space-y-2">
          <div v-if="dialogLoading" class="text-center text-xs text-neutral-400 py-8">Загрузка...</div>
          <template v-else>
            <div
              v-for="msg in messages" :key="msg.id"
              class="flex"
              :class="msg.senderId === myId ? 'justify-end' : 'justify-start'"
            >
              <div
                class="max-w-xs lg:max-w-md px-4 py-2.5 rounded-2xl text-sm shadow-sm"
                :class="msg.senderId === myId
                  ? 'bg-blue-500 text-white rounded-br-sm'
                  : 'bg-white text-neutral-900 rounded-bl-sm'"
              >
                <p class="leading-relaxed">{{ msg.text }}</p>
                <div class="flex items-center gap-1 mt-1" :class="msg.senderId === myId ? 'justify-end' : 'justify-start'">
                  <span class="text-[10px] opacity-60">{{ formatTime(msg.createdAt) }}</span>
                  <span v-if="msg.senderId === myId" class="text-[10px] opacity-60">{{ msg.isRead ? '✓✓' : '✓' }}</span>
                </div>
              </div>
            </div>
          </template>
        </div>

        <div class="px-4 py-3 bg-white border-t border-neutral-200 flex gap-2 shrink-0">
          <input
            v-model="newText"
            @keydown.enter.prevent="sendMessage"
            placeholder="Написать сообщение..."
            class="flex-1 px-4 py-2.5 text-sm border border-neutral-200 rounded-full focus:outline-none focus:border-blue-400"
          />
          <button
            @click="sendMessage"
            :disabled="!newText.trim() || sending"
            class="px-5 py-2.5 bg-blue-500 text-white text-sm font-semibold rounded-full hover:bg-blue-600 disabled:opacity-40 transition-colors"
          >Отправить</button>
        </div>
      </div>

      <!-- Детали рассылки -->
      <div v-else-if="activeTab === 'broadcasts' && selectedBroadcast" class="flex flex-col flex-1 min-h-0">
        <div class="px-5 py-3 bg-white border-b border-neutral-200 flex items-center justify-between shrink-0">
          <div class="flex items-center gap-3">
            <div class="w-9 h-9 rounded-xl bg-green-100 flex items-center justify-center shrink-0">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-green-600">
                <path stroke-linecap="round" stroke-linejoin="round" d="M11 5.882V19.24a1.76 1.76 0 01-3.417.592l-2.147-6.15M18 13a3 3 0 100-6M5.436 13.683A4.001 4.001 0 017 6h1.832c4.1 0 7.625-1.234 9.168-3v14c-1.543-1.766-5.067-3-9.168-3H7a3.988 3.988 0 01-1.564-.317z"/>
              </svg>
            </div>
            <div>
              <div class="font-semibold text-neutral-900 text-sm">{{ selectedBroadcast.title }}</div>
              <div class="text-xs text-neutral-400">{{ formatDate(selectedBroadcast.createdAt) }} · {{ selectedBroadcast.recipientsCount }} получателей</div>
            </div>
          </div>
          <button
            v-if="selectedBroadcast.createdById === auth.userId"
            @click="deleteBroadcast(selectedBroadcast)"
            class="w-8 h-8 rounded-xl flex items-center justify-center text-neutral-400 hover:text-red-500 hover:bg-red-50 transition-colors"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
            </svg>
          </button>
        </div>

        <div class="flex-1 overflow-y-auto p-5">
          <div class="bg-white rounded-2xl border border-neutral-100 p-5 max-w-2xl">
            <div class="text-xs font-semibold text-neutral-400 uppercase tracking-wide mb-2">Содержание</div>
            <p class="text-sm text-neutral-800 leading-relaxed whitespace-pre-wrap">{{ selectedBroadcast.text }}</p>
            <div v-if="selectedBroadcast.expireAt" class="mt-3 text-xs text-neutral-400">
              Истекает: {{ formatDate(selectedBroadcast.expireAt) }}
            </div>
          </div>

          <div class="mt-4 bg-white rounded-2xl border border-neutral-100 p-4">
            <div class="text-xs font-semibold text-neutral-400 uppercase tracking-wide mb-3">Статус прочтения</div>
            <div v-if="broadcastDetails" class="space-y-1.5">
              <div v-for="r in broadcastDetails.recipients" :key="r.userId"
                class="flex items-center justify-between px-3 py-2 rounded-xl bg-neutral-50">
                <span class="text-xs text-neutral-700">{{ r.userName ? `${r.userName} (ID ${r.userId})` : `ID ${r.userId}` }}</span>
                <span class="text-xs font-semibold" :class="r.isRead ? 'text-green-600' : 'text-neutral-400'">
                  {{ r.isRead ? 'Прочитано' : 'Не прочитано' }}
                </span>
              </div>
            </div>
            <div v-else class="text-xs text-neutral-400 text-center py-4">Загрузка...</div>
          </div>
        </div>
      </div>

      <!-- Пусто -->
      <div v-else class="flex-1 flex flex-col items-center justify-center gap-3 text-neutral-300">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5" class="w-14 h-14">
          <path stroke-linecap="round" stroke-linejoin="round" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-4 4-1-4z"/>
        </svg>
        <span class="text-sm">{{ activeTab === 'chats' ? 'Выберите диалог' : 'Выберите рассылку' }}</span>
      </div>
    </div>
  </div>

  <!-- Модалка нового чата -->
  <div v-if="newChatModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/30 backdrop-blur-sm p-4">
    <div class="bg-white rounded-2xl shadow-xl w-full max-w-sm flex flex-col max-h-[85vh]">
      <div class="flex items-center justify-between px-4 pt-4 pb-3 border-b border-neutral-100">
        <span class="text-sm font-semibold text-neutral-800">
          {{ newChatStep === 1 ? 'Новый чат — выбор получателя' : 'Новый чат — сообщение' }}
        </span>
        <button @click="newChatModal = false" class="text-neutral-400 hover:text-neutral-600">✕</button>
      </div>

      <!-- Шаг 1: выбор получателя -->
      <template v-if="newChatStep === 1">
        <!-- Фильтр группа/команда -->
        <div class="px-4 pt-3 pb-2 space-y-2">
          <select v-model="newChatGroupKey" @change="newChatSearch = ''" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50">
            <option value="">Все мои спортсмены</option>
            <optgroup v-if="myGroups.length" label="Группы">
              <option v-for="g in myGroups" :key="'g-' + g.id" :value="'g-' + g.id">{{ g.name }}</option>
            </optgroup>
            <optgroup v-if="myTeams.length" label="Команды">
              <option v-for="t in myTeams" :key="'t-' + t.id" :value="'t-' + t.id">{{ t.name }}</option>
            </optgroup>
          </select>
          <input v-model="newChatSearch" placeholder="Поиск по имени..." class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50"/>
        </div>
        <div class="overflow-y-auto flex-1 divide-y divide-neutral-50 px-2 pb-2">
          <div
            v-for="u in newChatUsers" :key="u.id"
            @click="selectNewChatUser(u)"
            class="flex items-center gap-3 px-3 py-2.5 rounded-xl cursor-pointer hover:bg-neutral-50 transition-colors"
            :class="newChatSelectedUser?.id === u.id ? 'bg-blue-50' : ''"
          >
            <div class="w-8 h-8 rounded-full flex items-center justify-center text-white text-xs font-bold shrink-0" :class="roleColor(u.role)">
              {{ initials(u.login) }}
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm font-medium text-neutral-800 truncate">{{ u.login }}</div>
              <div class="text-xs text-neutral-400">{{ roleLabel(u.role) }}</div>
            </div>
            <svg v-if="newChatSelectedUser?.id === u.id" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4 text-blue-500 shrink-0">
              <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7"/>
            </svg>
          </div>
          <div v-if="newChatUsers.length === 0" class="px-4 py-8 text-center text-xs text-neutral-400">Нет спортсменов</div>
        </div>
        <div class="p-4 border-t border-neutral-100">
          <button @click="newChatStep = 2" :disabled="!newChatSelectedUser"
            class="w-full py-2.5 rounded-xl text-sm font-semibold transition-colors"
            :class="newChatSelectedUser ? 'bg-blue-500 text-white hover:bg-blue-600' : 'bg-neutral-100 text-neutral-400 cursor-not-allowed'">
            Далее →
          </button>
        </div>
      </template>

      <!-- Шаг 2: текст сообщения -->
      <template v-else>
        <div class="px-4 py-3 border-b border-neutral-100 flex items-center gap-2">
          <div class="w-8 h-8 rounded-full flex items-center justify-center text-white text-xs font-bold shrink-0" :class="roleColor(newChatSelectedUser?.role ?? '')">
            {{ initials(newChatSelectedUser?.login ?? '') }}
          </div>
          <span class="text-sm font-semibold text-neutral-800">{{ newChatSelectedUser?.login }}</span>
        </div>
        <div class="px-4 py-3 flex-1">
          <textarea v-model="newChatText" rows="5" placeholder="Введите сообщение..."
            class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 resize-none"/>
        </div>
        <div class="px-4 pb-4 flex gap-2">
          <button @click="newChatStep = 1" class="px-4 py-2.5 rounded-xl border border-neutral-200 text-sm font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">← Назад</button>
          <button @click="startChatWithMessage" :disabled="!newChatText.trim() || sendingNewChat"
            class="flex-1 py-2.5 rounded-xl text-sm font-semibold transition-colors"
            :class="newChatText.trim() && !sendingNewChat ? 'bg-blue-500 text-white hover:bg-blue-600' : 'bg-neutral-100 text-neutral-400 cursor-not-allowed'">
            {{ sendingNewChat ? 'Отправка...' : 'Отправить' }}
          </button>
        </div>
      </template>
    </div>
  </div>

  <!-- Модалка новой рассылки -->
  <div v-if="broadcastModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/30 backdrop-blur-sm p-4">
    <div class="bg-white rounded-2xl shadow-xl w-full max-w-md flex flex-col">
      <div class="flex items-center justify-between px-5 pt-5 pb-4 border-b border-neutral-100">
        <span class="text-sm font-semibold text-neutral-800">Новая рассылка</span>
        <button @click="broadcastModal = false" class="text-neutral-400 hover:text-neutral-600">✕</button>
      </div>
      <div class="px-5 py-4 space-y-3">
        <div>
          <label class="block text-xs font-semibold text-neutral-500 mb-1">Заголовок</label>
          <input v-model="broadcastForm.title" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" placeholder="Заголовок рассылки"/>
        </div>
        <div>
          <label class="block text-xs font-semibold text-neutral-500 mb-1">Кому</label>
          <div class="grid grid-cols-2 gap-2">
            <button
              v-for="t in targetTypes" :key="t.value"
              @click="broadcastForm.targetType = t.value; broadcastForm.targetId = undefined"
              class="py-2 text-xs font-semibold rounded-xl border transition-colors"
              :class="broadcastForm.targetType === t.value ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-600 hover:border-neutral-300'"
            >{{ t.label }}</button>
          </div>
        </div>
        <div v-if="broadcastForm.targetType === 'Group'">
          <label class="block text-xs font-semibold text-neutral-500 mb-1">Группа</label>
          <select v-model="broadcastForm.targetId" class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2 focus:outline-none focus:border-blue-400">
            <option :value="undefined">Выберите группу</option>
            <option v-for="g in myGroups" :key="g.id" :value="g.id">{{ g.name }}</option>
          </select>
        </div>
        <div v-else-if="broadcastForm.targetType === 'Team'">
          <label class="block text-xs font-semibold text-neutral-500 mb-1">Команда</label>
          <select v-model="broadcastForm.targetId" class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2 focus:outline-none focus:border-blue-400">
            <option :value="undefined">Выберите команду</option>
            <option v-for="t in myTeams" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
        </div>
        <div v-else-if="broadcastForm.targetType === 'Individual'">
          <label class="block text-xs font-semibold text-neutral-500 mb-1">Спортсмен</label>
          <input v-model="individualSearch" placeholder="Поиск спортсмена..." class="w-full px-3 py-1.5 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 mb-2"/>
          <div class="max-h-32 overflow-y-auto rounded-xl border border-neutral-200 divide-y divide-neutral-50">
            <button type="button" v-for="s in filteredIndividuals" :key="s.id"
              @click="broadcastForm.targetId = s.id"
              class="w-full text-left px-3 py-2 text-xs transition-colors"
              :class="broadcastForm.targetId === s.id ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-neutral-700 hover:bg-neutral-50'">
              {{ s.fio }}
            </button>
          </div>
        </div>
        <div>
          <label class="block text-xs font-semibold text-neutral-500 mb-1">Текст сообщения</label>
          <textarea v-model="broadcastForm.text" rows="4" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 resize-none" placeholder="Введите сообщение..."/>
        </div>
      </div>
      <div class="px-5 pb-5 flex gap-2 border-t border-neutral-100 pt-4">
        <button @click="broadcastModal = false" class="flex-1 py-2.5 rounded-xl border border-neutral-200 text-sm font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">Отмена</button>
        <button @click="sendBroadcast" :disabled="!broadcastForm.title || !broadcastForm.text || sendingBroadcast"
          class="flex-1 py-2.5 rounded-xl bg-green-600 text-white text-sm font-semibold hover:bg-green-700 disabled:opacity-50 transition-colors">
          {{ sendingBroadcast ? 'Отправка...' : 'Отправить' }}
        </button>
      </div>
    </div>
  </div>
  </TrainerPageCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/store/auth'
import { useSignalR } from '@/composables/useSignalR'
import { useNotifications } from '@/composables/useNotifications'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'
import type { MessageResponse } from '@/types/message/dto'
import type { BroadcastResponse, BroadcastDetailsResponse } from '@/types/broadcast/dto'
import type { BroadcastTargetType } from '@/types/broadcast/model'

const auth = useAuthStore()
const { start, stop, on, off } = useSignalR()
const { markMessageReadBySender } = useNotifications()

const myId = computed(() => auth.userId)

// Tabs
const tabs: { value: 'chats' | 'broadcasts'; label: string }[] = [
  { value: 'chats', label: 'Чаты' },
  { value: 'broadcasts', label: 'Рассылки' },
]
const activeTab = ref<'chats' | 'broadcasts'>('chats')

// Search / sort
const search = ref('')
const sortBy = ref<'date' | 'name' | 'unread'>('date')
const sortDir = ref<'asc' | 'desc'>('desc')
const broadcastFilter = ref<'all' | 'mine'>('all')
const broadcastFilters: { value: 'all' | 'mine'; label: string }[] = [
  { value: 'all', label: 'Все' },
  { value: 'mine', label: 'Мои' },
]

// Users / chats
const users = ref<any[]>([])
const usersLoading = ref(false)
const selectedUser = ref<any>(null)
const messages = ref<MessageResponse[]>([])
const dialogLoading = ref(false)
const newText = ref('')
const sending = ref(false)
const messagesEl = ref<HTMLElement | null>(null)
const unread = ref<Record<number, number>>({})

// Broadcasts
const broadcasts = ref<BroadcastResponse[]>([])
const broadcastsLoading = ref(false)
const selectedBroadcast = ref<BroadcastResponse | null>(null)
const broadcastDetails = ref<BroadcastDetailsResponse | null>(null)
const sportsmenMap = ref<Record<number, string>>({})

// Groups/Teams for broadcast
const myGroups = ref<any[]>([])
const myTeams = ref<any[]>([])
const allSportsmen = ref<any[]>([])

// Groups/Teams sportsmen cache
const sportsmenByGroup = ref<Record<number, any[]>>({})
const sportsmenByTeam  = ref<Record<number, any[]>>({})

// Modals
const newChatModal = ref(false)
const newChatSearch = ref('')
const newChatStep = ref<1 | 2>(1)
const newChatGroupKey = ref('')
const newChatSelectedUser = ref<any>(null)
const newChatText = ref('')
const sendingNewChat = ref(false)
const broadcastModal = ref(false)
const sendingBroadcast = ref(false)
const individualSearch = ref('')
const broadcastForm = ref<{
  title: string
  text: string
  targetType: BroadcastTargetType
  targetId: number | undefined
}>({ title: '', text: '', targetType: 'All', targetId: undefined })

const targetTypes = [
  { value: 'All' as BroadcastTargetType, label: 'Все спортсмены' },
  { value: 'Group' as BroadcastTargetType, label: 'Группа' },
  { value: 'Team' as BroadcastTargetType, label: 'Команда' },
]

function roleLabel(role: string): string {
  const map: Record<string, string> = { admin: 'Администратор', personal: 'Сотрудник', sportsman: 'Спортсмен', trainer: 'Тренер', medical: 'Медик' }
  return map[role?.toLowerCase()] ?? role
}

function roleColor(role: string): string {
  const map: Record<string, string> = { admin: 'bg-red-400', personal: 'bg-blue-500', trainer: 'bg-green-500', medical: 'bg-teal-500', sportsman: 'bg-neutral-400' }
  return map[role?.toLowerCase()] ?? 'bg-neutral-400'
}

function initials(name: string): string {
  if (!name) return '?'
  const parts = name.trim().split(' ')
  return parts.length >= 2 ? (parts[0][0] + parts[1][0]).toUpperCase() : name.slice(0, 2).toUpperCase()
}

function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' })
}

function formatRelative(iso: string): string {
  if (!iso) return ''
  const d = new Date(iso)
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  const yesterday = new Date(today.getTime() - 86400000)
  const msgDay = new Date(d.getFullYear(), d.getMonth(), d.getDate())
  if (msgDay.getTime() === today.getTime()) return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
  if (msgDay.getTime() === yesterday.getTime()) return 'Вчера'
  return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
}

// Filtered users list
const filteredUsers = computed(() => {
  let list = users.value.filter(u => u.id !== myId.value)
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(u => u.login?.toLowerCase().includes(q))
  }
  return [...list].sort((a, b) => {
    if (sortBy.value === 'unread') {
      const diff = (unread.value[b.id] ?? 0) - (unread.value[a.id] ?? 0)
      return sortDir.value === 'desc' ? diff : -diff
    }
    if (sortBy.value === 'name') {
      const diff = (a.login ?? '').localeCompare(b.login ?? '')
      return sortDir.value === 'asc' ? diff : -diff
    }
    return 0
  })
})

// Filtered broadcasts
const filteredBroadcasts = computed(() => {
  let list = broadcasts.value
  if (broadcastFilter.value === 'mine') list = list.filter(b => b.createdById === auth.userId)
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(b => b.title?.toLowerCase().includes(q) || b.text?.toLowerCase().includes(q))
  }
  return [...list].sort((a, b) => {
    if (sortBy.value === 'name') {
      const diff = (a.title ?? '').localeCompare(b.title ?? '')
      return sortDir.value === 'asc' ? diff : -diff
    }
    const diff = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    return sortDir.value === 'desc' ? diff : -diff
  })
})

// New chat — filtered sportsmen pool
const newChatSportsmenPool = computed(() => {
  if (newChatGroupKey.value.startsWith('g-')) {
    const id = Number(newChatGroupKey.value.slice(2))
    return sportsmenByGroup.value[id] ?? []
  }
  if (newChatGroupKey.value.startsWith('t-')) {
    const id = Number(newChatGroupKey.value.slice(2))
    return sportsmenByTeam.value[id] ?? []
  }
  return allSportsmen.value
})

const newChatUsers = computed(() => {
  const q = newChatSearch.value.toLowerCase().trim()
  return newChatSportsmenPool.value.filter((s: any) =>
    !q || s.fio?.toLowerCase().includes(q) || s.login?.toLowerCase().includes(q)
  ).map((s: any) => ({
    id: s.userId ?? s.id,
    login: s.fio ?? s.login,
    role: 'Sportsman',
  }))
})

// Individual search for broadcast
const filteredIndividuals = computed(() => {
  const q = individualSearch.value.toLowerCase()
  return allSportsmen.value.filter(s => s.fio?.toLowerCase().includes(q))
})

async function loadUsers() {
  usersLoading.value = true
  try {
    const res = await api.get('/message/dialogs').catch(() => null)
    const dialogs = res?.data?.data ?? []
    users.value = dialogs.map((d: any) => ({
      id: d.userId,
      login: d.userName,
      role: d.userRole,
      lastMessage: d.lastMessage,
      lastMessageAt: d.lastMessageAt,
      hasUnread: d.hasUnread,
    }))
    dialogs.forEach((d: any) => { unread.value[d.userId] = d.unreadCount ?? (d.hasUnread ? 1 : 0) })
  } finally {
    usersLoading.value = false
  }
}

function ensureUserInList(user: any) {
  if (!user?.id) return
  const exists = users.value.some(u => u.id === user.id)
  if (!exists) {
    users.value = [{ id: user.id, login: user.login, role: user.role, lastMessage: '', lastMessageAt: new Date().toISOString(), hasUnread: false }, ...users.value]
  }
}

async function loadBroadcasts() {
  broadcastsLoading.value = true
  try {
    const res = await api.get('/message/broadcast').catch(() => null)
    broadcasts.value = res?.data?.data ?? []
  } finally {
    broadcastsLoading.value = false
  }
}

async function loadGroupsTeams() {
  const tid = auth.personalId
  const [gr, tm] = await Promise.allSettled([
    api.get('/group', { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
    api.get('/team',  { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
  ])
  myGroups.value = (gr as any).value?.data?.data ?? []
  myTeams.value  = (tm as any).value?.data?.data ?? []

  const sMap = new Map<number, any>()

  const groupResults = await Promise.allSettled(
    myGroups.value.map((g: any) => api.get(`/sportsman/group/${g.id}`).catch(() => null))
  )
  groupResults.forEach((r, i) => {
    if (r.status === 'fulfilled' && r.value) {
      const list = r.value.data.data ?? []
      sportsmenByGroup.value[myGroups.value[i].id] = list
      list.forEach((s: any) => { sMap.set(s.id, s); if (s.id && s.fio) sportsmenMap.value[s.id] = s.fio })
    }
  })

  const teamResults = await Promise.allSettled(
    myTeams.value.map((t: any) => api.get('/sportsman', { params: { filters: { teamId: [t.id] } } }).catch(() => null))
  )
  teamResults.forEach((r, i) => {
    if (r.status === 'fulfilled' && r.value) {
      const list = r.value.data.data ?? []
      sportsmenByTeam.value[myTeams.value[i].id] = list
      list.forEach((s: any) => { sMap.set(s.id, s); if (s.id && s.fio) sportsmenMap.value[s.id] = s.fio })
    }
  })

  allSportsmen.value = Array.from(sMap.values())
}

async function selectUser(user: any) {
  selectedUser.value = user
  unread.value[user.id] = 0
  markMessageReadBySender(user.id)
  dialogLoading.value = true
  try {
    const res = await api.get(`/message/dialog/${user.id}`).catch(() => null)
    messages.value = res?.data?.data?.messages ?? []
    // Помечаем все входящие прочитанными
    for (const msg of messages.value) {
      if (msg.senderId === user.id && !msg.isRead) {
        api.put(`/message/${msg.id}/read`).catch(() => null)
      }
    }
    await scrollToBottom()
  } finally {
    dialogLoading.value = false
  }
}

async function selectBroadcast(b: BroadcastResponse) {
  selectedBroadcast.value = b
  broadcastDetails.value = null
  const res = await api.get(`/message/broadcast/${b.id}`).catch(() => null)
  broadcastDetails.value = res?.data?.data ?? null
}

async function sendMessage() {
  if (!newText.value.trim() || !selectedUser.value || sending.value) return
  sending.value = true
  try {
    const res = await api.post('/message', { receiverId: selectedUser.value.id, text: newText.value.trim() })
    messages.value.push(res.data.data)
    newText.value = ''
    ensureUserInList(selectedUser.value)
    await scrollToBottom()
  } finally {
    sending.value = false
  }
}

async function scrollToBottom() {
  await nextTick()
  if (messagesEl.value) messagesEl.value.scrollTop = messagesEl.value.scrollHeight
}

function openNewChat() {
  newChatSearch.value = ''
  newChatGroupKey.value = ''
  newChatSelectedUser.value = null
  newChatText.value = ''
  newChatStep.value = 1
  newChatModal.value = true
}

function selectNewChatUser(u: any) {
  newChatSelectedUser.value = newChatSelectedUser.value?.id === u.id ? null : u
}

async function startChatWithMessage() {
  if (!newChatSelectedUser.value || !newChatText.value.trim() || sendingNewChat.value) return
  sendingNewChat.value = true
  try {
    const res = await api.post('/message', {
      receiverId: newChatSelectedUser.value.id,
      text: newChatText.value.trim(),
    })
    const sentMsg = res.data.data
    newChatModal.value = false
    // Открываем чат с получателем
    selectedUser.value = newChatSelectedUser.value
    activeTab.value = 'chats'
    // Грузим историю диалога
    dialogLoading.value = true
    const dlg = await api.get(`/message/dialog/${newChatSelectedUser.value.id}`).catch(() => null)
    messages.value = dlg?.data?.data?.messages ?? [sentMsg]
    dialogLoading.value = false
    await scrollToBottom()
    // Обновляем список пользователей в фоне
    loadUsers()
  } finally {
    sendingNewChat.value = false
  }
}


function openNewBroadcast() {
  broadcastForm.value = { title: '', text: '', targetType: 'All', targetId: undefined }
  individualSearch.value = ''
  broadcastModal.value = true
}

async function sendBroadcast() {
  if (!broadcastForm.value.title || !broadcastForm.value.text) return
  sendingBroadcast.value = true
  try {
    const payload: any = {
      title: broadcastForm.value.title,
      text: broadcastForm.value.text,
      targetType: broadcastForm.value.targetType,
    }
    if (broadcastForm.value.targetId) payload.targetId = broadcastForm.value.targetId
    await api.post('/message/broadcast', payload)
    broadcastModal.value = false
    await loadBroadcasts()
  } finally {
    sendingBroadcast.value = false
  }
}

async function deleteBroadcast(b: BroadcastResponse) {
  await api.delete(`/message/broadcast/${b.id}`).catch(() => null)
  broadcasts.value = broadcasts.value.filter(x => x.id !== b.id)
  if (selectedBroadcast.value?.id === b.id) selectedBroadcast.value = null
}

onMounted(async () => {
  await Promise.all([loadUsers(), loadBroadcasts(), loadGroupsTeams()])
  await start()

  on<MessageResponse>('ReceiveMessage', (msg) => {
    if (selectedUser.value?.id === msg.senderId) {
      messages.value.push(msg)
      scrollToBottom()
      api.put(`/message/${msg.id}/read`).catch(() => null)
    } else {
      unread.value[msg.senderId] = (unread.value[msg.senderId] ?? 0) + 1
    }
  })

  on<number>('MessageRead', (messageId) => {
    const msg = messages.value.find(m => m.id === messageId)
    if (msg) msg.isRead = true
  })
})

onUnmounted(() => {
  off('ReceiveMessage')
  off('MessageRead')
  stop()
})
</script>
