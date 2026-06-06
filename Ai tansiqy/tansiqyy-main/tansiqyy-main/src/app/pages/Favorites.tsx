import { Link } from "react-router";
import { useFavorites } from "../context/FavoritesContext";
import { CollegeCard } from "../components/CollegeCard";
import { Heart } from "lucide-react";
import { useEffect, useState } from "react";
import type { College } from "../data/mockData";
import { fetchCollegeById } from "@/lib/tansiqyApi";

export function Favorites() {
  const { favorites } = useFavorites();
  const [savedColleges, setSavedColleges] = useState<College[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (favorites.length === 0) {
      setSavedColleges([]);
      setLoading(false);
      return;
    }

    let cancelled = false;
    (async () => {
      setLoading(true);
      try {
        const results = await Promise.all(
          favorites.map((fid) =>
            fetchCollegeById(Number(fid)).catch(() => null),
          ),
        );
        if (!cancelled) {
          setSavedColleges(results.filter((c): c is College => c !== null));
        }
      } catch {
        if (!cancelled) setSavedColleges([]);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [favorites]);

  return (
    <div className="flex-grow bg-slate-50 min-h-[calc(100vh-80px)] py-12">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="mb-10 text-center md:text-right">
          <h1 className="text-3xl md:text-4xl font-black text-slate-900 mb-4 flex items-center justify-center md:justify-start gap-3">
            <Heart className="w-8 h-8 text-rose-500 fill-rose-500" />
            كلياتي المفضلة
          </h1>
          <p className="text-lg text-slate-600 font-medium">
            قائمة بالكليات التي قمت بحفظها للرجوع إليها لاحقاً ومقارنتها.
          </p>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
            {[1, 2].map((i) => (
              <div key={i} className="h-64 bg-white rounded-2xl border animate-pulse" />
            ))}
          </div>
        ) : savedColleges.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
            {savedColleges.map((college) => (
              <CollegeCard key={college.id} college={college} />
            ))}
          </div>
        ) : (
          <div className="bg-white rounded-3xl p-16 text-center shadow-sm border border-slate-100 max-w-2xl mx-auto mt-10">
            <div className="w-24 h-24 bg-rose-50 rounded-full flex items-center justify-center mx-auto mb-6">
              <Heart className="w-12 h-12 text-rose-300" />
            </div>
            <h3 className="text-2xl font-bold text-slate-800 mb-3">لا توجد كليات في المفضلة</h3>
            <p className="text-slate-500 font-medium mb-8">
              قم بتصفح الجامعات والكليات واحفظ ما يناسبك للرجوع إليه هنا.
            </p>
            <Link
              to="/universities"
              className="inline-flex items-center justify-center px-8 py-3.5 bg-blue-600 text-white rounded-xl font-bold hover:bg-blue-700 transition-colors shadow-sm"
            >
              تصفح الجامعات
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}
