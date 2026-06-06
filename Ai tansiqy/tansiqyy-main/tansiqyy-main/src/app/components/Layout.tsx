import { Outlet, Link, useLocation } from "react-router";
import { GraduationCap as CapIcon, Home, Heart, MessageCircle, Sparkles } from "lucide-react";
import clsx from "clsx";
import { useFavorites } from "../context/FavoritesContext";

export function Layout() {
  const location = useLocation();
  const { favorites } = useFavorites();

  const navLinks = [
    { key: "home", name: "الرئيسية", to: "/", icon: <Home className="w-4 h-4 md:w-5 md:h-5 shrink-0 md:ml-2" /> },
    { key: "universities", name: "الجامعات", to: "/universities", icon: <CapIcon className="w-4 h-4 md:w-5 md:h-5 shrink-0 md:ml-2" /> },
    {
      key: "personalized",
      name: "بحث مخصص لك",
      to: "/#personalized-search",
      icon: <Sparkles className="w-4 h-4 md:w-5 md:h-5 shrink-0 md:ml-2" />,
    },
    { key: "chat", name: "مساعد بحر", to: "/chat", icon: <MessageCircle className="w-4 h-4 md:w-5 md:h-5 shrink-0 md:ml-2" /> },
  ];

  const isNavActive = (to: string) => {
    if (to === "/#personalized-search") {
      return location.pathname === "/" && location.hash === "#personalized-search";
    }
    if (to === "/") {
      return location.pathname === "/" && location.hash === "";
    }
    return location.pathname === to || location.pathname.startsWith(`${to}/`);
  };

  return (
    <div
      className="min-h-screen flex flex-col font-sans text-slate-900 bg-gradient-to-b from-brand-cream via-slate-50/80 to-slate-100"
      dir="rtl"
      style={{ fontFamily: "'Cairo', sans-serif" }}
    >
      <header className="sticky top-0 z-50 border-b border-slate-200/70 bg-white/85 backdrop-blur-xl shadow-[0_1px_0_0_rgba(10,22,40,0.04)]">
        <div className="h-1 w-full bg-gradient-to-l from-brand-gold via-amber-500/90 to-brand-navy" aria-hidden />

        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-[4.25rem] sm:h-20 gap-2 sm:gap-3 min-h-[4.25rem]">
            <Link
              to="/"
              className="flex items-center gap-2 sm:gap-3 group shrink-0 focus-ring-brand rounded-xl py-1"
            >
              <div className="relative shrink-0">
                <div className="absolute inset-0 rounded-full bg-gradient-to-br from-brand-gold/30 to-brand-navy/20 blur-md opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                <img
                  src="/logo.png"
                  alt="Tansiqy EGY"
                  className="relative w-10 h-10 sm:w-12 sm:h-12 rounded-full object-cover shadow-lg shadow-brand-navy/10 ring-2 ring-white ring-offset-2 ring-offset-brand-cream/50 group-hover:ring-amber-200/80 transition-all duration-300 bg-white"
                  width={48}
                  height={48}
                />
              </div>
              <div className="flex flex-col text-right shrink-0">
                <span className="text-base sm:text-xl md:text-2xl font-black text-brand-navy tracking-tight leading-snug">
                  تنسيقي إيجي
                </span>
                <span className="text-[10px] sm:text-xs font-bold text-brand-gold tracking-wide uppercase whitespace-nowrap">
                  Tansiqy EGY
                </span>
              </div>
            </Link>

            <nav className="hidden md:flex items-center gap-1" aria-label="القائمة الرئيسية">
              {navLinks.map((link) => (
                <Link
                  key={link.key}
                  to={link.to}
                  className={clsx(
                    "flex items-center gap-0.5 px-4 py-2 rounded-xl text-sm font-bold transition-all duration-200 focus-ring-brand",
                    isNavActive(link.to)
                      ? "bg-brand-navy text-white shadow-md shadow-brand-navy/25"
                      : "text-slate-600 hover:text-brand-navy hover:bg-slate-100/90",
                  )}
                >
                  {link.icon}
                  {link.name}
                </Link>
              ))}
            </nav>

            <div className="flex items-center gap-2 sm:gap-4 shrink-0">
              <Link
                to="/favorites"
                className={clsx(
                  "flex items-center gap-1.5 sm:gap-2 font-bold transition-colors rounded-xl px-2 py-1.5 focus-ring-brand",
                  location.pathname === "/favorites"
                    ? "text-rose-600"
                    : "text-slate-600 hover:text-rose-600",
                )}
              >
                <div className="relative">
                  <Heart
                    className={clsx(
                      "w-5 h-5 sm:w-6 sm:h-6",
                      location.pathname === "/favorites" && "fill-rose-500 text-rose-500",
                    )}
                  />
                  {favorites.length > 0 && (
                    <span className="absolute -top-1.5 -right-1.5 min-w-[1.15rem] h-[1.15rem] px-0.5 bg-gradient-to-br from-rose-500 to-rose-600 text-white text-[10px] font-black flex items-center justify-center rounded-full border-2 border-white shadow-sm">
                      {favorites.length > 9 ? "9+" : favorites.length}
                    </span>
                  )}
                </div>
                <span className="hidden sm:inline text-sm">المفضلات</span>
              </Link>

              <Link
                to="/universities"
                className="hidden sm:inline-flex items-center justify-center px-5 py-2.5 rounded-xl text-sm font-black text-white bg-gradient-to-l from-brand-navy to-brand-navy-mid shadow-lg shadow-brand-navy/25 hover:shadow-xl hover:shadow-brand-navy/30 hover:from-brand-navy-mid hover:to-brand-navy-soft transition-all duration-300 focus-ring-brand ring-1 ring-white/10"
              >
                ابدأ الآن
              </Link>
            </div>
          </div>

          <nav
            className="md:hidden flex gap-2 pb-3 -mx-1 px-1 overflow-x-auto scrollbar-hide border-t border-slate-100/80 pt-2.5"
            aria-label="القائمة"
          >
            {navLinks.map((link) => (
              <Link
                key={link.key}
                to={link.to}
                className={clsx(
                  "flex items-center gap-1.5 shrink-0 px-3.5 py-2 rounded-xl text-xs font-bold transition-all focus-ring-brand",
                  isNavActive(link.to)
                    ? "bg-brand-navy text-white shadow-md"
                    : "bg-white/90 text-slate-700 border border-slate-200/80 shadow-sm",
                )}
              >
                {link.icon}
                {link.name}
              </Link>
            ))}
          </nav>
        </div>
      </header>

      <main className="flex-grow flex flex-col">
        <Outlet />
      </main>

      {location.pathname !== "/chat" && (
        <Link
          to="/chat"
          className="fixed bottom-5 end-5 z-[55] flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-brand-navy to-brand-navy-mid text-white shadow-[0_12px_40px_-8px_rgba(10,22,40,0.55)] ring-2 ring-amber-400/40 transition-all duration-300 hover:scale-105 hover:ring-amber-300/60 hover:shadow-[0_16px_48px_-8px_rgba(10,22,40,0.6)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400 focus-visible:ring-offset-2"
          aria-label="فتح مساعد بحر"
          title="مساعد بحر"
        >
          <MessageCircle className="h-7 w-7" strokeWidth={2.25} />
        </Link>
      )}

      <footer className="relative mt-auto overflow-hidden bg-gradient-to-b from-brand-navy via-brand-navy-mid to-[#071018] text-slate-300">
        <div
          className="absolute inset-0 opacity-[0.07] pointer-events-none"
          style={{
            backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='1'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
          }}
        />
        <div className="absolute top-0 inset-x-0 h-px bg-gradient-to-l from-transparent via-amber-500/50 to-transparent" />
        <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-12 relative">
          <div className="flex flex-col md:flex-row justify-between items-center gap-8">
            <div className="flex items-center gap-4">
              <img
                src="/logo.png"
                alt=""
                className="w-12 h-12 rounded-full object-cover ring-2 ring-amber-500/35 shadow-lg bg-white"
                width={48}
                height={48}
              />
              <div className="text-right">
                <span className="text-xl font-black text-white block tracking-tight">تنسيقي إيجي</span>
                <span className="text-sm font-semibold text-amber-200/90">Tansiqy EGY</span>
              </div>
            </div>
            <p className="text-sm font-medium text-slate-400 text-center md:text-right max-w-md leading-relaxed">
              دليلك الأكاديمي للجامعات والكليات في مصر — بيانات محدّثة وواجهة بسيطة.
            </p>
            <p className="text-xs font-semibold text-slate-500 whitespace-nowrap">
              © {new Date().getFullYear()} Tansiqy EGY
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
