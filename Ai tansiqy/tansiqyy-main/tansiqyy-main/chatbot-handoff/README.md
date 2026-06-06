# حزمة تسليم شات بوت «بحر» — ابعت المجلد `chatbot-handoff` كامل

## المحتويات

| مسار | الوصف |
|------|--------|
| `docs/` | دليل التسليم للفرونت + توثيق النظام |
| `src/` | كود React جاهز (هوك، صفحة مثال، مكوّنات، `ui/button`, مساعدات) |
| `styles/chat-bot-theming.css` | متغيرات الألوان والأنيميشن — دمجه في مشروعك (أو استيراده بعد Tailwind) |
| `tailwind.config.snippet.ts` | مقتطع لدمج ألوان الشات في `tailwind.config` |
| `supabase/functions/chat/` | مرجع الـ Edge Function (لمن يدير الديبلوي) |

## خطوات سريعة لمهندس React

1. انسخ محتويات `src/` داخل مشروعكم بحيث يبقى نفس الشجرة (`src/hooks`, `src/components`, …).
2. فعّل alias المسارات: `@` → مجلد `src` (مثل المشروع الأصلي في Vite/tsconfig).
3. ثبّت الحزم (إن لم تكن موجودة):

```bash
npm i lucide-react sonner clsx tailwind-merge class-variance-authority @radix-ui/react-slot next-themes tailwindcss-animate
```

4. غلّف التطبيق بـ `ThemeProvider` من `next-themes` (مطلوب لـ `ThemeToggle`). إن لم ترد الوضع الداكن: احذف `ThemeToggle` من `ChatHeader` أو بدّله بزر بسيط.
5. ادمج `styles/chat-bot-theming.css` في تطبيقك (مثلاً `import` بعد `@tailwind` في ملف CSS الرئيسي)، أو انسخ محتواه إلى `index.css`.
6. ادمج مقتطع `tailwind.config.snippet.ts` مع `tailwind.config` عندكم.
7. انسخ ملف `.env` من جذر هذا المجلد إلى جذر مشروع React عندكم (القيم جاهزة فيه). **لا ترفعه على Git عام** — وُضع `.gitignore` في المجلد لتقليل خطأ الرفع بالغلط.

8. ربط الراوت: استورد مكوّن الصفحة من `src/pages/Index.tsx` أو انسخ المحتوى إلى صفحة عندكم؛ نادِ `startConversation` في `useEffect` كما في المثال.
9. في جذر التطبيق (مثل `App.tsx`) أضِف: `<Toaster />` من `src/components/ui/sonner.tsx` (بعد تغليف `ThemeProvider` من `next-themes`).

## ملاحظة

- بدون `Toaster` لن تظهر رسائل `toast` عند 429/402 من `useChat.ts`.
- تفاصيل الـ API والرسائل الجاهزة للمهندس: `docs/HANDOFF_شات_بوت_للفرونت.md`
