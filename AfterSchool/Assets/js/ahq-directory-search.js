function directoryPageLoad() {
    // we need to show the filter button, once js will get loaded, to avoid any js related error
    $("#img-mobile-filter").removeClass("hide");

    $("#search-container").scroll(function () {
        if (isReachedScrollEnd == false &&
            isMobileFilterWindowOpened == false &&
            ($(this).scrollTop() + $(this).innerHeight() >= $(this)[0].scrollHeight)) {
            loadFiltersAndInitiateSearch();
        }
    });

    loadFiltersAndInitiateSearch();
    loadInitialTabs();    

    if (!isMobile) {
        $('#filters').show();
    }

    formatContactPhoneForDirectory();
}

function showModalIsyp() {
    $('#modal-isyp-intro').addClass('md-show');
    $('#blured-bg').addClass('blur-bg');
}

function closeModalIsyp() {
    $('#modal-isyp-intro').removeClass('md-show');
    $('#blured-bg').removeClass('blur-bg');
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
            }
            else {
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

function prepareSearchParams() {
    //.. reset scroll flag
    isReachedScrollEnd = false;

    //.. prepare params
    var mainUrl = '';
    var search = '';
    var zip = 0;
    var comma_delimited_neighborhoods = '';
    var comma_delimited_grades = '';
    var comma_delimited_ages = '';
    var comma_delimited_gender = '';
    var comma_delimited_services = '';
    var comma_delimited_timing = '';
    var comma_delimited_pricing = '';
    var comma_delimited_cost = '';
    var comma_delimited_starting = '';
    var comma_delimited_other = '';
    var comma_delimited_special = '';

    if (isMobile) {
        search = txtSearchMobile.val();
    } else {
        search = txtSearch.val();
    }

    if (search && search.length > 0) {
        mainUrl += `search_${search},`;
    }

    zip = txtZip.val() || 0;
    if (zip && zip.length > 0) {
        mainUrl += `zip_${zip},`;
    }

    $('input:checkbox[name=neighborhoods]:checked').each(function () {
        if (comma_delimited_neighborhoods.length > 0) {
            comma_delimited_neighborhoods += ',';
        }
        comma_delimited_neighborhoods += $(this).val();
    });

    $('input:checkbox[name=grades]:checked').each(function () {
        if (comma_delimited_grades.length > 0) {
            comma_delimited_grades += ',';
        }
        comma_delimited_grades += $(this).val();
    });

    $('input:checkbox[name=ages]:checked').each(function () {
        if (comma_delimited_ages.length > 0) {
            comma_delimited_ages += ',';
        }
        comma_delimited_ages += $(this).val();
    });

    $('input:checkbox[name=gender]:checked').each(function () {
        if (comma_delimited_gender.length > 0) {
            comma_delimited_gender += ',';
        }
        comma_delimited_gender += $(this).val();
    });

    $('input:checkbox[name=services]:checked').each(function () {
        if (comma_delimited_services.length > 0) {
            comma_delimited_services += ',';
        }
        comma_delimited_services += $(this).val();
    });

    $('input:checkbox[name=timing]:checked').each(function () {
        if (comma_delimited_timing.length > 0) {
            comma_delimited_timing += ',';
        }
        comma_delimited_timing += $(this).val();
    });

    $('input:checkbox[name=pricing]:checked').each(function () {
        if (comma_delimited_pricing.length > 0) {
            comma_delimited_pricing += ',';
        }
        comma_delimited_pricing += $(this).val();
    });

    $('input:checkbox[name=cost]:checked').each(function () {
        if (comma_delimited_cost.length > 0) {
            comma_delimited_cost += ',';
        }
        comma_delimited_cost += $(this).val();
    });

    $('input:checkbox[name=starting]:checked').each(function () {
        if (comma_delimited_starting.length > 0) {
            comma_delimited_starting += ',';
        }
        comma_delimited_starting += $(this).val();
    });

    $('input:checkbox[name=other]:checked').each(function () {
        if (comma_delimited_other.length > 0) {
            comma_delimited_other += ',';
        }
        comma_delimited_other += $(this).val();
    });

    $('input:checkbox[name=special-needs]:checked').each(function () {
        if (comma_delimited_special.length > 0) {
            comma_delimited_special += ',';
        }
        comma_delimited_special += $(this).val();
    });

    if (comma_delimited_neighborhoods.length > 0) {
        mainUrl += `${comma_delimited_neighborhoods},`;
    }

    if (comma_delimited_grades.length > 0) {
        mainUrl += `${comma_delimited_grades},`;
    }

    if (comma_delimited_ages.length > 0) {
        mainUrl += `${comma_delimited_ages},`;
    }

    if (comma_delimited_gender.length > 0) {
        mainUrl += `${comma_delimited_gender},`;
    }

    if (comma_delimited_services.length > 0) {
        mainUrl += `${comma_delimited_services},`;
    }

    if (comma_delimited_timing.length > 0) {
        mainUrl += `${comma_delimited_timing},`;
    }

    if (comma_delimited_pricing.length > 0) {
        mainUrl += `${comma_delimited_pricing},`;
    }

    if (comma_delimited_cost.length > 0) {
        mainUrl += `${comma_delimited_cost},`;
    }

    if (comma_delimited_starting.length > 0) {
        mainUrl += `${comma_delimited_starting},`;
    }

    if (comma_delimited_other.length > 0) {
        mainUrl += `${comma_delimited_other},`;
    }

    if (comma_delimited_special.length > 0) {
        mainUrl += `${comma_delimited_special},`;
    }

    //.. get current scrolltop
    var scr = $(window).scrollTop();

    if (mainUrl) {
        WebPage.addHash(encodeURI(mainUrl));
        $('#clear-filters').show();
    } else {
        WebPage.addHash('');
        $('#clear-filters').hide();
    }

    //..scroll to last known position, because location.href scrolls to top
    document.documentElement.scrollTop = page == 0 ? 0 : scr;

    openSeeMorePanelsForSelectedCheckboxes();

    // .. call api
    if (!isMobile || !isMobileFilterWindowOpened) {
        loadAccounts(search,
            comma_delimited_neighborhoods,
            comma_delimited_grades,
            comma_delimited_ages,
            comma_delimited_gender,
            comma_delimited_services,
            comma_delimited_timing,
            comma_delimited_pricing,
            comma_delimited_cost,
            comma_delimited_starting,
            comma_delimited_other,
            comma_delimited_special,
            zip
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

    prepareSearchParams();
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

        if (input.length > 6) { inputElement.value = `(${zip}) ${middle} ${last}`; } //(xxx) xxx xxxx
        else if (input.length > 3) { inputElement.value = `(${zip}) ${middle}`; }
        else if (input.length > 0) { inputElement.value = `(${zip}`; }
    }
}

function checkAndSearch(elem) {
    page = 0;
    prepareSearchParams()
}

function validatePostalCodeAndSearch() {
    var isValid = checkZip($('#txtZipCode').val());
    if (isValid) {
        if (isMobile && isMobileFilterWindowOpened) {
            mobileFilter();
        }

        page = 0;
        return prepareSearchParams(true);
    } else {
        Notifications.showBar({
            title: 'Error',
            message: 'Postal code is not in a proper format.',
            success: false
        });
    }
}

function checkZip(value) {
    //.. this is global regex to verify/sanity check postal code format but this is not or validating zip code precisly per country.
    //.. https://stackoverflow.com/a/7185241/5816119
    //.. (in order) US, IN, UK
    var zipCodePatternList = [/^\d{5}([\-]?\d{4})?$/, /^\d{6}$/, /^(GIR|[A-Z]\d[A-Z\d]??|[A-Z]{2}\d[A-Z\d]??)[ ]??(\d[A-Z]{2})$/]
    return zipCodePatternList.some(function (rx) { return rx.test(value); });
}

function loadInitialTabs() {
    loadTabs('neighborhoods');
    loadTabs('grades');
    loadTabs('ages');
    loadTabs('gender');
    loadTabs('services');
    loadTabs('timing');
    loadTabs('cost');
    loadTabs('pricing');
    loadTabs('starting');
    loadTabs('other');
    loadTabs('special-needs');
}

function loadTabs(name) {
    var element = $('#header-' + name);
    var isOpen = element.attr('isOpen');
    if (isOpen === 'true') {
        element.attr('isOpen', false);
        $('#tags-container-' + name).hide();
    }
    else {
        element.attr('isOpen', true);

        var tags = '';
        $('input:checkbox[name=' + name + ']:checked').each(function () {
            tags += getTagHtml($(this).val());
        });

        $('#tags-container-' + name).html(tags).show();
    }
}

function getTagHtml(name) {
    return '<span class="badge badge-success text-small primary-bg-color directory-badge">' + name + '</span>';
}

function resetTags() {
    $('#tags-container-neighborhoods').html('');
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
    var isOpen = $('#img-mobile-filter').attr('isOpen');
    if (isOpen == '0') {
        $('#filters').show();
        $('#mobile-filter-footer').show();

        $('#search-result').hide();
        $('#footerShowHide').hide();

        $('#img-mobile-filter').attr('isOpen', "1");
        isMobileFilterWindowOpened = true;
    }
    else {
        $('#filters').hide();
        $('#mobile-filter-footer').hide();
        $('#search-result').show();
        $('#footerShowHide').show();
        $('#img-mobile-filter').attr('isOpen', "0");
        isMobileFilterWindowOpened = false;
    }
}

function applyMobileFilter() {
    if (txtZip.val()?.length > 0) {
        //.. validate postal code first
        var isValid = checkZip($('#txtZipCode').val());
        if (isValid) {
            mobileFilter();
            page = 0;
            return prepareSearchParams(true);
        } else {
            $('#clear-filters').show();

            Notifications.showBar({
                title: 'Error',
                message: 'Postal code is not in a proper format.',
                success: false
            });
        }
    } else {
        mobileFilter();
        page = 0;
        prepareSearchParams();
    }
}

function openSeeMorePanelsForSelectedCheckboxes() {
    var hashUrl = WebPage.getHash();
    var filters = hashUrl.split(',');

    _.each(filters, function (filter) {
        if (filter.indexOf('search_') != 0 && filter.indexOf('zip_') != 0) {
            var inputToCheck = $(`input[value="${decodeURIComponent(filter.trim())}"]`);

            if (inputToCheck && inputToCheck.length > 0) {

                //..if checkbox is hidden, open that panel
                var showMoreElement = inputToCheck.parent().siblings('p');
                if (showMoreElement && showMoreElement.length > 0 && showMoreElement.text() == 'Show more...') {
                    $(showMoreElement).click();
                }
            }
        }
    });
}

function clearFilters() {
    //..clear all checkbox
    $(`input:checkbox[name=neighborhoods],[name=grades],[name=ages],[name=gender],[name=services],[name=timing],[name=pricing],[name=cost],[name=starting],[name=other],[name=special-needs]`).prop('checked', false);

    txtZip.val('');
    //..clear web link and hide clear filter link
    WebPage.addHash('');
    $('#clear-filters').hide();

    //..refresh accounts
    if (isMobile) {
        mobileFilter();
        $('#txtSearch-mobile').val('');
    } else {
        $('#txtSearch').val('');
    }

    resetTags();
    resetFlags();
    loadAccounts();
}

function resetFlags() {
    isReachedScrollEnd = false;
    page = 0;
    inCallback = false;
}

function toggleSideBar(element) {
    var isOpen = $(element).attr('isOpen');
    if (isOpen === 'true') {
        $(element).attr('isOpen', false)
        $('#search-sidebar-arrow').removeClass('fa-chevron-circle-left').addClass('fa-chevron-circle-right');
        $("#filters").animate({
            width: '0px'
        });
    } else {
        $(element).attr('isOpen', true)
        $('#search-sidebar-arrow').removeClass('fa-chevron-circle-right').addClass('fa-chevron-circle-left');
        $("#filters").animate({
            width: '25%'
        });
    }
}

function toggleMapView(element) {
    var isChecked = $(element).is(':checked')
    if (isChecked)
        $("#map-view").css('flex', '0.5')
    else
        $("#map-view").css('flex', '0')
}