interface BotAvatarProps {
  size?: "sm" | "md" | "lg";
}

export function BotAvatar({ size = "md" }: BotAvatarProps) {
  const sizeClasses = {
    sm: "w-10 h-10 text-xl",
    md: "w-11 h-11 text-2xl",
    lg: "w-14 h-14 text-3xl",
  };

  return (
    <div
      className={`${sizeClasses[size]} rounded-2xl gradient-hero flex items-center justify-center shadow-soft ring-2 ring-white/20`}
    >
      <span role="img" aria-label="مستشار" className="drop-shadow-sm">
        🧑‍🎓
      </span>
    </div>
  );
}
