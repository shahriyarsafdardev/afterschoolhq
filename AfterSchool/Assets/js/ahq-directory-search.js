function directoryPageLoad() {
    // we need to show the filter button, once js will get loaded, to avoid any js related error
    $("#img-mobile-filter").removeClass("hide");

    $("#results-container").scroll(function () {
        if (!isReachedScrollEnd &&
            !isMobileFilterWindowOpened &&
            (($(this).scrollTop() + $(this).innerHeight() + 1) >= $(this)[0].scrollHeight)) { //+1 to adjust height because of contact us button at bottom
            loadFiltersAndInitiateSearch();
        }
    });

    loadFiltersAndInitiateSearch();
    loadInitialTabs();

    if (isMobile) {
        $('#container-show-filter-button').removeClass('display-none').addClass('display-block');
        $('#main-view-container').removeClass('display-flex');
    } else {
        $('#container-show-filter-button').removeClass('display-block').addClass('display-none');
        $('#filters').show();
        $('#container-show-map-chk').show();
        $('#map-view').show();
    }

    formatContactPhoneForDirectory();
}

function showModalIsyp() {
    $('#modal-isyp-prompt').removeClass('display-none').addClass('display-block');
    $('#modal-isyp-background').removeClass('display-none').addClass('display-block');
}

function closeModalIsyp() {
    $('#modal-isyp-prompt').removeClass('display-block').addClass('display-none');
    $('#modal-isyp-background').removeClass('display-block').addClass('display-none');
}

function submitContactForDirectory() {
    if ($('#name').val() && $('#email').val() && $('#message').val()) {
        var subjectLine = 'New Search Page Form Submission From: ' + $('#name').val();
        if ($('#phone').val()) {
            subjectLine = subjectLine + ' - ' + $('#phone').val();
        }

        var params =
            'name=' + encodeURIComponent($('#name').val()) +
            '&pageName=search' +
            '&fromEmail=' + encodeURIComponent($('#email').val()) +
            '&message=' + encodeURIComponent($('#message').val()) +
            '&MainSubject=' + encodeURIComponent(subjectLine) +
            '&g_recaptcha_response=' + $('#g-recaptcha-response').val() +
            '&phone=' + $('#phone').val();

        $('.formSubmitting').html('Submitting...');
        $('.btnContactUsSubmit').prop('disabled', true);

        $.post('/services/submitcontactform', params, function (data) {
            $('.formSubmitting').html('Submit');
            $('.btnContactUsSubmit').prop('disabled', false);
            grecaptcha.reset();

            if (data && data.status == 'ok') {
                $('#name').val('');
                $('#email').val('');
                $('#message').val('');
                $('#phone').val('');

                Notifications.showBar({
                    title: 'Success',
                    message: 'Your message has been sent.',
                    success: true
                });
            } else {
                var text = "";
                for (var message of data.messages) {
                    text = text + message.Text;
                }

                Notifications.showBar({
                    title: 'Failed',
                    message: text,
                    success: false
                });
            }
        });

        return false;
    }

    return true;
}

