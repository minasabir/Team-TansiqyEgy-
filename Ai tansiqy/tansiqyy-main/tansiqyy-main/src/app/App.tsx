import { RouterProvider } from "react-router";
import { ThemeProvider } from "next-themes";
import { router } from "./routes";
import { FavoritesProvider } from "./context/FavoritesContext";
import { Toaster } from "./components/ui/sonner";

export default function App() {
  return (
    <ThemeProvider attribute="class" defaultTheme="light" enableSystem>
      <FavoritesProvider>
        <RouterProvider router={router} />
        <Toaster />
      </FavoritesProvider>
    </ThemeProvider>
  );
}
