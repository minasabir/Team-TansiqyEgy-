# AI System Documentation

## 1. AI System Overview

The Egypt Uniguide (Tansiqy) application includes an AI-powered chat assistant named **"بحر" (Bahr)**. The AI system provides:

- **University advisory chat** for Egyptian students: helping with university/college selection, admission scores (تنسيق), fees, and general guidance.
- **Context-aware responses** by injecting live data from the external **Tansiqy API** (`https://tansiqy.runasp.net`) into the system prompt based on keyword detection in the user's message. This is a **keyword-based RAG-style** flow (no embeddings or vector DB).
- **Streaming responses** via Server-Sent Events (SSE), using an OpenAI-compatible chat completions API through **Lovable's AI Gateway** with the **Google Gemini 3 Flash Preview** model.
- **Arabic (Egyptian dialect)** output, with strict rules: no markdown symbols, percentages for scores (not raw points), and no fabricated data.

The frontend does not call any LLM directly; all AI logic lives in a **Supabase Edge Function** (`chat`), which builds context from the Tansiqy API, constructs the system prompt, calls the gateway, and streams the response back.

---

## 2. AI Architecture

- **Client**: React app uses a custom `useChat` hook that POSTs conversation history to the Supabase Edge Function and consumes the response as an SSE stream, parsing OpenAI-format chunks (`choices[0].delta.content`).
- **Backend (AI entry point)**: Single Supabase Edge Function `supabase/functions/chat/index.ts`:
  - Reads the last user message and **keyword-detection** logic to decide which Tansiqy API endpoints to call (university types, by type, by name, colleges, news, etc.).
  - Fetches from Tansiqy API (with retry and timeout), builds an `apiContext` string, and appends it to a fixed **system prompt**.
  - Calls `https://ai.gateway.lovable.dev/v1/chat/completions` with `model: "google/gemini-3-flash-preview"`, `stream: true`, and the composed messages.
  - Handles 429 (rate limit) with retries and 402 (credits) with a friendly fallback; on success, forwards the gateway response body (SSE stream) to the client.
- **No embeddings, vector DBs, or RAG retrieval**; no LangChain, LlamaIndex, or other AI SDKs in the repo. The “RAG” is purely **keyword → API calls → concatenate JSON into system prompt**.

---

## 3. AI Flow Diagram

```
User types message in chat UI
         ↓
React: ChatInput → handleUserInput (useChat)
         ↓
useChat: appends message to conversationHistory, calls streamChat(messages, onDelta, onDone)
         ↓
streamChat: POST /functions/v1/chat with { messages } + Supabase auth header
         ↓
Supabase Edge Function (chat/index.ts)
         ↓
Parse last user message → keyword detection (أنواع الجامعات، جامعة خاصة، تنسيق، أخبار، etc.)
         ↓
fetchFromTansiqyAPI(...) for matching endpoints (retry × 3, 10s timeout)
         ↓
Build apiContext string (JSON dumps of API responses)
         ↓
Build systemPrompt = fixed Arabic prompt + apiContext
         ↓
POST https://ai.gateway.lovable.dev/v1/chat/completions
  body: { model: "google/gemini-3-flash-preview", messages: [system, ...userMessages], stream: true }
  headers: Authorization: Bearer LOVABLE_API_KEY
         ↓
If 429: retry with backoff (up to 3 attempts); if 402/429 final: return createSSETextResponse(fallbackMessage)
         ↓
On 200: return Response(response.body) with Content-Type: text/event-stream
         ↓
useChat: ReadableStream getReader → parse SSE lines → data: {...} → choices[0].delta.content → onDelta(chunk)
         ↓
UI: append chunks to last bot message (or create new bot message), then onDone() → setIsTyping(false)
         ↓
Response shown to user in chat
```

---

## 4. AI Code Map


| `supabase/functions/chat/index.ts` | **AI entry point**: Tansiqy context, prompt build, LLM call, streaming, fallbacks | Tansiqy API (HTTP), Lovable AI Gateway 
| `src/hooks/useChat.ts` | **Chat client**: sends messages to Edge Function, consumes SSE, parses OpenAI-format deltas | `CHAT_URL` (Supabase `/functions/v1/chat`), `Message` from `@/types/chat` |
| `src/types/chat.ts` | **Types** for chat messages (id, role, content, timestamp) | Used by `useChat`, `ChatMessage` |
| `src/pages/Index.tsx` | **UI**: wires `useChat` to ChatHeader, ChatMessage list, ChatInput, QuickOptions, TypingIndicator | `useChat` only (no direct AI) |
| `src/components/ChatMessage.tsx` | Renders a single message (styling only) | `Message` from `@/types/chat` |
| `src/components/ChatInput.tsx` | Input + send (no AI logic) | Calls `onSend` from parent |
| `src/components/QuickOptions.tsx` | Quick-reply buttons (no AI logic) | Calls `onSelect` from parent |
| `src/components/TypingIndicator.tsx` | “Bot is typing” UI (no AI logic) | — |

