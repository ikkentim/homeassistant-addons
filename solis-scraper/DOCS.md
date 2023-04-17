Configuration
-------------
A couple of options are available:

- **`Scraper.Host`**: (required) URI of host to be scraped. e.g. http://SOME_IP_ADDRESS/
- **`Scraper.Username`**: Username for Solis device. Default: `admin`
- **`Scraper.Password`**: Password of Solis device. Default: `admin`
- **`Mqtt.Host`**: Host of the MQTT service.
- **`Mqtt.Username`**: Username of the MQTT service.
- **`Mqtt.Password`**: Password of the MQTT service.
- **`Mqtt.ClientId`**: Client ID used in connection with MQTT service. Default: `solis_scraper`
- **`Mqtt.DiscoveryPrefix`**: Prefix of used topics. Default: `homeassistant`
- **`Mqtt.NodeId`**: The node ID used in topics. Change this value if you're running multiple scrapers. Default: `solis`
- **`Mqtt.UniqueIdPrefix`**: The unique ID prepended to the sensor entities configured by this scraper.  Change this value if you're running multiple scrapers. Default: `solis_scraper`