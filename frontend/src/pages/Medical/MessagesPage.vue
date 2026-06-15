<template>
  <div class="p-0 md:p-3 h-full">
  <AppCard no-border class="h-full flex flex-col p-0 overflow-hidden">
  <div class="flex h-full overflow-hidden bg-neutral-50 md:rounded-2xl">

    <!-- Левая панель. На мобиле скрываем когда что-то выбрано. -->
    <aside
      class="w-full md:w-72 bg-white border-r border-neutral-200 flex-col shrink-0"
      :class="hasSelection ? 'hidden md:flex' : 'flex'"
    >

      <!-- Вкладки -->
      <div class="flex border-b border-neutral-200">
        <button
          v-for="tab in tabs" :key="tab.value"
          @click="activeTab = tab.value"
          class="flex-1 py-3 text-sm font-bold transition-colors"
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
        <template v-if="activeTab === 'chats'">
          <button
            @click="openAdminChat"
            class="w-8 h-8 rounded-xl bg-violet-500 flex items-center justify-center hover:bg-violet-600 transition-colors shrink-0"
            title="Написать администратору (методисту)"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-white">
              <path stroke-linecap="round" stroke-linejoin="round" d="M3 8l9 6 9-6M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
            </svg>
          </button>
          <button
            @click="openNewChat"
            class="w-8 h-8 rounded-xl bg-blue-500 flex items-center justify-center hover:bg-blue-600 transition-colors shrink-0"
            title="Новый чат"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4 text-white">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
            </svg>
          </button>
        </template>
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

      <!-- Сортировка -->
      <div class="px-3 py-2 border-b border-neutral-100 flex items-center gap-2">
        <span class="text-[10px] text-neutral-400 uppercase tracking-wide">Сортировка</span>
        <select v-model="sortBy" class="flex-1 text-xs rounded-lg border border-neutral-200 px-2 py-1 focus:outline-none bg-neutral-50">
          <option value="date">По дате</option>
          <option value="name">По имени</option>
          <option v-if="activeTab === 'chats'" value="unread">По непрочитанным</option>
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
          <div class="w-9 h-9 rounded-full flex items-center justify-center text-white font-bold text-sm shrink-0" :class="roleColor(user.role)">
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
            </div>
            <div class="text-xs text-neutral-400 truncate">{{ b.text.slice(0, 60) }}</div>
            <div class="text-[10px] text-neutral-300 mt-0.5">{{ formatRelative(b.createdAt) }} · {{ b.recipientsCount }} получ.</div>
          </div>
        </div>
      </div>
    </aside>

    <!-- Основная область. На мобиле скрываем когда ничего не выбрано. -->
    <div
      class="flex-1 flex-col overflow-hidden"
      :class="hasSelection ? 'flex' : 'hidden md:flex'"
    >

      <!-- Чат с пользователем -->
      <div v-if="activeTab === 'chats' && selectedUser" class="flex flex-col flex-1 min-h-0">
        <div class="px-3 md:px-5 py-3 bg-white border-b border-neutral-200 flex items-center gap-2 md:gap-3 shrink-0">
          <button
            @click="clearSelection"
            class="md:hidden w-8 h-8 rounded-lg flex items-center justify-center text-neutral-500 hover:bg-neutral-100 transition-colors shrink-0"
            aria-label="К списку"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
            </svg>
          </button>
          <div class="w-9 h-9 rounded-full flex items-center justify-center text-white font-bold text-sm shrink-0" :class="roleColor(selectedUser.role)">
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
            <div v-for="msg in messages" :key="msg.id" class="flex" :class="msg.senderId === myId ? 'justify-end' : 'justify-start'">
              <div class="max-w-xs lg:max-w-md px-4 py-2.5 rounded-2xl text-sm shadow-sm"
                :class="msg.senderId === myId ? 'bg-blue-500 text-white rounded-br-sm' : 'bg-white text-neutral-900 rounded-bl-sm'">
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
          <input v-model="newText" @keydown.enter.prevent="sendMessage" placeholder="Написать сообщение..."
            class="flex-1 px-4 py-2.5 text-sm border border-neutral-200 rounded-full focus:outline-none focus:border-blue-400"/>
          <button @click="sendMessage" :disabled="!newText.trim() || sending"
            class="px-5 py-2.5 bg-blue-500 text-white text-sm font-semibold rounded-full hover:bg-blue-600 disabled:opacity-40 transition-colors">
            Отправить
          </button>
        </div>
      </div>

      <!-- Детали рассылки -->
      <div v-else-if="activeTab === 'broadcasts' && selectedBroadcast" class="flex flex-col flex-1 min-h-0">
        <div class="px-3 md:px-5 py-3 bg-white border-b border-neutral-200 flex items-center justify-between shrink-0">
          <div class="flex items-center gap-2 md:gap-3 min-w-0">
            <button
              @click="clearSelection"
              class="md:hidden w-8 h-8 rounded-lg flex items-center justify-center text-neutral-500 hover:bg-neutral-100 transition-colors shrink-0"
              aria-label="К списку"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
              </svg>
            </button>
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
          <button v-if="selectedBroadcast.createdById === auth.userId" @click="deleteBroadcast(selectedBroadcast)"
            class="w-8 h-8 rounded-xl flex items-center justify-center text-neutral-400 hover:text-red-500 hover:bg-red-50 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
            </svg>
          </button>
        </div>

        <div class="flex-1 overflow-y-auto p-5">
          <div class="bg-white rounded-2xl border border-neutral-100 p-5 max-w-2xl">
            <div class="text-xs font-semibold text-neutral-400 uppercase tracking-wide mb-2">Содержание</div>
            <div v-if="broadcastDetails?.createdByName || selectedBroadcast.createdByName" class="text-xs text-neutral-500 mb-2">
              От: <span class="font-semibold text-neutral-700">{{ broadcastDetails?.createdByName ?? selectedBroadcast.createdByName }}</span>
            </div>
            <p class="text-sm text-neutral-800 leading-relaxed whitespace-pre-wrap">{{ selectedBroadcast.text }}</p>
            <div v-if="selectedBroadcast.expireAt" class="mt-3 text-xs text-neutral-400">Истекает: {{ formatDate(selectedBroadcast.expireAt) }}</div>
          </div>
          <div class="mt-4 bg-white rounded-2xl border border-neutral-100 p-4">
            <div class="text-xs font-semibold text-neutral-400 uppercase tracking-wide mb-3">Статус прочтения</div>
            <div v-if="broadcastDetails" class="space-y-1.5">
              <div v-for="r in broadcastDetails.recipients" :key="r.userId"
                class="flex items-center justify-between px-3 py-2 rounded-xl bg-neutral-50">
                <span class="text-xs text-neutral-700">{{ sportsmenMap[r.userId] ?? r.userName ?? `Пользователь #${r.userId}` }}</span>
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

      <template v-if="newChatStep === 1">
        <div class="px-4 pt-3 pb-2">
          <input v-model="newChatSearch" placeholder="Поиск по имени..."
            class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50"/>
        </div>
        <div class="overflow-y-auto flex-1 divide-y divide-neutral-50 px-2 pb-2">
          <div v-for="u in newChatUsers" :key="u.id"
            @click="selectNewChatUser(u)"
            class="flex items-center gap-3 px-3 py-2.5 rounded-xl cursor-pointer hover:bg-neutral-50 transition-colors"
            :class="newChatSelectedUser?.id === u.id ? 'bg-blue-50' : ''">
            <div class="w-8 h-8 rounded-full flex items-center justify-center text-white text-xs font-bold shrink-0 bg-neutral-400">
              {{ initials(u.login) }}
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm font-medium text-neutral-800 truncate">{{ u.login }}</div>
              <div class="text-xs text-neutral-400">Спортсмен</div>
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

      <template v-else>
        <div class="px-4 py-3 border-b border-neutral-100 flex items-center gap-2">
          <div class="w-8 h-8 rounded-full bg-neutral-400 flex items-center justify-center text-white text-xs font-bold shrink-0">
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

  <!-- Модалка чата с админом -->
  <div v-if="adminChatModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/30 backdrop-blur-sm p-4">
    <div class="bg-white rounded-2xl shadow-xl w-full max-w-sm flex flex-col max-h-[85vh]">
      <div class="flex items-center justify-between px-4 pt-4 pb-3 border-b border-neutral-100">
        <span class="text-sm font-semibold text-neutral-800">
          {{ adminChatStep === 1 ? 'Написать администратору (методисту)' : 'Сообщение администратору (методисту)' }}
        </span>
        <button @click="adminChatModal = false" class="text-neutral-400 hover:text-neutral-600">✕</button>
      </div>

      <template v-if="adminChatStep === 1">
        <div class="px-4 pt-3 pb-2">
          <input v-model="adminSearch" placeholder="Поиск по имени..."
            class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-violet-400 bg-neutral-50"/>
        </div>
        <div class="overflow-y-auto flex-1 divide-y divide-neutral-50 px-2 pb-2">
          <div v-if="adminsLoading" class="px-4 py-8 text-center text-xs text-neutral-400">Загрузка...</div>
          <template v-else>
            <div v-for="a in filteredAdmins" :key="a.id"
              @click="adminSelected = a"
              class="flex items-center gap-3 px-3 py-2.5 rounded-xl cursor-pointer hover:bg-neutral-50 transition-colors"
              :class="adminSelected?.id === a.id ? 'bg-violet-50 ring-1 ring-violet-300' : ''">
              <div class="w-8 h-8 rounded-full bg-violet-500 flex items-center justify-center text-white text-xs font-bold shrink-0">
                {{ initials(a.login) }}
              </div>
              <div class="flex-1 min-w-0">
                <div class="text-sm font-medium text-neutral-800 truncate">{{ a.login }}</div>
                <div class="text-xs text-neutral-400">Администратор</div>
              </div>
            </div>
            <div v-if="filteredAdmins.length === 0" class="px-4 py-8 text-center text-xs text-neutral-400">Нет администраторов</div>
          </template>
        </div>
        <div class="p-4 border-t border-neutral-100">
          <button @click="adminChatStep = 2" :disabled="!adminSelected"
            class="w-full py-2.5 rounded-xl text-sm font-semibold transition-colors"
            :class="adminSelected ? 'bg-violet-500 text-white hover:bg-violet-600' : 'bg-neutral-100 text-neutral-400 cursor-not-allowed'">
            Далее →
          </button>
        </div>
      </template>

      <template v-else>
        <div class="px-4 py-3 border-b border-neutral-100 flex items-center gap-2">
          <div class="w-8 h-8 rounded-full bg-violet-500 flex items-center justify-center text-white text-xs font-bold shrink-0">
            {{ initials(adminSelected?.login ?? '') }}
          </div>
          <span class="text-sm font-semibold text-neutral-800">{{ adminSelected?.login }}</span>
        </div>
        <div class="px-4 py-3 flex-1">
          <textarea v-model="adminText" rows="5" placeholder="Введите сообщение администратору..."
            class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-violet-400 resize-none"/>
        </div>
        <div class="px-4 pb-4 flex gap-2">
          <button @click="adminChatStep = 1" class="px-4 py-2.5 rounded-xl border border-neutral-200 text-sm font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">← Назад</button>
          <button @click="sendAdminMessage" :disabled="!adminText.trim() || sendingAdmin"
            class="flex-1 py-2.5 rounded-xl text-sm font-semibold transition-colors"
            :class="adminText.trim() && !sendingAdmin ? 'bg-violet-500 text-white hover:bg-violet-600' : 'bg-neutral-100 text-neutral-400 cursor-not-allowed'">
            {{ sendingAdmin ? 'Отправка...' : 'Отправить' }}
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
          <div class="grid grid-cols-3 gap-2">
            <button v-for="t in targetTypes" :key="t.value"
              @click="broadcastForm.targetType = t.value; broadcastForm.targetId = undefined"
              class="py-2 text-xs font-semibold rounded-xl border transition-colors"
              :class="broadcastForm.targetType === t.value ? 'border-blue-400 bg-blue-50 text-blue-700' : 'border-neutral-200 text-neutral-600 hover:border-neutral-300'">
              {{ t.label }}
            </button>
          </div>
        </div>
        <div v-if="broadcastForm.targetType === 'Group'">
          <label class="block text-xs font-semibold text-neutral-500 mb-1">Группа</label>
          <select v-model="broadcastForm.targetId" class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2 focus:outline-none focus:border-blue-400">
            <option :value="undefined">Выберите группу</option>
            <option v-for="g in allGroups" :key="g.id" :value="g.id">{{ g.name }}</option>
          </select>
        </div>
        <div v-else-if="broadcastForm.targetType === 'Team'">
          <label class="block text-xs font-semibold text-neutral-500 mb-1">Команда</label>
          <select v-model="broadcastForm.targetId" class="w-full text-sm rounded-xl border border-neutral-200 px-3 py-2 focus:outline-none focus:border-blue-400">
            <option :value="undefined">Выберите команду</option>
            <option v-for="t in allTeams" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
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
  </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import api from '@/services/api'
