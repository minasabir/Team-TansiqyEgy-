import type { College, University, UniversityType } from "@/app/data/mockData";
import {
  fetchAllUniversityViewModels,
  fetchCollegeViewModelsForUniversity,
  fetchIntelligentSearchViewModels,
  fetchUniversityViewModelsByType,
  mapCollegeVmToCollege,
  mapUniversityVmToUniversity,
  mergeUniversitySearchViewModels,
  uiUniversityTypeToApiType,
  type CollegeViewModel,
  type UniversityViewModel,
} from "@/lib/tansiqyApi";

export type CustomSearchStudyBranch = "كل الشعب" | "علمي علوم" | "علمي رياضة" | "أدبي";

export type CustomUniversitySearchFilters = {
  universityName: string;
  collegeName: string;
  universityType: UniversityType | "الكل";
  studyBranch: CustomSearchStudyBranch;
  governorate: string;
  feesMin: number;
  feesMax: number;
  coordinationMin: number;
  coordinationMax: number;
};

export type CustomUniversitySearchResults = {
  universities: University[];
  collegeHits: { university: University; college: College }[];
  /** تم اقتصار الجامعات المفحوصة لتسريع الطلب */
  truncated?: boolean;
};

/** أقصى عدد جامعات نجلب لها كليات في طلب واحد لتفادي البطء */
const MAX_UNIVERSITIES_FOR_COLLEGE_SCAN = 40;

const STUDY_KEYWORDS: Record<Exclude<CustomSearchStudyBranch, "كل الشعب">, string[]> = {
  "علمي علوم": [
    "علوم",
    "أحياء",
    "كيمياء",
    "فيزياء",
    "صيدلة",
    "طب",
    "سن",
    "علاج طبيعي",
    "تمريض",
  ],
  "علمي رياضة": ["هندسة", "رياض", "حاسب", "حاسبات", "أعمال", "هندسه"],
  أدبي: ["أدب", "لغة", "حقوق", "تجارة", "إعلام", "آثار", "تاريخ", "جغرافيا", "فلسفة"],
};

function norm(s: string): string {
  return s.trim().toLowerCase().replace(/\s+/g, " ");
}

function textHasAny(haystack: string, needles: string[]): boolean {
  const h = norm(haystack);
  return needles.some((n) => h.includes(norm(n)));
}

function collegeNumericFees(vm: CollegeViewModel): number | null {
  if (vm.fees != null && vm.fees > 0) return vm.fees;
  const cats = [vm.feesCategoryA, vm.feesCategoryB, vm.feesCategoryC].filter(
    (x): x is number => x != null && x > 0,
  );
  if (cats.length > 0) return Math.max(...cats);
  if (vm.feesPerHour != null && vm.minimumHoursPerSemester != null) {
    return vm.feesPerHour * vm.minimumHoursPerSemester * 2;
  }
  return null;
}

function coordinationPercent(vm: CollegeViewModel, parent: UniversityViewModel | null): number {
  if (vm.lastYearCoordination != null && vm.lastYearCoordination > 0) return vm.lastYearCoordination;
  if (parent?.lastYearCoordination != null && parent.lastYearCoordination > 0) {
    return parent.lastYearCoordination;
  }
  return 0;
}

function matchesStudyBranch(
  collegeNameAr: string,
  depts: string[],
  branch: CustomSearchStudyBranch,
): boolean {
  if (branch === "كل الشعب") return true;
  const blob = [collegeNameAr, ...depts].join(" ");
  return textHasAny(blob, STUDY_KEYWORDS[branch]);
}

function vmMatchesGovernorate(vm: UniversityViewModel, governorate: string): boolean {
  if (!governorate || governorate === "كل المحافظات") return true;
  const g = norm(vm.governorateAr ?? "");
  return g.includes(norm(governorate)) || norm(governorate).includes(g);
}

function vmMatchesUniversityName(vm: UniversityViewModel, q: string): boolean {
  if (!q.trim()) return true;
  const blob = `${vm.nameAr ?? ""} ${vm.nameEn ?? ""}`;
  return norm(blob).includes(norm(q));
}

export function hasCollegeLevelFilter(f: CustomUniversitySearchFilters): boolean {
  if (f.collegeName.trim()) return true;
  if (f.studyBranch !== "كل الشعب") return true;
  if (f.coordinationMin > 0 || f.coordinationMax < 100) return true;
  if (f.feesMin > 0 || f.feesMax < 4_000_000) return true;
  return false;
}

