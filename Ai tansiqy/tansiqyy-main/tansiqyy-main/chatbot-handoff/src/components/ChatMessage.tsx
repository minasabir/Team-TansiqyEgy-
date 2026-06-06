import { Message } from '@/types/chat';
import { User } from 'lucide-react';
import { BotAvatar } from './BotAvatar';

interface ChatMessageProps {
  message: Message;
}

export function ChatMessage({ message }: ChatMessageProps) {
  const isBot = message.role === 'bot';

  return (
    <div 
      className={`flex gap-3 ${isBot ? 'justify-start animate-slide-in-right' : 'justify-end animate-slide-in-left'}`}
      dir="rtl"
    >
      {isBot && (
        <div className="flex-shrink-0 animate-pop-in">
          <BotAvatar size="sm" />
        </div>
      )}
      
      <div className="flex flex-col gap-3 max-w-[85%] md:max-w-[70%]">
        <div
          className={`px-5 py-4 rounded-3xl transition-all duration-300 hover:scale-[1.01] ${
            isBot 
              ? 'bg-card text-chat-bot-foreground rounded-tr-lg shadow-card border border-border/50 hover:shadow-lg' 
              : 'gradient-hero text-chat-user-foreground rounded-tl-lg shadow-soft hover:shadow-glow'
          }`}
        >
          <p className="text-[15px] leading-relaxed whitespace-pre-wrap text-right">{message.content}</p>
        </div>
      </div>

      {!isBot && (
        <div className="flex-shrink-0 w-9 h-9 rounded-full bg-muted flex items-center justify-center">
          <User className="w-5 h-5 text-muted-foreground" />
        </div>
      )}
    </div>
  );
}