import AppCard from '@/components/ui/AppCard.vue'
import { useAuthStore } from '@/store/auth'
import { useSignalR } from '@/composables/useSignalR'
import { useNotifications } from '@/composables/useNotifications'
import type { MessageResponse } from '@/types/message/dto'
import type { BroadcastResponse, BroadcastDetailsResponse } from '@/types/broadcast/dto'
import type { BroadcastTargetType } from '@/types/broadcast/model'

const auth = useAuthStore()
const { start, stop, on, off } = useSignalR()
const { markMessageReadBySender } = useNotifications()

const myId = computed(() => auth.userId)

const tabs: { value: 'chats' | 'broadcasts'; label: string }[] = [
  { value: 'chats', label: 'Чаты' },
  { value: 'broadcasts', label: 'Рассылки' },
]
const activeTab = ref<'chats' | 'broadcasts'>('chats')

const search = ref('')
const sortBy = ref<'date' | 'name' | 'unread'>('date')
const sortDir = ref<'asc' | 'desc'>('desc')

const users = ref<any[]>([])
const usersLoading = ref(false)
const selectedUser = ref<any>(null)
const messages = ref<MessageResponse[]>([])
const dialogLoading = ref(false)
const newText = ref('')
const sending = ref(false)
const messagesEl = ref<HTMLElement | null>(null)
const unread = ref<Record<number, number>>({})

