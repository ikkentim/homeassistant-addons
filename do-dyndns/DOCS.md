## Configuration

You'll need to generate an API token in the Digital Ocean control panel. The token needs the read and write scope.

The type A DNS record will not be created by this add-on. This add-on will only update an existing record!

The add-on will check for changes to the external IP every 15 minutes.

The following options can be configured:

- **`auth_token`**: The API token generated through the DO control panel
- **`domain`**: The domain name excluding the subdomain
- **`name`**: The subdomain name
- **`ip_service`** An URL to a webpage which contains only the IP address of the requester. Examples: http://ip.ikt.im , http://icanhazip.com
