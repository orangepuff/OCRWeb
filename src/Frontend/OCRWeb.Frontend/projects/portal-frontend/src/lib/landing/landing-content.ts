export interface LandingContent {
  title: string;
  tagline: string;
  // When unset, Landing falls back to a built-in abstract graphic themed off the current
  // Material palette (--mat-sys-*) instead of a literal image, so it stays on-brand for any
  // consumer without needing an asset.
  heroImageUrl?: string;
}