const broadcasts = ref<BroadcastResponse[]>([])
const broadcastsLoading = ref(false)
const selectedBroadcast = ref<BroadcastResponse | null>(null)
const broadcastDetails = ref<BroadcastDetailsResponse | null>(null)
const sportsmenMap = ref<Record<number, string>>({})

// На мобиле показываем либо список (левый aside), либо переписку (правый блок).
const hasSelection = computed(() =>
  (activeTab.value === 'chats' && selectedUser.value)
  || (activeTab.value === 'broadcasts' && selectedBroadcast.value)
)

function clearSelection() {
  selectedUser.value = null
  selectedBroadcast.value = null
}

const allGroups = ref<any[]>([])
const allTeams = ref<any[]>([])
const allSportsmen = ref<any[]>([])

const newChatModal = ref(false)
const newChatSearch = ref('')
const newChatStep = ref<1 | 2>(1)
const newChatSelectedUser = ref<any>(null)
const newChatText = ref('')
const sendingNewChat = ref(false)

const adminChatModal = ref(false)
const adminChatStep = ref<1 | 2>(1)
const admins = ref<any[]>([])
const adminsLoading = ref(false)
const adminSearch = ref('')
const adminSelected = ref<any>(null)
const adminText = ref('')
const sendingAdmin = ref(false)

