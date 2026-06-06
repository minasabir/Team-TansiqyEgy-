# تسليم شات بوت «بحر» لمهندس الفرونت (React)

ابعت **الملف ده كامل** + **مجلد المشروع (أو رابط الريبو)** + **قيم الـ `.env`** (في قناة خاصة، مش عامة).

---

## 1) إيه اللي يتبعت فعليًا

| # | الوصف |
|---|--------|
| 1 | نسخة من المشروع ده (ZIP أو Git) — عشان الكود المرجعي |
| 2 | الملف ده: `docs/HANDOFF_شات_بوت_للفرونت.md` |
| 3 | (اختياري) `docs/AI_SYSTEM_DOCUMENTATION.md` — شرح أعمق للنظام |
| 4 | قيم المتغيرات في القسم 3 — **في رسالة خاصة للمهندس** |

---

## 2) ملفات مهمة في المشروع (مرجع للنسخ أو الفهم)

| مسار | الغرض |
|------|--------|
| `src/hooks/useChat.ts` | الاتصال بالـ API + قراءة الرد المتدفق (SSE) |
| `src/components/ChatHeader.tsx` | هيدر الشات |
| `src/components/ChatMessage.tsx` | فقاعة رسالة |
| `src/components/ChatInput.tsx` | حقل الإرسال |
| `src/components/TypingIndicator.tsx` | مؤشر «بيكتب…» |
| `src/components/QuickOptions.tsx` | أزرار اختيارات سريعة |
| `src/pages/Index.tsx` | مثال تجميع الشاشة |
| `src/types/chat.ts` | نوع `Message` |
| `supabase/functions/chat/index.ts` | الـ Edge Function (الباك) — للديبلوي والسيكرتات |

---

## 3) متغيرات بيئة الفرونت (Vite)

المهندس يحطهم في `.env` عندهم (الاسم في المشروع الحالي كما هو):

```env
VITE_SUPABASE_URL=https://____________.supabase.co
VITE_SUPABASE_PUBLISHABLE_KEY=eyJ...الـanon_أو_publishable_key
```

- **ممنوع** يتحط في الفرونت أو يتبعت في جروب عام: مفتاح Lovable / أي مفتاح سري للـ AI.  
- السيكرت الخاص بالـ AI يتظبط **في Supabase** على الـ Function اسم المتغير: `LOVABLE_API_KEY`.

---

## 4) عقد الـ API (اللي الفرونت يطبّقه)

**الرابط النهائي**

```
{VITE_SUPABASE_URL}/functions/v1/chat
```

**طلب**

- Method: `POST`
- Headers:
  - `Content-Type: application/json`
  - `Authorization: Bearer {نفس VITE_SUPABASE_PUBLISHABLE_KEY}`

**جسم الطلب (JSON)**

```json
{
  "messages": [
    { "role": "user", "content": "نص المستخدم" },
    { "role": "assistant", "content": "نص رد البوت السابق" }
  ]
}
```

- لازم تتبعت **كل المحادثة** اللي عايز البوت يفهمها (user و assistant بالتناوب حسب اللي حصل).

**الرد**

- نوع: `text/event-stream` (SSE)
- سطور تبدأ بـ `data: ` ثم JSON
- الشكل المتوقع لكل chunk: `choices[0].delta.content` (نص يُضاف للرد)
- نهاية: سطر فيه `data: [DONE]`

**أكواد مفيدة يعالجها الفرونت**

- `429`: كتير طلبات — استنى وحاول تاني
- `402`: مشكلة رصيد/فوترة على بوابة الـ AI

---

## 5) رسالة جاهزة تبعتها للمهندس (إنجليزي — لو يفضّل)

```
We need to embed the “Bahr” chatbot into our React site.

Reference implementation is in this repo:
- API client + SSE parsing: src/hooks/useChat.ts
- UI pieces: src/components (ChatHeader, ChatMessage, ChatInput, TypingIndicator, QuickOptions)
- Page wiring example: src/pages/Index.ts

Integration:
- POST {VITE_SUPABASE_URL}/functions/v1/chat
- Headers: Content-Type: application/json, Authorization: Bearer <anon/publishable key>
- Body: { messages: [{ role: "user"|"assistant", content: string }, ...] }
- Response: OpenAI-style SSE; stream choices[0].delta.content until [DONE]

I will send VITE_SUPABASE_URL and VITE_SUPABASE_PUBLISHABLE_KEY privately.
LOVABLE_API_KEY is only on the Supabase Edge Function env, not in the frontend.

See docs/AI_SYSTEM_DOCUMENTATION.md for full system notes.
```

---

## 6) رسالة جاهزة تبعتها للمهندس (عربي)

```
محتاج نضيف شات بوت بحر على الموقع (React).

المرجع: نفس ريبو تنسيقي — الهوك في src/hooks/useChat.ts والواجهة تحت src/components وصفحة مثال Index.tsx.

الاتصال:
- POST على {رابط Supabase}/functions/v1/chat
- Authorization: Bearer + الـ anon/publishable key
- Body: { messages: [ { role, content } ... ] }
- الرد SSE (نفس ستايل OpenAI streaming)

هبعتلك قيم .env على الخاص. مفتاح الـ AI بس في Supabase على الفانكشن LOVABLE_API_KEY.

تفاصيل إضافية: docs/AI_SYSTEM_DOCUMENTATION.md
```

---

## 7) تشيك ليست قبل ما يقولوا «خلصنا»

- [ ] `.env` فيه `VITE_SUPABASE_URL` و `VITE_SUPABASE_PUBLISHABLE_KEY`
- [ ] طلب الشات بيصل للـ URL الصحيح والهيدر فيه الـ Bearer
- [ ] الرسائل اللي بتتبعت في `messages` فيها تاريخ المحادثة (مش آخر سطر بس)
- [ ] الـ UI بيعرض الـ stream (الكلام بيتكتب على مهل)
- [ ] الـ Edge Function متنشرة على Supabase و `LOVABLE_API_KEY` متظبطة

---

*آخر تحديث: مرتبط بمشروع Egypt Uniguide / تنسيقي.*
