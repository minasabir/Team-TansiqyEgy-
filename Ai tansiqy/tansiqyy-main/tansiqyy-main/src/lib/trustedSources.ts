/**
 * مصادر رسمية وطنية للتنسيق والمصاريف والقرارات (لا يُستبدل بها موقع الجامعة عند توفره).
 */
export const TRUSTED_EDUCATION_PORTALS: readonly {
  title: string;
  url: string;
  description: string;
}[] = [
  {
    title: "وزارة التعليم العالي والبحث العلمي",
    url: "https://mohesr.gov.eg/",
    description: "القرارات والأخبار الرسمية للقطاع",
  },
  {
    title: "بوابة التنسيق الإلكترونية",
    url: "https://www.tansik.egypt.gov.eg/",
    description: "التنسيق والقبول للجامعات الحكومية (حكومي)",
  },
  {
    title: "المجلس الأعلى للجامعات",
    url: "https://scu.eg/",
    description: "معلومات عامة وإطار الجامعات في مصر",
  },
] as const;

/**
 * تطبيع رابط الموقع الرسمي للجامعة/الكلية من الـ API.
 * يُرقّى `http://` إلى `https://` حتى لا تُحظر الروابط عند فتح الموقع من صفحة https (أو تُعرض كـ mixed content).
 */
export function normalizeOfficialUrl(raw: string | null | undefined): string | null {
  const s = raw?.trim();
  if (!s) return null;
  let cleaned = s.replace(/^\s+|\s+$/g, "");
  if (/^https?:\/\//i.test(cleaned)) {
    cleaned = cleaned.replace(/^http:\/\//i, "https://");
    return cleaned;
  }
  return `https://${cleaned.replace(/^\/+/, "")}`;
}

/** يفضّل موقع الكلية ثم موقع الجامعة إن وُجد في البيانات */
export function bestOfficialPageForCollege(
  college: { officialWebsite?: string | null },
  universityOfficialWebsite?: string | null,
): string | null {
  return (
    normalizeOfficialUrl(college.officialWebsite) ??
    normalizeOfficialUrl(universityOfficialWebsite)
  );
}

export function isCoordinationMissing(admissionPercentage: number): boolean {
  return admissionPercentage == null || admissionPercentage <= 0;
}

export function feesTextSuggestsMissingData(fees: string): boolean {
  return /^(غير محدد)$|لم تُذكر|^غير محدد$/i.test(fees.trim()) || /^غير محدد(\s|$)/.test(fees.trim());
}
