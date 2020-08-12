var pageEvents = new Array();

var PageEvent = {
    AFTER_INIT: 'afterinit',

    add: function (eventName, fn) {
        if (!(pageEvents[eventName] instanceof Array)) {
            pageEvents[eventName] = new Array();
        }
        pageEvents[eventName].push(fn);
    },
    trigger: function (eventName) {
        if (pageEvents[eventName] instanceof Array) {
            for (var i = 0; i < pageEvents[eventName].length; i++) {
                pageEvents[eventName][i]();
            }
        }
    }
}

var AHQRecaptcha = {
    renderFns: new Array(),
    renderElementIds: new Array(),
    renderedIds: new Array(),
    scriptTagRendered: false,
    add: function (id, fn) {
        AHQRecaptcha.renderFns.push(fn);
        AHQRecaptcha.renderElementIds.push(id);
    },
    getToken: function (id) {
        var index = AHQRecaptcha.renderElementIds.indexOf(id);
        var renderedId = AHQRecaptcha.renderedIds[index];
        return grecaptcha.getResponse(renderedId);
    },
    reset: function (id) {
        var index = AHQRecaptcha.renderElementIds.indexOf(id);
        var renderedId = AHQRecaptcha.renderedIds[index];
        grecaptcha.reset(renderedId);
    },
    writeScriptTag: function () {
        if (!AHQRecaptcha.scriptTagRendered) {
            document.write('<script src="https://www.google.com/recaptcha/api.js?onload=CaptchaCallback&render=explicit" async defer></script>');
            AHQRecaptcha.scriptTagRendered = true;
        }
    }
}

var CaptchaCallback = function () {
    for (var i = 0; i < AHQRecaptcha.renderFns.length; i++) {
        var id = AHQRecaptcha.renderFns[i]();
        AHQRecaptcha.renderedIds.push(id);
    }
};