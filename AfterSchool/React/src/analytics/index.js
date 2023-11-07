import Smartlook from 'smartlook-client';
import TagManager from 'react-gtm-module';

import GoogleTagManagerConfig from 'configs/googleTagManagerConfigs';

export default function(env) {
  TagManager.initialize(GoogleTagManagerConfig);
  if (env === 'Production') {
    Smartlook.init(process.env.SMART_LOOK_KEY);
    Smartlook.record({ forms: true, ips: true, emails: true, numbers: true });
  }
}
