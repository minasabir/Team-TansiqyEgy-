import type { College, University, UniversityType } from "@/app/data/mockData";
import {
  buildOfficialCoverIconUrl,
  buildOfficialLogoUrlFromWebsite,
} from "@/lib/officialUniversityMedia";
import { normalizeOfficialUrl } from "@/lib/trustedSources";

const DEFAULT_BASE = "https://tansiqy.runasp.net";

export function getTansiqyBaseUrl(): string {
  const fromEnv = import.meta.env.VITE_TANSIQY_API_URL?.replace(/\/$/, "");
  if (fromEnv) return fromEnv;
  if (import.meta.env.DEV) return "/tansiqy-api";
  return DEFAULT_BASE;
}

async function apiGet<T>(path: string): Promise<T> {
  const url = `${getTansiqyBaseUrl()}${path.startsWith("/") ? path : `/${path}`}`;
  const res = await fetch(url, {
    headers: { Accept: "application/json" },
  });
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(`Tansiqy API ${res.status}: ${text.slice(0, 200)}`);
  }
  return res.json() as Promise<T>;
}

export async function fetchIntelligentSearchViewModels(params: URLSearchParams): Promise<UniversityViewModel[]> {
  const qs = params.toString();
  const path = qs ? `/api/Universities/search/intelligent?${qs}` : `/api/Universities/search/intelligent`;
  return apiGet<UniversityViewModel[]>(path);
}

export function mergeUniversitySearchViewModels(
  a: UniversityViewModel[],
  b: UniversityViewModel[],
): UniversityViewModel[] {
  const byId = new Map<number, UniversityViewModel>();

  const mergeColleges = (u1: UniversityViewModel, u2: UniversityViewModel): CollegeViewModel[] | null => {
    const collegeMap = new Map<number, CollegeViewModel>();
    for (const c of u1.colleges ?? []) collegeMap.set(c.id, c);
    for (const c of u2.colleges ?? []) {
      if (!collegeMap.has(c.id)) collegeMap.set(c.id, c);
    }
    if (collegeMap.size === 0) return null;
    return [...collegeMap.values()];
  };

  const put = (u: UniversityViewModel) => {
    const ex = byId.get(u.id);
    if (!ex) {
      byId.set(u.id, { ...u, colleges: u.colleges ? [...u.colleges] : null });
      return;
    }
    const mergedColleges = mergeColleges(ex, u);
    byId.set(u.id, {
      ...ex,
      colleges: mergedColleges ?? ex.colleges ?? u.colleges ?? null,
    });
  };

  for (const u of a) put(u);
  for (const u of b) put(u);
  return [...byId.values()];
}

function normalizeMatchText(s: string): string {
  return s.trim().toLowerCase().replace(/\s+/g, " ");
}

function textContainsInsensitive(haystack: string, needle: string): boolean {
  const n = normalizeMatchText(needle);
  if (!n) return false;
  return normalizeMatchText(haystack).includes(n);
}

export interface IntelligentSearchBuckets {
  /** تطابق اسم الجامعة (استجابة الخادم لـ searchTerm) */
  universitiesByName: University[];
  /** تطابق اسم الكلية في بيانات مدمجة من الخادم */
  collegeHits: { university: University; college: College }[];
  /** تطابق اسم قسم / تخصص ضمن الكليات */
  departmentHits: { university: University; college: College; department: string }[];
}

const PLACEHOLDER_DEPARTMENTS_LABEL = "لم تُذكر أقسام ضمن البيانات المتاحة";

/**
 * بحث موحّد: طلبان متوازيان (اسم جامعة + اسم كلية) ثم تجميع النتائج،
 * مع استخراج تطابقات أسماء الأقسام محلياً على بيانات الكليات المرتجعة.
 */
