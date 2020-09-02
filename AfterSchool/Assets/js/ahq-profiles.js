var lastCount = 0;

function updateSrc(iframe, count) {
    if (lastCount !== count) {
        console.log('update source');

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
        console.log(iframe.src);
        lastCount = count;
        console.log(count);
    }

    return iframe;
}

function resizeProfiles() {
    var windowWidth = window.innerWidth; // $(this).width();
    var iframe = document.getElementById('profiles');

    if (windowWidth >= 1200) {
        console.log('desktop');
        iframe = updateSrc(iframe, 3);
        iframeContentHeight = '425px';
    } else if (windowWidth >= 980) {
        console.log('tablet');
        iframe = updateSrc(iframe, 4);
        iframeContentHeight = '850px';
    } else {
        console.log('mobile');
        iframe = updateSrc(iframe, 3);
        iframeContentHeight = '1250px';
    }

    iframe.height = iframeContentHeight;
}

window.addEventListener('resize', resizeProfiles);
//window.addEventListener('load', resizeProfiles);
//document.getElementsByTagName("body")[0].addEventListener('load', resizeProfiles);
window.addEventListener('DOMContentLoaded', resizeProfiles, false);