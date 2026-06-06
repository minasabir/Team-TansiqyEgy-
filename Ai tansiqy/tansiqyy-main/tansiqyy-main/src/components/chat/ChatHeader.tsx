import { RotateCcw, Zap } from "lucide-react";
import { Button } from "@/app/components/ui/button";
import { ThemeToggle } from "./ThemeToggle";

interface ChatHeaderProps {
  onReset: () => void;
  requestCount: number;
}

export function ChatHeader({ onReset, requestCount }: ChatHeaderProps) {
  const estimatedCost = (requestCount * 0.001).toFixed(3);

  return (
    <header className="flex flex-col bg-card/80 backdrop-blur-lg border-b border-border/50 shadow-card">
      {requestCount > 0 && (
        <div className="flex items-center justify-center gap-2 px-4 py-1.5 bg-accent/50 text-xs text-muted-foreground border-b border-border/30">
          <Zap className="w-3 h-3 text-primary" />
          <span>
            الجلسة دي: {requestCount} طلب · ~${estimatedCost}
          </span>
        </div>
      )}
      <div className="flex items-center justify-between px-5 py-4">
        <div className="flex items-center gap-3">
          <img
            src="/logo.png"
            alt=""
            className="h-10 w-10 rounded-full object-cover ring-2 ring-border shrink-0 bg-background"
            width={40}
            height={40}
          />
          <div className="min-w-0 text-right">
            <h1 className="font-display font-black text-lg text-foreground leading-tight tracking-tight">تنسيقي إيجي</h1>
            <p className="text-[11px] font-semibold text-muted-foreground leading-snug">
              <span className="text-amber-700/90 dark:text-amber-400/90">Tansiqy EGY</span>
              {" · "}
              مساعدك لاختيار الكلية
            </p>
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
