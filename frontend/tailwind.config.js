/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: [
    './index.html',
    './src/**/*.{vue,js,ts,jsx,tsx}'
  ],
  theme: {
    extend: {
      colors: {

        // Ключевые цвета
        primary: {
          50:  '#EFF6FF',
          100: '#DBEAFE',
          200: '#BFDBFE',
          300: '#93C5FD',
          400: '#60A5FA',
          500: '#3B82F6',
          600: '#2563EB',
          700: '#1D4ED8',
          800: '#1E40AF',
          900: '#1E3A8A',
        },

        // Базовые цвета (slate-серый с синим подтоном, гармонирует с primary)
          neutral: {
          50:  '#DDE3EB',  // фон карточек
          100: '#C8D1DB',  
          200: '#B3BECB',  
          300: '#CBD5E1',
          400: '#94A3B8',
          500: '#64748B',
          600: '#475569',
          700: '#334155',
          800: '#1E293B',
          900: '#0F172A',
        },
          
        // neutral: {
        // 50:  '#F8FAFC',
        // 100: '#E2E8F0',
        // 200: '#CBD5E1',
        // 300: '#CBD5E1',
        // 400: '#94A3B8',
        // 500: '#64748B',
        // 600: '#475569',
        // 700: '#334155',
        // 800: '#1E293B',
        // 900: '#0F172A',
        // },

        // Тёплый нейтрал (старая палитра, оставлена для совместимости)
        'neutral-classic': {
          50:  '#FAFAFA',
          100: '#F5F5F5',
          200: '#E5E5E5',
          300: '#E0E0E0',
          400: '#b9b9ba',
          500: '#97969f',
          600: '#727276',
          700: '#616161',
          800: '#424242',
          900: '#212121',
        },

        // Цвет навигационной панели (не привязан к palette primary,
        // чтобы смена темы не влияла на боковое меню)
        nav: {
          DEFAULT: '#60A5FA',
          hover:   'rgba(255, 255, 255, 0.18)',
          active:  'rgba(255, 255, 255, 0.28)',
          text:    '#EFF6FF',
          'text-active': '#FFFFFF',
        },

        // Тёмная тема — синие подтоны, без серого
        dk: {
          bg:       '#0B1220', // основной фон страницы
          surface:  '#111A2E', // карточки
          surface2: '#162238', // вложенные блоки, hover
          border:   '#1F2D4D', // границы
          line:     '#243557', // разделители
          text:     '#E5ECFA', // основной текст
          'text-2': '#A8B4D2', // вторичный текст
          muted:    '#6B7BA0', // приглушённый
          accent:   '#3B82F6', // акцент (primary)
          'accent-2': '#60A5FA',
          // Полоски на карточках тренера
          stripe: {
            blue:    '#1E3A8A',
            emerald: '#064E3B',
            amber:   '#78350F',
            red:     '#7F1D1D',
            violet:  '#4C1D95',
            sky:     '#0C4A6E',
            green:   '#14532D',
            purple:  '#4C1D95',
            teal:    '#134E4A',
            orange:  '#7C2D12',
          },
          // Цветные плашки на тёмном фоне
          chip: {
            'blue-bg':    'rgba(59, 130, 246, 0.18)',
            'blue-text':  '#93C5FD',
            'sky-bg':     'rgba(14, 165, 233, 0.18)',
            'sky-text':   '#7DD3FC',
            'emerald-bg':'rgba(16, 185, 129, 0.18)',
            'emerald-text':'#6EE7B7',
            'amber-bg':   'rgba(245, 158, 11, 0.18)',
            'amber-text': '#FCD34D',
            'red-bg':     'rgba(239, 68, 68, 0.18)',
            'red-text':   '#FCA5A5',
            'violet-bg':  'rgba(139, 92, 246, 0.18)',
            'violet-text':'#C4B5FD',
            'neutral-bg': 'rgba(168, 180, 210, 0.12)',
            'neutral-text':'#A8B4D2',
          },
        },

        // Темная и светлая тема
        background: {
          light: '#FFFFFF',
          dark:  '#0D1117',
        },

        surface: {
          light: '#F5F5F5',
          dark:  '#161B22',
        },

        border: {
          light: '#E8E2D9',
          dark:  '#30363D',
        },

        text: {
          primary: {
            light: '#212121',
            dark:  '#E6EDF3',
          },
          secondary: {
            light: '#616161',
            dark:  '#9DA7B1',
          },
          muted: {
            light: '#9E9E9E',
            dark:  '#6B7280',
          }
        },

        // Статусы (подтянуты к той же температуре)
        success: {
          50:  '#F0FDF4',
          100: '#DCFCE7',
          500: '#16A34A',
          700: '#15803D',
        },
        warning: {
          50:  '#FFFBEB',
          100: '#FEF3C7',
          500: '#D97706',
          700: '#B45309',
        },
        error: {
          50:  '#FFF1F2',
          100: '#FFE4E6',
          500: '#E11D48',
          700: '#BE123C',
        },

        // Chart — blue = primary, единая система
        chart: {
          blue: {
            DEFAULT: '#2D78DC',
            light:   '#5B98EC',
            dark:    '#1249A0',
            muted:   '#95BEF5',
            bg:      '#EBF4FF',
            text:    '#0B3582',
          },
          amber: {
            DEFAULT: '#F59E0B',
            light:   '#FCD34D',
            dark:    '#D97706',
            muted:   '#FDE68A',
            bg:      '#FFFBEB',
            text:    '#92400E',
          },
          red: {
            DEFAULT: '#E11D48',
            light:   '#FB7185',
            dark:    '#BE123C',
            muted:   '#FECDD3',
            bg:      '#FFF1F2',
            text:    '#9F1239',
          },
          violet: {
            DEFAULT: '#7C3AED',
            light:   '#A78BFA',
            dark:    '#6D28D9',
            muted:   '#C4B5FD',
            bg:      '#F5F3FF',
            text:    '#5B21B6',
          },
          green: {
            DEFAULT: '#16A34A',
            light:   '#4ADE80',
            dark:    '#15803D',
            muted:   '#86EFAC',
            bg:      '#F0FDF4',
            text:    '#14532D',
          },
        },
      }
    }
  },
  plugins: []
}
