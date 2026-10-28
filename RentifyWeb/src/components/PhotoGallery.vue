<script setup lang="ts">
import { computed, ref, watch } from "vue";
import type { RentalImage } from "../types/api";

const props = defineProps<{ images: RentalImage[]; title: string }>();
const activeIndex = ref(0);
const lightboxOpen = ref(false);
const activeImage = computed(() => props.images[activeIndex.value]);
watch(
  () => props.images,
  () => {
    activeIndex.value = 0;
    lightboxOpen.value = false;
  },
  { deep: true },
);
function move(step: number) {
  if (!props.images.length) return;
  activeIndex.value =
    (activeIndex.value + step + props.images.length) % props.images.length;
}
</script>

<template>
  <section class="photo-gallery" :aria-label="`${title} photos`">
    <button
      v-if="activeImage"
      class="gallery-feature"
      type="button"
      :aria-label="`Open photo ${activeIndex + 1} of ${images.length} for ${title}`"
      @click="lightboxOpen = true"
    >
      <img :src="activeImage.url" :alt="`${title} photo ${activeIndex + 1}`" />
      <span class="gallery-expand" aria-hidden="true">⤢</span>
    </button>
    <div v-else class="gallery-empty">
      <span aria-hidden="true">⌂</span>
      <p>No photos have been added to this listing.</p>
    </div>
    <div
      v-if="images.length > 1"
      class="gallery-thumbnails"
      role="list"
      :aria-label="`${title} photo thumbnails`"
    >
      <button
        v-for="(image, index) in images"
        :key="image.id"
        type="button"
        role="listitem"
        class="gallery-thumbnail"
        :class="{ 'is-active': index === activeIndex }"
        :aria-label="`Show photo ${index + 1}`"
        :aria-pressed="index === activeIndex"
        @click="activeIndex = index"
      >
        <img
          :src="image.url"
          :alt="`${title} thumbnail ${index + 1}`"
          loading="lazy"
        />
        <span v-if="image.isPrimary" class="cover-badge">Cover</span>
      </button>
    </div>
    <div
      v-if="lightboxOpen && activeImage"
      class="gallery-lightbox"
      role="dialog"
      aria-modal="true"
      :aria-label="`${title} photo viewer`"
      @click.self="lightboxOpen = false"
      @keydown.esc="lightboxOpen = false"
    >
      <button
        class="lightbox-close"
        type="button"
        aria-label="Close photo viewer"
        @click="lightboxOpen = false"
      >
        ×
      </button>
      <button
        v-if="images.length > 1"
        class="lightbox-arrow lightbox-prev"
        type="button"
        aria-label="Previous photo"
        @click="move(-1)"
      >
        ‹
      </button>
      <figure class="lightbox-figure">
        <img
          :src="activeImage.url"
          :alt="`${title} photo ${activeIndex + 1}`"
        />
        <figcaption>
          {{ activeIndex + 1 }} / {{ images.length }} · {{ title }}
        </figcaption>
      </figure>
      <button
        v-if="images.length > 1"
        class="lightbox-arrow lightbox-next"
        type="button"
        aria-label="Next photo"
        @click="move(1)"
      >
        ›
      </button>
    </div>
  </section>
</template>
