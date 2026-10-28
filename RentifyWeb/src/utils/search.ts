export function validateSearchQuery(query: string): string | null {
  const trimmed = query.trim();
  if (!trimmed) return "Tell us what you are looking for.";
  if (
    trimmed.split(/\s+/).length < 3 ||
    trimmed.replace(/[^\p{L}\p{N}]/gu, "").length < 15
  ) {
    return "Please use at least three words and 15 letters or numbers to describe your search.";
  }
  if (trimmed.length > 500) return "Your search cannot exceed 500 characters.";
  return null;
}
