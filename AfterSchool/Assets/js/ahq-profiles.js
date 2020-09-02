var lastCount = 0;

function updateSrc(iframe, count) {
    if (lastCount !== count) {

        if (iframe)
            iframe.remove();

        var container = document.getElementById('profiles-container')
        iframe = document.createElement("iframe");
        iframe.id = 'profiles';
        iframe.setAttribute('width', '100%');
        iframe.setAttribute('frameborder', '0');
        iframe.setAttribute('scrolling', 'no');
        container.appendChild(iframe);

        iframe.setAttribute('src', 'index-profiles.html?count=' + count);
        lastCount = count;
    }

    return iframe;
}

function resizeProfiles() {
    var windowWidth = window.innerWidth;
    var iframe = document.getElementById('profiles');

    if (windowWidth >= 1200) {
        iframe = updateSrc(iframe, 3);
        iframeContentHeight = '425px';
    } else if (windowWidth >= 980) {
        iframe = updateSrc(iframe, 4);
        iframeContentHeight = '850px';
    } else {
        iframe = updateSrc(iframe, 3);
        iframeContentHeight = '1250px';
    }

    iframe.height = iframeContentHeight;
}

window.addEventListener('resize', resizeProfiles);
window.addEventListener('DOMContentLoaded', resizeProfiles, false);