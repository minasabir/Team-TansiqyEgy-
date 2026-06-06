import { useState, useEffect } from "react";
import { Link, useSearchParams } from "react-router";
import type { UniversityType } from "../data/mockData";
import type { University } from "../data/mockData";
import { UniversityCard } from "../components/UniversityCard";
import {
  Search,
  Filter,
  Briefcase,
  Building2,
  Cpu,
  Globe,
  Library,
  Landmark,
  GraduationCap,
  BookOpen,
  ChevronLeft,
} from "lucide-react";
import clsx from "clsx";
import type { IntelligentSearchBuckets } from "@/lib/tansiqyApi";
import {
  fetchAllUniversities,
  fetchUniversitiesByType,
  intelligentSearchBuckets,
  uiUniversityTypeToApiType,
} from "@/lib/tansiqyApi";

const UNI_TYPES: { label: string; value: UniversityType | "الكل"; icon: React.ReactNode }[] = [
  { label: "الكل", value: "الكل", icon: <Building2 className="w-5 h-5" /> },
  { label: "حكومية", value: "حكومية", icon: <Landmark className="w-5 h-5" /> },
  { label: "خاصة", value: "خاصة", icon: <Briefcase className="w-5 h-5" /> },
  { label: "أهلية", value: "أهلية", icon: <Building2 className="w-5 h-5" /> },
  { label: "تكنولوجية", value: "تكنولوجية", icon: <Cpu className="w-5 h-5" /> },
  { label: "أجنبية", value: "أجنبية", icon: <Globe className="w-5 h-5" /> },
  { label: "معاهد عليا", value: "معاهد عليا", icon: <Library className="w-5 h-5" /> },
];

