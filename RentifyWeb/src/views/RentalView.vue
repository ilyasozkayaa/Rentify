<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { ApiError, errorText } from "../api/client";
import { createRental, getListing, getMyRents } from "../api/rentify";
import type { ListingDetails } from "../types/api";
import { rentalSuccessDestination } from "../utils/rentalNavigation";
import {
  calculateRentalTotal,
  formatMoney,
  rentalDaysBetween,
} from "../utils/money";

const props = defineProps<{ id: string }>();
const router = useRouter();
const listing = ref<ListingDetails | null>(null);
const startDate = ref("");
const endDate = ref("");
const loading = ref(true);
const submitting = ref(false);
const unavailable = ref(false);
const error = ref("");
const confirmedRental = ref<Awaited<ReturnType<typeof createRental>> | null>(null);
const confirmedRentId = ref<number | null>(null);
const rentalDays = computed(() =>
  rentalDaysBetween(startDate.value, endDate.value),
);
const estimatedTotal = computed(() =>
  listing.value
    ? calculateRentalTotal(listing.value.price, rentalDays.value)
    : 0,
);
const today = new Date().toLocaleDateString("en-CA");

function formatBookingDate(value: string): string {
  const [year, month, day] = value.split("-").map(Number);
  if (!year || !month || !day) return value;
  return new Intl.DateTimeFormat("tr-TR", { dateStyle: "long" }).format(
    new Date(year, month - 1, day),
  );
}

function rentalStatusLabel(status: number): string {
  if (status === 0) return "Onay bekliyor";
  if (status === 1) return "Onaylandı";
  if (status === 2) return "İptal edildi";
  return "Pasif";
}

function rentalStatusClass(status: number): string {
  if (status === 0) return "rent-status rent-status-pending";
  if (status === 1) return "rent-status rent-status-confirmed";
  if (status === 2) return "rent-status rent-status-cancelled";
  return "rent-status";
}

onMounted(async () => {
  const id = Number(props.id);
  if (!Number.isSafeInteger(id) || id <= 0) {
    unavailable.value = true;
    loading.value = false;
    return;
  }
  try {
    listing.value = await getListing(id);
    try {
      const saved = sessionStorage.getItem("rentify.pending-rental-submission");
      const pending = saved ? JSON.parse(saved) as { rentableProductId?: number; startDate?: string; endDate?: string } : null;
      if (pending?.rentableProductId === id) {
        startDate.value = pending.startDate ?? "";
        endDate.value = pending.endDate ?? "";
      }
    } catch {
      // Continue with an empty form when session storage is unavailable or invalid.
    }
  } catch (cause) {
    if (cause instanceof ApiError && cause.status === 404)
      unavailable.value = true;
    else error.value = errorText(cause);
  } finally {
    loading.value = false;
  }
});

