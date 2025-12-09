# Movie Night (ASP.NET Core MVC)

A minimal MVC experience that searches the RapidAPI-backed OMDb database and highlights the movie title, lead actor, and genre on a neon, movie-themed view.

## Getting started
1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download) if it is not already available.
2. Restore and run the project:
   ```bash
   dotnet restore
   dotnet run
   ```
3. Browse to `https://localhost:5001` (or the console URL) and open the **Movies** view.

## API configuration
Set your RapidAPI key so the HttpClient can call the OMDb alternative endpoint:

- Update `appsettings.json`:
  ```json
  {
    "RapidApi": {
      "Key": "YOUR_RAPIDAPI_KEY_HERE"
    }
  }
  ```
- Or prefer environment variables:
  ```bash
  export RapidApi__Key="<your-key>"
  ```

## Features
- Search any movie title from the Movies view.
- Each card shows the title, year, lead actor, genre, and a short plot summary.
- Movie-themed styling with hero art, neon accents, and quick tips about how data flows from the API.