**Data flow summary:**

- **User → AI:** User message → `handleUserInput` / `handleQuickOption` → `streamChat(conversationHistory)` → POST to Edge Function → keyword detection → Tansiqy API → `apiContext` → system prompt → LLM.
- **AI → User:** LLM stream → Edge Function → response.body (SSE) → `useChat` reader → `onDelta` → state update → `ChatMessage` re-render.

---

## 5. AI Related Files

### 5.1 `supabase/functions/chat/index.ts`

**Purpose:** Backend AI endpoint: Tansiqy context retrieval, system prompt construction, LLM call (Lovable gateway, Gemini), streaming and error/fallback handling.

**AI-related code:**

- **Tansiqy fetch with retry (context for “RAG”):**

```ts
const TANSIQY_API_BASE = "http://tansiqy.runasp.net";

async function fetchFromTansiqyAPI(endpoint: string, retries = 3): Promise<any> {
  for (let attempt = 1; attempt <= retries; attempt++) {
    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 10000);
      const response = await fetch(`${TANSIQY_API_BASE}${endpoint}`, {
        method: "GET",
        headers: { "Content-Type": "application/json", "Accept": "application/json", "User-Agent": "TansiqyBot/1.0" },
        signal: controller.signal,
      });
      clearTimeout(timeoutId);
      if (!response.ok) { /* retry or return null */ }
      return await response.json();
    } catch (error) { /* retry or return null */ }
  }
  return null;
}
```

- **SSE fallback response (for 429/402):**

```ts
function createSSETextResponse(message: string, status = 200): Response {
  const encoder = new TextEncoder();
  const streamData = `data: ${JSON.stringify({ choices: [{ delta: { content: message } }] })}\n\ndata: [DONE]\n\n`;
  return new Response(encoder.encode(streamData), {
    status,
    headers: { ...corsHeaders, "Content-Type": "text/event-stream" },
  });
}
```

- **Keyword-based context building (excerpt):** Last user message is lowercased; then conditional blocks append to `apiContext` by calling `fetchFromTansiqyAPI` for endpoints such as:
  - `/api/Universities/types` (أنواع الجامعات، انواع، نوع الجامعة، جامعة، كليات، تنسيق، مصاريف، موقع، etc.)
  - `/api/Universities/type/private` (جامعة خاصة، مصاريف)
  - `/api/Universities/type/public` (جامعة حكومية)
  - `/api/Universities/type/national` (جامعة أهلية)
  - `/api/Universities/search/name?name=...` and `/api/Universities/{id}/colleges` (when a university name is detected from a fixed list + partial names)
  - `/api/News` (أخبار، جديد، آخر، مستجدات)
- **System prompt (full):** See **Section 6** below.
- **LLM call with retry (rate limit 429):**

```ts
let response: Response | null = null;
for (let attempt = 1; attempt <= 3; attempt++) {
  response = await fetch("https://ai.gateway.lovable.dev/v1/chat/completions", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${LOVABLE_API_KEY}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      model: "google/gemini-3-flash-preview",
      messages: [{ role: "system", content: systemPrompt }, ...messages],
      stream: true,
    }),
  });
  if (response.status !== 429 || attempt === 3) break;
  // backoff using Retry-After or attempt * 2000
  await new Promise(resolve => setTimeout(resolve, waitMs));
}
```

- **Error handling:** On 429 after retries or 402, returns `createSSETextResponse(fallbackMessage)`; on other errors returns JSON `{ error: "..." }` or 500.

---

### 5.2 `src/hooks/useChat.ts`

**Purpose:** Chat state and stream consumer: sends conversation to the Edge Function and parses SSE into assistant text for the UI.

**AI-related code:**

- **Stream request and OpenAI-format parsing:**

