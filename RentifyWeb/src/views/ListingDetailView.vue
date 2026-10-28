<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import PhotoGallery from "../components/PhotoGallery.vue";
import { getListing } from "../api/rentify";
import { ApiError, errorText } from "../api/client";
import type { ListingDetails } from "../types/api";
import { formatMoney } from "../utils/money";

const props = defineProps<{ id: string }>();
const router = useRouter();
const listing = ref<ListingDetails | null>(null);
const loading = ref(true);
const unavailable = ref(false);
const error = ref("");
const attributes = computed(() =>
  Object.entries(listing.value?.attributes ?? {}),
);

onMounted(async () => {
  const id = Number(props.id);
  if (!Number.isSafeInteger(id) || id <= 0) {
    unavailable.value = true;
    loading.value = false;
    return;
  }
  try {
    listing.value = await getListing(id);
  } catch (cause) {
    if (cause instanceof ApiError && cause.status === 404)
      unavailable.value = true;
    else error.value = errorText(cause);
  } finally {
    loading.value = false;
  }
});

function attributeLabel(key: string) {
  return key
    .replace(/([A-Z])/g, " $1")
    .replace(/^./, (letter) => letter.toUpperCase());
}
function attributeValue(value: unknown) {
  if (typeof value === "boolean") return value ? "Yes" : "No";
  return String(value);
}
</script>

<template>
  <section v-if="loading" class="page-state">
    <span class="loader"></span>
    <p>Loading this rental…</p>
  </section>
  <section v-else-if="unavailable" class="page-wrap unavailable-state">
    <span class="eyebrow">RENTAL UNAVAILABLE</span>
    <h1>This listing is no longer <em>available.</em></h1>
    <p>
      It may have been removed or is not published. Search for another place
      that fits your plans.
    </p>
    <button class="button button-dark" @click="router.push('/')">
      Back to search <span aria-hidden="true">↗</span>
    </button>
  </section>
  <section v-else-if="error" class="page-wrap unavailable-state">
    <span class="eyebrow">COULD NOT LOAD LISTING</span>
    <h1>We couldn’t reach this <em>rental.</em></h1>
    <p>{{ error }}</p>
    <button class="button button-outline" @click="router.go(0)">
      Try again
    </button>
  </section>
  <section v-else-if="listing" class="listing-detail-page">
    <div class="detail-breadcrumb">
      <RouterLink to="/">Explore rentals</RouterLink
      ><span aria-hidden="true">/</span><span>{{ listing.rentalType }}</span>
    </div>
    <div class="detail-heading">
      <div>
        <span class="type-tag">{{ listing.rentalType }}</span>
        <h1>{{ listing.title }}</h1>
        <p class="detail-location">
          <span aria-hidden="true">⌖</span>
          {{ [listing.district, listing.city].filter(Boolean).join(", ") }}
        </p>
      </div>
      <p class="detail-price">
        <strong>{{ formatMoney(listing.price, listing.currency) }}</strong
        ><span>per day</span>
      </p>
    </div>
    <div class="detail-columns">
      <div class="detail-main">
        <PhotoGallery :images="listing.images" :title="listing.title" />
        <section class="detail-info">
          <div class="host-line">
            <span class="host-avatar" aria-hidden="true">{{
              listing.ownerFirstName.charAt(0).toUpperCase()
            }}</span>
            <div>
              <strong>Hosted by {{ listing.ownerFirstName }}</strong
              ><span>Rentify host</span>
            </div>
          </div>
          <div v-if="listing.description" class="detail-block">
            <span class="eyebrow">A NOTE FROM YOUR HOST</span>
            <h2>About this place</h2>
            <p class="detail-description">{{ listing.description }}</p>
          </div>
          <div class="detail-block">
            <span class="eyebrow">THE DETAILS</span>
            <h2>What this rental offers</h2>
            <dl class="detail-attributes">
              <div>
                <dt>Type</dt>
                <dd>{{ listing.rentalType }}</dd>
              </div>
              <div>
                <dt>Location</dt>
                <dd>
                  {{
                    [listing.district, listing.city].filter(Boolean).join(", ")
                  }}
                </dd>
              </div>
              <div v-for="[key, value] in attributes" :key="key">
                <dt>{{ attributeLabel(key) }}</dt>
                <dd>{{ attributeValue(value) }}</dd>
              </div>
            </dl>
          </div>
          <p class="detail-footnote">
            Listing #{{ listing.id }} · Added
            {{ new Date(listing.createdAt).toLocaleDateString() }}
          </p>
        </section>
      </div>
      <aside class="booking-card">
        <span class="eyebrow">MAKE IT YOURS</span>
        <p class="booking-price">
          <strong>{{ formatMoney(listing.price, listing.currency) }}</strong
          ><span>per day</span>
        </p>
        <p class="booking-summary">
          Dates and total will be confirmed by Rentify before your rental is
          complete.
        </p>
        <RouterLink
          :to="{ name: 'rent', params: { id: listing.id } }"
          class="button button-dark booking-button"
          >Choose dates <span aria-hidden="true">↗</span></RouterLink
        >
        <p class="booking-fee-note">
          No extra fees are added to the displayed estimate.
        </p>
        <div class="booking-meta">
          <span>Listing type</span><strong>{{ listing.rentalType }}</strong
          ><span>Host</span><strong>{{ listing.ownerFirstName }}</strong>
        </div>
      </aside>
    </div>
  </section>
</template>
