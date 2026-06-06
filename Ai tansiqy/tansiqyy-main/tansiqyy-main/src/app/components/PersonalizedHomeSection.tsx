import { useCallback, useEffect, useState } from "react";
import { Link, useLocation } from "react-router";
import { Sparkles, ChevronLeft } from "lucide-react";
import clsx from "clsx";
import type { University, UniversityType } from "@/app/data/mockData";
import { UniversityCard } from "@/app/components/UniversityCard";
import {
  fetchUniversityById,
  fetchUniversitiesByType,
  uiUniversityTypeToApiType,
} from "@/lib/tansiqyApi";
import {
  loadPersonalizedPrefs,
  loadRecentUniversityIds,
  setPreferredUniversityTypes,
} from "@/lib/personalizedSearchPrefs";
import { CustomUniversitySearchPanel } from "@/app/components/CustomUniversitySearchPanel";

const TYPE_CHIPS: { value: UniversityType; label: string }[] = [
  { value: "حكومية", label: "حكومية" },
  { value: "خاصة", label: "خاصة" },
  { value: "أهلية", label: "أهلية" },
  { value: "تكنولوجية", label: "تكنولوجية" },
  { value: "أجنبية", label: "أجنبية" },
  { value: "معاهد عليا", label: "معاهد عليا" },
];

export function PersonalizedHomeSection() {
  const location = useLocation();
  const [prefs, setPrefs] = useState(loadPersonalizedPrefs);
  const [suggested, setSuggested] = useState<University[]>([]);
  const [recent, setRecent] = useState<University[]>([]);
  const [loading, setLoading] = useState(true);

  const preferred = prefs.preferredTypes;

  useEffect(() => {
    if (location.hash !== "#personalized-search") return;
    requestAnimationFrame(() => {
      document.getElementById("personalized-search")?.scrollIntoView({ behavior: "smooth", block: "start" });
    });
  }, [location.pathname, location.hash]);

  const refreshPrefs = useCallback(() => {
    setPrefs(loadPersonalizedPrefs());
  }, []);

  useEffect(() => {
    refreshPrefs();
    const onStorage = (e: StorageEvent) => {
      if (e.key === "tansiqy_personalized_prefs") refreshPrefs();
    };
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, [refreshPrefs]);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      try {
        const primaryType = preferred[0];
        const list: University[] = [];
        if (primaryType) {
          const chunk = await fetchUniversitiesByType(uiUniversityTypeToApiType(primaryType));
          list.push(...chunk.slice(0, 6));
        }
        if (!cancelled) setSuggested(list);

        const ids = loadRecentUniversityIds().slice(0, 3);
        const recentList: University[] = [];
        for (const id of ids) {
          const n = Number(id);
          if (!Number.isFinite(n)) continue;
          try {
            const u = await fetchUniversityById(n);
            recentList.push(u);
          } catch {
            /* skip missing */
          }
        }
        if (!cancelled) setRecent(recentList);
      } catch {
        if (!cancelled) {
          setSuggested([]);
          setRecent([]);
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [preferred]);

  const toggleType = (t: UniversityType) => {
    const next = preferred.includes(t) ? preferred.filter((x) => x !== t) : [...preferred, t];
    setPreferredUniversityTypes(next);
    setPrefs(loadPersonalizedPrefs());
  };

  const primaryType = preferred[0];
  const searchHref =
    primaryType != null
      ? `/universities?type=${encodeURIComponent(primaryType)}`
      : "/universities";

  return (
    <section
      id="personalized-search"
      className="relative py-20 overflow-hidden border-t border-slate-200/80 scroll-mt-28"
    >
      <div className="absolute inset-0 bg-gradient-to-b from-white via-brand-cream/35 to-slate-50/90" />
      <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative">
        <div className="mb-14">
          <CustomUniversitySearchPanel />
        </div>

        <div className="h-px w-full max-w-4xl mx-auto bg-gradient-to-l from-transparent via-slate-200 to-transparent mb-14" />

        <div className="flex flex-col lg:flex-row lg:items-end lg:justify-between gap-8 mb-12">
          <div className="max-w-2xl">
            <p className="text-xs font-black uppercase tracking-widest text-brand-gold mb-2 flex items-center gap-2">
              <Sparkles className="w-4 h-4" />
              بحث مخصص لك
            </p>
            <h2 className="text-3xl md:text-4xl font-black text-brand-navy mb-3 leading-tight">
              اقتراحات سريعة
            </h2>
            <p className="text-slate-600 font-medium leading-relaxed">
              حدّد أنواع الجامعات اللي تهمك (يُحفظ على جهازك)، ونشوف لك عيّنة من الدليل. كمان نعرض آخر جامعات
              تصفّحتها.
            </p>
          </div>
          <Link
            to={searchHref}
            className="inline-flex items-center justify-center gap-2 self-start font-black text-sm text-white bg-gradient-to-l from-brand-navy to-brand-navy-mid px-6 py-3 rounded-2xl shadow-lg shadow-brand-navy/20 hover:shadow-xl transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400 focus-visible:ring-offset-2 shrink-0"
          >
            افتح الدليل الكامل
            <ChevronLeft className="w-5 h-5" />
          </Link>
        </div>

        <div className="mb-10">
          <p className="text-sm font-bold text-slate-700 mb-3">أنواع الجامعات المفضّلة عندك</p>
          <div className="flex flex-wrap gap-2">
            {TYPE_CHIPS.map((chip) => (
              <button
                key={chip.value}
                type="button"
                onClick={() => toggleType(chip.value)}
                className={clsx(
                  "px-4 py-2 rounded-full text-sm font-black border transition-all duration-200",
                  preferred.includes(chip.value)
                    ? "bg-brand-navy text-white border-brand-navy shadow-md shadow-brand-navy/20"
                    : "bg-white/90 text-slate-600 border-slate-200 hover:border-amber-300/80",
                )}
              >
                {chip.label}
              </button>
            ))}
          </div>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {[1, 2, 3].map((i) => (
              <div
                key={i}
                className="h-96 rounded-3xl bg-gradient-to-br from-slate-100 to-slate-50 border border-slate-200/80 animate-pulse"
              />
            ))}
          </div>
        ) : (
          <div className="space-y-14">
            {suggested.length > 0 && (
              <div dir="rtl">
                <h3 className="text-xl font-black text-brand-navy mb-6">
                  مقترحات حسب اختيارك
                  {primaryType ? (
                    <span className="text-slate-500 font-bold text-base mr-2">({primaryType})</span>
                  ) : null}
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                  {suggested.map((uni) => (
                    <UniversityCard key={uni.id} university={uni} />
                  ))}
                </div>
              </div>
            )}

            {recent.length > 0 && (
              <div dir="rtl">
                <h3 className="text-xl font-black text-brand-navy mb-6">آخر جامعات تصفّحتها</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                  {recent.map((uni) => (
                    <UniversityCard key={`recent-${uni.id}`} university={uni} />
                  ))}
                </div>
              </div>
            )}

            {suggested.length === 0 && recent.length === 0 && (
              <div className="rounded-3xl border border-dashed border-slate-200 bg-white/80 p-10 text-center">
                <p className="text-slate-600 font-medium max-w-lg mx-auto leading-relaxed">
                  اختر نوع جامعة من الأزرار فوق، أو تصفّح صفحة جامعة، وهنظهر لك اقتراحات وأحدث زيارات هنا.
                </p>
              </div>
            )}
          </div>
        )}
      </div>
    </section>
  );
}
