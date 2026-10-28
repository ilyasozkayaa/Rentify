export interface RentalImage {
  id: string;
  url: string;
  sortOrder: number;
  isPrimary: boolean;
}
export interface Listing {
  id: number;
  rentalType: string;
  title: string;
  city?: string | null;
  cityCode?: number;
  district?: string | null;
  price: number;
  currency: string;
  description?: string | null;
  attributes?: Record<string, unknown> | null;
  images: RentalImage[];
  createdAt?: string;
  ownerUserId?: number;
  ownerFirstName?: string;
}
export interface ListingDetails extends Listing {
  city: string;
  district?: string | null;
  createdAt: string;
  ownerFirstName: string;
}
export interface SearchResponse {
  results: Listing[];
  hasNextPage: boolean;
}
export interface PendingResponse {
  results: Listing[];
  hasNextPage: boolean;
}
export interface MyRent {
  id: number;
  rentableProductId: number;
  listingTitle: string;
  rentalType: string;
  city: string;
  district?: string | null;
  coverImageUrl?: string | null;
  startDate: string;
  endDate: string;
  dailyPrice: number;
  totalPrice: number;
  currency: string;
  status: number;
  createdAt: string;
}
export interface MyRentsResponse {
  page: number;
  pageSize: number;
  hasNextPage: boolean;
  results: MyRent[];
}
export interface CreateRentalResponse {
  rentableProductId: number;
  startDate: string;
  endDate: string;
  totalPrice: number;
  currency: string;
  status: number;
}
export interface LoginResponse {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  isAdmin: boolean;
  accessToken: string;
}
export interface UploadResponse {
  uploadBatchId: string;
  uploads: Array<{
    uploadId: string;
    url: string;
    storageKey: string;
    expiresAt: string;
  }>;
}
export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  errors?: Record<string, string[] | string>;
}
