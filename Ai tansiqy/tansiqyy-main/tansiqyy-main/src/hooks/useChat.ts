import { useState, useCallback } from "react";
import type { UniversityType } from "@/app/data/mockData";
import { Message } from "@/types/chat";
import { setPreferredUniversityTypes } from "@/lib/personalizedSearchPrefs";
import { toast } from "sonner";

const generateId = () => Math.random().toString(36).substr(2, 9);

function getChatConfig(): { url: string; key: string } | null {
  const url = import.meta.env.VITE_SUPABASE_URL?.replace(/\/$/, "") ?? "";
  const key = import.meta.env.VITE_SUPABASE_PUBLISHABLE_KEY ?? "";
  if (!url || !key) return null;
  return { url: `${url}/functions/v1/chat`, key };
}

type ChatMessage = { role: "user" | "assistant"; content: string };

type ConversationStep = "university_type" | "help_type" | "free_chat";

/** يُحفظ تفضيل «بحث مخصص لك» ويُرسل للـ API سياقاً يطابق كلمات المفتاح في الخادم */
const CHAT_OPTION_TO_UNIVERSITY_TYPE: Record<string, UniversityType> = {
  "أنا مهتم بجامعات حكومية": "حكومية",
  "أنا مهتم بجامعات خاصة": "خاصة",
  "أنا مهتم بجامعات أهلية": "أهلية",
  "أنا مهتم بجامعات تكنولوجية": "تكنولوجية",
  "أنا مهتم بجامعات أجنبية": "أجنبية",
  "أنا مهتم بالمعاهد العليا": "معاهد عليا",
};

