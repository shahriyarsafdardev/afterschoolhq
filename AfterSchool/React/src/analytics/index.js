import Smartlook from 'smartlook-client';
import TagManager from 'react-gtm-module';

import GoogleTagManagerConfig from 'configs/googleTagManagerConfigs';

export default function() {
  TagManager.initialize(GoogleTagManagerConfig);
  Smartlook.init(process.env.SMART_LOOK_KEY);
}
