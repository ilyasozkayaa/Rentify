<script setup lang="ts">
import { useRouter } from "vue-router";
import type { Listing } from "../types/api";
import { formatMoney } from "../utils/money";

defineProps<{ listing: Listing }>();
const router = useRouter();
function openDetails(id: number) {
  void router.push({ name: "listing-detail", params: { id } });
}
function handleCardKey(event: KeyboardEvent, id: number) {
  if (event.key === "Enter" || event.key === " ") {
    event.preventDefault();
    openDetails(id);
  }
}
</script>

<template>
  <article
    class="listing-card"
    role="link"
    tabindex="0"
    :aria-label="`View ${listing.title} details`"
    @click="openDetails(listing.id)"
    @keydown="handleCardKey($event, listing.id)"
  >
    <div class="listing-image-wrap">
      <img
        v-if="listing.images?.length"
        :src="listing.images[0]!.url"
        :alt="listing.title"
        class="listing-image"
        loading="lazy"
      />
      <div v-else class="image-placeholder" aria-label="No image provided">
        <span>✦</span>
      </div>
      <span class="type-tag">{{ listing.rentalType }}</span>
    </div>
    <div class="listing-content">
      <div class="listing-heading">
        <h3>{{ listing.title }}</h3>
        <span class="listing-price"
          >{{ formatMoney(listing.price, listing.currency) }}
          <small>per day</small></span
        >
      </div>
      <p class="listing-location">
        {{
          [listing.district, listing.city].filter(Boolean).join(", ") ||
          "Location not specified"
        }}
      </p>
      <p v-if="listing.description" class="listing-description">
        {{ listing.description }}
      </p>
      <div
        v-if="listing.attributes && Object.keys(listing.attributes).length"
        class="attributes"
      >
        <span
          v-for="[key, value] in Object.entries(listing.attributes).slice(0, 3)"
          :key="key"
          >{{ key.replace(/([A-Z])/g, " $1") }}: {{ String(value) }}</span
        >
      </div>
      <RouterLink
        :to="{ name: 'listing-detail', params: { id: listing.id } }"
        class="button button-dark card-action"
        @click.stop
        >View listing <span aria-hidden="true">↗</span></RouterLink
      >
    </div>
  </article>
</template>