function prepareSearchParams(deleteAllMarkers = true) {
    //.. reset scroll flag
    isReachedScrollEnd = false;

    //.. prepare params
    let mainUrl = '';
    let commaDelimitedEnrollToday = '';
    let commaDelimitedGrades = '';
    let commaDelimitedAges = '';
    let commaDelimitedGender = '';
    let commaDelimitedServices = '';
    let commaDelimitedTiming = '';
    let commaDelimitedPricing = '';
    let commaDelimitedCost = '';
    let commaDelimitedStarting = '';
    let commaDelimitedOther = '';
    let commaDelimitedSpecial = '';

    let search = txtSearch.val();

    if (search && search.length > 0) {
        mainUrl += `search_${search},`;
    }

    let latitude, longitude = undefined;
    let zipCode = txtZip.val() || '';
    if (!zipCode) {
        let selectedSchoolData = $('#ipsSchools option:selected').data();
        if(selectedSchoolData) {
            ({latitude, longitude, zipCode} = selectedSchoolData);
            if (zipCode) {
                zipCode = zipCode.toString().padStart(5, '0');
            }
        }
    }
    if (zipCode && zipCode.length > 0) {
        mainUrl += `zip_${zipCode},`;
    }

    $('input:checkbox[name=enrolling-now]:checked').each(function () {
        if (commaDelimitedEnrollToday.length > 0) {
            commaDelimitedEnrollToday += ',';
        }
        commaDelimitedEnrollToday += $(this).val();
    });

    $('input:checkbox[name=grades]:checked').each(function () {
        if (commaDelimitedGrades.length > 0) {
            commaDelimitedGrades += ',';
        }
        commaDelimitedGrades += $(this).val();
    });

    $('input:checkbox[name=ages]:checked').each(function () {
        if (commaDelimitedAges.length > 0) {
            commaDelimitedAges += ',';
        }
        commaDelimitedAges += $(this).val();
    });

    $('input:checkbox[name=gender]:checked').each(function () {
        if (commaDelimitedGender.length > 0) {
            commaDelimitedGender += ',';
        }
        commaDelimitedGender += $(this).val();
    });

    $('input:checkbox[name=services]:checked').each(function () {
        if (commaDelimitedServices.length > 0) {
            commaDelimitedServices += ',';
        }
        commaDelimitedServices += $(this).val();
    });

    $('input:checkbox[name=timing]:checked').each(function () {
        if (commaDelimitedTiming.length > 0) {
            commaDelimitedTiming += ',';
        }
        commaDelimitedTiming += $(this).val();
    });

    $('input:checkbox[name=pricing]:checked').each(function () {
        if (commaDelimitedPricing.length > 0) {
            commaDelimitedPricing += ',';
        }
        commaDelimitedPricing += $(this).val();
    });

    $('input:checkbox[name=cost]:checked').each(function () {
        if (commaDelimitedCost.length > 0) {
            commaDelimitedCost += ',';
        }
        commaDelimitedCost += $(this).val();
    });

    $('input:checkbox[name=starting]:checked').each(function () {
        if (commaDelimitedStarting.length > 0) {
            commaDelimitedStarting += ',';
        }
        commaDelimitedStarting += $(this).val();
    });

    $('input:checkbox[name=other]:checked').each(function () {
        if (commaDelimitedOther.length > 0) {
            commaDelimitedOther += ',';
        }
        commaDelimitedOther += $(this).val();
    });

    $('input:checkbox[name=special-needs]:checked').each(function () {
        if (commaDelimitedSpecial.length > 0) {
            commaDelimitedSpecial += ',';
        }
        commaDelimitedSpecial += $(this).val();
    });

    if (commaDelimitedEnrollToday.length > 0) {
        mainUrl += `${commaDelimitedEnrollToday},`;
    }

    if (commaDelimitedGrades.length > 0) {
        mainUrl += `${commaDelimitedGrades},`;
    }

    if (commaDelimitedAges.length > 0) {
        mainUrl += `${commaDelimitedAges},`;
    }

    if (commaDelimitedGender.length > 0) {
        mainUrl += `${commaDelimitedGender},`;
    }

    if (commaDelimitedServices.length > 0) {
        mainUrl += `${commaDelimitedServices},`;
    }

    if (commaDelimitedTiming.length > 0) {
        mainUrl += `${commaDelimitedTiming},`;
    }

    if (commaDelimitedPricing.length > 0) {
        mainUrl += `${commaDelimitedPricing},`;
    }

    if (commaDelimitedCost.length > 0) {
        mainUrl += `${commaDelimitedCost},`;
    }

    if (commaDelimitedStarting.length > 0) {
        mainUrl += `${commaDelimitedStarting},`;
    }

    if (commaDelimitedOther.length > 0) {
        mainUrl += `${commaDelimitedOther},`;
    }

    if (commaDelimitedSpecial.length > 0) {
        mainUrl += `${commaDelimitedSpecial},`;
    }

    let currentScrollTop = $(window).scrollTop();

    if (mainUrl) {
        WebPage.addHash(encodeURI(mainUrl));
        if (deleteAllMarkers) {
            deleteAllMarkersOnMap();
        }
        $('#clear-filters').show();
    } else {
        WebPage.addHash('');
        $('#clear-filters').hide();
    }

    //..scroll to last known position, because location.href scrolls to top
    document.documentElement.scrollTop = page === 0 ? 0 : currentScrollTop;

    // .. call api
    if (!isMobile || !isMobileFilterWindowOpened) {
        loadAccounts(search,
            commaDelimitedEnrollToday,
            commaDelimitedGrades,
            commaDelimitedAges,
            commaDelimitedGender,
            commaDelimitedServices,
            commaDelimitedTiming,
            commaDelimitedPricing,
            commaDelimitedCost,
            commaDelimitedStarting,
            commaDelimitedOther,
            commaDelimitedSpecial,
            zipCode,
            latitude,
            longitude
        );
    }
}

