import { useEffect, useRef } from "react";
import { ChatHeader } from "@/components/chat/ChatHeader";
import { ChatMessage } from "@/components/chat/ChatMessage";
import { ChatInput } from "@/components/chat/ChatInput";
import { TypingIndicator } from "@/components/chat/TypingIndicator";
import {
  QuickOptions,
  universityTypeOptions,
  helpTypeOptions,
} from "@/components/chat/QuickOptions";
import { useChat } from "@/hooks/useChat";

export function ChatBot() {
  const {
    messages,
    isTyping,
    conversationStep,
    showInput,
    requestCount,
    handleUserInput,
    handleQuickOption,
    startConversation,
    resetChat,
  } = useChat();
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    startConversation();
  }, [startConversation]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isTyping]);

  const getCurrentOptions = () => {
    switch (conversationStep) {
      case "university_type":
        return { options: universityTypeOptions, title: "اختار نوع الجامعة اللي يهمك:" };
      case "help_type":
        return { options: helpTypeOptions, title: "إزاي أقدر أساعدك؟" };
      default:
        return null;
    }
  };

  const currentOptions = getCurrentOptions();

  return (
    <div className="flex flex-col flex-1 min-h-[calc(100dvh-14rem)] md:min-h-[calc(100dvh-16rem)] bg-background rounded-2xl border border-border/60 overflow-hidden shadow-sm">
      <ChatHeader onReset={resetChat} requestCount={requestCount} />

      <div className="flex-1 overflow-y-auto min-h-0 gradient-subtle">
        <div className="max-w-3xl mx-auto p-4 md:p-6 space-y-5 pb-8">
          {messages.map((message) => (
            <ChatMessage key={message.id} message={message} />
          ))}

          {isTyping && <TypingIndicator />}

          {currentOptions && !isTyping && messages.length > 0 && (
            <div className="pt-2 animate-fade-in">
              <QuickOptions
                options={currentOptions.options}
                title={currentOptions.title}
                onSelect={handleQuickOption}
                disabled={isTyping}
              />
            </div>
          )}

          <div ref={messagesEndRef} />
        </div>
      </div>

      {showInput && (
        <div className="max-w-3xl mx-auto w-full px-2 md:px-0 animate-fade-in shrink-0">
          <ChatInput onSend={handleUserInput} disabled={isTyping} />
        </div>
      )}
    </div>
  );
}
