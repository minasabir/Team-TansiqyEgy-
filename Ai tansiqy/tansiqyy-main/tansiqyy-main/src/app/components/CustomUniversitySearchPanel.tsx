import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router";
import {
  Search,
  SlidersHorizontal,
  GraduationCap,
  ChevronLeft,
  Sparkles,
  MapPin,
  Loader2,
} from "lucide-react";
import { toast } from "sonner";
import type { UniversityType } from "@/app/data/mockData";
import type { College, University } from "@/app/data/mockData";
import { UniversityCard } from "@/app/components/UniversityCard";
import { Input } from "@/app/components/ui/input";
import { Label } from "@/app/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/app/components/ui/select";
import { fetchAllUniversityViewModels } from "@/lib/tansiqyApi";
import {
  runCustomUniversitySearch,
  uniqueGovernoratesFromVms,
  type CustomSearchStudyBranch,
  type CustomUniversitySearchFilters,
  type CustomUniversitySearchResults,
  hasCollegeLevelFilter,
} from "@/lib/customUniversitySearch";

const UNI_TYPE_OPTIONS: { value: UniversityType | "الكل"; label: string }[] = [
  { value: "الكل", label: "كل الأنواع" },
  { value: "حكومية", label: "حكومية" },
  { value: "خاصة", label: "خاصة" },
  { value: "أهلية", label: "أهلية" },
  { value: "تكنولوجية", label: "تكنولوجية" },
  { value: "أجنبية", label: "أجنبية" },
  { value: "معاهد عليا", label: "معاهد عليا" },
];

const STUDY_OPTIONS: { value: CustomSearchStudyBranch; label: string }[] = [
  { value: "كل الشعب", label: "كل الشعب" },
  { value: "علمي علوم", label: "علمي علوم" },
  { value: "علمي رياضة", label: "علمي رياضة" },
  { value: "أدبي", label: "أدبي" },
];

function buildFilters(
  universityName: string,
  collegeName: string,
  universityType: UniversityType | "الكل",
  studyBranch: CustomSearchStudyBranch,
  governorate: string,
  feesMin: number,
  feesMax: number,
  coordinationMin: number,
  coordinationMax: number,
): CustomUniversitySearchFilters {
  return {
    universityName,
    collegeName,
    universityType,
    studyBranch,
    governorate,
    feesMin,
    feesMax,
    coordinationMin,
    coordinationMax,
  };
}

