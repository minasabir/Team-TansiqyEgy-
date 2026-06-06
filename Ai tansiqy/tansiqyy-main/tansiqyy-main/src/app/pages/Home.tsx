import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { Search, ChevronLeft, Briefcase, Building2, Cpu, Globe, Library, Landmark } from "lucide-react";
import type { University } from "../data/mockData";
import { UniversityCard } from "../components/UniversityCard";
import { PersonalizedHomeSection } from "../components/PersonalizedHomeSection";
import { fetchUniversitiesByType } from "@/lib/tansiqyApi";

const UNI_TYPES = [
  {
    label: "جامعات حكومية",
    value: "حكومية" as const,
    icon: <Landmark className="w-8 h-8 mb-3 text-blue-600" />,
    bg: "bg-blue-50",
    hover: "hover:bg-blue-600 hover:text-white",
    border: "border-blue-100",
    textHover: "group-hover:text-blue-100",
  },
  {
    label: "جامعات خاصة",
    value: "خاصة" as const,
    icon: <Briefcase className="w-8 h-8 mb-3 text-purple-600" />,
    bg: "bg-purple-50",
    hover: "hover:bg-purple-600 hover:text-white",
    border: "border-purple-100",
    textHover: "group-hover:text-purple-100",
  },
  {
    label: "جامعات أهلية",
    value: "أهلية" as const,
    icon: <Building2 className="w-8 h-8 mb-3 text-teal-600" />,
    bg: "bg-teal-50",
    hover: "hover:bg-teal-600 hover:text-white",
    border: "border-teal-100",
    textHover: "group-hover:text-teal-100",
  },
  {
    label: "جامعات تكنولوجية",
    value: "تكنولوجية" as const,
    icon: <Cpu className="w-8 h-8 mb-3 text-orange-600" />,
    bg: "bg-orange-50",
    hover: "hover:bg-orange-600 hover:text-white",
    border: "border-orange-100",
    textHover: "group-hover:text-orange-100",
  },
  {
    label: "جامعات أجنبية",
    value: "أجنبية" as const,
    icon: <Globe className="w-8 h-8 mb-3 text-rose-600" />,
    bg: "bg-rose-50",
    hover: "hover:bg-rose-600 hover:text-white",
    border: "border-rose-100",
    textHover: "group-hover:text-rose-100",
  },
  {
    label: "معاهد عليا",
    value: "معاهد عليا" as const,
    icon: <Library className="w-8 h-8 mb-3 text-indigo-600" />,
    bg: "bg-indigo-50",
    hover: "hover:bg-indigo-600 hover:text-white",
    border: "border-indigo-100",
    textHover: "group-hover:text-indigo-100",
  },
];