async function collectInitialViewModels(filters: CustomUniversitySearchFilters): Promise<UniversityViewModel[]> {
  const apiType =
    filters.universityType === "الكل" ? undefined : uiUniversityTypeToApiType(filters.universityType);

  const hasUni = filters.universityName.trim().length > 0;
  const hasCol = filters.collegeName.trim().length > 0;

  if (hasUni || hasCol) {
    const parts: UniversityViewModel[][] = [];
    if (hasUni) {
      const p = new URLSearchParams();
      p.set("searchTerm", filters.universityName.trim());
      if (apiType != null) p.set("type", String(apiType));
      parts.push(await fetchIntelligentSearchViewModels(p));
    }
    if (hasCol) {
      const p = new URLSearchParams();
      p.set("collegeName", filters.collegeName.trim());
      if (apiType != null) p.set("type", String(apiType));
      parts.push(await fetchIntelligentSearchViewModels(p));
    }
    return parts.length === 1 ? parts[0]! : mergeUniversitySearchViewModels(parts[0]!, parts[1]!);
  }

  if (apiType != null) {
    return fetchUniversityViewModelsByType(apiType);
  }

  return fetchAllUniversityViewModels();
}

export async function runCustomUniversitySearch(
  filters: CustomUniversitySearchFilters,
): Promise<CustomUniversitySearchResults> {
  let vms = await collectInitialViewModels(filters);
  vms = vms.filter((vm) => vmMatchesGovernorate(vm, filters.governorate));
  vms = vms.filter((vm) => vmMatchesUniversityName(vm, filters.universityName));

  if (!hasCollegeLevelFilter(filters)) {
    return { universities: vms.map(mapUniversityVmToUniversity), collegeHits: [] };
  }

  let truncated = false;
  if (vms.length > MAX_UNIVERSITIES_FOR_COLLEGE_SCAN) {
    vms = vms.slice(0, MAX_UNIVERSITIES_FOR_COLLEGE_SCAN);
    truncated = true;
  }

  const collegeHits: { university: University; college: College }[] = [];
  const universityIdsWithMatch = new Set<number>();

  const BATCH = 6;
  for (let i = 0; i < vms.length; i += BATCH) {
    const slice = vms.slice(i, i + BATCH);
    const loaded = await Promise.all(
      slice.map(async (vm) => {
        const { parent, colleges } = await fetchCollegeViewModelsForUniversity(vm.id);
        return { vm, parent: parent ?? vm, colleges };
      }),
    );

    for (const { vm, parent, colleges } of loaded) {
      const u = mapUniversityVmToUniversity(parent);
      for (const cvm of colleges) {
        const college = mapCollegeVmToCollege(cvm, parent);
        const depts =
          cvm.departments?.map((d) => d.nameAr?.trim()).filter((x): x is string => Boolean(x)) ?? [];

        if (filters.collegeName.trim()) {
          const q = filters.collegeName.trim();
          const hitName =
            norm(college.nameAr).includes(norm(q)) || norm(college.nameEn).includes(norm(q));
          if (!hitName) continue;
        }

        if (!matchesStudyBranch(college.nameAr, depts, filters.studyBranch)) continue;

        const coord = coordinationPercent(cvm, parent);
        if (coord > 0) {
          if (coord < filters.coordinationMin || coord > filters.coordinationMax) continue;
        } else if (filters.coordinationMin > 0) {
          continue;
        }

        const feeN = collegeNumericFees(cvm);
        if (feeN != null) {
          if (feeN < filters.feesMin || feeN > filters.feesMax) continue;
        }

        collegeHits.push({ university: u, college });
        universityIdsWithMatch.add(vm.id);
      }
    }
  }

  const universities = vms
    .filter((vm) => universityIdsWithMatch.has(vm.id))
    .map((vm) => mapUniversityVmToUniversity(vm));

  return {
    universities,
    collegeHits,
    truncated: truncated || undefined,
  };
}

/** استخراج قائمة محافظات من عيّنة الجامعات لملء القائمة المنسدلة */
export function uniqueGovernoratesFromVms(vms: UniversityViewModel[]): string[] {
  const set = new Set<string>();
  for (const vm of vms) {
    const g = vm.governorateAr?.trim();
    if (g) set.add(g);
  }
  return [...set].sort((a, b) => a.localeCompare(b, "ar"));
}
