var lastCount = 3;

function updateSrc(iframe, count) {
    if (lastCount !== count) {
        iframe.src = 'index-profiles.html?count=' + count;
        lastCount = count;
    }
}

function resizeProfiles() {
    console.log('resizeProfiles');
    var windowWidth = window.innerWidth; // $(this).width();
    var iframe = document.getElementById('profiles'); //$('#profiles');

    if (windowWidth >= 1200) {
        updateSrc(iframe, 3);
        iframeContentHeight = '425px';
    } else if (windowWidth >= 980) {
        updateSrc(iframe, 4);
        iframeContentHeight = '850px';
    } else {
        updateSrc(iframe, 3);
        iframeContentHeight = '1250px';
    }

    iframe.height = iframeContentHeight;
}

window.addEventListener('resize', resizeProfiles);
resizeProfiles();