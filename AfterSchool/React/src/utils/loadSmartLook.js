function loadSmartlook(smartlookKey, documentObject) {
  return new Promise((resolve, reject) => {
    window.smartlook =
      window.smartlook ||
      function() {
        window.smartlook.api.push(arguments);
      };
    window.smartlook.api = window.smartlook.api || new Array();

    var c = documentObject.createElement('script');
    c.async = true;
    c.type = 'text/javascript';
    c.charset = 'utf-8';
    c.src = 'https://rec.smartlook.com/recorder.js';

    c.onload = function() {
      window.smartlook('init', smartlookKey);
      resolve(window.smartlook);
    };

    c.onerror = reject;

    documentObject.getElementsByTagName('head')[0].appendChild(c);
  });
}
export default loadSmartlook;
