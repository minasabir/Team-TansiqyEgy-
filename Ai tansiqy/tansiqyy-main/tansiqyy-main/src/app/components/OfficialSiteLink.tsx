import type { ReactNode } from "react";
import { ExternalLink } from "lucide-react";
import { normalizeOfficialUrl } from "@/lib/trustedSources";

interface OfficialSiteLinkProps {
  href: string;
  className?: string;
  children?: ReactNode;
}

/** رابط خارجي لموقع رسمي (يفتح في تاب جديد) */
export function OfficialSiteLink({ href, className = "", children }: OfficialSiteLinkProps) {
  const url = normalizeOfficialUrl(href) ?? href;
  return (
    <a
      href={url}
      target="_blank"
      rel="noopener noreferrer"
      className={`inline-flex items-center gap-1.5 font-bold text-emerald-700 hover:text-emerald-900 underline-offset-2 hover:underline text-sm ${className}`}
    >
      {children ?? "الموقع الرسمي"}
      <ExternalLink className="w-3.5 h-3.5 shrink-0 opacity-80" />
    </a>
  );
}
