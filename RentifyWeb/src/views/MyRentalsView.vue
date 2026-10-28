<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { RouterLink } from "vue-router";
import { errorText } from "../api/client";
import { getMyRents } from "../api/rentify";
import type { MyRent } from "../types/api";
import { formatMoney } from "../utils/money";

type Filter = "all" | "upcoming" | "past" | "cancelled";

const pageSize = 20;
const page = ref(1);
const hasNextPage = ref(false);
const rents = ref<MyRent[]>([]);
const loading = ref(true);
const loadingMore = ref(false);
const error = ref("");
const filter = ref<Filter>("all");
const now = new Date();
const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-${String(now.getDate()).padStart(2, "0")}`;

const filteredRents = computed(() =>
  rents.value.filter((rent) => {
    if (filter.value === "upcoming") return rent.status !== 2 && rent.endDate >= today;
    if (filter.value === "past") return rent.status !== 2 && rent.endDate < today;
    if (filter.value === "cancelled") return rent.status === 2;
    return true;
  }),
);

function statusLabel(status: number): string {
  switch (status) {
    case 0: return "Onay bekliyor";
    case 1: return "Onaylandı";
    case 2: return "İptal edildi";
    case 3: return "Pasif";
    default: return "Durum bilgisi yok";
  }
}

function statusClass(status: number): string {
  if (status === 1) return "rent-status rent-status-confirmed";
  if (status === 0) return "rent-status rent-status-pending";
  if (status === 2) return "rent-status rent-status-cancelled";
  return "rent-status";
}

function formatDate(value: string): string {
  const [year, month, day] = value.slice(0, 10).split("-").map(Number);
  if (!year || !month || !day) return value;
  return new Intl.DateTimeFormat("tr-TR", { dateStyle: "medium" }).format(
    new Date(year, month - 1, day),
  );
}

function formatCreatedAt(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? value
    : new Intl.DateTimeFormat("tr-TR", { dateStyle: "medium", timeStyle: "short" }).format(date);
}

async function loadRents(reset = false) {
  if (loadingMore.value || (loading.value && !reset)) return;
  error.value = "";
  if (reset) {
    loading.value = true;
    page.value = 1;
  } else {
    loadingMore.value = true;
  }

  try {
    const response = await getMyRents(page.value, pageSize);
    rents.value = reset ? response.results : [...rents.value, ...response.results];
    hasNextPage.value = response.hasNextPage;
  } catch (cause) {
    error.value = errorText(cause);
  } finally {
    loading.value = false;
    loadingMore.value = false;
  }
}

async function loadMore() {
  if (!hasNextPage.value || loadingMore.value) return;
  page.value += 1;
  await loadRents();
  if (error.value) page.value -= 1;
}

onMounted(() => void loadRents(true));
</script>

<template>
  <section class="page-wrap my-rentals-page">
    <header class="my-rentals-heading">
      <div>
        <span class="eyebrow">KİRALAMA GEÇMİŞİN</span>
        <h1>Kiralamalarım</h1>
        <p>Rezervasyonlarının durumunu ve ayrıntılarını buradan takip et.</p>
      </div>
      <RouterLink class="button button-dark" :to="{ name: 'home' }">
        İlanları Keşfet <span aria-hidden="true">↗</span>
      </RouterLink>
    </header>

    <div v-if="loading" class="state-card my-rentals-state" role="status">
      <span class="loader"></span>
      <p>Kiralamaların yükleniyor…</p>
    </div>

    <div v-else-if="error && rents.length === 0" class="my-rentals-error notice notice-error" role="alert">
      <span>{{ error }}</span>
      <button class="button button-outline" type="button" @click="loadRents(true)">
        Tekrar dene
      </button>
    </div>

    <div v-else-if="rents.length === 0" class="empty-state my-rentals-empty">
      <div class="empty-icon" aria-hidden="true">⌂</div>
      <h2>Henüz bir kiralaman yok</h2>
      <p>Yeni bir yer keşfet ve bir sonraki planını oluşturmaya başla.</p>
      <RouterLink class="button button-dark" :to="{ name: 'home' }">
        İlanları Keşfet <span aria-hidden="true">↗</span>
      </RouterLink>
    </div>

    <template v-else>
      <div v-if="error" class="my-rentals-error notice notice-error" role="alert">
        <span>{{ error }}</span>
        <button class="button button-outline" type="button" @click="loadMore">Tekrar dene</button>
      </div>
      <div class="rent-filters" role="group" aria-label="Kiralamaları filtrele">
        <button :class="{ active: filter === 'all' }" @click="filter = 'all'">Tümü</button>
        <button :class="{ active: filter === 'upcoming' }" @click="filter = 'upcoming'">Yaklaşan</button>
        <button :class="{ active: filter === 'past' }" @click="filter = 'past'">Geçmiş</button>
        <button :class="{ active: filter === 'cancelled' }" @click="filter = 'cancelled'">İptal edilen</button>
      </div>

      <div v-if="filteredRents.length" class="my-rentals-list">
        <article v-for="rent in filteredRents" :key="rent.id" class="my-rent-card">
          <RouterLink
            v-if="rent.rentableProductId > 0"
            class="my-rent-photo-link"
            :to="{ name: 'listing-detail', params: { id: rent.rentableProductId } }"
            :aria-label="`${rent.listingTitle} ilanını görüntüle`"
          >
            <img v-if="rent.coverImageUrl" :src="rent.coverImageUrl" :alt="rent.listingTitle" loading="lazy" />
            <span v-else class="image-placeholder" aria-hidden="true">⌂</span>
          </RouterLink>
          <div v-else class="my-rent-photo-link">
            <span class="image-placeholder" aria-hidden="true">⌂</span>
          </div>

          <div class="my-rent-content">
            <div class="my-rent-topline">
              <span class="type-tag">{{ rent.rentalType }}</span>
              <span :class="statusClass(rent.status)">{{ statusLabel(rent.status) }}</span>
            </div>
            <h2>
              <RouterLink
                v-if="rent.rentableProductId > 0"
                :to="{ name: 'listing-detail', params: { id: rent.rentableProductId } }"
              >{{ rent.listingTitle || 'İlan bilgisi artık mevcut değil' }}</RouterLink>
              <span v-else>{{ rent.listingTitle || 'İlan bilgisi artık mevcut değil' }}</span>
            </h2>
            <p class="listing-location">{{ [rent.district, rent.city].filter(Boolean).join(", ") || "Konum belirtilmemiş" }}</p>
            <dl class="my-rent-details">
              <div><dt>Tarihler</dt><dd>{{ formatDate(rent.startDate) }} – {{ formatDate(rent.endDate) }}</dd></div>
              <div><dt>Günlük fiyat</dt><dd>{{ formatMoney(rent.dailyPrice, rent.currency) }}</dd></div>
              <div><dt>Toplam</dt><dd>{{ formatMoney(rent.totalPrice, rent.currency) }}</dd></div>
            </dl>
            <div class="my-rent-meta">
              <span>Rezervasyon no: <strong>#{{ rent.id }}</strong></span>
              <span>Oluşturulma: {{ formatCreatedAt(rent.createdAt) }}</span>
            </div>
          </div>
        </article>
      </div>
      <div v-else class="empty-state my-rentals-filter-empty">
        <h2>Bu filtrede kiralama bulunamadı</h2>
        <button class="button button-outline" type="button" @click="filter = 'all'">Tüm kiralamaları göster</button>
      </div>

      <div v-if="hasNextPage" class="my-rentals-more">
        <button class="button button-outline" :disabled="loadingMore" @click="loadMore">
          {{ loadingMore ? "Yükleniyor…" : "Daha fazla göster" }}
        </button>
      </div>
    </template>
  </section>
</template>
