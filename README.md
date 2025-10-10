# tsw6-realtime-weather
Realtime weather mod for Train Sim World, using open data.

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

#### API Keys
- **`api_keys.openweather`** (optional)
  - Your OpenWeather API key
  - If left empty, the application will fall back to reading from `WeatherApiKey.txt`
  - Recommended: Store your API key in the config file for easier management
  - Example: `openweather: "your_api_key_here"`

### Example config.yaml

```yaml
weather:
  update_threshold_km: 15.0

update:
  location_check_interval_seconds: 45

api_keys:
  openweather: "your_openweather_api_key_here"
```

### Legacy API Key File
For backward compatibility, you can still use a `WeatherApiKey.txt` file instead of adding the key to `config.yaml`. Simply create a text file named `WeatherApiKey.txt` in the same folder as the executable and paste your OpenWeather API key into it.

Priority: The application checks `config.yaml` first, then falls back to `WeatherApiKey.txt`.

## Features
- Automatic weather synchronization based on player location
- Distance-based weather updates (configurable threshold)
- Efficient API usage with configurable update intervals
- Automatic distance tracking and accumulation
- Logs all activity to both console and file

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

## License
MIT License - feel free to modify and distribute.
