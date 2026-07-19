export interface LandingContent {
  title: string;
  tagline: string;
  // When unset, Landing falls back to a built-in abstract graphic themed off the current
  // Material palette (--mat-sys-*) instead of a literal image, so it stays on-brand for any
  // project built on this template without needing an asset.
  heroImageUrl?: string;
}

// Hardcoded until site settings (planned, see Design.md) exists to serve this per deployment.
export const DEFAULT_LANDING_CONTENT: LandingContent = {
  title: 'Orangepuff Web Template',
  tagline: 'A ready-made starting point for your next project.'
};
