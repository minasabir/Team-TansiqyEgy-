import { Link } from "react-router";
import { University } from "../data/mockData";
import { MapPin, BookOpen, ChevronLeft, Globe } from "lucide-react";
import { OfficialSiteLink } from "@/app/components/OfficialSiteLink";

interface UniversityCardProps {
  university: University;
}

export function UniversityCard({ university }: UniversityCardProps) {
  const brandMode = university.coverMode === "brand";

  return (
    <article className="group relative flex flex-col h-full rounded-3xl overflow-hidden bg-white border border-slate-200/80 shadow-[0_8px_30px_-12px_rgba(10,22,40,0.12)] hover:shadow-[0_20px_50px_-16px_rgba(10,22,40,0.18)] hover:border-amber-200/60 transition-all duration-500 hover:-translate-y-1">
      <div className="absolute inset-x-0 top-0 h-1 bg-gradient-to-l from-brand-gold/80 via-amber-400/70 to-brand-navy opacity-90" />

      <div className="relative h-48 overflow-hidden shrink-0">
        {brandMode ? (
          <>
            <div className="absolute inset-0 bg-gradient-to-br from-brand-navy via-slate-800 to-brand-navy-mid" />
            <div
              className="absolute inset-0 opacity-[0.14]"
              style={{
                backgroundImage: `radial-gradient(circle at 25% 15%, rgba(255,255,255,0.45), transparent 45%)`,
              }}
            />
            <div className="absolute inset-0 flex items-center justify-center p-8 pt-10">
              <img
                src={university.logo}
                alt=""
                className="max-w-[min(9rem,42vw)] max-h-28 w-auto h-auto object-contain drop-shadow-[0_12px_32px_rgba(0,0,0,0.35)] transition-transform duration-500 group-hover:scale-[1.04]"
              />
            </div>
            <div className="absolute inset-0 bg-gradient-to-t from-brand-navy/90 via-brand-navy/25 to-transparent pointer-events-none" />
          </>
        ) : (
          <img
            src={university.image}
            alt={university.nameAr}
            className="w-full h-full object-cover transition-transform duration-700 ease-out group-hover:scale-[1.06]"
          />
        )}
        {!brandMode ? (
          <div className="absolute inset-0 bg-gradient-to-t from-brand-navy/85 via-brand-navy/20 to-transparent opacity-90" />
        ) : null}
        <div className="absolute top-4 right-4 flex items-center gap-2">
          <span className="inline-flex items-center px-3 py-1 rounded-full text-[11px] font-black text-brand-navy bg-white/95 backdrop-blur-sm shadow-md border border-amber-200/50">
            {university.type}
          </span>
        </div>
        <div className="absolute bottom-4 right-4 left-4">
          <div className="flex items-end gap-3">
            {!brandMode ? (
              <div className="w-14 h-14 rounded-2xl border-2 border-white/90 bg-white shadow-lg overflow-hidden p-1 shrink-0">
                <img src={university.logo} alt="" className="w-full h-full object-contain rounded-xl" />
              </div>
            ) : null}
            <div className="min-w-0 flex-1 pb-0.5">
              <h3 className="text-lg font-black text-white drop-shadow-sm line-clamp-2 leading-snug">
                {university.nameAr}
              </h3>
              <p className="text-xs text-white/85 mt-0.5 line-clamp-1 font-medium text-right" dir="ltr">
                {university.nameEn}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="p-5 flex-grow flex flex-col gap-4 min-h-0">
        <div
          className="rounded-2xl border border-slate-100 bg-gradient-to-br from-brand-cream/80 via-white to-slate-50/90 p-4 shadow-inner"
          dir="rtl"
        >
          <div className="flex items-center gap-2 mb-2 text-brand-navy">
            <BookOpen className="w-4 h-4 shrink-0 text-brand-gold" strokeWidth={2.5} />
            <span className="text-[11px] font-black uppercase tracking-wider text-brand-navy/80">نبذة عن الجامعة</span>
          </div>
          <p className="text-sm text-slate-600 leading-relaxed line-clamp-4 font-medium">{university.description}</p>
        </div>

        <div className="flex items-center gap-2 text-slate-500 text-sm font-semibold">
          <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-slate-100 text-brand-navy shrink-0">
            <MapPin className="w-4 h-4" />
          </span>
          <span className="line-clamp-1 text-slate-700">{university.location}</span>
        </div>

        <div
          className="rounded-2xl border border-emerald-200/70 bg-gradient-to-br from-emerald-50/90 via-white to-teal-50/40 p-4 shadow-[inset_0_1px_0_0_rgba(255,255,255,0.8)]"
          dir="rtl"
        >
          <div className="flex items-center gap-2 mb-2.5">
            <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-emerald-600/10 text-emerald-800 ring-1 ring-emerald-600/15 shrink-0">
              <Globe className="w-4 h-4" strokeWidth={2.5} />
            </span>
            <span className="text-[11px] font-black uppercase tracking-wider text-emerald-900/90">
              الموقع الإلكتروني للجامعة
            </span>
          </div>
          {university.officialWebsite ? (
            <OfficialSiteLink href={university.officialWebsite} className="text-sm font-black text-emerald-800">
              فتح موقع الجامعة الرسمي
            </OfficialSiteLink>
          ) : (
            <p className="text-xs text-slate-500 font-semibold leading-relaxed">
              لا يوجد رابط مسجّل لهذه الجامعة ضمن البيانات المعروضة حالياً.
            </p>
          )}
        </div>

        <Link
          to={`/universities/${university.id}/colleges`}
          className="mt-auto w-full flex items-center justify-center gap-2 py-3 rounded-2xl text-sm font-black text-white bg-gradient-to-l from-brand-navy to-brand-navy-mid shadow-md shadow-brand-navy/20 hover:shadow-lg hover:from-brand-navy-mid hover:to-brand-navy-soft transition-all duration-300 border border-white/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400 focus-visible:ring-offset-2"
        >
          عرض الكليات
          <ChevronLeft className="w-4 h-4 opacity-90" />
        </Link>
      </div>
    </article>
  );
}