const filteredAdmins = computed(() => {
  const q = adminSearch.value.toLowerCase().trim()
  return admins.value.filter(a => !q || a.login?.toLowerCase().includes(q))
})
const broadcastModal = ref(false)
const sendingBroadcast = ref(false)
const broadcastForm = ref<{
  title: string
  text: string
  targetType: BroadcastTargetType
  targetId: number | undefined
}>({ title: '', text: '', targetType: 'All', targetId: undefined })

const targetTypes = [
  { value: 'All' as BroadcastTargetType, label: 'Все' },
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

const filteredBroadcasts = computed(() => {
  let list = broadcasts.value
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

const newChatUsers = computed(() => {
  const q = newChatSearch.value.toLowerCase().trim()
  return allSportsmen.value
    .filter((s: any) => !q || s.fio?.toLowerCase().includes(q))
    .map((s: any) => ({ id: s.userId ?? s.id, login: s.fio ?? s.login }))
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

async function loadBroadcasts() {
  broadcastsLoading.value = true
  try {
    const res = await api.get('/message/broadcast').catch(() => null)
    broadcasts.value = res?.data?.data ?? []
  } finally {
    broadcastsLoading.value = false
  }
}

async function loadData() {
  const [gr, tm, sm] = await Promise.allSettled([
    api.get('/group'),
    api.get('/team'),
    api.get('/sportsman'),
  ])
  allGroups.value = (gr as any).value?.data?.data ?? []
  allTeams.value  = (tm as any).value?.data?.data ?? []
  allSportsmen.value = (sm as any).value?.data?.data ?? []
  allSportsmen.value.forEach((s: any) => { if (s.id && s.fio) sportsmenMap.value[s.id] = s.fio })
}

async function selectUser(user: any) {
  selectedUser.value = user
  unread.value[user.id] = 0
  markMessageReadBySender(user.id)
  dialogLoading.value = true
  try {
    const res = await api.get(`/message/dialog/${user.id}`).catch(() => null)
    messages.value = res?.data?.data?.messages ?? []
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
  api.put(`/message/broadcast/${b.id}/read`).catch(() => null)
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
    selectedUser.value = newChatSelectedUser.value
    activeTab.value = 'chats'
    dialogLoading.value = true
    const dlg = await api.get(`/message/dialog/${newChatSelectedUser.value.id}`).catch(() => null)
    messages.value = dlg?.data?.data?.messages ?? [sentMsg]
    dialogLoading.value = false
    await scrollToBottom()
    loadUsers()
  } finally {
    sendingNewChat.value = false
  }
}

async function loadAdmins() {
  if (admins.value.length) return
  adminsLoading.value = true
  try {
    const res = await api.get('/users', { params: { filters: { role: ['admin'] } } }).catch(() => null)
    let list: any[] = res?.data?.data ?? []
    list = list.filter((u: any) => (u.role ?? '').toLowerCase() === 'admin' && u.id !== auth.userId)
    admins.value = list
  } finally {
    adminsLoading.value = false
  }
}

function openAdminChat() {
  adminSearch.value = ''
  adminSelected.value = null
  adminText.value = ''
  adminChatStep.value = 1
  adminChatModal.value = true
  loadAdmins()
}

async function sendAdminMessage() {
  if (!adminSelected.value || !adminText.value.trim() || sendingAdmin.value) return
  sendingAdmin.value = true
  try {
    const target = { id: adminSelected.value.id, login: adminSelected.value.login, role: 'admin' }
    const res = await api.post('/message', { receiverId: target.id, text: adminText.value.trim() })
    const sentMsg = res.data.data
    adminChatModal.value = false
    selectedUser.value = target
    activeTab.value = 'chats'
    dialogLoading.value = true
    const dlg = await api.get(`/message/dialog/${target.id}`).catch(() => null)
    messages.value = dlg?.data?.data?.messages ?? [sentMsg]
    dialogLoading.value = false
    await scrollToBottom()
    loadUsers()
  } finally {
    sendingAdmin.value = false
  }
}

function openNewBroadcast() {
  broadcastForm.value = { title: '', text: '', targetType: 'All', targetId: undefined }
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
  await Promise.all([loadUsers(), loadBroadcasts(), loadData()])
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
