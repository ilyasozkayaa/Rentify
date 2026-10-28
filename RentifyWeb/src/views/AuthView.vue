<script setup lang="ts">
import { computed, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { login, register } from "../api/rentify";
import { errorText, setToken } from "../api/client";

const props = defineProps<{ mode: "login" | "register" }>();
const router = useRouter();
const route = useRoute();
const email = ref("");
const password = ref("");
const firstName = ref("");
const lastName = ref("");
const loading = ref(false);
const error = ref("");
const success = ref("");
const registering = computed(() => props.mode === "register");

async function submit() {
  if (loading.value) return;
  error.value = "";
  success.value = "";
  loading.value = true;
  try {
    if (registering.value) {
      await register({
        email: email.value.trim(),
        password: password.value,
        firstName: firstName.value.trim(),
        lastName: lastName.value.trim(),
      });
      success.value = "Your account is ready. Sign in to continue.";
      await router.replace("/login");
    } else {
      const user = await login(email.value.trim(), password.value);
      setToken(user.accessToken);
      await router.replace(
        typeof route.query.redirect === "string" ? route.query.redirect : "/",
      );
    }
  } catch (cause) {
    error.value = errorText(cause);
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <section class="auth-layout">
    <div class="auth-art">
      <div class="auth-art-label">
        <span class="brand-mark">r</span> RENTIFY JOURNAL · ISSUE 01
      </div>
      <div class="auth-art-copy">
        <span class="eyebrow">THE WORLD IS OPEN</span>
        <h1>Somewhere<br />good starts<br /><em>with you.</em></h1>
        <p>Find your next favorite place, one thoughtful stay at a time.</p>
      </div>
      <div class="auth-art-stamp">R<br /><small>EST. 2026</small></div>
    </div>
    <div class="auth-form-wrap">
      <div class="auth-form-head">
        <span class="eyebrow">{{
          registering ? "YOUR NEXT CHAPTER" : "WELCOME BACK"
        }}</span>
        <h2>
          {{ registering ? "Create your account" : "Sign in to Rentify" }}
        </h2>
        <p>
          {{
            registering
              ? "A few details and you’re on your way."
              : "Pick up where your next adventure begins."
          }}
        </p>
      </div>
      <form class="form-stack" @submit.prevent="submit">
        <template v-if="registering"
          ><div class="form-row">
            <label class="field"
              ><span>First name</span
              ><input
                v-model="firstName"
                required
                maxlength="100"
                autocomplete="given-name" /></label
            ><label class="field"
              ><span>Last name</span
              ><input
                v-model="lastName"
                required
                maxlength="50"
                autocomplete="family-name"
            /></label></div
        ></template>
        <label class="field"
          ><span>Email address</span
          ><input
            v-model="email"
            type="email"
            required
            maxlength="150"
            autocomplete="email"
        /></label>
        <label class="field"
          ><span>Password</span
          ><input
            v-model="password"
            type="password"
            required
            :minlength="registering ? 8 : undefined"
            maxlength="50"
            :autocomplete="registering ? 'new-password' : 'current-password'"
          /><small v-if="registering">Use 8–50 characters.</small></label
        >
        <div v-if="error" class="notice notice-error" role="alert">
          {{ error }}
        </div>
        <div v-if="success" class="notice notice-success">{{ success }}</div>
        <button
          class="button button-dark form-submit"
          type="submit"
          :disabled="loading"
        >
          {{
            loading
              ? "Please wait…"
              : registering
                ? "Create account"
                : "Sign in"
          }}
          <span aria-hidden="true">↗</span>
        </button>
      </form>
      <p class="auth-switch">
        {{ registering ? "Already have an account?" : "New to Rentify?" }}
        <RouterLink :to="registering ? '/login' : '/register'">{{
          registering ? "Sign in" : "Create an account"
        }}</RouterLink>
      </p>
    </div>
  </section>
</template>
