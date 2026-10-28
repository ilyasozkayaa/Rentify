import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";
import AuthView from "../views/AuthView.vue";
import PublishView from "../views/PublishView.vue";
import AdminView from "../views/AdminView.vue";
import RentalView from "../views/RentalView.vue";
import ListingDetailView from "../views/ListingDetailView.vue";
import MyRentalsView from "../views/MyRentalsView.vue";
import { isAdmin, isAuthenticated } from "../utils/auth";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", name: "home", component: HomeView },
    {
      path: "/login",
      name: "login",
      component: AuthView,
      props: { mode: "login" },
    },
    {
      path: "/register",
      name: "register",
      component: AuthView,
      props: { mode: "register" },
    },
    {
      path: "/publish",
      name: "publish",
      component: PublishView,
      meta: { auth: true },
    },
    {
      path: "/admin",
      name: "admin",
      component: AdminView,
      meta: { auth: true, admin: true },
    },
    {
      path: "/listings/:id(\\d+)",
      name: "listing-detail",
      component: ListingDetailView,
      props: true,
    },
    {
      path: "/rent/:id(\\d+)",
      name: "rent",
      component: RentalView,
      props: true,
      meta: { auth: true },
    },
    {
      path: "/my-rentals",
      name: "my-rentals",
      component: MyRentalsView,
      meta: { auth: true },
    },
    { path: "/:pathMatch(.*)*", redirect: "/" },
  ],
  scrollBehavior: () => ({ top: 0 }),
});

router.beforeEach((to) => {
  if (to.meta.auth && !isAuthenticated())
    return { name: "login", query: { redirect: to.fullPath } };
  if (to.meta.admin && !isAdmin()) return { name: "home" };
});

export default router;