export function CustomUniversitySearchPanel() {
  const [universityName, setUniversityName] = useState("");
  const [collegeName, setCollegeName] = useState("");
  const [universityType, setUniversityType] = useState<UniversityType | "الكل">("الكل");
  const [studyBranch, setStudyBranch] = useState<CustomSearchStudyBranch>("كل الشعب");
  const [governorate, setGovernorate] = useState("كل المحافظات");
  const [governorates, setGovernorates] = useState<string[]>(["كل المحافظات"]);
  const [feesMin, setFeesMin] = useState(0);
  const [feesMax, setFeesMax] = useState(5_000_000);
  const [coordinationMin, setCoordinationMin] = useState(0);
  const [coordinationMax, setCoordinationMax] = useState(100);
  const [loadingGovs, setLoadingGovs] = useState(true);
  const [searching, setSearching] = useState(false);
  const [results, setResults] = useState<CustomUniversitySearchResults | null>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const vms = await fetchAllUniversityViewModels();
        if (!cancelled) {
          setGovernorates(["كل المحافظات", ...uniqueGovernoratesFromVms(vms)]);
        }
      } catch {
        if (!cancelled) setGovernorates(["كل المحافظات"]);
      } finally {
        if (!cancelled) setLoadingGovs(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const onSearch = useCallback(async () => {
    const filters = buildFilters(
      universityName,
      collegeName,
      universityType,
      studyBranch,
      governorate,
      feesMin,
      feesMax,
      coordinationMin,
      coordinationMax,
    );
    setSearching(true);
    setResults(null);
    try {
      const data = await runCustomUniversitySearch(filters);
      setResults(data);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "تعذر تنفيذ البحث");
      setResults(null);
    } finally {
      setSearching(false);
    }
  }, [
    universityName,
    collegeName,
    universityType,
    studyBranch,
    governorate,
    feesMin,
    feesMax,
    coordinationMin,
    coordinationMax,
  ]);

  const filters = buildFilters(
    universityName,
    collegeName,
    universityType,
    studyBranch,
    governorate,
    feesMin,
    feesMax,
    coordinationMin,
    coordinationMax,
  );
  const collegeFilterActive = hasCollegeLevelFilter(filters);

  return (
    <div className="relative" dir="rtl">
      <div className="absolute -inset-[1px] rounded-[1.75rem] bg-gradient-to-l from-amber-400/50 via-brand-navy/25 to-violet-500/40 blur-sm opacity-80" />
      <div className="relative rounded-[1.65rem] border border-white/60 bg-white/95 backdrop-blur-xl shadow-[0_24px_64px_-24px_rgba(10,22,40,0.25)] overflow-hidden">
        <div className="absolute inset-x-0 top-0 h-px bg-gradient-to-l from-transparent via-amber-300/60 to-transparent" />
        <div className="p-6 sm:p-8 md:p-10">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
            <div className="flex items-start gap-4">
              <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-brand-navy to-brand-navy-mid text-white shadow-lg shadow-brand-navy/30 ring-4 ring-amber-200/30">
                <SlidersHorizontal className="w-7 h-7" strokeWidth={2.2} />
              </div>
              <div>
                <p className="text-xs font-black uppercase tracking-widest text-brand-gold mb-1 flex items-center gap-1.5">
                  <Sparkles className="w-3.5 h-3.5" />
                  دليل تنسيقي
                </p>
                <h3 className="text-2xl sm:text-3xl font-black text-brand-navy leading-tight">
                  البحث المخصص عن الجامعات والكليات
                </h3>
                <p className="text-sm text-slate-500 font-medium mt-2 max-w-xl leading-relaxed">
                  صفِّ حسب النوع والمحافظة والتنسيق والمصاريف — البيانات من قاعدة تنسيقي.
                </p>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-5 md:gap-6 mb-6">
            <div className="space-y-2">
              <Label className="text-sm font-bold text-brand-navy">اسم الجامعة</Label>
              <Input
                value={universityName}
                onChange={(e) => setUniversityName(e.target.value)}
                placeholder="ابحث باسم الجامعة…"
                className="h-12 rounded-xl border-slate-200/90 bg-brand-cream/40 text-base font-semibold placeholder:text-slate-400 focus-visible:ring-amber-400/40"
              />
            </div>
            <div className="space-y-2">
              <Label className="text-sm font-bold text-brand-navy">اسم الكلية</Label>
              <Input
                value={collegeName}
                onChange={(e) => setCollegeName(e.target.value)}
                placeholder="مثال: هندسة، طب، علوم…"
                className="h-12 rounded-xl border-slate-200/90 bg-brand-cream/40 text-base font-semibold placeholder:text-slate-400 focus-visible:ring-amber-400/40"
              />
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 md:gap-6 mb-6">
            <div className="space-y-2">
              <Label className="text-sm font-bold text-brand-navy">نوع الجامعة</Label>
              <Select
                value={universityType}
                onValueChange={(v) => setUniversityType(v as UniversityType | "الكل")}
              >
                <SelectTrigger className="h-12 rounded-xl border-slate-200/90 bg-brand-cream/40 font-semibold text-brand-navy">
                  <SelectValue placeholder="النوع" />
                </SelectTrigger>
                <SelectContent>
                  {UNI_TYPE_OPTIONS.map((o) => (
                    <SelectItem key={o.value} value={o.value} className="font-medium">
                      {o.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label className="text-sm font-bold text-brand-navy">نوع الدراسة (شعبة تقريبية)</Label>
              <Select value={studyBranch} onValueChange={(v) => setStudyBranch(v as CustomSearchStudyBranch)}>
                <SelectTrigger className="h-12 rounded-xl border-slate-200/90 bg-brand-cream/40 font-semibold text-brand-navy">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {STUDY_OPTIONS.map((o) => (
                    <SelectItem key={o.value} value={o.value} className="font-medium">
                      {o.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2 sm:col-span-2 lg:col-span-1">
              <Label className="text-sm font-bold text-brand-navy flex items-center gap-1.5">
                <MapPin className="w-4 h-4 text-brand-gold" />
                المحافظة
              </Label>
              <Select
                value={governorate}
                onValueChange={setGovernorate}
                disabled={loadingGovs}
              >
                <SelectTrigger className="h-12 rounded-xl border-slate-200/90 bg-brand-cream/40 font-semibold text-brand-navy">
                  <SelectValue placeholder={loadingGovs ? "جاري التحميل…" : "المحافظة"} />
                </SelectTrigger>
                <SelectContent className="max-h-60">
                  {governorates.map((g) => (
                    <SelectItem key={g} value={g} className="font-medium">
                      {g}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
            <div className="rounded-2xl border border-slate-200/80 bg-gradient-to-br from-slate-50/90 to-white p-5 sm:p-6">
              <p className="text-sm font-black text-brand-navy mb-4">المصاريف (ج.م / تقدير سنوي)</p>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label className="text-xs font-bold text-slate-600">من</Label>
                  <Input
                    type="number"
                    min={0}
                    value={feesMin || ""}
                    onChange={(e) => setFeesMin(Number(e.target.value) || 0)}
                    className="h-11 rounded-xl font-bold"
                  />
                </div>
                <div className="space-y-2">
                  <Label className="text-xs font-bold text-slate-600">إلى</Label>
                  <Input
                    type="number"
                    min={0}
                    value={feesMax || ""}
                    onChange={(e) => setFeesMax(Number(e.target.value) || 0)}
                    className="h-11 rounded-xl font-bold"
                  />
                </div>
              </div>
            </div>
            <div className="rounded-2xl border border-slate-200/80 bg-gradient-to-br from-amber-50/50 via-white to-brand-cream/40 p-5 sm:p-6">
              <p className="text-sm font-black text-brand-navy mb-4">التنسيق (نسبة مئوية تقريبية)</p>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label className="text-xs font-bold text-slate-600">من</Label>
                  <Input
                    type="number"
                    min={0}
                    max={100}
                    value={coordinationMin || ""}
                    onChange={(e) => setCoordinationMin(Number(e.target.value) || 0)}
                    className="h-11 rounded-xl font-bold"
                  />
                </div>
                <div className="space-y-2">
                  <Label className="text-xs font-bold text-slate-600">إلى</Label>
                  <Input
                    type="number"
                    min={0}
                    max={100}
                    value={coordinationMax || ""}
                    onChange={(e) => setCoordinationMax(Number(e.target.value) || 0)}
                    className="h-11 rounded-xl font-bold"
                  />
                </div>
              </div>
            </div>
          </div>

          <button
            type="button"
            onClick={onSearch}
            disabled={searching}
            className="w-full flex items-center justify-center gap-3 py-4 rounded-2xl text-base font-black text-white bg-gradient-to-l from-brand-navy via-brand-navy-mid to-violet-800 shadow-[0_16px_40px_-12px_rgba(10,22,40,0.45)] hover:shadow-[0_20px_48px_-12px_rgba(10,22,40,0.5)] hover:from-brand-navy-mid hover:to-violet-900 transition-all duration-300 disabled:opacity-60 disabled:pointer-events-none border border-white/10"
          >
            {searching ? (
              <Loader2 className="w-6 h-6 animate-spin" />
            ) : (
              <Search className="w-6 h-6" strokeWidth={2.5} />
            )}
            {searching ? "جاري البحث…" : "ابدأ البحث المخصص الآن"}
          </button>

          {collegeFilterActive && (
            <p className="text-center text-xs text-slate-500 font-medium mt-3">
              البحث يمر على الكليات عند تفعيل شعبة أو مصاريف أو تنسيق أو اسم كلية — قد يستغرق ثوانٍ.
            </p>
          )}
        </div>
      </div>

      {results ? (
        <SearchResultsSection results={results} collegeFilterActive={collegeFilterActive} />
      ) : null}
    </div>
  );
}

function SearchResultsSection({
  results,
  collegeFilterActive,
}: {
  results: CustomUniversitySearchResults;
  collegeFilterActive: boolean;
}) {
  const { universities, collegeHits } = results;
  const sortedHits = [...collegeHits].sort((a, b) => {
    const cu = a.university.nameAr.localeCompare(b.university.nameAr, "ar");
    if (cu !== 0) return cu;
    return a.college.nameAr.localeCompare(b.college.nameAr, "ar");
  });
  const sortedUnis = [...universities].sort((a, b) => a.nameAr.localeCompare(b.nameAr, "ar"));

  const total = sortedUnis.length + sortedHits.length;

  if (total === 0) {
    return (
      <div className="mt-10 rounded-3xl border border-dashed border-slate-300 bg-white/70 p-12 text-center">
        <p className="text-lg font-black text-brand-navy mb-2">لا توجد نتائج بعد</p>
        <p className="text-slate-600 font-medium max-w-md mx-auto">
          عدّل المعايير واضغط البحث مرة أخرى.
        </p>
      </div>
    );
  }

  return (
    <div className="mt-12 space-y-12">
      {results.truncated ? (
        <p className="text-sm text-amber-800 font-bold text-center bg-amber-50 border border-amber-200/80 rounded-2xl py-3 px-4">
          عُرضت نتائج جزئية لسرعة التحميل. اضغط بحثاً أضيقاً (نوع جامعة، محافظة، أو اسم) لدقة أعلى.
        </p>
      ) : null}

      {collegeFilterActive && sortedHits.length > 0 ? (
        <section>
          <div className="flex items-center gap-3 mb-6">
            <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-violet-600 to-violet-900 text-white shadow-lg">
              <GraduationCap className="w-5 h-5" />
            </div>
            <div>
              <h4 className="text-xl font-black text-brand-navy">كليات مطابقة</h4>
              <p className="text-sm text-slate-500 font-medium">حسب المعايير التي اخترتها</p>
            </div>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {sortedHits.map(({ university, college }) => (
              <CollegeHitCard key={college.id} university={university} college={college} />
            ))}
          </div>
        </section>
      ) : null}

      {sortedUnis.length > 0 ? (
        <section>
          <div className="flex items-center gap-3 mb-6">
            <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-brand-navy to-brand-navy-mid text-white shadow-lg">
              <Sparkles className="w-5 h-5" />
            </div>
            <div>
              <h4 className="text-xl font-black text-brand-navy">
                {collegeFilterActive && sortedHits.length > 0 ? "جامعات ذات كليات مطابقة" : "جامعات مطابقة"}
              </h4>
              <p className="text-sm text-slate-500 font-medium">{sortedUnis.length} جامعة</p>
            </div>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
            {sortedUnis.map((uni) => (
              <UniversityCard key={uni.id} university={uni} />
            ))}
          </div>
        </section>
      ) : null}
    </div>
  );
}

function CollegeHitCard({ university, college }: { university: University; college: College }) {
  return (
    <Link
      to={`/colleges/${college.id}`}
      className="group flex flex-col rounded-3xl border border-slate-200/90 bg-white p-5 shadow-[0_8px_30px_-12px_rgba(10,22,40,0.1)] transition-all duration-300 hover:border-amber-200/70 hover:shadow-[0_16px_40px_-14px_rgba(10,22,40,0.15)] hover:-translate-y-0.5"
    >
      <div className="flex items-start justify-between gap-3 mb-2">
        <div>
          <p className="text-lg font-black text-brand-navy group-hover:text-violet-700 transition-colors">
            {college.nameAr}
          </p>
          {college.nameEn ? (
            <p className="text-xs text-slate-500 mt-0.5" dir="ltr">
              {college.nameEn}
            </p>
          ) : null}
        </div>
        <ChevronLeft className="w-5 h-5 text-slate-300 group-hover:text-violet-600 shrink-0 mt-1" />
      </div>
      <p className="text-sm font-semibold text-slate-600 mt-auto pt-3 border-t border-slate-100">
        {university.nameAr}
        <span className="text-slate-400 font-medium mx-1">·</span>
        <span className="text-xs font-bold text-slate-500">{university.type}</span>
      </p>
    </Link>
  );
}