function loadFiltersAndInitiateSearch() {
    var hashUrl = WebPage.getHash();
    var filters = hashUrl.split(',');
    _.each(filters, function (filter) {
        if (filter.indexOf('search_') == 0) {
            $(`input[name="search"]`).val(filter.split('_')[1]);
        } else if (filter.indexOf('zip_') == 0) {
            $(`input[name="zipcode"]`).val(filter.split('_')[1]);
        } else {
            var inputToCheck = $(`input[value="${decodeURIComponent(filter.trim())}"]`);

            if (inputToCheck && inputToCheck.length > 0) {
                inputToCheck.attr('checked', 'checked');
            }
        }
    });

    prepareSearchParams(false);
}

function waitAndFilterSearch(search) {
    clearTimeout(delayTimer);
    delayTimer = setTimeout(function () {
        page = 0;
        prepareSearchParams();
    }, 1000); //1 second wait
}

function formatContactPhoneForDirectory() {
    var inputElement = document.getElementById('phone');
    inputElement.addEventListener('keydown', enforceFormat);
    inputElement.addEventListener('keyup', formatToPhone);

    //format phone number when we load
    if (inputElement && inputElement.value) {
        var input = inputElement.value.replace(/\D/g, '').substring(0, 10); // First ten digits of input only
        var zip = input.substring(0, 3);
        var middle = input.substring(3, 6);
        var last = input.substring(6, 10);

        if (input.length > 6) {
            inputElement.value = `(${zip}) ${middle} ${last}`; //(xxx) xxx xxxx
        } else if (input.length > 3) {
            inputElement.value = `(${zip}) ${middle}`;
        } else if (input.length > 0) {
            inputElement.value = `(${zip}`;
        }
    }
}

function checkAndSearch(elem, label) {
    if (isMobile) {
        var name = $(elem).attr('name').replaceAll(' ', '').toLowerCase();
        var filterValue = $(elem).attr('valueForFilter');
        if (!$(elem).is(":checked")) {
            clearSpecificFilter(name, filterValue)
        } else {
            var tag = getTagHtml(name, filterValue, label);
            $('#tags-container-mobile-view').append(tag);
        }
    } else {
        page = 0;
        prepareSearchParams()
    }
}

function validatePostalCodeAndApplyFiltersIfValid() {
    if ($('#txtZipCode').val().trim().length > 0) {
        resetIpsSchoolDropDownSelection();
    }
    if (isMobile)
        return;
    trimAndValidatePostalCode() && applySearchFilters();
}

