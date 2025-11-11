import { serve } from "https://deno.land/std@0.168.0/http/server.ts";
const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type, http-referer, x-title",
  "Access-Control-Allow-Methods": "POST, OPTIONS"
};
const OPENROUTER_API_URL = "https://openrouter.ai/api/v1";
const OPENROUTER_API_KEY = Deno.env.get("OPENROUTER_API_KEY");
serve(async (req) => {
  if (req.method === "OPTIONS") {
    return new Response("ok", {
      headers: corsHeaders
    });
  }
  try {
    if (req.method !== "POST") {
      return new Response(JSON.stringify({
        error: "Method not allowed"
      }), {
        status: 405,
        headers: {
          ...corsHeaders,
          "Content-Type": "application/json"
        }
      });
    }
    if (!OPENROUTER_API_KEY) {
      return new Response(JSON.stringify({
        error: "API key not configured"
      }), {
        status: 500,
        headers: {
          ...corsHeaders,
          "Content-Type": "application/json"
        }
      });
    }
    const authHeader = req.headers.get("Authorization");
    if (!authHeader) {
      return new Response(JSON.stringify({
        error: "Missing authorization"
      }), {
        status: 401,
        headers: {
          ...corsHeaders,
          "Content-Type": "application/json"
        }
      });
    }
    const requestBody = await req.json();
    const openRouterResponse = await fetch(`${OPENROUTER_API_URL}/chat/completions`, {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${OPENROUTER_API_KEY}`,
        "HTTP-Referer": "https://domadev0101.github.io/10xMotoNomad",
        "X-Title": "MotoNomad",
        "Content-Type": "application/json"
      },
      body: JSON.stringify(requestBody)
    });
    const responseText = await openRouterResponse.text();
    if (!openRouterResponse.ok) {
      return new Response(JSON.stringify({
        error: "OpenRouter error",
        details: responseText
      }), {
        status: openRouterResponse.status,
        headers: {
          ...corsHeaders,
          "Content-Type": "application/json"
        }
      });
    }
    return new Response(responseText, {
      status: 200,
      headers: {
        ...corsHeaders,
        "Content-Type": "application/json"
      }
    });
  } catch (error) {
    return new Response(JSON.stringify({
      error: "Internal error",
      message: String(error)
    }), {
      status: 500,
      headers: {
        ...corsHeaders,
        "Content-Type": "application/json"
      }
    });
  }
});
