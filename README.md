# tsw6-realtime-weather
Realtime weather mod for Train Sim World, using open data.

Synchronizes real-world weather conditions with Train Sim World 6 based on your exact location in the game. As you drive your train through different regions, the application automatically fetches and displays current weather data from OpenWeather API.

## Example Output

```
Status Checks
┌─────────────────────┬────────┐
│ Check               │ Status │
├─────────────────────┼────────┤
│ TSW6 Connection     │ ✓      │
│ API Keys            │ ✓      │
│ Subscription Active │ ✓      │
└─────────────────────┴────────┘

Weather Information
📍 Location: London, GB
☁️  Condition: Clouds - overcast clouds
🌡️  Temperature: 12.5°C (feels like 11.8°C)
   Min/Max: 11.2°C / 13.8°C
💧 Humidity: 78%
🔽 Pressure: 1015 hPa
💨 Wind: 18.5 km/h SW
☁️  Cloudiness: 90%
👁️  Visibility: 10.0 km
🌅 Sunrise: 06:45 | 🌇 Sunset: 18:32
⏰ Updated: 2025-10-10 14:23:15
```

## Setup

### Prerequisites
1. Train Sim World 6 installed
2. OpenWeather API key (free tier available at https://openweathermap.org/api)

### Installation
1. Download the latest release
2. Extract to a folder of your choice
3. Configure the application (see Configuration section below)
4. Launch Train Sim World 6 with the `-HTTPAPI` flag
5. Run `tsw6-realtime-weather.exe`

## Configuration

The application uses a `config.yaml` file for configuration. On first run, a default configuration file will be created automatically.

### Configuration Options

#### Weather Settings
- **`weather.update_threshold_km`** (default: 10.0)
  - Distance in kilometers the player must travel before new weather data is fetched
  - Lower values = more accurate weather but more API calls
  - Recommended range: 10-50 km
  - Example: `update_threshold_km: 25.0` to update every 25 km

#### Update Timing
- **`update.location_check_interval_seconds`** (default: 60)
  - How often (in seconds) the application checks the player's location
  - Lower values = more responsive but higher CPU usage
  - Recommended range: 30-120 seconds
  - Example: `location_check_interval_seconds: 45` to check every 45 seconds

#### HTTP Retry Settings
- **`retry.max_retries`** (default: 5)
  - Maximum number of retry attempts for failed HTTP requests
  - Uses exponential backoff between retries
  - Set to 0 to disable retries
  - Example: `max_retries: 3` for fewer retries
  
- **`retry.initial_delay_ms`** (default: 100)
  - Initial delay in milliseconds before the first retry
  - Subsequent retries use exponential backoff (100ms → 200ms → 400ms → 800ms → 1600ms)
  - Example: `initial_delay_ms: 200` for longer initial wait

#### API Keys
- **`api_keys.openweather`** (optional)
  - Your OpenWeather API key
  - If left empty, the application will fall back to reading from `WeatherApiKey.txt`
  - Recommended: Store your API key in the config file for easier management
  - Example: `openweather: "your_api_key_here"`

#### Logging Settings
- **`logging.level`** (default: Information)
  - Minimum logging level: Debug, Information, Warning, Error
  - **Debug**: Verbose logging including all location updates and distance calculations
  - **Information**: Standard operational logging (recommended for normal use)
  - **Warning**: Only warnings and errors (includes retry attempts)
  - **Error**: Only error messages
  - Example: `level: Debug` for troubleshooting

### Example config.yaml

```yaml
weather:
  update_threshold_km: 15.0

update:
  location_check_interval_seconds: 45

retry:
  max_retries: 5
  initial_delay_ms: 100

logging:
  level: Information

api_keys:
  openweather: "your_openweather_api_key_here"
```

### Legacy API Key File
For backward compatibility, you can still use a `WeatherApiKey.txt` file instead of adding the key to `config.yaml`. Simply create a text file named `WeatherApiKey.txt` in the same folder as the executable and paste your OpenWeather API key into it.

Priority: The application checks `config.yaml` first, then falls back to `WeatherApiKey.txt`.

## Features
- 🌦️  **Real-time weather data** from OpenWeather API based on your exact train location
- 📍 **Automatic location tracking** via TSW6 HTTP API
- 🚂 **Distance-based updates** - weather refreshes after traveling configured distance (default: 10km)
- 📊 **Rich terminal UI** with live status updates, progress tracking, and weather display
- ⚙️  **Configurable everything** - update intervals, thresholds, logging levels, retry behavior
- 🔄 **HTTP retry resilience** - automatic recovery from network issues with exponential backoff
- 📝 **Comprehensive logging** - file and console logging with configurable verbosity
- 🎯 **Native AOT compilation** - small executable size, no .NET runtime required

### Weather Information Displayed
- Current conditions (sunny, rainy, cloudy, etc.) with emoji indicators
- Temperature (current, feels like, min/max) in Celsius
- Humidity and atmospheric pressure
- Wind speed and direction with gusts
- Precipitation (rain/snow) intensity
- Cloud coverage percentage
- Visibility distance
- Sunrise and sunset times
- Location name and country

## How It Works
1. The application connects to the TSW6 HTTP API to monitor player location
2. As you play, it tracks your movement distance using the Haversine formula
3. When you've traveled the configured threshold distance (default: 10km), it fetches current weather data from OpenWeather
4. The weather data is synchronized with Train Sim World 6

## Troubleshooting

### "No TSW6 API Key found"
Make sure you launched Train Sim World 6 with the `-HTTPAPI` flag. You can add this to your Steam launch options.

### "TSW6 HTTP Server isn't accessible"
- Verify TSW6 is running
- Verify you used the `-HTTPAPI` launch flag
- Check that no firewall is blocking localhost connections

### "No OpenWeather API Key found"
- Add your API key to `config.yaml` under `api_keys.openweather`, OR
- Create a `WeatherApiKey.txt` file with your API key

## Building from Source

### Prerequisites
- .NET 9.0 SDK or later
- Windows, Linux, or macOS

### Build Commands

#### Development Build
```powershell
dotnet build
```

#### Release Build (AOT Compiled)
```powershell
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained -p:PublishAot=true -o ./publish

# The executable will be in ./publish/tsw6-realtime-weather.exe
```

### Automated Releases

The project includes a GitHub Actions workflow that automatically builds and packages releases:

1. **Automatic**: Push a version tag to trigger a release
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Manual**: Go to Actions → Build and Release → Run workflow

The workflow produces a ZIP file containing:
- Native AOT compiled executable (no .NET runtime required)
- Configuration template (`config.yaml`)
- Documentation (`README.md` and `QUICKSTART.txt`)

## License
MIT License - feel free to modify and distribute.
