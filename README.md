# RapSuite

AI-powered lyrics creation platform built with Blazor. Generate original rap songs from any situation in any language, or transform existing lyrics into rap format with rhymes, flow, and structure.

## Features

- **AI Lyrics Generation** — Describe a situation/theme and get a full rap song with verses, hooks, and bridges
- **Rephrase to Rap** — Paste any lyrics and transform them into rap format with customizable style (Trap, Boom Bap, Drill, etc.)
- **Multi-Language Support** — Tamil (pure செந்தமிழ்), Hindi, English, Spanish, Korean, Japanese, French, Portuguese
- **Album Management** — Create albums, save generated songs, edit lyrics, and organize your collection
- **User Authentication** — Sign up / sign in via Firebase Auth with session management per Blazor circuit

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | .NET 10.0 / Blazor Server (Interactive Server) |
| **AI** | NVIDIA NIM API — Meta LLaMA 3.1 405B Instruct |
| **Auth** | Firebase Authentication (REST API) |
| **Database** | Cloud Firestore (REST API) |
| **UI** | Bootstrap 5, custom dark theme with glass-card aesthetic |
| **Packages** | Blazored.LocalStorage, Blazored.Toast, FirebaseAdmin, Google.Cloud.Firestore |

## Project Structure

```
RapSuite/
├── Components/
│   ├── Layout/                  # MainLayout, NavMenu (with code-behind)
│   └── Pages/
│       ├── Albums/              # Albums list & AlbumDetail
│       ├── Auth/                # Login & Signup
│       ├── Home/                # Landing page / Dashboard
│       └── Lyrics/              # GenerateLyrics & RephraseLyrics
│
├── Configuration/               # DI extension methods
│
├── Domain/
│   ├── Auth/                    # Firebase auth request/response models
│   ├── Lyrics/                  # GenerationRequest, RephraseRequest, LyricsResult
│   └── Music/                   # Album, Song, AppUser entities
│
├── Infrastructure/
│   ├── AI/                      # ILyricsAiService, NvidiaLyricsAiService, PromptTemplateService
│   ├── Firebase/                # IFirebaseAuthService, IFirestoreService + implementations
│   └── Session/                 # UserSessionService (per-circuit state)
│
└── wwwroot/                     # Static assets, CSS
```

## Getting Started

1. **Clone** the repository
2. **Configure** `appsettings.json` with your API keys:
   ```json
   {
     "Firebase": {
       "ApiKey": "<your-firebase-api-key>",
       "ProjectId": "<your-firebase-project-id>"
     },
     "Nvidia": {
       "ApiKey": "<your-nvidia-nim-api-key>",
       "Model": "meta/llama-3.1-405b-instruct"
     }
   }
   ```
3. **Run**
   ```bash
   cd RapSuite
   dotnet run --configfile nuget.config
   ```

## Architecture Highlights

- **Domain-Driven layering** — Domain models have zero infrastructure dependencies
- **Interface-based DI** — All services registered via interfaces (`IFirebaseAuthService`, `IFirestoreService`, `ILyricsAiService`)
- **Code-behind pattern** — Razor pages use separate `.razor.cs` partial classes for clean separation of markup and logic
- **Centralized DI** — `ServiceCollectionExtensions.AddRapSuiteServices()` registers all services in one place