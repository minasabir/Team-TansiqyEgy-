/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_SUPABASE_URL?: string;
  readonly VITE_SUPABASE_PUBLISHABLE_KEY?: string;
  /** Override Tansiqy API root (default: dev uses Vite proxy /tansiqy-api, prod uses https://tansiqy.runasp.net) */
  readonly VITE_TANSIQY_API_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
