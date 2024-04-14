## Configuration

A couple of options are available:

### Scraper
- **`Instances`**: Scraper instances, each instance object has the following properties:
  - **`Host`**: URI of host to be scraped. e.g. http://SOME_IP_ADDRESS/
  - **`Username`**: Username for Solis device. Default: `admin`
  - **`Password`**: Password of Solis device. Default: `admin`. For newer Solis devices this might be either `123456789` or the password of the WiFi network your Solis device is connected to.
  - **`Name`**: (optional) The name of the device as published in the auto discovery. Default: `Solis Energy
  - **`NodeId`**: (optional) The node ID used in MQTT topics. **Make sure to change this value if you're running multiple instances.** Default: `solis`
  - **`UniqueId`**: (optional) The unique ID prepended to the sensor entities configured by this instance. **Make sure to change this value if you're running multiple instances.** Default: `solis_scraper`
  - **`Format`**: (optional) The initial format of the scraped data. The software will autodetect the correct format for scraping. This setting only configures the initial scrape attempt. Format `1` for older devices, `2` for newer devices. Default: `1`

### Mqtt

- **`Host`**: Host of the MQTT service.
- **`Username`**: Username of the MQTT service.
- **`Password`**: Password of the MQTT service.
- **`ClientId`**: Client ID used in connection with MQTT service. Default: `solis_scraper`
- **`DiscoveryPrefix`**: Prefix of used topics. Default: `homeassistant`