export async function intelligentSearchBuckets(
  searchTerm: string,
  apiType?: number,
): Promise<IntelligentSearchBuckets> {
  const q = searchTerm.trim();
  if (!q) {
    return { universitiesByName: [], collegeHits: [], departmentHits: [] };
  }

  const paramsName = new URLSearchParams();
  paramsName.set("searchTerm", q);
  if (apiType != null) paramsName.set("type", String(apiType));

  const paramsCollege = new URLSearchParams();
  paramsCollege.set("collegeName", q);
  if (apiType != null) paramsCollege.set("type", String(apiType));

  const [byNameVms, byCollegeVms] = await Promise.all([
    fetchIntelligentSearchViewModels(paramsName),
    fetchIntelligentSearchViewModels(paramsCollege),
  ]);

  const merged = mergeUniversitySearchViewModels(byNameVms, byCollegeVms);

  const universitiesByName = [
    ...new Map(byNameVms.map((vm) => [vm.id, mapUniversityVmToUniversity(vm)])).values(),
  ];

  const collegeHitsMap = new Map<string, { university: University; college: College }>();
  const departmentHitsMap = new Map<
    string,
    { university: University; college: College; department: string }
  >();

  for (const vm of merged) {
    const u = mapUniversityVmToUniversity(vm);
    const colleges = (vm.colleges ?? []).map((c) => mapCollegeVmToCollege(c, vm));

    for (const c of colleges) {
      if (textContainsInsensitive(c.nameAr, q) || textContainsInsensitive(c.nameEn, q)) {
        collegeHitsMap.set(c.id, { university: u, college: c });
      }
      for (const dept of c.departments) {
        if (dept === PLACEHOLDER_DEPARTMENTS_LABEL) continue;
        if (textContainsInsensitive(dept, q)) {
          departmentHitsMap.set(`${c.id}-${dept}`, { university: u, college: c, department: dept });
        }
      }
    }
  }

  return {
    universitiesByName,
    collegeHits: [...collegeHitsMap.values()],
    departmentHits: [...departmentHitsMap.values()],
  };
}

/** API enum: 1=Governmental, 2=Private, 3=National, 4=HigherInstitute, 5=Foreign, 6=Technological */
export function uiUniversityTypeToApiType(t: UniversityType): number {
  const map: Record<UniversityType, number> = {
    حكومية: 1,
    خاصة: 2,
    أهلية: 3,
    تكنولوجية: 6,
    أجنبية: 5,
    "معاهد عليا": 4,
  };
  return map[t];
}

export function apiTypeToUiUniversityType(apiType: number): UniversityType {
  const map: Record<number, UniversityType> = {
    1: "حكومية",
    2: "خاصة",
    3: "أهلية",
    4: "معاهد عليا",
    5: "أجنبية",
    6: "تكنولوجية",
  };
  return map[apiType] ?? "حكومية";
}

export interface DepartmentViewModel {
  id: number;
  nameAr: string | null;
  nameEn: string | null;
}

export interface UniversityBasicViewModel {
  id: number;
  nameAr: string | null;
  nameEn: string | null;
}

export interface CollegeViewModel {
  id: number;
  nameAr: string | null;
  nameEn: string | null;
  universityId: number;
  officialWebsite: string | null;
  location: string | null;
  description: string | null;
  fees: number | null;
  lastYearCoordination: number | null;
  feesCategoryA: number | null;
  feesCategoryB: number | null;
  feesCategoryC: number | null;
  feesPerHour: number | null;
  minimumHoursPerSemester: number | null;
  additionalFees: number | null;
  departmentsCount: number;
  departments: DepartmentViewModel[] | null;
  university?: UniversityBasicViewModel | null;
}

export interface UniversityViewModel {
  id: number;
  nameAr: string | null;
  nameEn: string | null;
  type: number;
  typeAr: string | null;
  officialWebsite: string | null;
  location: string | null;
  governorate: number;
  governorateAr: string | null;
  lastYearCoordination: number | null;
  fees: number | null;
  informationSources: string | null;
  description: string | null;
  collegesCount: number;
  branchesCount: number;
  colleges: CollegeViewModel[] | null;
}

function placeholderImageForUniversity(id: number): string {
  return `https://picsum.photos/seed/tansiqy-u${id}/800/480`;
}

function placeholderLogoForUniversity(id: number): string {
  return `https://picsum.photos/seed/tansiqy-l${id}/200/200`;
}

