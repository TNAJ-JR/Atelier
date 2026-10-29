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
import { productsLoader } from "./loaders/productsLoader";
import RouteError, { RouteErrorView } from "./components/RouteError/RouteError";
import ProductFormPage from "./pages/Products/ProductFormPage";
import ClientDetailPage from "./pages/Clients/ClientDetailPage";
import ClientFormPage from "./pages/Clients/ClientFormPage";
import OrderFormPage from "./pages/Commands/OrderFormPage";
import OrderDetailPage from "./pages/Commands/OrderDetailPage";
import PreviewsPage from "./pages/Previews/PreviewsPage";
import VisualStatesPage from "./pages/Previews/VisualStatesPage";
const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      {
        errorElement: <RouteError />,
        children: [
          { path: "products/new", element: <ProductFormPage /> },
          { path: "products/:id/edit", element: <ProductFormPage /> },
          { path: "clients/new", element: <ClientFormPage /> },
          { path: "clients/:id", element: <ClientDetailPage /> },
          { path: "customers", element: <Clients /> },
          { path: "customers/new", element: <ClientFormPage /> },
          { path: "customers/:id", element: <ClientDetailPage /> },
          { path: "commandes/new", element: <OrderFormPage /> },
          { path: "commandes/:id", element: <OrderDetailPage /> },
          { path: "orders", element: <Commandes /> },
          { path: "orders/new", element: <OrderFormPage /> },
          { path: "orders/:id", element: <OrderDetailPage /> },
          { path: "maquettes", element: <PreviewsPage /> },
          { path: "maquettes/catalogue", element: <Products /> },
          { path: "maquettes/etats", element: <VisualStatesPage /> },
          { path: "maquettes/produit-invalide", element: <ProductFormPage skuError /> },
          { path: "maquettes/commande-invalide", element: <OrderFormPage quantityError /> },
          { path: "maquettes/erreur", element: <RouteErrorView /> },
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
            loader: productsLoader,
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
          { path: "*", element: <NotFoundPage /> },
        ],
      },
    ],
  },
  { path: "/login", element: <LoginPage /> },
]);
export default router;
