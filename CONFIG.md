# Configuration Guide

The application is configured via `config.json` file in the same directory as the executable.

## Configuration Options

### Weather Settings

**`weather.update_threshold_km`** (default: `5.0`)
- Distance in kilometers the player must travel before new weather data is fetched
- Lower values = more accurate weather but more API calls
- Recommended range: 5-50 km
- Example: `"update_threshold_km": 25.0` to update every 25 km

**`weather.transition_duration_seconds`** (default: `30`)
- Duration in seconds for smooth weather transitions
- Weather values (temperature, clouds, etc.) are gradually interpolated over this time
- Updates TSW6 every second during the transition for smooth changes
- Lower values = faster weather changes, higher values = more gradual
- Recommended range: 15-60 seconds
- Example: `"transition_duration_seconds": 45` for slower, more realistic transitions

### Update Timing

**`update.location_check_interval_seconds`** (default: `5`)
- How often (in seconds) the application checks the player's location
- Lower values = more responsive but higher CPU usage
- Recommended range: 5-60 seconds
- Example: `"location_check_interval_seconds": 10` to check every 10 seconds

### HTTP Retry Settings

**`retry.max_retries`** (default: `5`)
- Maximum number of retry attempts for failed HTTP requests
- Uses exponential backoff between retries
- Set to 0 to disable retries
- Example: `"max_retries": 3` for fewer retries

**`retry.initial_delay_ms`** (default: `100`)
- Initial delay in milliseconds before the first retry
- Subsequent retries use exponential backoff (100ms → 200ms → 400ms → 800ms → 1600ms)
- Example: `"initial_delay_ms": 200` for longer initial wait

### Logging Settings

**`logging.level`** (default: `"Information"`)
- Minimum logging level to display
- Options: `"Debug"`, `"Information"`, `"Warning"`, `"Error"`
  - **Debug**: Verbose logging including all location updates and distance calculations
  - **Information**: Standard operational logging (recommended for normal use)
  - **Warning**: Only warnings and errors (includes retry attempts)
  - **Error**: Only error messages
- Example: `"level": "Debug"` for troubleshooting

### API Keys

**`api_keys.openweather`** (optional)
- Your OpenWeather API key
- If left empty (default), the application will fall back to reading from `WeatherApiKey.txt`
- Get a free API key at: https://openweathermap.org/api
- Example: `"openweather": "your_api_key_here"`

## Example config.json

```json
{
  "weather": {
    "update_threshold_km": 5.0,
    "transition_duration_seconds": 30
  },
  "update": {
    "location_check_interval_seconds": 5
  },
  "retry": {
    "max_retries": 5,
    "initial_delay_ms": 100
  },
  "logging": {
    "level": "Information"
  },
  "api_keys": {
    "openweather": "your_openweather_api_key_here"
  }
}
```

## Legacy API Key File

For backward compatibility, you can use a `WeatherApiKey.txt` file instead of adding the key to `config.json`. Simply create a text file named `WeatherApiKey.txt` in the same folder as the executable and paste your OpenWeather API key into it.

**Priority**: The application checks `config.json` first, then falls back to `WeatherApiKey.txt`.