```ts
const CHAT_URL = `${import.meta.env.VITE_SUPABASE_URL}/functions/v1/chat`;

const streamChat = async (
  userMessages: ChatMessage[],
  onDelta: (deltaText: string) => void,
  onDone: () => void
) => {
  const resp = await fetch(CHAT_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${import.meta.env.VITE_SUPABASE_PUBLISHABLE_KEY}`,
    },
    body: JSON.stringify({ messages: userMessages }),
  });
  // ... status checks (429, 402, !resp.ok) ...
  const reader = resp.body.getReader();
  const decoder = new TextDecoder();
  let textBuffer = "";
  let streamDone = false;
  while (!streamDone) {
    const { done, value } = await reader.read();
    if (done) break;
    textBuffer += decoder.decode(value, { stream: true });
    // Parse SSE lines: "data: {...}" and "data: [DONE]"
    // Extract: parsed.choices?.[0]?.delta?.content
    if (content) onDelta(content);
  }
  onDone();
};
```

- **Invocation from user input:** `handleUserInput` builds `newHistory = [...conversationHistory, { role: "user", content: input }]`, then `await streamChat(newHistory, updateAssistantMessage, () => { setIsTyping(false); ... })`. The `updateAssistantMessage` callback appends chunks to the last bot message (or creates one), so the UI shows streamed AI output.

---

### 5.3 `src/types/chat.ts`

**Purpose:** Type definitions for chat messages. No AI logic; used by the chat UI and `useChat`.

```ts
export interface Message {
  id: string;
  role: 'user' | 'bot';
  content: string;
  timestamp: Date;
}
```

---

## 6. Prompts

There is a **single system prompt** in `supabase/functions/chat/index.ts`. The variable part is the appended `apiContext` (JSON from Tansiqy API).

**Full system prompt (Arabic):**

```text
أنت "بحر" - المساعد الذكي من موقع تنسيقي. التاريخ الحالي: 2026.

اسمك "بحر" وهو اختصار لـ "بوابة لحياة جامعية رائعة". لما حد يسألك عن اسمك أو معناه، قول "أنا بحر، واختارنا الاسم ده لأنه اختصار لـ بوابة لحياة جامعية رائعة".

أنت مستشار جامعي ذكي للطلاب المصريين من موقع تنسيقي.