async function submit() {
  if (!listing.value || submitting.value) return;
  error.value = "";
  confirmedRental.value = null;
  confirmedRentId.value = null;
  if (rentalDays.value <= 0) {
    error.value = "Choose an end date after your start date.";
    return;
  }
  submitting.value = true;
  const payload = {
    rentableProductId: listing.value.id,
    startDate: startDate.value,
    endDate: endDate.value,
  };
  const storageKey = "rentify.pending-rental-submission";
  let idempotencyKey = "";
  try {
    const saved = sessionStorage.getItem(storageKey);
    const pending = saved ? JSON.parse(saved) as { key?: string; rentableProductId?: number; startDate?: string; endDate?: string } : null;
    idempotencyKey = pending?.key && pending.rentableProductId === payload.rentableProductId && pending.startDate === payload.startDate && pending.endDate === payload.endDate
      ? pending.key
      : crypto.randomUUID();
    sessionStorage.setItem(storageKey, JSON.stringify({ key: idempotencyKey, ...payload }));
  } catch {
    idempotencyKey = crypto.randomUUID();
  }
  const attemptStartedAt = Date.now();
  try {
    confirmedRental.value = await createRental(payload, idempotencyKey);
    try { sessionStorage.removeItem(storageKey); } catch { /* Optional storage only. */ }
    submitting.value = false;
    try {
      const response = await getMyRents();
      confirmedRentId.value = response.results.find((rent) =>
        rent.rentableProductId === payload.rentableProductId &&
        rent.startDate === payload.startDate &&
        rent.endDate === payload.endDate &&
        rent.totalPrice === confirmedRental.value?.totalPrice,
      )?.id ?? null;
    } catch {
      // The successful create response is authoritative; the optional number may be unavailable.
    }
  } catch (cause) {
    if (cause instanceof ApiError && (cause.status === 0 || cause.status >= 500 || (cause.status === 409 && !cause.code))) {
      try {
        const response = await getMyRents();
        const recentRent = response.results.find((rent) =>
          rent.rentableProductId === payload.rentableProductId &&
          rent.startDate === payload.startDate &&
          rent.endDate === payload.endDate &&
          Date.parse(rent.createdAt) >= attemptStartedAt - 5_000,
        );
        if (recentRent) {
          confirmedRentId.value = recentRent.id;
          confirmedRental.value = {
            rentableProductId: recentRent.rentableProductId,
            startDate: recentRent.startDate,
            endDate: recentRent.endDate,
            totalPrice: recentRent.totalPrice,
            currency: recentRent.currency,
            status: recentRent.status,
          };
          try { sessionStorage.removeItem(storageKey); } catch { /* Optional storage only. */ }
        } else {
          error.value = "İsteğin sonucu doğrulanamadı. Kiralamalarım sayfasını kontrol edebilir veya aynı işlemi güvenli şekilde tekrar deneyebilirsin.";
        }
      } catch {
        error.value = "İsteğin sonucu doğrulanamadı. Yeni işlem başlatmadan önce Kiralamalarım sayfasını kontrol et.";
      }
    } else {
      if (cause instanceof ApiError && (cause.status !== 409 || cause.code)) {
        try { sessionStorage.setItem(storageKey, JSON.stringify({ key: "", ...payload })); } catch { /* Optional storage only. */ }
      }
      error.value = errorText(cause);
    }
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <section v-if="loading" class="page-state">
    <span class="loader"></span>
    <p>Loading your rental details…</p>
  </section>
  <section v-else-if="unavailable" class="page-wrap unavailable-state">
    <span class="eyebrow">RENTAL UNAVAILABLE</span>
    <h1>This listing is no longer <em>available.</em></h1>
    <p>It may have been removed or is not published.</p>
    <button class="button button-dark" @click="router.push('/')">
      Back to search
    </button>
  </section>
  <section v-else-if="listing && confirmedRental" class="page-wrap rental-success-page" aria-live="polite">
    <div class="rental-success-card">
      <div class="rental-success-icon" aria-hidden="true">✓</div>
      <span class="eyebrow">KİRALAMA TAMAMLANDI</span>
      <h1>Kiralama talebiniz <em>başarıyla oluşturuldu.</em></h1>
      <p v-if="confirmedRentId" class="rental-confirmation-number">Rezervasyon numarası <strong>#{{ confirmedRentId }}</strong></p>
      <p class="rental-success-lead">Rezervasyon bilgileriniz aşağıda. Güncel durumunuzu Kiralamalarım sayfasından takip edebilirsiniz.</p>
      <article class="rental-confirmation-listing">
        <img v-if="listing.images.length" :src="listing.images.find((image) => image.isPrimary)?.url ?? listing.images[0]!.url" :alt="listing.title" />
        <div v-else class="image-placeholder" aria-hidden="true">⌂</div>
        <div>
          <span class="type-tag">{{ listing.rentalType }}</span>
          <h2>{{ listing.title }}</h2>
          <p>{{ [listing.district, listing.city].filter(Boolean).join(", ") }}</p>
        </div>
      </article>
      <dl class="rental-confirmation-details">
        <div><dt>Başlangıç</dt><dd>{{ formatBookingDate(confirmedRental.startDate) }}</dd></div>
        <div><dt>Bitiş</dt><dd>{{ formatBookingDate(confirmedRental.endDate) }}</dd></div>
        <div><dt>Günlük fiyat</dt><dd>{{ formatMoney(listing.price, listing.currency) }}</dd></div>
        <div><dt>Toplam tutar</dt><dd>{{ formatMoney(confirmedRental.totalPrice, confirmedRental.currency) }}</dd></div>
        <div><dt>Durum</dt><dd><span :class="rentalStatusClass(confirmedRental.status)">{{ rentalStatusLabel(confirmedRental.status) }}</span></dd></div>
      </dl>
      <div class="rental-success-actions">
        <RouterLink class="button button-outline" :to="rentalSuccessDestination('complete')">Tamam</RouterLink>
        <RouterLink class="button button-dark" :to="rentalSuccessDestination('my-rentals')">Kiralamalarımı Gör <span aria-hidden="true">↗</span></RouterLink>
      </div>
    </div>
  </section>
  <section v-else-if="listing" class="rental-page rental-page-detail">
    <div class="rental-side">
      <RouterLink
        :to="{ name: 'listing-detail', params: { id: listing.id } }"
        class="detail-breadcrumb"
        >← Back to listing</RouterLink
      ><span class="eyebrow">YOUR RENTAL</span>
      <h1>A little time<br /><em>away.</em></h1>
      <div class="rental-summary-card">
        <img
          v-if="listing.images.length"
          :src="
            listing.images.find((image) => image.isPrimary)?.url ??
            listing.images[0]!.url
          "
          :alt="listing.title"
        />
        <div v-else class="image-placeholder">⌂</div>
        <div class="rental-summary-content">
          <span class="type-tag">{{ listing.rentalType }}</span>
          <h2>{{ listing.title }}</h2>
          <p>
            {{ [listing.district, listing.city].filter(Boolean).join(", ") }}
          </p>
          <strong
            >{{ formatMoney(listing.price, listing.currency) }}
            <small>per day</small></strong
          >
        </div>
      </div>
      <div class="rental-host-line">
        <span class="host-avatar">{{
          listing.ownerFirstName.charAt(0).toUpperCase()
        }}</span
        ><span
          >Hosted by <strong>{{ listing.ownerFirstName }}</strong></span
        >
      </div>
    </div>
    <form class="rental-form" @submit.prevent="submit">
      <span class="eyebrow">DATES & PRICE</span>
      <h2>Choose your dates</h2>
      <label class="field"
        ><span>Check-in</span
        ><input
          v-model="startDate"
          type="date"
          required
          :min="today"
          @change="
            endDate && endDate <= startDate ? (endDate = '') : undefined
          " /></label
      ><label class="field"
        ><span>Check-out</span
        ><input
          v-model="endDate"
          type="date"
          required
          :min="startDate || today"
      /></label>
      <div class="price-breakdown">
        <div>
          <span
            >{{ formatMoney(listing.price, listing.currency) }} ×
            {{ rentalDays || 0 }} days</span
          ><strong>{{ formatMoney(estimatedTotal, listing.currency) }}</strong>
        </div>
        <div><span>Service fee</span><span>None</span></div>
        <div><span>Deposit</span><span>None</span></div>
        <div class="price-total">
          <strong>Estimated total</strong
          ><strong>{{ formatMoney(estimatedTotal, listing.currency) }}</strong>
        </div>
        <p>
          Estimate based on the current daily rate. Rentify checks the live
          price and date availability before confirming.
        </p>
      </div>
      <div v-if="error" class="notice notice-error" role="alert">
        {{ error }}
      </div>
      <button
        class="button button-dark form-submit"
        type="submit"
        :disabled="submitting || !rentalDays"
      >
        {{ submitting ? "Confirming…" : "Confirm rental" }}
        <span aria-hidden="true">↗</span>
      </button>
      <p class="fine-print">
        No booking is completed until Rentify confirms the current price and
        availability.
      </p>
    </form>
  </section>
  <section v-else-if="error" class="page-wrap unavailable-state">
    <span class="eyebrow">COULD NOT LOAD LISTING</span>
    <h1>We couldn’t reach this <em>rental.</em></h1>
    <p>{{ error }}</p>
    <button class="button button-outline" @click="router.go(0)">
      Try again
    </button>
  </section>
</template>