export function mapUniversityVmToUniversity(vm: UniversityViewModel): University {
  const typeLabel = apiTypeToUiUniversityType(vm.type) as UniversityType;
  const location =
    [vm.location?.trim(), vm.governorateAr?.trim()].filter(Boolean).join("، ") || "مصر";

  const description =
    vm.description?.trim() ||
    "لا توجد نبذة متاحة حالياً ضمن المصادر المعروضة؛ يُفضّل زيارة الموقع الرسمي للجامعة لمزيد من التفاصيل.";

  const officialSite = normalizeOfficialUrl(vm.officialWebsite);
  const logoFromSite = buildOfficialLogoUrlFromWebsite(vm.officialWebsite);
  const coverIconFromSite = buildOfficialCoverIconUrl(vm.officialWebsite);
  const useOfficialBranding = Boolean(logoFromSite);

  return {
    id: String(vm.id),
    nameAr: vm.nameAr?.trim() || "بدون اسم",
    nameEn: vm.nameEn?.trim() || "",
    type: typeLabel,
    location,
    image: useOfficialBranding && coverIconFromSite ? coverIconFromSite : placeholderImageForUniversity(vm.id),
    logo: logoFromSite ?? placeholderLogoForUniversity(vm.id),
    coverMode: useOfficialBranding ? "brand" : "photo",
    description,
    officialWebsite: officialSite,
  };
}

function collegeHasAnyFeeBreakdown(vm: CollegeViewModel): boolean {
  if (vm.fees != null && vm.fees > 0) return true;
  if (vm.feesCategoryA != null || vm.feesCategoryB != null || vm.feesCategoryC != null) return true;
  if (vm.feesPerHour != null && vm.minimumHoursPerSemester != null) return true;
  if (vm.additionalFees != null && vm.additionalFees > 0) return true;
  return false;
}

function formatCollegeFees(vm: CollegeViewModel): string {
  if (vm.fees != null && vm.fees > 0) {
    return `${vm.fees.toLocaleString("ar-EG")} ج.م (سنوياً تقريباً)`;
  }
  const parts: string[] = [];
  if (vm.feesCategoryA != null) parts.push(`فئة أ: ${vm.feesCategoryA.toLocaleString("ar-EG")}`);
  if (vm.feesCategoryB != null) parts.push(`فئة ب: ${vm.feesCategoryB.toLocaleString("ar-EG")}`);
  if (vm.feesCategoryC != null) parts.push(`فئة ج: ${vm.feesCategoryC.toLocaleString("ar-EG")}`);
  if (vm.feesPerHour != null && vm.minimumHoursPerSemester != null) {
    parts.push(
      `${vm.feesPerHour.toLocaleString("ar-EG")} ج.م/ساعة (حد أدنى ${vm.minimumHoursPerSemester} ساعة/فصل)`,
    );
  }
  if (vm.additionalFees != null && vm.additionalFees > 0) {
    parts.push(`رسوم إضافية: ${vm.additionalFees.toLocaleString("ar-EG")}`);
  }
  return parts.length > 0 ? parts.join(" — ") : "غير محدد";
}

/** مصاريف مسجّلة على مستوى الجامعة في الـ API */
function formatUniversityFeesLine(parent: UniversityViewModel): string | null {
  if (parent.fees != null && parent.fees > 0) {
    return `${parent.fees.toLocaleString("ar-EG")} ج.م (سنوياً تقريباً)`;
  }
  return null;
}

/**
 * يدمج بيانات الكلية مع بيانات الجامعة الأم عندما تكون حقول الكلية فارغة في المنصة
 * (نفس مصدر الـ API — ليس استيراداً من مواقع خارجية).
 */