export function Home() {
  const navigate = useNavigate();
  const [featuredUniversities, setFeaturedUniversities] = useState<University[]>([]);
  const [featuredLoading, setFeaturedLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const list = await fetchUniversitiesByType(1);
        if (!cancelled) setFeaturedUniversities(list.slice(0, 3));
      } catch {
        if (!cancelled) setFeaturedUniversities([]);
      } finally {
        if (!cancelled) setFeaturedLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/universities?search=${encodeURIComponent(searchQuery)}`);
    }
  };

  return (
    <div className="flex flex-col flex-grow w-full font-sans">
      <section className="relative overflow-hidden pt-20 pb-28 md:pb-32">
        <div className="absolute inset-0 bg-gradient-to-b from-brand-cream via-white to-slate-50/90" />
        <div className="absolute top-20 -right-24 w-[28rem] h-[28rem] rounded-full bg-gradient-to-br from-amber-200/35 to-transparent blur-3xl" />
        <div className="absolute bottom-10 -left-20 w-80 h-80 rounded-full bg-gradient-to-tr from-brand-navy-mid/10 to-transparent blur-3xl" />
        <div
          className="absolute inset-0 opacity-[0.35] pointer-events-none"
          style={{
            backgroundImage: `radial-gradient(circle at 1px 1px, rgba(10,22,40,0.06) 1px, transparent 0)`,
            backgroundSize: "24px 24px",
          }}
        />

        <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10 flex flex-col items-center">
          <div className="inline-flex items-center gap-2.5 px-5 py-2.5 rounded-full bg-white/90 backdrop-blur-sm text-brand-navy font-bold text-sm mb-10 border border-amber-200/60 shadow-[0_8px_30px_-12px_rgba(10,22,40,0.15)]">
            <span className="relative flex h-2.5 w-2.5">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-amber-400 opacity-60" />
              <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-gradient-to-br from-amber-400 to-brand-gold" />
            </span>
            معلومات من مواقع موثوقة — تسهيلاً على الطالب
          </div>

          <h1 className="text-4xl md:text-5xl lg:text-[3.5rem] font-black text-brand-navy mb-6 leading-[1.15] tracking-tight text-center max-w-4xl">
            اعرف الكلية المناسبة ليك{" "}
            <span className="text-transparent bg-clip-text bg-gradient-to-l from-brand-gold via-amber-500 to-brand-navy-mid">
              بسهولة
            </span>
          </h1>

          <p className="text-lg md:text-xl text-slate-600 mb-12 max-w-2xl font-medium leading-relaxed text-center">
            اختار الجامعة – اختار الكلية – واعرف التنسيق والمصاريف في مكان واحد، بواجهة واضحة لطلاب الثانوية.
          </p>

          <form
            onSubmit={handleSearch}
            className="w-full max-w-2xl mb-20 relative z-20 group"
          >
            <div className="absolute -inset-1 bg-gradient-to-l from-amber-300/40 via-brand-navy/10 to-brand-gold/30 rounded-[1.35rem] blur-md opacity-70 group-focus-within:opacity-100 transition-opacity duration-300" />
            <div className="relative flex flex-col sm:flex-row sm:items-stretch gap-2 p-2 rounded-2xl bg-white/95 backdrop-blur-md border border-slate-200/80 shadow-[0_20px_50px_-20px_rgba(10,22,40,0.2)]">
              <div className="relative flex-1 flex items-center min-h-[3.25rem]">
                <Search className="absolute right-4 w-5 h-5 text-slate-400 group-focus-within:text-brand-gold transition-colors pointer-events-none" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="w-full pr-12 pl-4 py-3.5 text-base rounded-xl bg-transparent focus:outline-none font-semibold text-slate-800 placeholder:text-slate-400"
                  placeholder="ابحث عن جامعة، كلية، أو تخصص..."
                  dir="rtl"
                />
              </div>
              <button
                type="submit"
                className="shrink-0 px-8 py-3.5 rounded-xl font-black text-white bg-gradient-to-l from-brand-navy to-brand-navy-mid shadow-lg shadow-brand-navy/25 hover:from-brand-navy-mid hover:to-brand-navy-soft transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400 focus-visible:ring-offset-2"
              >
                بحث
              </button>
            </div>
          </form>

          <div className="w-full max-w-6xl mx-auto relative z-20">
            <div className="text-center mb-12">
              <h2 className="text-2xl md:text-3xl font-black text-brand-navy mb-3">ابدأ باختيار نوع الجامعة</h2>
              <p className="text-slate-500 font-medium max-w-xl mx-auto leading-relaxed">
                تصفّح حسب النوع — كل بطاقة تفتح لك قائمة الجامعات المناسبة فوراً.
              </p>
            </div>

            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3 sm:gap-4">
              {UNI_TYPES.map((type) => (
                <Link
                  key={type.value}
                  to={`/universities?type=${type.value}`}
                  className={`group flex flex-col items-center text-center p-5 sm:p-6 rounded-3xl transition-all duration-300 border bg-white/90 backdrop-blur-sm shadow-[0_6px_24px_-8px_rgba(10,22,40,0.12)] hover:shadow-[0_16px_40px_-12px_rgba(10,22,40,0.2)] hover:-translate-y-1 ${type.border} ${type.hover}`}
                >
                  <div className="p-3.5 rounded-2xl mb-3 transition-colors duration-300 bg-slate-50/90 group-hover:bg-white/25 ring-1 ring-slate-100/80 group-hover:ring-white/30">
                    {type.icon}
                  </div>
                  <span className="font-black text-sm sm:text-base text-slate-800 transition-colors duration-300 group-hover:text-white leading-snug">
                    {type.label}
                  </span>
                </Link>
              ))}
            </div>
          </div>
        </div>
      </section>

      <section className="relative py-24 overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-b from-slate-100/80 via-white to-brand-cream/30" />
        <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative">
          <div className="flex flex-col sm:flex-row sm:justify-between sm:items-end gap-6 mb-14">
            <div>
              <p className="text-xs font-black uppercase tracking-widest text-brand-gold mb-2">مختارات</p>
              <h2 className="text-3xl md:text-4xl font-black text-brand-navy mb-2">أشهر الجامعات</h2>
              <p className="text-slate-500 font-medium">عيّنة من الجامعات الحكومية المعروضة في الدليل</p>
            </div>
            <Link
              to="/universities"
              className="inline-flex items-center justify-center gap-2 self-start sm:self-auto font-black text-sm text-white bg-gradient-to-l from-brand-navy to-brand-navy-mid px-6 py-3 rounded-2xl shadow-lg shadow-brand-navy/20 hover:shadow-xl transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400 focus-visible:ring-offset-2"
            >
              عرض كل الجامعات
              <ChevronLeft className="w-5 h-5" />
            </Link>
          </div>

          {featuredLoading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
              {[1, 2, 3].map((i) => (
                <div
                  key={i}
                  className="h-96 rounded-3xl bg-gradient-to-br from-slate-100 to-slate-50 border border-slate-200/80 animate-pulse"
                />
              ))}
            </div>
          ) : featuredUniversities.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
              {featuredUniversities.map((uni) => (
                <UniversityCard key={uni.id} university={uni} />
              ))}
            </div>
          ) : (
            <p className="text-center text-slate-500 font-medium py-12">تعذر تحميل عيّنة الجامعات. جرّب صفحة الجامعات.</p>
          )}

          <div className="mt-10 flex justify-center sm:hidden">
            <Link
              to="/universities"
              className="w-full text-center py-4 bg-brand-navy text-white font-black rounded-2xl shadow-lg shadow-brand-navy/20 active:scale-[0.99] transition-transform"
            >
              عرض كل الجامعات
            </Link>
          </div>
        </div>
      </section>

      <PersonalizedHomeSection />
    </div>
  );
}
