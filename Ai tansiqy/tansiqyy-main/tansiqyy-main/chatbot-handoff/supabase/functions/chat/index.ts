import { serve } from "https://deno.land/std@0.168.0/http/server.ts";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type",
};

const TANSIQY_API_BASE = "http://tansiqy.runasp.net";

// Helper function to fetch data from Tansiqy API with retry
async function fetchFromTansiqyAPI(endpoint: string, retries = 3): Promise<any> {
  for (let attempt = 1; attempt <= retries; attempt++) {
    try {
      console.log(`Fetching from Tansiqy API (attempt ${attempt}): ${TANSIQY_API_BASE}${endpoint}`);
      
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 10000); // 10 second timeout
      
      const response = await fetch(`${TANSIQY_API_BASE}${endpoint}`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
          "User-Agent": "TansiqyBot/1.0",
        },
        signal: controller.signal,
      });
      
      clearTimeout(timeoutId);
      
      if (!response.ok) {
        console.error(`Tansiqy API error: ${response.status} - ${response.statusText}`);
        if (attempt < retries) {
          await new Promise(resolve => setTimeout(resolve, 1000 * attempt));
          continue;
        }
        return null;
      }
      
      const data = await response.json();
      console.log(`Tansiqy API response:`, JSON.stringify(data).substring(0, 500));
      return data;
    } catch (error) {
      console.error(`Error fetching from Tansiqy API (attempt ${attempt}):`, error);
      if (attempt < retries) {
        await new Promise(resolve => setTimeout(resolve, 1000 * attempt));
        continue;
      }
      return null;
    }
  }
  return null;
}

function createSSETextResponse(message: string, status = 200): Response {
  const encoder = new TextEncoder();
  const streamData = `data: ${JSON.stringify({ choices: [{ delta: { content: message } }] })}\n\ndata: [DONE]\n\n`;

  return new Response(encoder.encode(streamData), {
    status,
    headers: { ...corsHeaders, "Content-Type": "text/event-stream" },
  });
}