export function mapCollegeVmToCollege(
  vm: CollegeViewModel,
  parentUniversity: UniversityViewModel | null,
): College {
  const deptNames =
    vm.departments?.map((d) => d.nameAr?.trim()).filter((x): x is string => Boolean(x)) ?? [];

  const hasCollegeCoord =
    vm.lastYearCoordination != null && vm.lastYearCoordination > 0;
  const parentCoord =
    parentUniversity?.lastYearCoordination != null && parentUniversity.lastYearCoordination > 0
      ? parentUniversity.lastYearCoordination
      : 0;
  const admissionPercentage = hasCollegeCoord ? vm.lastYearCoordination! : parentCoord;
  const coordinationFromUniversity = !hasCollegeCoord && parentCoord > 0;

  let fees = formatCollegeFees(vm);
  let feesFromUniversity = false;
  if (!collegeHasAnyFeeBreakdown(vm) && parentUniversity) {
    const uniFees = formatUniversityFeesLine(parentUniversity);
    if (uniFees) {
      fees = `${uniFees} — إطار عام للجامعة (لم يُسجَّل تفصيل منفصل لهذه الكلية ضمن البيانات المعروضة)`;
      feesFromUniversity = true;
    }
  }

  const description =
    vm.description?.trim() ||
    parentUniversity?.description?.trim() ||
    "لا يوجد وصف متاح حالياً.";

  return {
    id: String(vm.id),
    universityId: String(vm.universityId),
    nameAr: vm.nameAr?.trim() || "كلية",
    nameEn: vm.nameEn?.trim() || "",
    admissionPercentage,
    fees,
    description,
    departments: deptNames.length > 0 ? deptNames : [PLACEHOLDER_DEPARTMENTS_LABEL],
    officialWebsite: normalizeOfficialUrl(vm.officialWebsite),
    coordinationFromUniversity,
    feesFromUniversity,
  };
}

export async function fetchUniversitiesByType(type: number): Promise<University[]> {
  const list = await apiGet<UniversityViewModel[]>(`/api/Universities/type/${type}`);
  return list.map(mapUniversityVmToUniversity);
}

export async function fetchAllUniversities(): Promise<University[]> {
  const types = [1, 2, 3, 4, 5, 6];
  const chunks = await Promise.all(types.map((t) => fetchUniversitiesByType(t)));
  const byId = new Map<number, University>();
  for (const chunk of chunks) {
    for (const u of chunk) {
      byId.set(Number(u.id), u);
    }
  }
  return [...byId.values()];
}

export async function fetchUniversityById(id: number): Promise<University> {
  const vm = await apiGet<UniversityViewModel>(`/api/Universities/${id}`);
  return mapUniversityVmToUniversity(vm);
}

export async function fetchCollegesByUniversityId(universityId: number): Promise<College[]> {
  let parent: UniversityViewModel | null = null;
  try {
    parent = await apiGet<UniversityViewModel>(`/api/Universities/${universityId}`);
  } catch {
    parent = null;
  }
  const list = await apiGet<CollegeViewModel[]>(`/api/Universities/${universityId}/colleges`);
  return list.map((c) => mapCollegeVmToCollege(c, parent));
}

export async function fetchCollegeById(collegeId: number): Promise<College> {
  const vm = await apiGet<CollegeViewModel>(`/api/Colleges/${collegeId}`);
  let parent: UniversityViewModel | null = null;
  try {
    parent = await apiGet<UniversityViewModel>(`/api/Universities/${vm.universityId}`);
  } catch {
    parent = null;
  }
  return mapCollegeVmToCollege(vm, parent);
}

export async function searchUniversitiesIntelligent(
  searchTerm: string,
  apiType?: number,
): Promise<University[]> {
  const params = new URLSearchParams();
  const q = searchTerm.trim();
  if (q) params.set("searchTerm", q);
  if (apiType != null) params.set("type", String(apiType));
  const list = await fetchIntelligentSearchViewModels(params);
  return list.map(mapUniversityVmToUniversity);
}

export async function fetchUniversityViewModelsByType(type: number): Promise<UniversityViewModel[]> {
  return apiGet<UniversityViewModel[]>(`/api/Universities/type/${type}`);
}

export async function fetchAllUniversityViewModels(): Promise<UniversityViewModel[]> {
  const types = [1, 2, 3, 4, 5, 6];
  const chunks = await Promise.all(types.map((t) => fetchUniversityViewModelsByType(t)));
  const byId = new Map<number, UniversityViewModel>();
  for (const chunk of chunks) {
    for (const u of chunk) {
      byId.set(u.id, u);
    }
  }
  return [...byId.values()];
}

export async function fetchCollegeViewModelsForUniversity(
  universityId: number,
): Promise<{ parent: UniversityViewModel | null; colleges: CollegeViewModel[] }> {
  let parent: UniversityViewModel | null = null;
  try {
    parent = await apiGet<UniversityViewModel>(`/api/Universities/${universityId}`);
  } catch {
    parent = null;
  }
  const colleges = await apiGet<CollegeViewModel[]>(`/api/Universities/${universityId}/colleges`);
  return { parent, colleges };
}
