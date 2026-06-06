import type { UniversityType } from "@/app/data/mockData";

const STORAGE_KEY = "tansiqy_personalized_prefs";
const RECENT_KEY = "tansiqy_recent_university_ids";

export type PersonalizedPrefs = {
  /** أنواع جامعات يهتم بها المستخدم (للبحث المخصص) */
  preferredTypes: UniversityType[];
};

const defaultPrefs: PersonalizedPrefs = {
  preferredTypes: [],
};

export function loadPersonalizedPrefs(): PersonalizedPrefs {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return { ...defaultPrefs };
    const parsed = JSON.parse(raw) as Partial<PersonalizedPrefs>;
    return {
      preferredTypes: Array.isArray(parsed.preferredTypes) ? parsed.preferredTypes : [],
    };
  } catch {
    return { ...defaultPrefs };
  }
}

export function savePersonalizedPrefs(prefs: PersonalizedPrefs): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(prefs));
  } catch {
    /* ignore quota */
  }
}

export function setPreferredUniversityTypes(types: UniversityType[]): void {
  savePersonalizedPrefs({ preferredTypes: [...new Set(types)] });
}

const MAX_RECENT = 12;

/** تسجيل زيارة صفحة جامعة لاقتراحات «بحث مخصص لك» */
export function recordUniversityPageView(universityId: string): void {
  const id = universityId.trim();
  if (!id) return;
  try {
    const raw = localStorage.getItem(RECENT_KEY);
    const list: string[] = raw ? JSON.parse(raw) : [];
    const next = [id, ...list.filter((x) => x !== id)].slice(0, MAX_RECENT);
    localStorage.setItem(RECENT_KEY, JSON.stringify(next));
  } catch {
    /* ignore */
  }
}

export function loadRecentUniversityIds(): string[] {
  try {
    const raw = localStorage.getItem(RECENT_KEY);
    const list: unknown = raw ? JSON.parse(raw) : [];
    return Array.isArray(list) ? list.filter((x): x is string => typeof x === "string") : [];
  } catch {
    return [];
  }
}
