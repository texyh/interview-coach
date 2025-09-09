/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        // Primary brand colors following your Slate + Indigo palette
        primary: {
          50: "#eef2ff",
          100: "#e0e7ff",
          200: "#c7d2fe",
          300: "#a5b4fc",
          400: "#818cf8",
          500: "#6366f1",
          600: "#4f46e5", // Main indigo-600 for primary buttons
          700: "#4338ca", // Hover state indigo-700
          800: "#3730a3",
          900: "#312e81",
          950: "#1e1b4b",
        },
        // Extended slate colors for backgrounds and text
        slate: {
          50: "#f8fafc", // Light background
          100: "#f1f5f9", // Dark mode text
          200: "#e2e8f0", // Borders and lines
          300: "#cbd5e1",
          400: "#94a3b8",
          500: "#64748b",
          600: "#475569", // Subtext and placeholder
          700: "#334155",
          800: "#1e293b", // Headings
          900: "#0f172a", // Main text
          950: "#020617", // Dark mode background
        },
        // Accent colors for CTAs and highlights
        accent: {
          rose: "#f43f5e", // Rose 500 for call-to-action
          amber: "#fbbf24", // Amber 400 for warnings/highlights
          emerald: "#10b981", // Success states
          red: "#ef4444", // Error states
        },
        // Semantic color system
        background: {
          DEFAULT: "#f8fafc", // slate-50
          secondary: "#ffffff",
          dark: "#020617", // slate-950
        },
        foreground: {
          DEFAULT: "#0f172a", // slate-900
          secondary: "#475569", // slate-600
          dark: "#f1f5f9", // slate-100
        },
        border: {
          DEFAULT: "#e2e8f0", // slate-200
          dark: "#334155", // slate-700
        },
        card: {
          DEFAULT: "#ffffff",
          dark: "#1e293b", // slate-800
        },
      },
      fontFamily: {
        sans: [
          "Inter",
          "system-ui",
          "-apple-system",
          "BlinkMacSystemFont",
          "Segoe UI",
          "Roboto",
          "sans-serif",
        ],
      },
      fontSize: {
        xs: ["0.75rem", { lineHeight: "1rem" }],
        sm: ["0.875rem", { lineHeight: "1.25rem" }],
        base: ["1rem", { lineHeight: "1.5rem" }],
        lg: ["1.125rem", { lineHeight: "1.75rem" }],
        xl: ["1.25rem", { lineHeight: "1.75rem" }],
        "2xl": ["1.5rem", { lineHeight: "2rem" }],
        "3xl": ["1.875rem", { lineHeight: "2.25rem" }],
        "4xl": ["2.25rem", { lineHeight: "2.5rem" }],
        "5xl": ["3rem", { lineHeight: "1" }],
      },
      spacing: {
        18: "4.5rem",
        88: "22rem",
      },
      borderRadius: {
        "2xl": "1rem",
        "3xl": "1.5rem",
      },
      boxShadow: {
        soft: "0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)",
        medium:
          "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)",
        large:
          "0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)",
      },
      animation: {
        "fade-in": "fadeIn 0.3s ease-in-out",
        "slide-up": "slideUp 0.3s ease-out",
        "scale-in": "scaleIn 0.2s ease-out",
      },
      keyframes: {
        fadeIn: {
          "0%": { opacity: "0" },
          "100%": { opacity: "1" },
        },
        slideUp: {
          "0%": { transform: "translateY(10px)", opacity: "0" },
          "100%": { transform: "translateY(0)", opacity: "1" },
        },
        scaleIn: {
          "0%": { transform: "scale(0.95)", opacity: "0" },
          "100%": { transform: "scale(1)", opacity: "1" },
        },
      },
    },
  },
  plugins: [],
};
