import { createBrowserRouter } from "react-router";
import { Layout } from "./components/Layout";
import { Home } from "./pages/Home";
import { Universities } from "./pages/Universities";
import { Colleges } from "./pages/Colleges";
import { CollegeDetails } from "./pages/CollegeDetails";
import { Favorites } from "./pages/Favorites";
import { ChatBot } from "./pages/ChatBot";
export const router = createBrowserRouter([
  {
    path: "/",
    Component: Layout,
    children: [
      { index: true, Component: Home },
      { path: "universities", Component: Universities },
      { path: "universities/:id/colleges", Component: Colleges },
      { path: "colleges/:id", Component: CollegeDetails },
      { path: "favorites", Component: Favorites },
      { path: "chat", Component: ChatBot },
    ],
  },
]);
