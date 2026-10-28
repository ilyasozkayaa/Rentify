<script setup lang="ts">
import { onMounted, ref } from "vue";
import { activateListings, getPending, rejectListing } from "../api/rentify";
import { errorText } from "../api/client";
import type { Listing } from "../types/api";
import PhotoGallery from "../components/PhotoGallery.vue";
import { formatMoney } from "../utils/money";

const listings = ref<Listing[]>([]);
const selected = ref<number[]>([]);
const loading = ref(true);
const acting = ref(false);
const error = ref("");
const notice = ref("");
const reviewListing = ref<Listing | null>(null);
const rejectionReason = ref("");
const cities = [
  "Adana",
  "Adiyaman",
  "Afyonkarahisar",
  "Agri",
  "Amasya",
  "Ankara",
  "Antalya",
  "Artvin",
  "Aydin",
  "Balikesir",
  "Bilecik",
  "Bingol",
  "Bitlis",
  "Bolu",
  "Burdur",
  "Bursa",
  "Canakkale",
  "Cankiri",
  "Corum",
  "Denizli",
  "Diyarbakir",
  "Edirne",
  "Elazig",
  "Erzincan",
  "Erzurum",
  "Eskisehir",
  "Gaziantep",
  "Giresun",
  "Gumushane",
  "Hakkari",
  "Hatay",
  "Isparta",
  "Mersin",
  "Istanbul",
  "Izmir",
  "Kars",
  "Kastamonu",
  "Kayseri",
  "Kirklareli",
  "Kirsehir",
  "Kocaeli",
  "Konya",
  "Kutahya",
  "Malatya",
  "Manisa",
  "Kahramanmaras",
  "Mardin",
  "Mugla",
  "Mus",
  "Nevsehir",
  "Nigde",
  "Ordu",
  "Rize",
  "Sakarya",
  "Samsun",
  "Siirt",
  "Sinop",
  "Sivas",
  "Tekirdag",
  "Tokat",
  "Trabzon",
  "Tunceli",
  "Sanliurfa",
  "Usak",
  "Van",
  "Yozgat",
  "Zonguldak",
  "Aksaray",
  "Bayburt",
  "Karaman",
  "Kirikkale",
  "Batman",
  "Sirnak",
  "Bartin",
  "Ardahan",
  "Igdir",
  "Yalova",
  "Karabuk",
  "Kilis",
  "Osmaniye",
  "Duzce",
];
async function load() {
  loading.value = true;
  error.value = "";
  try {
    const result = await getPending();
    listings.value = result.results;
    selected.value = [];
  } catch (cause) {
    error.value = errorText(cause);
  } finally {
    loading.value = false;
  }
}
function toggle(id: number) {
  selected.value = selected.value.includes(id)
    ? selected.value.filter((value) => value !== id)
    : [...selected.value, id];
}
async function approve() {
  if (!selected.value.length || acting.value) return;
  acting.value = true;
  error.value = "";
  notice.value = "";
  try {
    const result = await activateListings(selected.value);
    notice.value = `${result.activatedCount} listing${result.activatedCount === 1 ? "" : "s"} approved and activated.`;
    await load();
  } catch (cause) {
    error.value = errorText(cause);
  } finally {
    acting.value = false;
  }
}
function inspect(listing: Listing) {
  reviewListing.value = listing;
  rejectionReason.value = "";
  error.value = "";
}
async function approveOne() {
  if (!reviewListing.value || acting.value) return;
  acting.value = true;
  error.value = "";
  try {
    await activateListings([reviewListing.value.id]);
    notice.value = `“${reviewListing.value.title}” was approved and activated.`;
    reviewListing.value = null;
    await load();
  } catch (cause) {
    error.value = errorText(cause);
  } finally {
    acting.value = false;
  }
}
async function reject() {
  if (!reviewListing.value || acting.value || !rejectionReason.value.trim())
    return;
  acting.value = true;
  error.value = "";
  try {
    const result = await rejectListing(
      reviewListing.value.id,
      rejectionReason.value.trim(),
    );
    notice.value = `“${result.title}” was rejected. The reason was saved.`;
    reviewListing.value = null;
    await load();
  } catch (cause) {
    error.value = errorText(cause);
  } finally {
    acting.value = false;
  }
}
function closeReview() {
  if (!acting.value) reviewListing.value = null;
}
onMounted(load);
</script>

