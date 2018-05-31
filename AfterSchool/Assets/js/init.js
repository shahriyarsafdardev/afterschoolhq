var pageEvents = new Array();

var PageEvent = {
    AFTER_INIT: 'afterinit',

    add: function (eventName, fn) {
        //alert(eventName);
        if (!(pageEvents[eventName] instanceof Array)) {
            pageEvents[eventName] = new Array();
        }
        //alert('adding function');
        pageEvents[eventName].push(fn);
    },
    trigger: function (eventName) {
        //alert('trigger: ' + eventName);
        if (pageEvents[eventName] instanceof Array) {
            for (var i = 0; i < pageEvents[eventName].length; i++) {
                pageEvents[eventName][i]();
            }
        }
    }
}