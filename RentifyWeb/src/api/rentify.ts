import { api } from "./client";
import type {
  CreateRentalResponse,
  Listing,
  ListingDetails,
  LoginResponse,
  MyRentsResponse,
  PendingResponse,
  SearchResponse,
  UploadResponse,
} from "../types/api";

export const searchRentals = (query: string, page = 1) =>
  api<SearchResponse>("/api/Search/search", {
    method: "POST",
    body: JSON.stringify({ query, page }),
  });
export const login = (email: string, password: string) =>
  api<LoginResponse>("/api/Auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
export const register = (payload: {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}) =>
  api<{ email: string; firstName: string; lastName: string }>(
    "/api/Auth/register",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
  );
export const getPending = (page = 1) =>
  api<PendingResponse>(`/api/Search/pending?page=${page}&pageSize=50`);
export const getListing = (id: number) =>
  api<ListingDetails>(`/api/RentalListings/${id}`);
export const getMyRents = (page = 1, pageSize = 20) =>
  api<MyRentsResponse>(`/api/Rents?page=${page}&pageSize=${pageSize}`);
export const activateListings = (rentalProductIds: number[]) =>
  api<{ activatedIds: number[]; activatedCount: number }>(
    "/api/RentalListings/activate",
    {
      method: "PUT",
      body: JSON.stringify({ rentalProductIds }),
    },
  );
export const rejectListing = (rentalProductId: number, reason: string) =>
  api<{ rentalProductId: number; title: string; reason: string }>(
    "/api/RentalListings/reject",
    { method: "PUT", body: JSON.stringify({ rentalProductId, reason }) },
  );
export async function createListing(
  payload: Record<string, unknown>,
  exactPrice: string,
): Promise<{ title: string; status: number; createdAt: string }> {
  const body = JSON.stringify({ ...payload, price: null }).replace(
    '"price":null',
    `"price":${exactPrice}`,
  );
  return api("/api/RentalListings", {
    method: "POST",
    body,
  });
}
export async function uploadImages(files: File[]): Promise<string[]> {
  if (files.length === 0) return [];
  const batch = await api<UploadResponse>("/api/RentalListings/image-uploads", {
    method: "POST",
    body: JSON.stringify({
      files: files.map((file) => ({
        contentType: file.type,
        fileSize: file.size,
      })),
    }),
  });
  const results = await Promise.allSettled(
    batch.uploads.map(async (upload, index) => {
      const response = await fetch(upload.url, {
        method: "PUT",
        headers: { "Content-Type": files[index]!.type },
        body: files[index],
      });
      if (!response.ok)
        throw new Error(
          `Image ${index + 1} upload failed (${response.status}).`,
        );
      return upload.uploadId;
    }),
  );
  const failed = results.find((result) => result.status === "rejected");
  if (failed?.status === "rejected")
    throw failed.reason instanceof Error
      ? failed.reason
      : new Error("An image upload failed.");
  return results.flatMap((result) =>
    result.status === "fulfilled" ? [result.value] : [],
  );
}
export const createRental = (payload: {
  rentableProductId: number;
  startDate: string;
  endDate: string;
}, idempotencyKey: string) =>
  api<CreateRentalResponse>("/api/Rents", {
    method: "POST",
    headers: { "Idempotency-Key": idempotencyKey },
    body: JSON.stringify(payload),
  });

export type { Listing };