<template>
  <section class="page-wrap admin-wrap">
    <div class="page-heading-row">
      <div>
        <span class="eyebrow">ADMINISTRATION · REVIEW QUEUE</span>
        <h1>Pending <em>listings.</em></h1>
        <p>
          Review newly submitted rentals and activate the ones ready to be seen.
        </p>
      </div>
      <button class="button button-outline" :disabled="loading" @click="load">
        ↻ Refresh
      </button>
    </div>
    <div v-if="notice" class="notice notice-success">{{ notice }}</div>
    <div v-if="error" class="notice notice-error" role="alert">{{ error }}</div>
    <div v-if="loading" class="state-card">
      <span class="loader"></span>
      <p>Loading pending listings…</p>
    </div>
    <div v-else-if="!listings.length" class="empty-state">
      <div class="empty-icon">✓</div>
      <h3>All caught up</h3>
      <p>There are no pending listings to review right now.</p>
    </div>
    <template v-else
      ><div class="admin-toolbar">
        <span>{{ listings.length }} awaiting review</span
        ><button
          class="button button-dark"
          :disabled="!selected.length || acting"
          @click="approve"
        >
          {{ acting ? "Approving…" : `Approve selected (${selected.length})` }}
        </button>
      </div>
      <div class="admin-list">
        <article
          v-for="listing in listings"
          :key="listing.id"
          class="admin-card"
        >
          <label class="select-listing"
            ><input
              type="checkbox"
              :checked="selected.includes(listing.id)"
              :aria-label="`Select ${listing.title}`"
              @change="toggle(listing.id)" /><span></span
          ></label>
          <div class="admin-photo">
            <img
              v-if="listing.images?.length"
              :src="listing.images[0]!.url"
              :alt="listing.title"
            />
            <span v-else>⌂</span>
          </div>
          <div class="admin-info">
            <div class="admin-card-heading">
              <span class="type-tag">{{ listing.rentalType }}</span
              ><time>{{
                listing.createdAt
                  ? new Date(listing.createdAt).toLocaleDateString()
                  : ""
              }}</time>
            </div>
            <h2>{{ listing.title }}</h2>
            <p>
              {{
                [listing.district, cities[(listing.cityCode ?? 0) - 1]]
                  .filter(Boolean)
                  .join(", ")
              }}
            </p>
            <p v-if="listing.description" class="admin-description">
              {{ listing.description }}
            </p>
            <div class="attributes">
              <span
                v-for="[key, value] in Object.entries(
                  listing.attributes ?? {},
                ).slice(0, 5)"
                :key="key"
                >{{ key }}: {{ String(value) }}</span
              >
            </div>
          </div>
          <div class="admin-price">
            {{ formatMoney(listing.price, listing.currency) }}
          </div>
          <button
            class="button button-outline admin-inspect"
            @click="inspect(listing)"
          >
            Inspect <span aria-hidden="true">↗</span>
          </button>
        </article>
      </div>
    </template>
    <div
      v-if="reviewListing"
      class="review-overlay"
      role="dialog"
      aria-modal="true"
      :aria-label="`Review ${reviewListing.title}`"
      @click.self="closeReview"
      @keydown.esc="closeReview"
    >
      <section class="review-dialog">
        <header class="review-dialog-head">
          <div>
            <span class="eyebrow"
              >PENDING REVIEW · LISTING #{{ reviewListing.id }}</span
            >
            <h2>{{ reviewListing.title }}</h2>
          </div>
          <button
            class="icon-button"
            type="button"
            aria-label="Close review"
            @click="closeReview"
          >
            ×
          </button>
        </header>
        <div v-if="error" class="notice notice-error" role="alert">
          {{ error }}
        </div>
        <PhotoGallery
          :images="reviewListing.images"
          :title="reviewListing.title"
        />
        <div class="review-detail-grid">
          <div>
            <span class="eyebrow">LISTING DETAILS</span>
            <dl class="detail-attributes">
              <div>
                <dt>Type</dt>
                <dd>{{ reviewListing.rentalType }}</dd>
              </div>
              <div>
                <dt>Location</dt>
                <dd>
                  {{
                    [
                      reviewListing.district,
                      cities[(reviewListing.cityCode ?? 0) - 1],
                    ]
                      .filter(Boolean)
                      .join(", ")
                  }}
                </dd>
              </div>
              <div>
                <dt>Daily rate</dt>
                <dd>
                  {{ formatMoney(reviewListing.price, reviewListing.currency) }}
                </dd>
              </div>
              <div>
                <dt>Owner user ID</dt>
                <dd>{{ reviewListing.ownerUserId }}</dd>
              </div>
              <div
                v-for="[key, value] in Object.entries(
                  reviewListing.attributes ?? {},
                )"
                :key="key"
              >
                <dt>{{ key.replace(/([A-Z])/g, " $1") }}</dt>
                <dd>
                  {{
                    typeof value === "boolean"
                      ? value
                        ? "Yes"
                        : "No"
                      : String(value)
                  }}
                </dd>
              </div>
            </dl>
          </div>
          <div class="review-description">
            <span class="eyebrow">DESCRIPTION</span>
            <p>
              {{
                reviewListing.description ||
                "The owner did not add a description."
              }}
            </p>
            <label class="field"
              ><span>Reason for rejection</span
              ><textarea
                v-model="rejectionReason"
                rows="4"
                maxlength="1000"
                placeholder="Tell the owner what needs to change…"
              /></label
            ><small class="reason-note"
              >This reason will be stored with the rejected listing.</small
            >
          </div>
        </div>
        <footer class="review-actions">
          <button
            class="button button-outline"
            type="button"
            :disabled="acting"
            @click="closeReview"
          >
            Close</button
          ><button
            class="button button-reject"
            type="button"
            :disabled="acting || !rejectionReason.trim()"
            @click="reject"
          >
            {{ acting ? "Saving…" : "Reject listing" }}</button
          ><button
            class="button button-dark"
            type="button"
            :disabled="acting"
            @click="approveOne"
          >
            {{ acting ? "Saving…" : "Approve and publish" }}
            <span aria-hidden="true">↗</span>
          </button>
        </footer>
      </section>
    </div>
  </section>
</template>
