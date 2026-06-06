import { GraduationCap } from "lucide-react";

export function TypingIndicator() {
  return (
    <div className="flex gap-3 justify-start animate-fade-in">
      <div className="flex-shrink-0 w-9 h-9 rounded-full gradient-hero flex items-center justify-center shadow-soft">
        <GraduationCap className="w-5 h-5 text-primary-foreground" />
      </div>
      <div className="px-4 py-3 rounded-2xl rounded-tl-md bg-chat-bot shadow-card">
        <div className="typing-indicator flex gap-1">
          <span className="w-2 h-2 rounded-full bg-muted-foreground/60" />
          <span className="w-2 h-2 rounded-full bg-muted-foreground/60" />
          <span className="w-2 h-2 rounded-full bg-muted-foreground/60" />
        </div>
      </div>
    </div>
  );
}