أنت متصل بقاعدة بيانات تنسيقي الخلفية (https://tansiqy.runasp.net) وتستخدم البيانات الحقيقية للرد على أسئلة الطلاب.

مهمتك مساعدة الطلاب في:
- اختيار الجامعات والكليات المناسبة لمجموعهم
- معرفة درجات القبول والحد الأدنى للكليات (بناءً على البيانات المتاحة)
- مقارنة التخصصات وفرص العمل
- نصائح للتنسيق والتقديم
- معلومات عن الجامعات الخاصة والمصاريف

قاعدة مهمة جداً للدرجات:
- دايماً اذكر درجات القبول كنسبة مئوية وليس كمجموع درجات
- مثال: قول "الحد الأدنى 85%" بدلاً من "الحد الأدنى 340 درجة"
- لو الطالب قالك مجموعه بالدرجات، حوّله لنسبة مئوية في ردك

قواعد مهمة جداً للكتابة:
- لا تستخدم أي رموز تنسيق مثل النجوم ** أو * أو #
- لا تستخدم اختصارات مثل "الخ" أو "إلخ" أو "وهكذا"
- اكتب الكلام كامل ومنسق بشكل طبيعي بدون رموز
- استخدم الفقرات والأسطر الجديدة للتنظيم بدلاً من الرموز

قواعد استخدام البيانات:
- استخدم البيانات المقدمة من قاعدة البيانات فقط للرد على الأسئلة
- لو مفيش بيانات متاحة ، قول للطالب إنك هتحدث المعلومات قريباً وشجعه يتابع معانا
- لا تختلق أي بيانات أو أرقام من عندك

تحدث دائماً بالعربية المصرية (عامية مصرية). كن ودوداً ومشجعاً. 
إذا سأل الطالب عن مجموعه، اقترح له الكليات المتاحة حسب شعبته.
لا توجه الطلاب لأي مواقع خارجية، دايماً قولهم يتابعوا معانا هنا على تنسيقي.${apiContext}
```

**Fallback messages (hardcoded in same file):**

- **429 (rate limit):**  
  `"السيرفر عليه ضغط كبير دلوقتي 🙏\n\nحاول تاني بعد دقيقة، أو ابعت سؤالك بشكل أقصر شوية وأنا معاك."`
- **402 (credits):**  
  `"معلش يا صديقي، النظام مشغول دلوقتي شوية 🙈\n\nممكن تجرب تاني كمان شوية؟ أو لو عندك سؤال محدد عن جامعة معينة أو كلية، قولي وأنا هحاول أساعدك! 💪\n\nتقدر كمان تتصفح موقع تنسيقي للمعلومات اللي محتاجها."`

---

## 7. Models and Providers


| **Lovable AI Gateway** (`https://ai.gateway.lovable.dev`) | `google/gemini-3-flash-preview` | Chat completions, streaming. Authenticated with `LOVABLE_API_KEY` (Bearer). |

- No direct OpenAI, Anthropic, or local model usage in the repo.
- No embeddings or other models.

---

## 8. Data Flow

1. **User message:** Entered in UI or via quick option → `handleUserInput` / `handleQuickOption` → message appended to `conversationHistory` and `messages` state.
2. **Request:** `streamChat(conversationHistory)` POSTs `{ messages }` to Supabase Edge Function `chat`.
3. **Edge Function:**  
   - Reads last user message.  
   - Runs keyword rules (Arabic) to choose Tansiqy endpoints.  
   - Fetches from Tansiqy API (with retries).  
   - Builds `apiContext` (JSON strings).  
   - Builds `systemPrompt = staticPrompt + apiContext`.  
   - POSTs to Lovable gateway with `[system, ...messages]`, `stream: true`.  
   - On 429: retries up to 3 times; on 402 or final 429: returns SSE fallback message.  
   - On 200: returns gateway `response.body` (SSE stream).
4. **Client:** Reads stream, splits by SSE lines, parses `data: {...}` for `choices[0].delta.content`, calls `onDelta(content)` to append to current bot message.
5. **UI:** React state updates cause re-render of message list; when stream ends, `onDone()` clears typing state.

No vector DB or embedding pipeline; context is purely **keyword → Tansiqy API → string concatenation into system prompt**.

---

## 9. Dependencies

**Frontend (`package.json`):**

- No AI/LLM SDKs (no `openai`, `@anthropic-ai/sdk`, `langchain`, `llamaindex`, `@ai-sdk/*`, etc.).
- Chat uses native `fetch` and `ReadableStream` for SSE.
- Relevant: `@supabase/supabase-js` (Supabase client; auth used for Edge Function call).

**Backend (Supabase Edge Function, Deno):**

- No `npm`/`package.json` in `supabase/functions/chat/`; uses Deno `fetch` and `Deno.env.get("LOVABLE_API_KEY")`.
- Imports: `https://deno.land/std@0.168.0/http/server.ts` for `serve`.

**External services:**

- **Lovable AI Gateway:** OpenAI-compatible `/v1/chat/completions`, model `google/gemini-3-flash-preview`.
- **Tansiqy API:** `http://tansiqy.runasp.net` (university types, by type, by name, colleges, news).

---

## 10. Improvement Suggestions

### Architecture

- **Separate prompt and context builder:** Move the system prompt and the keyword→endpoint mapping into a dedicated module (e.g. `prompts.ts`, `contextBuilder.ts`) so changes don’t require editing the main handler and can be tested or localized more easily.
- **Configurable model and gateway:** Move `model` and gateway URL (and optionally API key name) to env or config so you can switch models or providers without code changes.
- **Structured context instead of raw JSON:** Pass Tansiqy data as structured sections (e.g. “University types”, “Colleges”) in the system prompt instead of one large JSON block to reduce token usage and improve model focus.

### Code / Refactoring

- **Type the Tansiqy responses:** Define interfaces for the main Tansiqy payloads and use them in `fetchFromTansiqyAPI` and when building `apiContext` to avoid mistakes and document the contract.
- **Extract keyword→endpoint map:** Replace the long if/else chain with a data structure (e.g. list of `{ keywords: string[], endpoint: string }`) and a small loop to select endpoints and fetch in parallel where possible.
- **Parallel API calls:** When multiple keyword groups match, call Tansiqy endpoints in parallel with `Promise.all` instead of sequential fetches to reduce latency.

### Performance

- **Cache Tansiqy data:** Cache responses (e.g. university types, static lists) with a short TTL (e.g. 5–15 minutes) in Supabase/KV or in-memory to reduce load on Tansiqy and speed up repeated queries.
- **Limit context size:** Cap the length of `apiContext` or summarize large JSON (e.g. only first N items + “and X more”) to stay within model context and reduce cost/latency.
- **Streaming from first byte:** Ensure the Edge Function doesn’t buffer the gateway response; current `return new Response(response.body)` is correct—keep it that way.

### Prompts

- **Put “no fabrication” and “percentages only” at the top:** So the model sees critical constraints early.
- **Few-shot examples:** Add 1–2 short examples of desired answer format (e.g. “الحد الأدنى 85%”) to stabilize output style.
- **Version prompt in code or config:** Store prompt version or hash to track which prompt is live and A/B test later.

### AI Best Practices

- **RAG with embeddings (optional):** If the Tansiqy corpus grows, consider real RAG: embed university/college descriptions, store in a vector DB, and retrieve by semantic similarity instead of (or in addition to) keyword rules.
- **Guardrails:** Validate that the model response doesn’t contain forbidden patterns (e.g. external URLs, raw scores in digits when you want percentages) before sending to the client.
- **Logging and monitoring:** Log prompt length, token usage (if gateway returns it), and latency in the Edge Function to tune context size and spot regressions.
- **Rate limiting and cost:** You already handle 429/402; consider per-user or per-session rate limits on the Edge Function to protect the gateway and control cost.

---

*Document generated from repository analysis. Last substantive code references: `supabase/functions/chat/index.ts`, `src/hooks/useChat.ts`, `src/types/chat.ts`, `package.json`, and chat UI components.*