serve(async (req) => {
  if (req.method === "OPTIONS") {
    return new Response(null, { headers: corsHeaders });
  }

  try {
    const { messages } = await req.json();
    const LOVABLE_API_KEY = Deno.env.get("LOVABLE_API_KEY");
    
    if (!LOVABLE_API_KEY) {
      throw new Error("LOVABLE_API_KEY is not configured");
    }

    // Get the last user message to check if we need to fetch data
    const lastUserMessage = messages.filter((m: any) => m.role === "user").pop();
    let apiContext = "";
    
    if (lastUserMessage?.content) {
      const userQuery = lastUserMessage.content.toLowerCase();
      
      // ========== University Types Query ==========
      if (userQuery.includes("أنواع الجامعات") || userQuery.includes("انواع الجامعات") || userQuery.includes("نوع الجامعة") || userQuery.includes("أنواع") || userQuery.includes("انواع")) {
        const typesData = await fetchFromTansiqyAPI("/api/Universities/types");
        if (typesData) {
          apiContext += `\n\n[بيانات من API - أنواع الجامعات]:\n${JSON.stringify(typesData, null, 2)}`;
        }
      }
      
      // ========== Universities by Type ==========
      if (userQuery.includes("جامعة خاصة") || userQuery.includes("جامعات خاصة") || userQuery.includes("الخاصة")) {
        const privateUniData = await fetchFromTansiqyAPI("/api/Universities/type/private");
        if (privateUniData) {
          apiContext += `\n\n[بيانات من API - الجامعات الخاصة]:\n${JSON.stringify(privateUniData, null, 2)}`;
        }
      }
      
      if (userQuery.includes("جامعة حكومية") || userQuery.includes("جامعات حكومية") || userQuery.includes("الحكومية")) {
        const publicUniData = await fetchFromTansiqyAPI("/api/Universities/type/public");
        if (publicUniData) {
          apiContext += `\n\n[بيانات من API - الجامعات الحكومية]:\n${JSON.stringify(publicUniData, null, 2)}`;
        }
      }
      
      if (userQuery.includes("جامعة أهلية") || userQuery.includes("جامعات أهلية") || userQuery.includes("الأهلية") || userQuery.includes("اهلية")) {
        const ahlyaUniData = await fetchFromTansiqyAPI("/api/Universities/type/national");
        if (ahlyaUniData) {
          apiContext += `\n\n[بيانات من API - الجامعات الأهلية]:\n${JSON.stringify(ahlyaUniData, null, 2)}`;
        }
      }

      if (
        userQuery.includes("تكنولوجية") ||
        userQuery.includes("تكنولوجي") ||
        userQuery.includes("جامعات تكنولوجية")
      ) {
        const techUniData = await fetchFromTansiqyAPI("/api/Universities/type/6");
        if (techUniData) {
          apiContext += `\n\n[بيانات من API - الجامعات التكنولوجية]:\n${JSON.stringify(techUniData, null, 2)}`;
        }
      }

      if (
        userQuery.includes("أجنبية") ||
        userQuery.includes("أجنبي") ||
        userQuery.includes("جامعات أجنبية") ||
        userQuery.includes("دولية")
      ) {
        const foreignUniData = await fetchFromTansiqyAPI("/api/Universities/type/5");
        if (foreignUniData) {
          apiContext += `\n\n[بيانات من API - الجامعات الأجنبية]:\n${JSON.stringify(foreignUniData, null, 2)}`;
        }
      }

      if (
        userQuery.includes("معهد") ||
        userQuery.includes("معاهد") ||
        userQuery.includes("معاهد عليا") ||
        userQuery.includes("المعاهد العليا")
      ) {
        const instUniData = await fetchFromTansiqyAPI("/api/Universities/type/4");
        if (instUniData) {
          apiContext += `\n\n[بيانات من API - المعاهد العليا]:\n${JSON.stringify(instUniData, null, 2)}`;
        }
      }
      
      // ========== Search for Specific University by Name ==========
      // Extract any university name mentioned in the query
      const universityKeywords = [
        "جامعة الفيوم", "جامعة القاهرة", "جامعة عين شمس", "جامعة الأزهر", "جامعة حلوان",
        "جامعة الإسكندرية", "جامعة أسيوط", "جامعة المنصورة", "جامعة الزقازيق", "جامعة طنطا",
        "جامعة المنوفية", "جامعة بنها", "جامعة كفر الشيخ", "جامعة دمياط", "جامعة بورسعيد",
        "جامعة السويس", "جامعة جنوب الوادي", "جامعة أسوان", "جامعة سوهاج", "جامعة المنيا",
        "جامعة بني سويف", "جامعة الوادي الجديد", "جامعة مطروح", "جامعة العريش", "جامعة الأقصر",
        "جامعة دمنهور", "جامعة السادات", "جامعة النهضة", "جامعة مصر للعلوم", "جامعة الأهرام الكندية",
        "جامعة المستقبل", "جامعة 6 أكتوبر", "جامعة أكتوبر", "جامعة مصر الدولية",
        "الجامعة الألمانية", "الجامعة البريطانية", "الجامعة الأمريكية", "جامعة زويل",
        "جامعة الجلالة", "جامعة العلمين", "جامعة الملك سلمان", "الجامعة المصرية اليابانية"
      ];
      
      // Also check for partial names
      const partialNames = [
        "الفيوم", "القاهرة", "عين شمس", "الأزهر", "حلوان", "الإسكندرية", "أسيوط",
        "المنصورة", "الزقازيق", "طنطا", "المنوفية", "بنها", "كفر الشيخ", "دمياط",
        "بورسعيد", "السويس", "جنوب الوادي", "أسوان", "سوهاج", "المنيا", "بني سويف",
        "الوادي الجديد", "مطروح", "العريش", "الأقصر", "دمنهور", "السادات",
        "النهضة", "مصر للعلوم", "الأهرام الكندية", "المستقبل", "6 أكتوبر", "أكتوبر",
        "مصر الدولية", "الألمانية", "البريطانية", "الأمريكية", "زويل",
        "الجلالة", "العلمين", "الملك سلمان", "اليابانية"
      ];
      
      let searchedUniversity = null;
      
      // First check for full university names
      for (const uniName of universityKeywords) {
        if (userQuery.includes(uniName)) {
          searchedUniversity = uniName.replace("جامعة ", "").replace("الجامعة ", "");
          break;
        }
      }
      
      // Then check for partial names if no full name found
      if (!searchedUniversity) {
        for (const partialName of partialNames) {
          if (userQuery.includes(partialName)) {
            searchedUniversity = partialName;
            break;
          }
        }
      }
      
      if (searchedUniversity) {
        console.log(`Searching for university: ${searchedUniversity}`);
        
        // Use /api/Universities/search/name for name-based search
        const searchData = await fetchFromTansiqyAPI(`/api/Universities/search/name?name=${encodeURIComponent(searchedUniversity)}`);
        
        if (searchData && Array.isArray(searchData) && searchData.length > 0) {
          apiContext += `\n\n[بيانات من API - نتائج البحث عن "${searchedUniversity}"]:\n${JSON.stringify(searchData, null, 2)}`;
          
          // Fetch colleges for the first matching university
          const universityId = searchData[0]?.id;
          if (universityId) {
            const collegesData = await fetchFromTansiqyAPI(`/api/Universities/${universityId}/colleges`);
            if (collegesData) {
              apiContext += `\n\n[بيانات من API - كليات الجامعة]:\n${JSON.stringify(collegesData, null, 2)}`;
            }
          }
        } else {
          // Fallback to advanced search with filters
          const fallbackData = await fetchFromTansiqyAPI(`/api/Universities/search?name=${encodeURIComponent(searchedUniversity)}`);
          if (fallbackData && Array.isArray(fallbackData) && fallbackData.length > 0) {
            apiContext += `\n\n[بيانات من API - نتائج البحث المتقدم عن "${searchedUniversity}"]:\n${JSON.stringify(fallbackData, null, 2)}`;
          } else {
            apiContext += `\n\n[ملاحظة: لم يتم العثور على بيانات لجامعة "${searchedUniversity}" في قاعدة البيانات]`;
          }
        }
      }
      
      // ========== General University Query (without specific name) ==========
      if ((userQuery.includes("جامعة") || userQuery.includes("جامعات")) && !searchedUniversity && 
          !userQuery.includes("خاصة") && !userQuery.includes("حكومية") && !userQuery.includes("أهلية")) {
        const uniTypesData = await fetchFromTansiqyAPI("/api/Universities/types");
        if (uniTypesData) {
          apiContext += `\n\n[بيانات من API - أنواع الجامعات المتاحة]:\n${JSON.stringify(uniTypesData, null, 2)}`;
        }
      }
      
      // ========== College Queries ==========
      if ((userQuery.includes("كلية") || userQuery.includes("كليات")) && !searchedUniversity) {
        // If asking about colleges without specifying a university
        const uniTypesData = await fetchFromTansiqyAPI("/api/Universities/types");
        if (uniTypesData) {
          apiContext += `\n\n[بيانات من API - لمعرفة الكليات، هذه أنواع الجامعات المتاحة]:\n${JSON.stringify(uniTypesData, null, 2)}`;
        }
      }
      
      // ========== Coordination/Admission Score Queries ==========
      if (userQuery.includes("تنسيق") || userQuery.includes("حد أدنى") || userQuery.includes("درجات") || 
          userQuery.includes("مجموع") || userQuery.includes("قبول") || userQuery.includes("نسبة")) {
        // Fetch all university types to provide comprehensive data
        if (!apiContext.includes("أنواع الجامعات")) {
          const typesData = await fetchFromTansiqyAPI("/api/Universities/types");
          if (typesData) {
            apiContext += `\n\n[بيانات من API - معلومات التنسيق]:\n${JSON.stringify(typesData, null, 2)}`;
          }
        }
      }
      
      // ========== Fees/Cost Queries ==========
      if (userQuery.includes("مصاريف") || userQuery.includes("رسوم") || userQuery.includes("تكلفة") || userQuery.includes("سعر")) {
        // If asking about fees, we need university data
        if (!searchedUniversity && !apiContext.includes("الخاصة")) {
          const privateUniData = await fetchFromTansiqyAPI("/api/Universities/type/private");
          if (privateUniData) {
            apiContext += `\n\n[بيانات من API - مصاريف الجامعات الخاصة]:\n${JSON.stringify(privateUniData, null, 2)}`;
          }
        }
      }
      
      // ========== News Queries ==========
      if (userQuery.includes("أخبار") || userQuery.includes("جديد") || userQuery.includes("آخر") || 
          userQuery.includes("اخبار") || userQuery.includes("خبر") || userQuery.includes("مستجدات")) {
        const newsData = await fetchFromTansiqyAPI("/api/News");
        if (newsData) {
          apiContext += `\n\n[بيانات من API - آخر الأخبار]:\n${JSON.stringify(newsData, null, 2)}`;
        }
      }
      
      // ========== Location/Governorate Queries ==========
      if (userQuery.includes("موقع") || userQuery.includes("مكان") || userQuery.includes("فين") || 
          userQuery.includes("عنوان") || userQuery.includes("محافظة")) {
        // Location info is included in university search results
        if (!searchedUniversity && !apiContext) {
          const typesData = await fetchFromTansiqyAPI("/api/Universities/types");
          if (typesData) {
            apiContext += `\n\n[بيانات من API - للحصول على موقع جامعة محددة، حدد اسم الجامعة]:\n${JSON.stringify(typesData, null, 2)}`;
          }
        }
      }
    }

    console.log("Calling AI gateway with messages:", messages.length);
    if (apiContext) {
      console.log("Including API context in request");
    }

    const systemPrompt = `أنت "بحر" - المساعد الذكي من موقع تنسيقي. التاريخ الحالي: 2026.

اسمك "بحر" وهو اختصار لـ "بوابة لحياة جامعية رائعة". لما حد يسألك عن اسمك أو معناه، قول "أنا بحر، واختارنا الاسم ده لأنه اختصار لـ بوابة لحياة جامعية رائعة".

أنت مستشار جامعي ذكي للطلاب المصريين من موقع تنسيقي.

أنت متصل بقاعدة بيانات تنسيقي الخلفية (https://tansiqy.runasp.net) وتستخدم البيانات الحقيقية للرد على أسئلة الطلاب.

مهمتك مساعدة الطلاب في:
- اختيار الجامعات والكليات المناسبة لمجموعهم
- معرفة درجات القبول والحد الأدنى للكليات (بناءً على البيانات المتاحة)
- مقارنة التخصصات وفرص العمل
- نصائح للتنسيق والتقديم
- معلومات عن الجامعات الخاصة والمصاريف

قاعدة مهمة جداً للدرجات:
- دايماً اذكر درجات القبول كنسبة مئوية وليس كمجموع درجات
- مثال: قول "الحد الأدنى 85%" بدلاً من "الحد الأدنى 340 درجة"
- لو الطالب قالك مجموعه بالدرجات، حوّله لنسبة مئوية في ردك

قواعد مهمة جداً للكتابة:
- لا تستخدم أي رموز تنسيق مثل النجوم ** أو * أو # 
- لا تستخدم اختصارات مثل "الخ" أو "إلخ" أو "وهكذا"
- اكتب الكلام كامل ومنسق بشكل طبيعي بدون رموز
- استخدم الفقرات والأسطر الجديدة للتنظيم بدلاً من الرموز

قواعد استخدام البيانات:
- استخدم البيانات المقدمة من قاعدة البيانات فقط للرد على الأسئلة
- لو مفيش بيانات متاحة، قول للطالب إنك هتحدث المعلومات قريباً وشجعه يتابع معانا
- لا تختلق أي بيانات أو أرقام من عندك

تحدث دائماً بالعربية المصرية (عامية مصرية). كن ودوداً ومشجعاً. 
إذا سأل الطالب عن مجموعه، اقترح له الكليات المتاحة حسب شعبته.
لا توجه الطلاب لأي مواقع خارجية، دايماً قولهم يتابعوا معانا هنا على تنسيقي.${apiContext}`;

    let response: Response | null = null;

    for (let attempt = 1; attempt <= 3; attempt++) {
      response = await fetch("https://ai.gateway.lovable.dev/v1/chat/completions", {
        method: "POST",
        headers: {
          Authorization: `Bearer ${LOVABLE_API_KEY}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          model: "google/gemini-3-flash-preview",
          messages: [
            {
              role: "system",
              content: systemPrompt
            },
            ...messages,
          ],
          stream: true,
        }),
      });

      if (response.status !== 429 || attempt === 3) {
        break;
      }

      const retryAfterHeader = response.headers.get("retry-after");
      const retryAfterSeconds = retryAfterHeader ? Number(retryAfterHeader) : NaN;
      const waitMs = Number.isFinite(retryAfterSeconds)
        ? Math.max(1000, retryAfterSeconds * 1000)
        : attempt * 2000;

      console.warn(`AI rate-limited (attempt ${attempt}), retrying in ${waitMs}ms`);
      await new Promise((resolve) => setTimeout(resolve, waitMs));
    }

    if (!response) {
      throw new Error("AI gateway request failed to initialize");
    }

    if (!response.ok) {
      const errorText = await response.text();
      console.error("AI gateway error:", response.status, errorText);
      
      if (response.status === 429) {
        console.log("AI rate limit reached after retries, returning fallback message");
        const fallbackMessage = "السيرفر عليه ضغط كبير دلوقتي 🙏\n\nحاول تاني بعد دقيقة، أو ابعت سؤالك بشكل أقصر شوية وأنا معاك.";
        return createSSETextResponse(fallbackMessage, 200);
      }
      
      // Handle 402 (credits exhausted) gracefully with a friendly fallback message
      if (response.status === 402) {
        console.log("AI credits exhausted, returning fallback message");
        const fallbackMessage = "معلش يا صديقي، النظام مشغول دلوقتي شوية 🙈\n\nممكن تجرب تاني كمان شوية؟ أو لو عندك سؤال محدد عن جامعة معينة أو كلية، قولي وأنا هحاول أساعدك! 💪\n\nتقدر كمان تتصفح موقع تنسيقي للمعلومات اللي محتاجها.";
        return createSSETextResponse(fallbackMessage, 200);
      }
      
      return new Response(JSON.stringify({ error: "حصل مشكلة، حاول تاني" }), {
        status: 500,
        headers: { ...corsHeaders, "Content-Type": "application/json" },
      });
    }

    console.log("Streaming response from AI gateway");

    return new Response(response.body, {
      headers: { ...corsHeaders, "Content-Type": "text/event-stream" },
    });
  } catch (error) {
    console.error("Chat function error:", error);
    return new Response(JSON.stringify({ error: error instanceof Error ? error.message : "Unknown error" }), {
      status: 500,
      headers: { ...corsHeaders, "Content-Type": "application/json" },
    });
  }
});
