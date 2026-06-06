import { useState, KeyboardEvent } from 'react';
import { Button } from './ui/button';
import { Send } from 'lucide-react';

interface ChatInputProps {
  onSend: (message: string) => void;
  placeholder?: string;
  disabled?: boolean;
}

export function ChatInput({ onSend, placeholder = "اكتب إجابتك...", disabled }: ChatInputProps) {
  const [input, setInput] = useState('');

  const handleSend = () => {
    if (input.trim() && !disabled) {
      onSend(input.trim());
      setInput('');
    }
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="flex gap-3 p-4 bg-card/80 backdrop-blur-lg border-t border-border/50" dir="rtl">
      <input
        type="text"
        value={input}
        onChange={(e) => setInput(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        disabled={disabled}
        className="flex-1 px-5 py-4 rounded-2xl bg-muted/70 border border-border/50 text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary/50 focus:bg-card transition-all duration-300 text-right"
        dir="rtl"
      />
      <Button 
        onClick={handleSend} 
        disabled={!input.trim() || disabled}
        size="icon"
        className="w-14 h-14 rounded-2xl gradient-hero hover:opacity-90 hover:scale-105 transition-all duration-300 shadow-soft disabled:opacity-50 disabled:scale-100"
      >
        <Send className="w-5 h-5 rotate-180" />
      </Button>
    </div>
  );
}
