<script setup lang="ts">
import { ref } from "vue";
import { searchRentals } from "../api/rentify";
import { ApiError, errorText } from "../api/client";
import ListingCard from "../components/ListingCard.vue";
import type { Listing } from "../types/api";
import { validateSearchQuery } from "../utils/search";

const query = ref("");
const listings = ref<Listing[]>([]);
const loading = ref(false);
const searched = ref(false);
const error = ref("");
const requiresSignIn = ref(false);

async function submitSearch() {
  if (loading.value) return;
  listings.value = [];
  searched.value = false;
  error.value = "";
  requiresSignIn.value = false;
  const trimmed = query.value.trim();
  const validationError = validateSearchQuery(trimmed);
  if (validationError) {
    error.value = validationError;
    return;
  }
  loading.value = true;
  try {
    const result = await searchRentals(trimmed);
    listings.value = result.results;
    searched.value = true;
  } catch (cause) {
    error.value = errorText(cause);
    requiresSignIn.value = cause instanceof ApiError && cause.status === 401;
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <section class="hero">
    <div class="hero-backdrop" aria-hidden="true">
      <div class="hero-orbit orbit-one"></div>
      <div class="hero-orbit orbit-two"></div>
      <div class="hero-sun"></div>
    </div>
    <div class="hero-copy">
      <span class="eyebrow"
        ><span class="eyebrow-dot"></span> A better way to find your stay</span
      >
      <h1>Find a place<br />that feels <em>like yours.</em></h1>
      <p>
        Tell us what you have in mind. We’ll find the rental that fits your next
        chapter.
      </p>
    </div>
    <form class="search-panel" @submit.prevent="submitSearch()">
      <label class="sr-only" for="rental-search"
        >Describe the rental you need</label
      >
      <span class="search-icon" aria-hidden="true">⌕</span>
      <input
        id="rental-search"
        v-model="query"
        name="query"
        type="search"
        maxlength="500"
        placeholder="A sunny villa in Antalya for two weeks in June..."
        :disabled="loading"
      />
      <button
        class="button button-dark search-button"
        type="submit"
        :disabled="loading"
      >
        {{ loading ? "Searching…" : "Find a rental" }}
        <span aria-hidden="true">↗</span>
      </button>
    </form>
    <p class="search-hint">
      Try something like “Automatic car in Izmir from July 10 to July 15”
    </p>
  </section>

  <section class="results-section" aria-live="polite">
    <div v-if="error" class="notice notice-error" role="alert">
      <span
        >{{ error }}
        <RouterLink v-if="requiresSignIn" to="/login" class="text-link"
          >Sign in</RouterLink
        ></span
      ><button
        class="icon-button"
        aria-label="Dismiss error"
        @click="
          error = '';
          requiresSignIn = false;
        "
      >
        ×
      </button>
    </div>
    <div v-if="loading && !listings.length" class="state-card">
      <span class="loader"></span>
      <p>Finding stays that fit your plans…</p>
    </div>
    <template v-else-if="searched">
      <div class="section-heading">
        <div>
          <span class="eyebrow">CURATED FOR YOUR PLANS</span>
          <h2>
            {{
              listings.length
                ? "Places worth a closer look"
                : "A quiet corner, just for you"
            }}
          </h2>
        </div>
        <span v-if="listings.length" class="result-count"
          >{{ listings.length }}
          {{ listings.length === 1 ? "rental" : "rentals" }}</span
        >
      </div>
      <div v-if="listings.length" class="listing-grid">
        <ListingCard
          v-for="listing in listings"
          :key="listing.id"
          :listing="listing"
        />
      </div>
      <div v-else-if="!error" class="empty-state">
        <div class="empty-icon">⌂</div>
        <h3>No rentals found yet</h3>
        <p>
          Try adding a location, dates, or a few more details to your search.
        </p>
      </div>
    </template>
    <div v-else class="inspiration-row">
      <div class="inspiration-mark">✳</div>
      <div>
        <strong>Start with a feeling.</strong
        ><span
          >Need a quiet week by the sea? A car for the open road? Just say the
          word.</span
        >
      </div>
    </div>
  </section>
  <section class="promise-band">
    <div>
      <span class="promise-number">01</span><strong>Search naturally</strong>
      <p>Describe the trip you’re dreaming about.</p>
    </div>
    <div>
      <span class="promise-number">02</span><strong>Find your fit</strong>
      <p>Explore real rentals returned by Rentify.</p>
    </div>
    <div>
      <span class="promise-number">03</span><strong>Make it yours</strong>
      <p>Request the dates that work for you.</p>
    </div>
  </section>
</template>
