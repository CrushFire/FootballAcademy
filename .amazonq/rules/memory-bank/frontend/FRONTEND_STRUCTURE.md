# Frontend — структура проекта

## Стек
- **Vite** — сборщик
- **Vue 3** + Composition API + TypeScript
- **Tailwind CSS** — стили
- **Pinia** — глобальный стейт
- **Vue Router 4** — маршрутизация
- **Axios** — HTTP-запросы + JWT interceptor

## Запуск

```bash
cd frontend
npm install
npm run dev        # dev-сервер на :3000
npm run build      # продакшн сборка
```

## Структура файлов

```
frontend/
├── Dockerfile                  # продакшн: node:20 + serve, порт 3000
├── index.html                  # точка входа HTML
├── package.json
├── vite.config.ts              # proxy /api → backend:5000, alias @/
├── tailwind.config.js          # цвета: primary (синий)
├── tsconfig.json
│
└── src/
    ├── main.ts                 # точка входа: createApp + Pinia + Router
    ├── App.vue                 # корневой компонент: <RouterView />
    │
    ├── assets/
    │   └── styles/
    │       └── main.css        # @tailwind base/components/utilities
    │
    ├── types/
    │   └── index.ts            # все TypeScript интерфейсы:
    │                           # User, Sportsman, Group, Team, Training,
    │                           # Match, MatchEvent, MatchStats,
    │                           # Message, Normative, Attendance, Filter
    │
    ├── constants/
    │   └── index.ts            # ROLES, POSITIONS (с группами),
    │                           # ATTENDANCE_STATUS, MATCH_STATUS
    │
    ├── utils/
    │   └── formatDate.ts       # formatDate(), formatDateTime(), calcAge()
    │
    ├── services/
    │   └── api.ts              # Axios instance baseURL=/api
    │                           # request interceptor → Bearer token
    │                           # response interceptor → 401 = logout
    │
    ├── store/
    │   └── auth.ts             # Pinia store: token, userId, isAuthenticated
    │                           # login() → POST /auth/login → localStorage
    │                           # logout() → clear + redirect /login
    │
    ├── router/
    │   └── index.ts            # все маршруты + beforeEach guard
    │                           # requiresAuth: true → редирект на /login
    │                           # уже авторизован + /login → редирект на /
    │
    ├── composables/
    │   ├── useAuth.ts          # обёртка над auth store: login/logout + loading/error
    │   └── useFetch.ts         # универсальный GET: data/loading/error + fetch(filter?)
    │
    ├── layouts/
    │   └── DefaultLayout.vue   # sidebar (навигация) + header + <RouterView />
    │                           # кнопка выхода в sidebar
    │
    ├── components/
    │   └── ui/
    │       ├── AppButton.vue   # variants: primary/secondary/danger/ghost
    │       ├── AppTable.vue    # универсальная таблица: columns + rows + loading
    │       │                   # слоты для кастомных ячеек, emit rowClick
    │       ├── AppModal.vue    # модальное окно: show/title + slot + emit close
    │       └── StatCard.vue    # карточка статистики: icon + value + label
    │
    └── pages/
        ├── LoginPage.vue           # форма входа (email + password)
        ├── DashboardPage.vue       # главная: 4 StatCard + быстрый доступ
        ├── SportsmenPage.vue       # список спортсменов, клик → детали
        ├── SportsmanDetailPage.vue # профиль спортсмена (заглушка)
        ├── GroupsPage.vue          # список групп
        ├── TeamsPage.vue           # список команд
        ├── TrainingsPage.vue       # список тренировок
        ├── MatchesPage.vue         # список матчей
        ├── NormativesPage.vue      # список нормативов
        ├── SchedulePage.vue        # расписание (заглушка)
        ├── MessagesPage.vue        # сообщения (заглушка)
        └── AnalyticsPage.vue       # аналитика спортсмена (заглушка)
```

## Маршруты

| Путь | Компонент | Auth |
|---|---|---|
| `/login` | LoginPage | нет |
| `/` | DashboardPage | да |
| `/sportsmen` | SportsmenPage | да |
| `/sportsmen/:id` | SportsmanDetailPage | да |
| `/groups` | GroupsPage | да |
| `/teams` | TeamsPage | да |
| `/trainings` | TrainingsPage | да |
| `/matches` | MatchesPage | да |
| `/normatives` | NormativesPage | да |
| `/schedule` | SchedulePage | да |
| `/messages` | MessagesPage | да |
| `/analytics/:id` | AnalyticsPage | да |

## Связь с бэкендом

Все запросы идут через `/api` → proxy на `http://backend:5000`.
JWT токен хранится в `localStorage`, добавляется автоматически через Axios interceptor.

## Страницы-заглушки (нужно реализовать)
- `SportsmanDetailPage` — профиль + аналитика
- `SchedulePage` — расписание занятий
- `MessagesPage` — чат + рассылки
- `AnalyticsPage` — пятиугольник, графики, медицина
