import { useParams, Link } from "react-router";
import {
  ChevronRight,
  Percent,
  Wallet,
  Info,
  Grid3x3,
  GraduationCap,
  MapPin,
  Building2,
  Calendar,
  Heart,
  BookOpen,
  Shield,
  ExternalLink,
} from "lucide-react";
import { useFavorites } from "../context/FavoritesContext";
import { useEffect, useState } from "react";
import type { College, University } from "../data/mockData";
import { fetchCollegeById, fetchUniversityById } from "@/lib/tansiqyApi";
import { recordUniversityPageView } from "@/lib/personalizedSearchPrefs";
import {
  TRUSTED_EDUCATION_PORTALS,
  bestOfficialPageForCollege,
  feesTextSuggestsMissingData,
  isCoordinationMissing,
} from "@/lib/trustedSources";
import { OfficialSiteLink } from "../components/OfficialSiteLink";

export function CollegeDetails() {
  const { id } = useParams<{ id: string }>();
  const { isFavorite, toggleFavorite } = useFavorites();

  const [college, setCollege] = useState<College | null>(null);
  const [university, setUniversity] = useState<University | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const cid = id ? Number(id) : NaN;
    if (!Number.isFinite(cid)) {
      setCollege(null);
      setUniversity(null);
      setLoading(false);
      return;
    }

    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const c = await fetchCollegeById(cid);
        if (cancelled) return;
        setCollege(c);
        recordUniversityPageView(c.universityId);
        const u = await fetchUniversityById(Number(c.universityId));
        if (!cancelled) setUniversity(u);
      } catch (e) {
        if (!cancelled) {
          setCollege(null);
          setUniversity(null);
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

  if (loading) {
    return (
      <div className="flex-grow bg-slate-50 min-h-screen pb-20">
        <div className="container mx-auto px-4 py-24">
          <div className="max-w-3xl mx-auto space-y-6 animate-pulse">
            <div className="h-8 bg-slate-200 rounded w-1/3" />
            <div className="h-12 bg-slate-200 rounded w-full" />
            <div className="h-40 bg-slate-100 rounded-2xl" />
          </div>
        </div>
      </div>
    );
  }

  if (error || !college || !university) {
    return (
      <div className="container mx-auto p-20 text-center">
        <h2 className="text-3xl font-bold text-slate-800 mb-4">الكلية غير موجودة</h2>
        {error && <p className="text-slate-500 mb-4">{error}</p>}
        <Link to="/universities" className="text-blue-600 hover:underline font-bold">
          العودة للجامعات
        </Link>
      </div>
    );
  }

  const isSaved = isFavorite(college.id);
  const officialPage = bestOfficialPageForCollege(college, university.officialWebsite);
  const dataIncomplete =
    isCoordinationMissing(college.admissionPercentage) || feesTextSuggestsMissingData(college.fees);
  const isPrivateOrNational = university.type === "خاصة" || university.type === "أهلية";

  return (
    <div className="flex-grow bg-slate-50 min-h-screen pb-20">
      <div className="bg-white border-b border-slate-200 shadow-sm relative overflow-hidden pt-12 pb-16">
        <div className="absolute inset-0 bg-blue-600/5 mix-blend-multiply"></div>
        <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
          <Link
            to={`/universities/${university.id}/colleges`}
            className="inline-flex items-center gap-2 text-slate-500 hover:text-blue-600 transition-colors font-bold mb-8 text-sm"
          >
            <ChevronRight className="w-4 h-4" />
            العودة لكليات {university.nameAr}
          </Link>

          <div className="max-w-4xl mx-auto text-center md:text-right">
            <div className="flex flex-wrap items-center justify-center md:justify-start gap-3 mb-6">
              <span className="flex items-center gap-1.5 px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-xs font-bold shadow-sm border border-blue-200">
                <Building2 className="w-3 h-3" /> {university.nameAr}
              </span>
              <span className="flex items-center gap-1.5 px-3 py-1 bg-slate-100 text-slate-600 rounded-full text-xs font-bold shadow-sm border border-slate-200">
                <MapPin className="w-3 h-3" /> {university.location}
              </span>
            </div>

            <h1 className="text-4xl md:text-5xl font-black text-slate-900 mb-4">{college.nameAr}</h1>
            <p className="text-xl md:text-2xl text-slate-500 font-medium text-center md:text-right" dir="ltr">
              {college.nameEn}
            </p>
          </div>
        </div>
      </div>

      <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 max-w-6xl mx-auto">
          <div className="lg:col-span-2 space-y-8">
            <div
              className="bg-gradient-to-br from-blue-50/80 to-slate-50 rounded-3xl p-8 shadow-sm border border-blue-100/80"
              dir="rtl"
            >
              <h2 className="text-xl font-bold text-slate-800 mb-4 flex items-center gap-3">
                <BookOpen className="w-6 h-6 text-blue-600" />
                نبذة عن الجامعة ({university.nameAr})
              </h2>
              <p className="text-slate-600 leading-relaxed font-medium text-lg">{university.description}</p>
            </div>

            <div className="bg-white rounded-3xl p-8 shadow-sm border border-slate-100">
              <h2 className="text-2xl font-bold text-slate-800 mb-6 flex items-center gap-3">
                <Info className="w-6 h-6 text-blue-600" />
                نبذة عن الكلية
              </h2>
              <p className="text-slate-600 leading-relaxed font-medium text-lg">{college.description}</p>
            </div>

            <div className="bg-white rounded-3xl p-8 shadow-sm border border-slate-100">
              <h2 className="text-2xl font-bold text-slate-800 mb-6 flex items-center gap-3">
                <Grid3x3 className="w-6 h-6 text-blue-600" />
                الأقسام المتاحة
              </h2>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {college.departments.map((dept, idx) => (
                  <div
                    key={idx}
                    className="flex items-center gap-4 p-4 rounded-2xl bg-slate-50 border border-slate-100 shadow-sm hover:shadow-md transition-shadow"
                  >
                    <div className="w-10 h-10 bg-blue-100 rounded-xl flex items-center justify-center text-blue-600 shrink-0">
                      <GraduationCap className="w-5 h-5" />
                    </div>
                    <span className="font-semibold text-slate-800">{dept}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          <div className="space-y-6">
            <div className="bg-gradient-to-br from-blue-600 to-blue-800 rounded-3xl p-8 shadow-lg shadow-blue-900/20 text-white relative overflow-hidden">
              <div className="absolute -top-10 -right-10 w-40 h-40 bg-white/10 rounded-full blur-2xl"></div>
              <div className="absolute -bottom-10 -left-10 w-40 h-40 bg-white/10 rounded-full blur-2xl"></div>

              <h3 className="text-xl font-bold mb-6 flex items-center gap-3 relative z-10">
                <Percent className="w-6 h-6 text-blue-200" />
                الحد الأدنى للتنسيق
              </h3>

              <div className="text-center relative z-10">
                <p className="text-6xl font-black mb-2 tracking-tight" dir="ltr">
                  {college.admissionPercentage > 0 ? `${college.admissionPercentage}%` : "—"}
                </p>
                <p className="text-blue-100 font-medium text-sm">تنسيق العام الماضي (استرشادي)</p>
              </div>
            </div>

            <div className="bg-white rounded-3xl p-8 shadow-sm border border-slate-100">
              <h3 className="text-xl font-bold text-slate-800 mb-6 flex items-center gap-3">
                <Wallet className="w-6 h-6 text-blue-600" />
                المصروفات الدراسية
              </h3>

              <div className="p-5 bg-slate-50 rounded-2xl border border-slate-100 text-center">
                <p className="text-2xl font-black text-slate-800 mb-2 leading-relaxed">{college.fees}</p>
                <p className="text-slate-500 font-medium text-sm">حسب المعلومات المعروضة في الدليل</p>
              </div>

              <div className="mt-6 space-y-4">
                <div className="flex items-start gap-3 p-4 bg-yellow-50/50 rounded-xl border border-yellow-100">
                  <Calendar className="w-5 h-5 text-yellow-600 shrink-0 mt-0.5" />
                  <div>
                    <p className="text-sm font-bold text-yellow-800 mb-1">تنبيه هام</p>
                    <p className="text-xs font-medium text-yellow-700/80">
                      {isPrivateOrNational
                        ? "المصروفات والتنسيق قابلة للتغيير. راجع الإعلان الرسمي للجامعة أو الكلية عند اتخاذ القرار."
                        : "المصروفات والتنسيق قابلة للتغيير. اعتمد دائماً على الإعلان الرسمي للجامعة أو الكلية أو على البوابات الحكومية الموثوقة أدناه."}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {isPrivateOrNational ? (
              officialPage ? (
                <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm" dir="rtl">
                  <h3 className="text-lg font-black text-slate-900 mb-3">الموقع الرسمي</h3>
                  <p className="text-xs font-bold text-slate-500 mb-1.5">الجامعة / الكلية</p>
                  <OfficialSiteLink href={officialPage}>فتح الموقع الرسمي</OfficialSiteLink>
                </div>
              ) : null
            ) : (
              <div className="rounded-3xl border border-emerald-200 bg-gradient-to-br from-emerald-50/90 to-white p-6 shadow-sm" dir="rtl">
                <h3 className="text-lg font-black text-slate-900 mb-3 flex items-center gap-2">
                  <Shield className="w-5 h-5 text-emerald-700" />
                  مصادر رسمية للتحقق
                </h3>
                {dataIncomplete && (
                  <p className="text-sm text-emerald-900 font-medium mb-4 leading-relaxed">
                    جزء من البيانات غير متوفر ضمن المعلومات المعروضة؛ راجع التنسيق والمصاريف من المصادر التالية قبل اتخاذ القرار.
                  </p>
                )}
                {officialPage && (
                  <div className="mb-4">
                    <p className="text-xs font-bold text-slate-500 mb-1.5">الموقع الرسمي للجامعة / الكلية</p>
                    <OfficialSiteLink href={officialPage}>فتح الموقع الرسمي</OfficialSiteLink>
                  </div>
                )}
                <p className="text-xs font-bold text-slate-500 mb-2">بوابات حكومية موثوقة</p>
                <ul className="space-y-2">
                  {TRUSTED_EDUCATION_PORTALS.map((p) => (
                    <li key={p.url}>
                      <a
                        href={p.url}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-sm font-semibold text-emerald-800 hover:text-emerald-950 flex items-center gap-1.5"
                      >
                        {p.title}
                        <ExternalLink className="w-3.5 h-3.5 opacity-70" />
                      </a>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            <div className="pt-4">
              <button
                type="button"
                onClick={() => toggleFavorite(college.id)}
                className={`w-full py-4 rounded-2xl font-bold text-lg transition-all shadow-lg flex items-center justify-center gap-3 ${
                  isSaved
                    ? "bg-rose-50 text-rose-600 border-2 border-rose-200 hover:bg-rose-100 shadow-rose-900/5"
                    : "bg-slate-900 text-white border-2 border-slate-900 hover:bg-slate-800 shadow-slate-900/20"
                }`}
              >
                <Heart className={`w-6 h-6 transition-colors ${isSaved ? "fill-rose-500 text-rose-500" : ""}`} />
                {isSaved ? "محفوظ في المفضلة" : "حفظ في المفضلة"}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
