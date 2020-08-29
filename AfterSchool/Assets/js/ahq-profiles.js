var lastCount = 3;

function updateSrc(iframe, count) {
    if (lastCount != count) {
        console.log('updatesrc');
        iframe.attr('src', 'index-profiles.html?count=' + count);
        lastCount = count;
    }
}

function resizeProfiles() {

    var windowWidth = $(this).width();
    var iframe = $('#profiles');

    if (windowWidth >= 1200) {
        updateSrc(iframe, 3);
        iframeContentHeight = '425px';
    } else if (windowWidth >= 980) {
        updateSrc(iframe, 4);
        iframe.attr('src', 'index-profiles.html?count=4');
        iframeContentHeight = '850px';
    } else {
        updateSrc(iframe, 3);
        iframeContentHeight = '1250px';
    }

    iframe.css('height', iframeContentHeight);
}

$(document).ready(function () {
    $(window).on('resize', function () {
        resizeProfiles();
    });
    resizeProfiles();
});