export function Universities() {
  const [searchParams, setSearchParams] = useSearchParams();
  const typeParam = searchParams.get("type") as UniversityType | null;
  const searchParam = searchParams.get("search") || "";

  const [filterType, setFilterType] = useState<UniversityType | "الكل">(typeParam || "الكل");
  const [searchQuery, setSearchQuery] = useState(searchParam);
  const [debouncedSearch, setDebouncedSearch] = useState(searchParam);

  const [universities, setUniversities] = useState<University[]>([]);
  const [searchBuckets, setSearchBuckets] = useState<IntelligentSearchBuckets | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (typeParam) {
      setFilterType(typeParam);
    } else {
      setFilterType("الكل");
    }
  }, [typeParam]);

  useEffect(() => {
    setSearchQuery(searchParam);
  }, [searchParam]);

  useEffect(() => {
    const t = window.setTimeout(() => setDebouncedSearch(searchQuery), 400);
    return () => window.clearTimeout(t);
  }, [searchQuery]);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const q = debouncedSearch.trim();
        if (q) {
          const apiType = filterType === "الكل" ? undefined : uiUniversityTypeToApiType(filterType);
          const buckets = await intelligentSearchBuckets(q, apiType);
          if (!cancelled) {
            setSearchBuckets(buckets);
            setUniversities([]);
          }
        } else if (filterType === "الكل") {
          const list = await fetchAllUniversities();
          if (!cancelled) {
            setSearchBuckets(null);
            setUniversities(list);
          }
        } else {
          const list = await fetchUniversitiesByType(uiUniversityTypeToApiType(filterType));
          if (!cancelled) {
            setSearchBuckets(null);
            setUniversities(list);
          }
        }
      } catch (e) {
        if (!cancelled) {
          setUniversities([]);
          setSearchBuckets(null);
          setError(e instanceof Error ? e.message : "تعذر تحميل الجامعات");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [filterType, debouncedSearch]);

  const handleTypeChange = (value: UniversityType | "الكل") => {
    setFilterType(value);
    const newParams = new URLSearchParams(searchParams);
    if (value === "الكل") {
      newParams.delete("type");
    } else {
      newParams.set("type", value);
    }
    setSearchParams(newParams);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setSearchQuery(val);
    const newParams = new URLSearchParams(searchParams);
    if (!val) {
      newParams.delete("search");
    } else {
      newParams.set("search", val);
    }
    setSearchParams(newParams, { replace: true });
  };

  const resetSearch = () => {
    setSearchQuery("");
    setFilterType("الكل");
    setSearchParams(new URLSearchParams());
  };

  const hasActiveSearch = debouncedSearch.trim().length > 0;
  const totalSearchHits =
    searchBuckets == null
      ? 0
      : searchBuckets.universitiesByName.length +
        searchBuckets.collegeHits.length +
        searchBuckets.departmentHits.length;

  const sortedUnisByName = searchBuckets
    ? [...searchBuckets.universitiesByName].sort((a, b) => a.nameAr.localeCompare(b.nameAr, "ar"))
    : [];
  const sortedCollegeHits = searchBuckets
    ? [...searchBuckets.collegeHits].sort((a, b) => {
        const cmpU = a.university.nameAr.localeCompare(b.university.nameAr, "ar");
        if (cmpU !== 0) return cmpU;
        return a.college.nameAr.localeCompare(b.college.nameAr, "ar");
      })
    : [];
  const sortedDeptHits = searchBuckets
    ? [...searchBuckets.departmentHits].sort((a, b) => {
        const cmpU = a.university.nameAr.localeCompare(b.university.nameAr, "ar");
        if (cmpU !== 0) return cmpU;
        const cmpC = a.college.nameAr.localeCompare(b.college.nameAr, "ar");
        if (cmpC !== 0) return cmpC;
        return a.department.localeCompare(b.department, "ar");
      })
    : [];

  const showSearchSections = hasActiveSearch && searchBuckets != null;
  const hasAnyBrowseResults = !hasActiveSearch && universities.length > 0;
  const hasAnySearchResults = showSearchSections && totalSearchHits > 0;

  return (
    <div className="flex-grow min-h-[calc(100vh-80px)]">
      <div className="relative overflow-hidden border-b border-slate-200/80 py-14 md:py-20 bg-gradient-to-b from-white via-brand-cream/40 to-slate-50/90">
        <div className="absolute top-0 right-0 w-96 h-96 rounded-full bg-amber-200/20 blur-3xl -translate-y-1/2" />
        <div className="absolute bottom-0 left-0 w-72 h-72 rounded-full bg-brand-navy/5 blur-3xl" />
        <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
          <div className="max-w-3xl">
            <p className="text-xs font-black uppercase tracking-[0.2em] text-brand-gold mb-3">الدليل الأكاديمي</p>
            <h1 className="text-3xl md:text-5xl font-black text-brand-navy mb-5 tracking-tight leading-tight">
              استكشف الجامعات
            </h1>
            <p className="text-lg md:text-xl text-slate-600 font-medium leading-relaxed max-w-3xl">
              نحن منصة تجمع معلوماتها من مواقع موثوقة، تسهيلاً على الطالب في تجاوز صعوبة الوصول إلى المعلومات —
              مع بحث موحّد للجامعات والكليات والأقسام.
            </p>
          </div>
        </div>
      </div>

      <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="flex flex-col lg:flex-row gap-8 items-start">
          <div className="w-full lg:w-72 flex-shrink-0 rounded-3xl p-6 lg:sticky lg:top-28 bg-white/90 backdrop-blur-md border border-slate-200/80 shadow-[0_12px_40px_-16px_rgba(10,22,40,0.12)]">
            <h2 className="text-lg font-black text-brand-navy mb-6 flex items-center gap-2">
              <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-brand-navy to-brand-navy-mid text-white shadow-md">
                <Filter className="w-4 h-4" />
              </span>
              تصفية النتائج
            </h2>

            <div className="space-y-6">
              <div>
                <label className="block text-sm font-semibold text-slate-700 mb-3">بحث بالاسم</label>
                <div className="relative">
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={handleSearchChange}
                    placeholder="جامعة، كلية، قسم أو تخصص..."
                    className="w-full pl-4 pr-10 py-3.5 bg-brand-cream/50 border border-slate-200/90 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-amber-400/40 focus:border-brand-navy/30 transition-all font-semibold"
                    dir="rtl"
                  />
                  <Search className="w-4 h-4 text-slate-400 absolute right-3 top-3.5" />
                </div>
              </div>

              <div className="h-px w-full bg-slate-100"></div>

              <div>
                <label className="block text-sm font-semibold text-slate-700 mb-3">نوع الجامعة</label>
                <div className="flex flex-col gap-2">
                  {UNI_TYPES.map((type) => (
                    <button
                      key={type.value}
                      onClick={() => handleTypeChange(type.value)}
                      className={clsx(
                        "w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium transition-all duration-200 text-right",
                        filterType === type.value
                          ? "bg-brand-navy text-white shadow-lg shadow-brand-navy/20 border border-brand-navy"
                          : "bg-transparent text-slate-600 hover:bg-slate-50 hover:text-brand-navy border border-transparent",
                      )}
                    >
                      <div
                        className={clsx(
                          "p-1.5 rounded-lg",
                          filterType === type.value
                            ? "bg-white/20 text-white"
                            : "bg-slate-100 text-slate-500",
                        )}
                      >
                        {type.icon}
                      </div>
                      {type.label}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </div>

          <div className="flex-grow w-full">
            <div className="mb-6 flex justify-between items-center flex-wrap gap-2">
              <p className="text-slate-500 font-medium">
                {loading ? (
                  <>جاري التحميل…</>
                ) : hasActiveSearch ? (
                  <>
                    <span className="font-bold text-slate-800">{totalSearchHits}</span> نتيجة إجمالية:{" "}
                    <span className="text-slate-600">
                      {searchBuckets ? (
                        <>
                          <span className="font-semibold text-slate-800">
                            {searchBuckets.universitiesByName.length}
                          </span>{" "}
                          جامعة،{" "}
                          <span className="font-semibold text-slate-800">
                            {searchBuckets.collegeHits.length}
                          </span>{" "}
                          كلية،{" "}
                          <span className="font-semibold text-slate-800">
                            {searchBuckets.departmentHits.length}
                          </span>{" "}
                          قسم/تخصص
                        </>
                      ) : null}
                    </span>
                  </>
                ) : (
                  <>
                    عرض <span className="font-bold text-slate-800">{universities.length}</span> جامعة
                  </>
                )}
              </p>
              {error && <p className="text-sm text-red-600 font-medium">{error}</p>}
            </div>

            {loading ? (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                {[1, 2, 3, 4, 5, 6].map((i) => (
                  <div
                    key={i}
                    className="h-80 rounded-3xl bg-gradient-to-br from-slate-100 to-slate-50 border border-slate-200/80 animate-pulse"
                  />
                ))}
              </div>
            ) : hasAnySearchResults ? (
              <div className="space-y-14">
                {sortedUnisByName.length > 0 && (
                  <section dir="rtl">
                    <div className="flex items-center gap-3 mb-6">
                      <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-brand-navy to-brand-navy-mid text-white shadow-lg shadow-brand-navy/25">
                        <Landmark className="w-5 h-5" />
                      </div>
                      <div>
                        <h2 className="text-xl font-black text-brand-navy">جامعات</h2>
                        <p className="text-sm text-slate-500 font-medium">تطابق اسم الجامعة</p>
                      </div>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                      {sortedUnisByName.map((uni) => (
                        <UniversityCard key={`uni-${uni.id}`} university={uni} />
                      ))}
                    </div>
                  </section>
                )}

                {sortedCollegeHits.length > 0 && (
                  <section dir="rtl">
                    <div className="flex items-center gap-3 mb-6">
                      <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-violet-600 to-violet-800 text-white shadow-lg shadow-violet-900/20">
                        <GraduationCap className="w-5 h-5" />
                      </div>
                      <div>
                        <h2 className="text-xl font-black text-brand-navy">كليات</h2>
                        <p className="text-sm text-slate-500 font-medium">تطابق اسم الكلية</p>
                      </div>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      {sortedCollegeHits.map(({ university, college }) => (
                        <Link
                          key={`col-${college.id}`}
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
                      ))}
                    </div>
                  </section>
                )}

                {sortedDeptHits.length > 0 && (
                  <section dir="rtl">
                    <div className="flex items-center gap-3 mb-6">
                      <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-teal-600 to-emerald-800 text-white shadow-lg shadow-teal-900/20">
                        <BookOpen className="w-5 h-5" />
                      </div>
                      <div>
                        <h2 className="text-xl font-black text-brand-navy">أقسام وتخصصات</h2>
                        <p className="text-sm text-slate-500 font-medium">تطابق اسم القسم ضمن بيانات الكلية</p>
                      </div>
                    </div>
                    <ul className="space-y-3">
                      {sortedDeptHits.map(({ university, college, department }) => (
                        <li
                          key={`${college.id}-${department}`}
                          className="rounded-3xl border border-slate-200/90 bg-white p-5 shadow-[0_8px_28px_-12px_rgba(10,22,40,0.1)] flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 transition-shadow hover:shadow-md"
                        >
                          <div>
                            <p className="font-black text-brand-navy">{department}</p>
                            <p className="text-sm text-slate-600 font-medium mt-1">
                              {college.nameAr}
                              <span className="text-slate-400 mx-1">·</span>
                              {university.nameAr}
                            </p>
                          </div>
                          <Link
                            to={`/colleges/${college.id}`}
                            className="shrink-0 inline-flex items-center gap-1 text-sm font-black text-teal-700 hover:text-brand-navy transition-colors"
                          >
                            صفحة الكلية
                            <ChevronLeft className="w-4 h-4" />
                          </Link>
                        </li>
                      ))}
                    </ul>
                  </section>
                )}
              </div>
            ) : hasAnyBrowseResults ? (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                {universities.map((uni) => (
                  <UniversityCard key={uni.id} university={uni} />
                ))}
              </div>
            ) : (
              <div className="bg-white/95 backdrop-blur-sm rounded-3xl p-14 md:p-16 text-center shadow-[0_20px_50px_-20px_rgba(10,22,40,0.12)] border border-slate-200/80">
                <div className="w-20 h-20 bg-gradient-to-br from-brand-cream to-amber-100/80 rounded-2xl flex items-center justify-center mx-auto mb-6 ring-4 ring-amber-200/30">
                  <Search className="w-9 h-9 text-brand-navy/40" />
                </div>
                <h3 className="text-2xl font-black text-brand-navy mb-2">لا توجد نتائج</h3>
                <p className="text-slate-500 font-medium max-w-md mx-auto leading-relaxed">
                  لم نعثر على جامعة أو كلية أو قسم يطابق البحث. جرّب كلمات أخرى أو غيّر نوع الجامعة.
                </p>
                <button
                  type="button"
                  onClick={resetSearch}
                  className="mt-8 px-8 py-3.5 bg-gradient-to-l from-brand-navy to-brand-navy-mid text-white font-black rounded-2xl shadow-lg shadow-brand-navy/25 hover:shadow-xl transition-all"
                >
                  إعادة ضبط البحث
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
