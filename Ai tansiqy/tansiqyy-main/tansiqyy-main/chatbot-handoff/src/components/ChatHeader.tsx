import { RotateCcw, Zap } from 'lucide-react';
import { Button } from './ui/button';
import { BotAvatar } from './BotAvatar';
import { ThemeToggle } from './ThemeToggle';

interface ChatHeaderProps {
  onReset: () => void;
  requestCount: number;
}

export function ChatHeader({ onReset, requestCount }: ChatHeaderProps) {
  // Rough estimate: ~$0.001 per request for Gemini Flash
  const estimatedCost = (requestCount * 0.001).toFixed(3);

  return (
    <header className="flex flex-col bg-card/80 backdrop-blur-lg border-b border-border/50 shadow-card">
      {requestCount > 0 && (
        <div className="flex items-center justify-center gap-2 px-4 py-1.5 bg-accent/50 text-xs text-muted-foreground border-b border-border/30">
          <Zap className="w-3 h-3 text-primary" />
          <span>الجلسة دي: {requestCount} طلب · ~${estimatedCost}</span>
        </div>
      )}
      <div className="flex items-center justify-between px-5 py-4">
        <div className="flex items-center gap-3">
          <BotAvatar size="md" />
          <div>
            <h1 className="font-display font-bold text-lg text-foreground leading-tight">
              تنسيقي
            </h1>
            <p className="text-xs text-muted-foreground">مساعدك الذكي لاختيار الكلية المناسبة</p>
          </div>
        </div>
        <div className="flex items-center gap-1">
          <ThemeToggle />
          <Button 
            variant="ghost" 
            size="icon" 
            onClick={onReset}
            className="text-muted-foreground hover:text-foreground"
            title="ابدأ من الأول"
          >
            <RotateCcw className="w-5 h-5" />
          </Button>
        </div>
      </div>
    </header>
  );
}
