import { normalizeOfficialUrl } from "@/lib/trustedSources";

/** يستخرج اسم النطاق من رابط الموقع الرسمي للجامعة لاستخدامه في شعار/أيقونة من نفس المصدر */
export function officialWebsiteHostname(officialWebsite: string | null | undefined): string | null {
  const url = normalizeOfficialUrl(officialWebsite);
  if (!url) return null;
  try {
    const u = new URL(url);
    return u.hostname.replace(/^www\./, "") || null;
  } catch {
    return null;
  }
}

/** شعار من نطاق الموقع الرسمي (أيقونة الموقع عالي الدقة — مصدرها نطاق الجامعة نفسه) */
export function buildOfficialLogoUrlFromWebsite(officialWebsite: string | null | undefined): string | null {
  const host = officialWebsiteHostname(officialWebsite);
  if (!host) return null;
  return `https://www.google.com/s2/favicons?domain=${encodeURIComponent(host)}&sz=128`;
}

/** صورة غلاف بديلة: نفس المصدر كخلفية كبيرة مع تمويه في الواجهة عند الحاجة */
export function buildOfficialCoverIconUrl(officialWebsite: string | null | undefined): string | null {
  const host = officialWebsiteHostname(officialWebsite);
  if (!host) return null;
  return `https://www.google.com/s2/favicons?domain=${encodeURIComponent(host)}&sz=256`;
}
