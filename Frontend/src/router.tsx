import { createBrowserRouter } from "react-router";
import {
  LoginPage,
  Products,
  Clients,
  Commandes,
  Profile,
  NotFoundPage,
  //HomePage,
  Dashboard,
} from "./pages";
import App from "./App";
const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      {
        index: true,
        element: <Dashboard />,
      },
      {
        path: "dashboard",
        element: <Dashboard />,
      },
      {
        path: "Products",
        element: <Products />,
      },

      {
        path: "clients",
        element: <Clients />,
      },
      {
        path: "commandes",
        element: <Commandes />,
      },
      {
        path: "profile",
        element: <Profile />,
      },
    ],
  },
  { path: "/login", element: <LoginPage /> },
  {
    path: "*",
    element: <NotFoundPage />,
  },
]);
export default router;
