// Shared configuration CORS for all Edge Functions
export const corsHeaders = {
  "Access-Control-Allow-Origin": "*", // In production, change to a specific domain
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type, http-referer, x-title",
  "Access-Control-Allow-Methods": "POST, GET, OPTIONS",
};
