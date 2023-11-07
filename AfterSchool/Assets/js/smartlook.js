const getEnv = () => {
    const envs = {
        ['staging.afterschoolhq.com']: 'Staging',
        ['afterschoolhq.com']: 'Production',
        ['qa.afterschoolhq.com']: 'QA',
        ['localhost:54849']: 'Local'
    };
    return envs[window.location.host] || 'Local';
}

const recordSmartlookSession = () => {
    window.smartlook || (function (d) {
        var o = smartlook = function () { o.api.push(arguments) }, h = d.getElementsByTagName('head')[0];
        var c = d.createElement('script'); o.api = new Array(); c.async = true; c.type = 'text/javascript';
        c.charset = 'utf-8'; c.src = 'https://rec.smartlook.com/recorder.js'; h.appendChild(c);
    })(document);
    smartlook('init', '4a12ea16e5c85b6e6ec074dac9806b9588793fb2');
    smartlook('record', { forms: true, ips: true, emails: true, numbers: true });
}

if (getEnv() === "Production") {
    recordSmartlookSession();
}