function validatePostalCodeAndApplyFilters() {
    if (trimAndValidatePostalCode()) {
        applySearchFilters();
    } else {
        if (isMobile) {
            $('#clear-filters').show();
        }
        Notifications.showBar({
            title: 'Error',
            message: 'Postal code is not in a proper format.',
            success: false
        });
    }
}

function trimAndValidatePostalCode() {
    $('#txtZipCode').val($('#txtZipCode').val().trim());
    return checkZip($('#txtZipCode').val()) || $('#txtZipCode').val().length === 0;
}

function applySearchFilters() {
    if (isMobile && isMobileFilterWindowOpened) {
        mobileFilter();
    }

    page = 0;
    prepareSearchParams();
}

function checkZip(value) {
    //.. this is global regex to verify/sanity check postal code format but this is not or validating zip code precisly per country.
    //.. https://stackoverflow.com/a/7185241/5816119
    //.. (in order) US, IN, UK
    var zipCodePatternList = [/^\d{5}(-?\d{4})?$/, /^\d{6}$/, /^(GIR|[A-Z]\d[A-Z\d]??|[A-Z]{2}\d[A-Z\d]??) ??(\d[A-Z]{2})$/]
    return zipCodePatternList.some(function (rx) {
        return rx.test(value);
    });
}

function loadInitialTabs() {
    loadTabs('grades', 1);
    loadTabs('ages', 1);
    loadTabs('gender', 1);
    loadTabs('services', 1);
    loadTabs('timing', 1);
    loadTabs('cost', 1);
    loadTabs('pricing', 1);
    loadTabs('starting', 1);
    loadTabs('other', 1);
    loadTabs('special-needs', 1);

    if (isMobile) {
        loadTabsForMobile();
    }
}

function loadTabs(name, pageInitialization) {
    var element = $('#header-' + name);
    var isOpen = element.attr('aria-expanded');
    if (isOpen === 'true' || pageInitialization) {
        var tags = '';
        $('input:checkbox[name=' + name + ']:checked').each(function () {
            tags += getTagHtml(name, $(this).attr('valueForFilter'), $(this).parent().text().trim());
        });

        $('#tags-container-' + name).html(tags).show();
    } else {
        $('#tags-container-' + name).hide();
    }
}

function loadTabsForMobile() {
    var tags = '';
    $('input:checkbox[name=grades]:checked,[name=ages]:checked,[name=gender]:checked,[name=services]:checked,[name=timing]:checked,[name=pricing]:checked,[name=cost]:checked,[name=starting]:checked,[name=other]:checked,[name=special-needs]:checked').each(function () {
        tags += getTagHtml($(this).attr('name'), $(this).attr('valueForFilter'), $(this).parent().text().trim());
    });
    $('#tags-container-mobile-view').html('').append(tags);
}

function getTagHtml(containerName, id, label) {
    return `<span class="badge-identity-` + containerName + `-` + id + ` badge badge-pill badge-success text-small primary-bg-color directory-badge margin-right-10px">` + label +
        `<button type="button" class="close" data-dismiss="alert" aria-label="Close" onclick='return clearSpecificFilter("` + containerName + `", "` + id + `")'><i class="remove glyphicon glyphicon-remove-circle glyphicon-white"></i></button></span>`;
}

function resetTags() {
    $('#tags-container-grades').html('');
    $('#tags-container-ages').html('');
    $('#tags-container-gender').html('');
    $('#tags-container-services').html('');
    $('#tags-container-timing').html('');
    $('#tags-container-cost').html('');
    $('#tags-container-pricing').html('');
    $('#tags-container-starting').html('');
    $('#tags-container-other').html('');
    $('#tags-container-special-needs').html('');
}

function mobileFilter() {
    var isOpen = $('#btn-mobile-filter').attr('isOpen');
    if (isOpen == '0') {
        showMobileFilters();
    } else {
        hideMobileView();
    }
}