export function useChat() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const [conversationHistory, setConversationHistory] = useState<ChatMessage[]>([]);
  const [conversationStep, setConversationStep] = useState<ConversationStep>("university_type");
  const [showInput, setShowInput] = useState(false);
  const [requestCount, setRequestCount] = useState(0);

  const addBotMessage = useCallback((content: string) => {
    setMessages((prev) => [
      ...prev,
      {
        id: generateId(),
        role: "bot",
        content,
        timestamp: new Date(),
      },
    ]);
  }, []);

  const startConversation = useCallback(() => {
    const welcomeMessage =
      "أهلاً بيك في تنسيقي إيجي (Tansiqy EGY)! 👋\n\nأنا بحر، واختارنا الاسم ده لأنه اختصار لـ \"بوابة لحياة جامعية رائعة\" 🎓\n\nعشان أقدر أساعدك صح، محتاج أعرف أنت مهتم بأي نوع جامعة؟";
    addBotMessage(welcomeMessage);
    setConversationStep("university_type");
    setShowInput(false);
  }, [addBotMessage]);

  const streamChat = async (
    userMessages: ChatMessage[],
    onDelta: (deltaText: string) => void,
    onDone: () => void,
  ) => {
    const cfg = getChatConfig();
    if (!cfg) {
      toast.error("إعدادات الشات ناقصة: ضيف VITE_SUPABASE_URL و VITE_SUPABASE_PUBLISHABLE_KEY في ملف .env");
      throw new Error("Missing Supabase env");
    }

    const resp = await fetch(cfg.url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${cfg.key}`,
      },
      body: JSON.stringify({ messages: userMessages }),
    });

    if (resp.status === 429) {
      toast.error("عدد الطلبات كتير، استنى شوية وحاول تاني");
      throw new Error("Rate limited");
    }
    if (resp.status === 402) {
      toast.error("محتاج تشحن رصيد في حسابك");
      throw new Error("Payment required");
    }
    if (!resp.ok || !resp.body) {
      throw new Error("Failed to start stream");
    }

    const reader = resp.body.getReader();
    const decoder = new TextDecoder();
    let textBuffer = "";
    let streamDone = false;

    while (!streamDone) {
      const { done, value } = await reader.read();
      if (done) break;
      textBuffer += decoder.decode(value, { stream: true });

      let newlineIndex: number;
      while ((newlineIndex = textBuffer.indexOf("\n")) !== -1) {
        let line = textBuffer.slice(0, newlineIndex);
        textBuffer = textBuffer.slice(newlineIndex + 1);

        if (line.endsWith("\r")) line = line.slice(0, -1);
        if (line.startsWith(":") || line.trim() === "") continue;
        if (!line.startsWith("data: ")) continue;

        const jsonStr = line.slice(6).trim();
        if (jsonStr === "[DONE]") {
          streamDone = true;
          break;
        }

        try {
          const parsed = JSON.parse(jsonStr);
          const content = parsed.choices?.[0]?.delta?.content as string | undefined;
          if (content) onDelta(content);
        } catch {
          textBuffer = line + "\n" + textBuffer;
          break;
        }
      }
    }

    if (textBuffer.trim()) {
      for (let raw of textBuffer.split("\n")) {
        if (!raw) continue;
        if (raw.endsWith("\r")) raw = raw.slice(0, -1);
        if (raw.startsWith(":") || raw.trim() === "") continue;
        if (!raw.startsWith("data: ")) continue;
        const jsonStr = raw.slice(6).trim();
        if (jsonStr === "[DONE]") continue;
        try {
          const parsed = JSON.parse(jsonStr);
          const content = parsed.choices?.[0]?.delta?.content as string | undefined;
          if (content) onDelta(content);
        } catch {
          /* ignore partial leftovers */
        }
      }
    }

    onDone();
  };

  const handleUserInput = useCallback(
    async (input: string) => {
      setShowInput(true);
      setConversationStep("free_chat");

      const userMessage: Message = {
        id: generateId(),
        role: "user",
        content: input,
        timestamp: new Date(),
      };
      setMessages((prev) => [...prev, userMessage]);

      const newHistory: ChatMessage[] = [...conversationHistory, { role: "user", content: input }];
      setConversationHistory(newHistory);

      setIsTyping(true);
      let assistantContent = "";

      const updateAssistantMessage = (chunk: string) => {
        assistantContent += chunk;
        setMessages((prev) => {
          const last = prev[prev.length - 1];
          if (last?.role === "bot" && prev.length > 0 && prev[prev.length - 2]?.role === "user") {
            return prev.map((m, i) => (i === prev.length - 1 ? { ...m, content: assistantContent } : m));
          }
          return [
            ...prev,
            {
              id: generateId(),
              role: "bot",
              content: assistantContent,
              timestamp: new Date(),
            },
          ];
        });
      };

      try {
        setRequestCount((prev) => prev + 1);
        await streamChat(
          newHistory,
          (chunk) => updateAssistantMessage(chunk),
          () => {
            setIsTyping(false);
            setConversationHistory((prev) => [...prev, { role: "assistant", content: assistantContent }]);
          },
        );
      } catch (error) {
        console.error("Chat error:", error);
        setIsTyping(false);
        if (assistantContent === "") {
          addBotMessage("معلش حصل مشكلة، ممكن تحاول تاني؟");
        }
      }
    },
    [conversationHistory, addBotMessage],
  );

  const handleQuickOption = useCallback(
    (message: string) => {
      if (message === "__ASK_CUSTOM__") {
        setConversationStep("free_chat");
        setShowInput(true);
        return;
      }

      if (conversationStep === "help_type") {
        handleUserInput(message);
        return;
      }

      const userMessage: Message = {
        id: generateId(),
        role: "user",
        content: message,
        timestamp: new Date(),
      };
      setMessages((prev) => [...prev, userMessage]);

      const newHistory: ChatMessage[] = [...conversationHistory, { role: "user", content: message }];
      setConversationHistory(newHistory);

      if (conversationStep === "university_type") {
        const pref = CHAT_OPTION_TO_UNIVERSITY_TYPE[message];
        if (pref) setPreferredUniversityTypes([pref]);
        setTimeout(() => {
          addBotMessage("تمام! إزاي أقدر أساعدك النهاردة؟");
          setConversationStep("help_type");
        }, 500);
      }
    },
    [conversationStep, conversationHistory, addBotMessage, handleUserInput],
  );

  const resetChat = useCallback(() => {
    setMessages([]);
    setConversationHistory([]);
    setIsTyping(false);
    setConversationStep("university_type");
    setShowInput(false);
    setRequestCount(0);
    setTimeout(startConversation, 300);
  }, [startConversation]);

  return {
    messages,
    isTyping,
    conversationStep,
    showInput,
    requestCount,
    handleUserInput,
    handleQuickOption,
    startConversation,
    resetChat,
  };
}
