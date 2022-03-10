function googleTranslateElementInit() {
    new google.translate.TranslateElement({
        pageLanguage: 'en',
        includedLanguages: 'en,es,fr,ht,sw',
        layout: google.translate.TranslateElement.InlineLayout.HORIZONTAL
    }, 'google_translate_element');
}

function triggerHtmlEvent(element, eventName) {
    var event;
    if (document.createEvent) {
        event = document.createEvent('HTMLEvents');
        event.initEvent(eventName, true, true);
        element.dispatchEvent(event);
    } else {
        event = document.createEventObject();
        event.eventType = eventName;
        element.fireEvent('on' + event.eventType, event);
    }
}

$(document).ready(function () {
    $(document).on('click', '.languageOption', function () {
        var value = $(this).attr("data-lang");
        setCurrentLanguage(value);

        localStorage.setItem('languageSelected', value);

        setTimeout(function () {
            updateLanguage(value);
        }, 1000);
    })

    checkLocalStorage();

    function checkLocalStorage() {
        var checkLocalStorage = localStorage.getItem('languageSelected');
        if (checkLocalStorage) {
            setTimeout(function () {
                updateLanguage(checkLocalStorage);
            }, 1000);
            setCurrentLanguage(checkLocalStorage);
        }
    }

    function setCurrentLanguage(languageCode) {

        var currentLanguage = 'English';

        switch (languageCode) {
            case "es":
                currentLanguage = 'Español';
                break;
            case "fr":
                currentLanguage = 'Français';
                break;
            case "ht":
                currentLanguage = 'Kreyòl ayisyen';
                break;
            case "sw":
                currentLanguage = 'Kiswahili';
                break;
        }

        $("#dropdownMenuButton").html(currentLanguage);
        $(".selectedLang").removeClass('selectedLang');
        $(".languageOption").addClass('notSelectedLang');
        $('*[data-lang="' + languageCode + '"]').removeClass('notSelectedLang').addClass('selectedLang');
    }

    function updateLanguage(languageCode) {
        var selectIndex = 0;
        var a = document.querySelector("#google_translate_element select");
        switch (languageCode) {
            case "en":
                selectIndex = 0;
                break;
            case "es":
                selectIndex = 3;
                break;
            case "fr":
                selectIndex = 1;
                break;
            case "ht":
                selectIndex = 2;
                break;
            case "sw":
                selectIndex = 4;
                break;
        }
        a.selectedIndex = selectIndex;
        a.dispatchEvent(new Event('change'));
        if (languageCode === "en") {
            localStorage.removeItem('languageSelected');
            window.location.reload();
        }
    }
})