function showMobileFilters() {
    $('#filters').addClass('mobile-filter-window').show();
    $('#search-and-results-container').hide();
    $('#mobile-filter-footer').show();
    $('#footerShowHide').hide();
    $('#directory-sidebar-icon').hide();

    $('#btn-mobile-filter').attr('isOpen', "1");
    loadInitialTabs()
    isMobileFilterWindowOpened = true;
}

function hideMobileView() {
    $('#filters').removeClass('mobile-filter-window').hide();
    $('#search-and-results-container').show();
    $('#mobile-filter-footer').hide();
    $('#footerShowHide').show();
    $('#directory-sidebar-icon').show();

    $('#btn-mobile-filter').attr('isOpen', "0");
    isMobileFilterWindowOpened = false;
}

function clearFilters() {
    $(`input:checkbox[name=enrolling-now], [name=grades], [name=ages], [name=gender], [name=services], [name=timing], [name=pricing], [name=cost], [name=starting], [name=other], [name=special-needs]`).prop('checked', false);
    $('[class^=badge-identity-]').remove();

    txtZip.val('');
    //..clear web link and hide clear filter link
    WebPage.addHash('');
    $('#clear-filters').hide();

    resetIpsSchoolDropDownSelection();

    //..refresh accounts
    if (isMobile) {
        mobileFilter();
        $('#txtSearch-mobile').val('');
    } else {
        $('#txtSearch').val('');
    }

    deleteAllMarkersOnMap();
    resetTags();
    resetFlags();
    loadAccounts();
}

function resetIpsSchoolDropDownSelection() {
    $('#ipsSchools').val('');
}

function clearSpecificFilter(tagContainer, name) {
    if (tagContainer !== '' && name !== '') {
        $(`input:checkbox[name=` + tagContainer + `][valueforfilter=` + name + `]`).prop('checked', false);
        $(`.badge-identity-` + tagContainer + `-` + name).remove();

        removeSpecificFilterFromUrl(name);
        resetFlags();
        prepareSearchParams();
    }
}

function removeSpecificFilterFromUrl(name) {
    var hashUrl = WebPage.getHash();
    var filters = hashUrl.split(',');
    _.each(filters, function (filter) {
        if (filter.toLowerCase().indexOf(name.trim().toLowerCase()) == 0) {
            hashUrl = hashUrl.replace(filter + ',', '')
        }
    });

    WebPage.addHash(encodeURI(hashUrl));
}

function resetFlags() {
    isReachedScrollEnd = false;
    page = 0;
    inCallback = false;
}

function toggleSideBar(element) {
    toggleFilterOverflow();
    var isOpen = $(element).attr('isOpen');
    if (isOpen === 'true') {
        $(element).attr('isOpen', false)
        $('#search-sidebar-arrow').removeClass('fa-chevron-circle-left').addClass('fa-chevron-circle-right');
        $("#filters").removeClass('directory-filters-open').addClass('directory-filters-close');
    } else {
        $(element).attr('isOpen', true)
        $('#search-sidebar-arrow').removeClass('fa-chevron-circle-right').addClass('fa-chevron-circle-left');
        $("#filters").removeClass('directory-filters-close').addClass('directory-filters-open');
    }
}

function toggleMapView(element) {
    var isChecked = $(element).is(':checked')
    if (isChecked)
        $("#map-view").removeClass('flex-close').addClass('flex-open');
    else
        $("#map-view").removeClass('flex-open').addClass('flex-close');
}

function toggleFilterOverflow() {
    var isOpen = $('#directory-sidebar-icon').attr('isOpen')
    if (isOpen === 'true') {
        $('#filters').css('overflow-y', 'hidden');
    } else {
        $('#filters').css('overflow-y', 'auto');
    }
}

function searchBySchool() {
    txtZip.val('');
    if (isMobile)
        return;

    let {latitude, longitude} = $('#ipsSchools option:selected').data();
    if (latitude && longitude) {
        setMapCenter(latitude, longitude);
    }
    applySearchFilters();
}