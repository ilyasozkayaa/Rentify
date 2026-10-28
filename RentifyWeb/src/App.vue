<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { RouterLink, RouterView, useRouter } from "vue-router";
import { clearToken } from "./api/client";
import { isAdmin, isAuthenticated } from "./utils/auth";

const router = useRouter();
const currentUser = ref({ signedIn: isAuthenticated(), admin: isAdmin() });
const signedIn = computed(() => currentUser.value.signedIn);
const admin = computed(() => currentUser.value.admin);
function refreshUser() {
  currentUser.value = { signedIn: isAuthenticated(), admin: isAdmin() };
}
const handleAuthChanged = () => refreshUser();
router.afterEach(refreshUser);
onMounted(() =>
  window.addEventListener("rentify:auth-changed", handleAuthChanged),
);
onUnmounted(() =>
  window.removeEventListener("rentify:auth-changed", handleAuthChanged),
);
function logout() {
  clearToken();
  refreshUser();
  void router.push("/");
}
</script>

<template>
  <div class="site-shell">
    <header class="topbar">
      <RouterLink to="/" class="brand" aria-label="Rentify home"
        ><span class="brand-mark">r</span><span>rentify</span></RouterLink
      >
      <nav class="nav-links" aria-label="Main navigation">
        <RouterLink to="/" class="nav-link">Explore</RouterLink>
        <RouterLink v-if="signedIn" to="/publish" class="nav-link"
          >List your place</RouterLink
        >
        <RouterLink v-if="signedIn" to="/my-rentals" class="nav-link"
          >Kiralamalarım</RouterLink
        >
        <RouterLink v-if="admin" to="/admin" class="nav-link"
          >Review listings</RouterLink
        >
      </nav>
      <div class="nav-actions">
        <button v-if="signedIn" class="button button-quiet" @click="logout">
          Sign out
        </button>
        <RouterLink v-else to="/login" class="button button-outline"
          >Sign in</RouterLink
        >
        <RouterLink v-if="!signedIn" to="/register" class="button button-dark"
          >Join Rentify</RouterLink
        >
      </div>
    </header>
    <main><RouterView /></main>
    <footer class="footer">
      <RouterLink to="/" class="brand footer-brand"
        ><span class="brand-mark">r</span><span>rentify</span></RouterLink
      ><span>Make room for what matters.</span
      ><span>© {{ new Date().getFullYear() }} Rentify</span>
    </footer>
  </div>
</template>
