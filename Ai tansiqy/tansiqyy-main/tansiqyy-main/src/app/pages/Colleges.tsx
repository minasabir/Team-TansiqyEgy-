import { useParams, Link } from "react-router";
import { CollegeCard } from "../components/CollegeCard";
import { ChevronRight, MapPin, Building2, Search, BookOpen, Globe } from "lucide-react";
import { useState, useEffect } from "react";
import type { University, College } from "../data/mockData";
import { fetchUniversityById, fetchCollegesByUniversityId } from "@/lib/tansiqyApi";
import { recordUniversityPageView } from "@/lib/personalizedSearchPrefs";
import { OfficialSiteLink } from "@/app/components/OfficialSiteLink";

export function Colleges() {
  const { id } = useParams<{ id: string }>();
  const [searchQuery, setSearchQuery] = useState("");

  const [university, setUniversity] = useState<University | null>(null);
  const [universityColleges, setUniversityColleges] = useState<College[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const uniId = id ? Number(id) : NaN;
    if (!Number.isFinite(uniId)) {
      setUniversity(null);
      setUniversityColleges([]);
      setLoading(false);
      return;
    }

    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const [u, cols] = await Promise.all([
          fetchUniversityById(uniId),
          fetchCollegesByUniversityId(uniId),
        ]);
        if (!cancelled) {
          setUniversity(u);
          setUniversityColleges(cols);
          recordUniversityPageView(String(uniId));
        }
      } catch (e) {
        if (!cancelled) {
          setUniversity(null);
          setUniversityColleges([]);
          setError(e instanceof Error ? e.message : "تعذر التحميل");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [id]);

  const filteredColleges = universityColleges.filter(
    (c) =>
      c.nameAr.includes(searchQuery) || c.nameEn.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  if (!loading && !university && error) {
    return (
      <div className="container mx-auto p-20 text-center">
        <h2 className="text-3xl font-bold text-slate-800 mb-2">تعذر تحميل الجامعة</h2>
        <p className="text-slate-500 mb-4">{error}</p>
        <Link to="/universities" className="text-blue-600 mt-4 inline-block hover:underline font-bold">
          العودة للجامعات
        </Link>
      </div>
    );
  }

  if (!loading && !university) {
    return (
      <div className="container mx-auto p-20 text-center">
        <h2 className="text-3xl font-bold text-slate-800">الجامعة غير موجودة</h2>
        <Link to="/universities" className="text-blue-600 mt-4 inline-block hover:underline font-bold">
          العودة للجامعات
        </Link>
      </div>
    );
  }

  return (
    <div className="flex-grow bg-slate-50 min-h-screen">
      <div className="bg-white border-b border-slate-200 shadow-sm relative overflow-hidden pt-12 pb-16">
        {university && (
          <div
            className="absolute inset-0 opacity-10"
            style={{
              backgroundImage: `url(${university.image})`,
              backgroundSize: "cover",
              backgroundPosition: "center",
            }}
          ></div>
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-white via-white/90 to-transparent"></div>

        <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
          <Link
            to="/universities"
            className="inline-flex items-center gap-2 text-slate-500 hover:text-blue-600 transition-colors font-bold mb-8 text-sm"
          >
            <ChevronRight className="w-4 h-4" />
            العودة للجامعات
          </Link>

          {loading || !university ? (
            <div className="animate-pulse flex flex-col md:flex-row gap-8">
              <div className="w-32 h-32 rounded-3xl bg-slate-200 shrink-0" />
              <div className="flex-grow space-y-4">
                <div className="h-10 bg-slate-200 rounded w-2/3" />
                <div className="h-6 bg-slate-100 rounded w-1/2" />
              </div>
            </div>
          ) : (
            <div className="flex flex-col md:flex-row items-center md:items-start gap-8">
              <div className="w-32 h-32 rounded-3xl bg-white shadow-xl border border-slate-100 p-2 shrink-0">
                <img
                  src={university.logo}
                  alt={university.nameAr}
                  className="w-full h-full object-contain rounded-2xl"
                />
              </div>

              <div className="text-center md:text-right flex-grow">
                <div className="flex flex-wrap items-center justify-center md:justify-start gap-3 mb-4">
                  <span className="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-xs font-bold shadow-sm border border-blue-200">
                    {university.type}
                  </span>
                  <span className="flex items-center gap-1.5 px-3 py-1 bg-slate-100 text-slate-600 rounded-full text-xs font-bold shadow-sm border border-slate-200">
                    <MapPin className="w-3 h-3" /> {university.location}
                  </span>
                  <span className="flex items-center gap-1.5 px-3 py-1 bg-slate-100 text-slate-600 rounded-full text-xs font-bold shadow-sm border border-slate-200">
                    <Building2 className="w-3 h-3" /> {universityColleges.length} كليات
                  </span>
                </div>
                <h1 className="text-4xl md:text-5xl font-black text-slate-900 mb-2">{university.nameAr}</h1>
                <p className="text-xl text-slate-500 font-medium text-center md:text-right" dir="ltr">
                  {university.nameEn}
                </p>
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {university && !loading && (
          <div className="max-w-4xl mx-auto mb-10 space-y-6">
            <div
              className="rounded-2xl border border-slate-200 bg-white p-6 md:p-8 shadow-sm"
              dir="rtl"
            >
              <div className="flex items-center gap-2 text-blue-700 mb-4">
                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-50 border border-blue-100">
                  <BookOpen className="h-5 w-5" />
                </div>
                <h2 className="text-lg font-black text-slate-800">نبذة عن الجامعة</h2>
              </div>
              <p className="text-slate-600 leading-relaxed font-medium text-base md:text-lg">
                {university.description}
              </p>
            </div>

            <div
              className="rounded-2xl border border-emerald-200/80 bg-gradient-to-br from-emerald-50/80 via-white to-teal-50/50 p-6 md:p-8 shadow-sm"
              dir="rtl"
            >
              <div className="flex items-center gap-3 mb-4">
                <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-emerald-600/12 text-emerald-900 ring-1 ring-emerald-600/20">
                  <Globe className="h-5 w-5" strokeWidth={2.2} />
                </div>
                <h2 className="text-lg font-black text-slate-800">الموقع الرسمي للجامعة</h2>
              </div>
              {university.officialWebsite ? (
                <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                  <p className="text-sm text-slate-600 font-medium break-all" dir="ltr">
                    {university.officialWebsite}
                  </p>
                  <OfficialSiteLink href={university.officialWebsite} className="shrink-0 text-sm">
                    فتح الموقع في نافذة جديدة
                  </OfficialSiteLink>
                </div>
              ) : (
                <p className="text-sm text-slate-500 font-medium leading-relaxed">
                  لا يوجد رابط موقع إلكتروني مسجّل لهذه الجامعة ضمن البيانات المعروضة حالياً.
                </p>
              )}
            </div>
          </div>
        )}

        <div className="flex flex-col md:flex-row justify-between items-end gap-6 mb-10">
          <div>
            <h2 className="text-3xl font-bold text-slate-800 mb-3">كليات الجامعة</h2>
            <p className="text-slate-500 font-medium max-w-2xl">
              تصفح الكليات المتاحة وتعرف على التنسيق المتوقع والمصروفات الدراسية.
            </p>
          </div>

          <div className="w-full md:w-auto min-w-[300px] relative">
            <Search className="w-5 h-5 text-slate-400 absolute right-4 top-3.5" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="ابحث عن كلية..."
              className="w-full pl-4 pr-12 py-3.5 bg-white border border-slate-200 shadow-sm rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all font-medium text-slate-700"
            />
          </div>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-64 bg-white rounded-2xl border animate-pulse" />
            ))}
          </div>
        ) : filteredColleges.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
            {filteredColleges.map((college) => (
              <CollegeCard
                key={college.id}
                college={college}
                universityOfficialWebsite={university?.officialWebsite ?? null}
                suppressMohesrFallback={
                  university?.type === "خاصة" || university?.type === "أهلية"
                }
              />
            ))}
          </div>
        ) : (
          <div className="bg-white rounded-3xl p-16 text-center shadow-sm border border-slate-100">
            <div className="w-24 h-24 bg-slate-50 rounded-full flex items-center justify-center mx-auto mb-6">
              <Search className="w-12 h-12 text-slate-300" />
            </div>
            <h3 className="text-2xl font-bold text-slate-800 mb-3">لا توجد كليات مطابقة</h3>
            <p className="text-slate-500 font-medium">جرب البحث باسم آخر.</p>
          </div>
        )}
      </div>
    </div>
  );
}
