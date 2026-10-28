<script setup lang="ts">
import { computed, ref } from "vue";
import { createListing, uploadImages } from "../api/rentify";
import { errorText } from "../api/client";
import { parsePriceInput } from "../utils/money";

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
const types = [
  { value: 1, label: "Vehicle" },
  { value: 2, label: "Property" },
  { value: 3, label: "Hotel" },
  { value: 4, label: "Villa" },
];
const type = ref(2);
const cityCode = ref(34);
const district = ref("");
const title = ref("");
const description = ref("");
const price = ref("");
const currency = ref("TRY");
const attributes = ref(
  '{\n  "bedrooms": 2,\n  "bathrooms": 1,\n  "seaView": false,\n  "furnished": true,\n  "pool": false,\n  "garden": false,\n  "detached": false\n}',
);
const files = ref<File[]>([]);
const fileInput = ref<HTMLInputElement>();
const loading = ref(false);
const error = ref("");
const success = ref("");
const typeLabel = computed(
  () =>
    types.find((option) => option.value === type.value)?.label ?? "Property",
);

function onFiles(event: Event) {
  const selected = [...((event.target as HTMLInputElement).files ?? [])];
  files.value = selected.slice(0, 3);
  error.value =
    selected.length > 3 ? "A maximum of 3 images can be uploaded." : "";
}
async function submit() {
  if (loading.value) return;
  error.value = "";
  success.value = "";
  if (
    files.value.some(
      (file) => !["image/jpeg", "image/png", "image/webp"].includes(file.type),
    )
  ) {
    error.value = "Only JPEG, PNG and WebP images are supported.";
    return;
  }
  if (files.value.some((file) => file.size > 5 * 1024 * 1024)) {
    error.value = "Each image must be no larger than 5 MB.";
    return;
  }
  let parsedAttributes: unknown;
  try {
    parsedAttributes = JSON.parse(attributes.value);
  } catch {
    error.value = "Attributes must be valid JSON.";
    return;
  }
  if (
    parsedAttributes === null ||
    typeof parsedAttributes !== "object" ||
    Array.isArray(parsedAttributes)
  ) {
    error.value = "Attributes must be a JSON object.";
    return;
  }
  const parsedPrice = parsePriceInput(price.value);
  if (parsedPrice === null) {
    error.value =
      "Enter a price greater than zero with no more than two decimal places.";
    return;
  }
  loading.value = true;
  try {
    const imageUploadIds = await uploadImages(files.value);
    const result = await createListing({
      rentalType: type.value,
      cityCode: Number(cityCode.value),
      district: district.value.trim() || null,
      title: title.value.trim(),
      description: description.value.trim() || null,
      currency: currency.value,
      attributes: parsedAttributes,
      imageUploadIds,
    }, parsedPrice);
    success.value = `${result.title} was submitted and is pending admin review.`;
    title.value = "";
    description.value = "";
    price.value = "";
    files.value = [];
    attributes.value = "{}";
    if (fileInput.value) fileInput.value.value = "";
  } catch (cause) {
    error.value = errorText(cause);
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <section class="page-wrap publish-wrap">
    <div class="page-intro">
      <span class="eyebrow">MAKE IT AVAILABLE</span>
      <h1>Share a place<br />worth <em>finding.</em></h1>
      <p>
        Give your rental a little introduction. Every new listing is reviewed
        before it goes live.
      </p>
    </div>
    <form class="publish-form" @submit.prevent="submit">
      <div class="form-section">
        <div class="form-section-title">
          <span>01</span>
          <div>
            <h2>The essentials</h2>
            <p>A few details to help guests find the right fit.</p>
          </div>
        </div>
        <div class="form-stack">
          <label class="field"
            ><span>Listing title</span
            ><input
              v-model="title"
              required
              maxlength="200"
              placeholder="A sunlit apartment near the old town" /></label
          ><label class="field"
            ><span>Description</span
            ><textarea
              v-model="description"
              rows="4"
              maxlength="2000"
              placeholder="What makes your place special?"
            />
          </label>
          <div class="form-row">
            <label class="field"
              ><span>Rental type</span
              ><select v-model.number="type">
                <option
                  v-for="option in types"
                  :key="option.value"
                  :value="option.value"
                >
                  {{ option.label }}
                </option>
              </select></label
            ><label class="field"
              ><span>City</span
              ><select v-model.number="cityCode">
                <option
                  v-for="(city, index) in cities"
                  :key="city"
                  :value="index + 1"
                >
                  {{ city }}
                </option>
              </select></label
            >
          </div>
          <label class="field"
            ><span>District <small>(optional)</small></span
            ><input v-model="district" maxlength="100" placeholder="Kadıköy"
          /></label>
          <div class="form-row">
            <label class="field"
              ><span>Price per day</span
              ><input
                v-model="price"
                type="text"
                inputmode="decimal"
                required
                placeholder="10.000,00" /></label
            ><label class="field"
              ><span>Currency</span
              ><select v-model="currency">
                <option>TRY</option>
                <option>USD</option>
                <option>EUR</option>
              </select></label
            >
          </div>
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">
          <span>02</span>
          <div>
            <h2>Listing attributes</h2>
            <p>
              Use the API’s JSON attributes for your
              {{ typeLabel.toLowerCase() }}.
            </p>
          </div>
        </div>
        <label class="field"
          ><span>Attributes (JSON object)</span
          ><textarea
            v-model="attributes"
            rows="7"
            spellcheck="false"
            required
            class="code-input"
          /><small
            >Vehicle: brand, model, modelYear, transmission, fuelType, seats.
            Property/Villa: bedrooms, bathrooms, seaView, detached, furnished,
            pool, garden. Hotel: stars, breakfastIncluded, pool, openBuffet,
            allInclusive, guestCapacity, parking, seaView, airConditioning,
            wifi, restaurant, gym, spa.</small
          ></label
        >
      </div>
      <div class="form-section">
        <div class="form-section-title">
          <span>03</span>
          <div>
            <h2>Add a few images</h2>
            <p>Up to 3 JPEG, PNG, or WebP files, 5 MB each.</p>
          </div>
        </div>
        <input
          ref="fileInput"
          class="file-input"
          type="file"
          accept="image/jpeg,image/png,image/webp"
          multiple
          @change="onFiles"
        /><button class="upload-drop" type="button" @click="fileInput?.click()">
          <span class="upload-icon">↥</span><strong>Choose images</strong
          ><span>or select files from your device</span>
        </button>
        <ul v-if="files.length" class="file-list">
          <li v-for="file in files" :key="file.name">
            {{ file.name }}
            <span>{{ (file.size / 1024 / 1024).toFixed(1) }} MB</span>
          </li>
        </ul>
      </div>
      <div v-if="error" class="notice notice-error" role="alert">
        {{ error }}
      </div>
      <div v-if="success" class="notice notice-success">{{ success }}</div>
      <div class="publish-submit">
        <p>
          Your listing will be marked <strong>Pending</strong> until reviewed.
        </p>
        <button class="button button-dark" type="submit" :disabled="loading">
          {{ loading ? "Publishing…" : "Submit for review" }}
          <span aria-hidden="true">↗</span>
        </button>
      </div>
    </form>
  </section>
</template>
