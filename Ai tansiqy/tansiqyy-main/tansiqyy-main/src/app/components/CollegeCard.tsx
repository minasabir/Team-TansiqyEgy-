import { Link } from "react-router";
import type { College } from "../data/mockData";
import { Percent, Wallet } from "lucide-react";
import {
  bestOfficialPageForCollege,
  feesTextSuggestsMissingData,
  isCoordinationMissing,
} from "@/lib/trustedSources";
import { OfficialSiteLink } from "./OfficialSiteLink";

interface CollegeCardProps {
  college: College;
  universityOfficialWebsite?: string | null;
  /** لا نعرض روابط الجهات الحكومية البديلة (مثلاً لجامعات خاصة وأهلية) */
  suppressMohesrFallback?: boolean;
}

export function CollegeCard({ college, universityOfficialWebsite, suppressMohesrFallback }: CollegeCardProps) {
  const officialUrl = bestOfficialPageForCollege(college, universityOfficialWebsite);
  const coordMissing = isCoordinationMissing(college.admissionPercentage);
  const feesUnclear = feesTextSuggestsMissingData(college.fees);
  const suggestOfficial = coordMissing || feesUnclear;

  return (
    <div className="bg-white rounded-2xl shadow-sm hover:shadow-md transition-shadow duration-300 border border-slate-100 p-6 flex flex-col h-full">
      <h3 className="text-xl font-bold text-slate-800 mb-1">{college.nameAr}</h3>
      <p className="text-sm text-slate-500 mb-6 text-right" dir="ltr">
        {college.nameEn}
      </p>

      <div className="space-y-3 mb-4 flex-grow">
        <div className="flex items-center gap-3 p-3 bg-blue-50/50 rounded-xl">
          <div className="bg-blue-100 p-2 rounded-lg text-blue-600">
            <Percent className="w-5 h-5" />
          </div>
          <div>
            <p className="text-xs text-slate-500 font-medium mb-0.5">الحد الأدنى المتوقع</p>
            <p className="text-lg font-bold text-slate-800" dir="ltr">
              {coordMissing ? "—" : `${college.admissionPercentage}%`}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-3 p-3 bg-slate-50 rounded-xl border border-slate-100">
          <div className="bg-slate-200 p-2 rounded-lg text-slate-600">
            <Wallet className="w-5 h-5" />
          </div>
          <div>
            <p className="text-xs text-slate-500 font-medium mb-0.5">المصروفات الدراسية</p>
            <p className="text-sm font-semibold text-slate-800">{college.fees}</p>
          </div>
        </div>
      </div>

      {suggestOfficial && officialUrl && (
        <div className="mb-4 rounded-xl border border-emerald-100 bg-emerald-50/60 px-3 py-2.5" dir="rtl">
          <p className="text-[11px] font-bold text-emerald-900 mb-1.5">تأكد من المصدر الرسمي</p>
          <OfficialSiteLink href={officialUrl} className="text-xs">
            موقع الجامعة / الكلية
          </OfficialSiteLink>
        </div>
      )}
      {suggestOfficial && !officialUrl && !suppressMohesrFallback && (
        <p className="mb-4 text-[11px] font-medium text-amber-800 bg-amber-50 border border-amber-100 rounded-xl px-3 py-2" dir="rtl">
          لم يُسجَّل رابط رسمي هنا. راجع صفحة الجامعة على{" "}
          <a
            href="https://mohesr.gov.eg/"
            target="_blank"
            rel="noopener noreferrer"
            className="font-bold text-amber-900 underline"
          >
            وزارة التعليم العالي
          </a>{" "}
          أو موقع الجامعة مباشرة.
        </p>
      )}
      {suggestOfficial && !officialUrl && suppressMohesrFallback && (
        <p className="mb-4 text-[11px] font-medium text-slate-500 bg-slate-50 border border-slate-100 rounded-xl px-3 py-2" dir="rtl">
          لم يُسجَّل رابط رسمي للجامعة أو الكلية ضمن البيانات المعروضة.
        </p>
      )}

      <Link
        to={`/colleges/${college.id}`}
        className="w-full block text-center py-3 bg-blue-600 text-white rounded-xl font-medium hover:bg-blue-700 transition-colors shadow-sm mt-auto"
      >
        التفاصيل
      </Link>
    </div>
  );
}
