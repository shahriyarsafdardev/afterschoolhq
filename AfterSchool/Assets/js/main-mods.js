function formHasErrors() {
    var hasErrors = false;

    $('[required]').each(function () {

        var hasError = false;
        var id = $(this).attr('id');
        var name = $(this).attr('name');

        switch ($(this).prop('tagName').toLowerCase()) {

            case 'input':

                if ($(this).attr('type') == 'checkbox' && !$(this).is(':checked')) {
                    hasError = true;

                    $('input[id^="' + name + '"]').change(function () {
                        if ($(this).is(':checked')) {
                            clearValidationCSS(id);
                        }
                    });

                } else if ($(this).attr('type') == 'radio') {

                    var anyChecked = false;

                    $('input[id^="' + name + '"]').each(function () {
                        if ($(this).is(':checked')) {
                            anyChecked = true;
                        }
                    });

                    hasError = !anyChecked;

                    if (hasError) {

                        $('input[id^="' + name + '"]').change(function () {
                            if ($(this).is(':checked')) {
                                clearValidationCSS(id);
                            }
                        });
                    }

                } else if ($(this).val().length == 0) {
                    hasError = true;

                    $(this).change(function () {
                        if ($(this).val().length > 0) {
                            clearValidationCSS(id);
                        }
                    });
                }

            case 'textarea':
            case 'select':

                if ($(this).val().length == 0) {
                    hasError = true;

                    $(this).change(function () {
                        if ($(this).val().length > 0) {
                            clearValidationCSS(id);
                        }
                    });
                }
                break;

            case '':
                break;
        }

        if (hasError) {
            setValidationCSS(id);
            hasErrors = true;
        }

    });

    return hasErrors;
}

$(document).ready(function () {
    try{
        if (PageEvent) {
            if (PageEvent.AFTER_INIT) {
                PageEvent.trigger(PageEvent.AFTER_INIT);
            }
        }
    } catch(err) { }

    //$('[data-tooltip-id]').tooltip();

    // may need to uncomment
    // formValidation();

    //$('[data-tooltip-id]').tooltip();
    //$('[data-tooltip-id]').tooltip('show');

})

function formValidation() {
    var forms = document.getElementsByTagName('form');
    for (var i = 0; i < forms.length; i++) {
        forms[i].noValidate = true;

        forms[i].addEventListener('submit', function (event) {

            //Prevent submission if checkValidity on the form returns false.
            if (!event.target.checkValidity()) {
                // console.log('event.target.checkValidity');

                var hasErrors = false;

                var submitButtonId = '';
                var submitButtonText = '';
                var submittingButtonText = '';

                if ($('#form-validation-settings').length > 0) {
                    submitButtonId = $('#form-validation-settings').attr('data-form-validation-submit-button');
                    submitButtonText = $('#form-validation-settings').attr('data-form-validation-submit-text');
                    submittingButtonText = $('#form-validation-settings').attr('data-form-validation-submitting-text');
                }

                hasErrors = formHasErrors();

                if (hasErrors) {
                    console.log('Has Errors');

                    if (submitButtonId.length > 0) {
                        $('#' + submitButtonId).html(submitButtonText);
                        $('#' + submitButtonId).prop('disabled', false);
                    }

                    event.preventDefault();

                    Notifications.showBar({
                        title: 'Error',
                        message: 'Please fill in all required fields',
                        success: false
                    });
                } else {
                    console.log('No Errors');
                }
            } else { console.log('checkValidity==true'); }
        }, false);
    }
}

function clearValidationCSS(id) {

    var obj = $('[data-tooltip-id="' + id + '"]');
    var label = $('label[for="' + id + '"]');

    obj.css('border-width', obj.attr('border-width'));
    obj.css('border-color', obj.attr('border-color'));
    obj.css('border-style', obj.attr('border-style'));
    obj.css('background-color', obj.attr('background-color'));
    obj.css('color', obj.attr('color'));
    label.css('color', label.attr('color'));
}

function setValidationCSS(id) {
    var obj = $('[data-tooltip-id="' + id + '"]');
    var label = $('label[for="' + id + '"]');

    obj.attr('border-width', obj.css('border-width'));
    obj.attr('border-color', obj.css('border-color'));
    obj.attr('border-style', obj.css('border-style'));
    obj.attr('background-color', obj.css('background-color'));
    obj.attr('color', obj.css('color'));
    label.attr('color', label.css('color'));

    obj.css('background-color', 'pink');
    obj.css('border-width', '1px');
    obj.css('border-color', 'pink');
    obj.css('border-style', 'solid');
    obj.css('color', 'red');

    label.css('color', 'red');
}

var WebPage = {
    addHash: function (hash) {
        location.href = location.href.split('#')[0] + '#' + hash;
    },
    getHash: function(){
        var arr = location.href.split('#');
        if (arr.length == 2) {
            return arr[1];
        }
        else {
            return "";
        }
    },
    getUrlParamValue: function (key) {
        var params = WebPage.getUrlParams();
        var rtn = '';
        for (var i = 0; i < params.length; i++) {
            if (params[i][0] == key) {
                rtn = params[i][1];
            }
        }
        return rtn;
    },
    getUrlParamsAsString: function (itemsToRemove, itemsToAdd) {
        var paramStr = '';
        var params = WebPage.getUrlParams(itemsToRemove);
        if (params) {
            for (var i = 0; i < params.length; i++) {
                if (paramStr.length > 0) paramStr += '&';
                paramStr += params[i][0] + '=' + params[i][1];
            }
        }
        if (itemsToAdd) {
            for (var i = 0; i < itemsToAdd.length; i++) {
                if (paramStr.length > 0) paramStr += '&';
                paramStr += itemsToAdd[i];
            }
        }
        return paramStr;
    },
    getUrlParams: function (itemsToRemove) {
        var rtn = new Array();
        var url = window.location.href.split('?');
        if (url.length == 2) {
            if (!itemsToRemove) {
                itemsToRemove = new Array();
            }
            params = url[1].split('&');
            for (var i = 0; i < params.length; i++) {
                var keyVal = params[i].split('=');
                if ($.inArray(keyVal[0], itemsToRemove) == -1) {
                    rtn.push(keyVal);
                }
            }
        }
        return rtn;
    },
    getRelativePath: function () {
        var path = WebPage.getUrlPath();
        var startIndex = path.indexOf(location.hostname) + location.hostname.length + (location.port ? ':' + location.port : '').length;
        var relativePath = path.substring(startIndex);
        return relativePath;
    },
    getRelativePathValue: function (index) {
        var relativePath = WebPage.getRelativePath();
        relativePath = relativePath.substring(1);
        var pathArray = relativePath.split('/');

        if (index > -1 && index < pathArray.length) {
            return pathArray[index];
        }
        else {
            return "";
        }
    },
    getUrlPath: function () {
        var url = window.location.href.split('?');
        return url[0];
    },
    copySelection: function (url, successText, callback) {

        var textArea = document.createElement("textarea");
        textArea.value = url;

        // Avoid scrolling to bottom
        textArea.style.top = "0";
        textArea.style.left = "0";
        textArea.style.position = "fixed";
        // textArea.style.visibility = "hidden";

        document.body.appendChild(textArea);

        textArea.focus();
        textArea.select();

        var successful = document.execCommand('copy');
        textArea.remove();

        if (successful) {
            Notifications.showMessage("Copied", successText, true);

            if (callback) {
                callback();
            }
        }

    },
    navigateIframe: function(url) {
        window.location !== window.parent.location ? parent.location.href = url : location.href = url;
    }
}

var Notifications = {
    showBar: function (options) {

        var alertIcon = 's7-check-circle';
        var alertTitle = 'Success';
        var alertClass = 'success';

        if (options.title) {
            alertTitle = options.title;
        }

        if (options.success === false) {
            alertIcon = 's7-close-circle';
            alertClass = 'danger'
            alertTitle = 'Error';
        }

        $.gritter.add({
            title: '<i class="' + alertIcon + '"></i> ' + alertTitle,
            class_name: "color " + alertClass,
            text: options.message
        });
    },
    showMessage: function (title, message, isSuccess) {
        Notifications.showBar({
            title: title,
            message: message,
            success: isSuccess
        });
    },
    showErrors: function (messages) {
        var items = "";
        for (var i = 0; i < messages.length; i++) {
            items += messages[i].Text + "<br />";
        }

        Notifications.showBar({
            title: 'Error',
            message: items,
            success: false
        });
    }
}

var ActivitySetup = {
    showAdvancedSettings: function (cmd) {
        if ($('.advanced-settings').attr('class').indexOf('hide') > -1) {
            ActivitySetup.showAdvancedSettingsYes();
        } else {
            ActivitySetup.showAdvancedSettingsNo();
        }
    },
    showAdvancedSettingsYes: function () {
        $('#hdnAdvanceSettingToggle').val(true);
        $('.advanced-settings').removeClass('hide');
        $('#btn-advanced-settings').html('Hide Advanced Settings');
    },
    showAdvancedSettingsNo: function () {
        $('#hdnAdvanceSettingToggle').val(false);
        $('.advanced-settings').addClass('hide');
        $('#btn-advanced-settings').html('Show Advanced Settings');
    },
    navigateActivity: function (url) {
        $('#form1').attr('action', url);
    },
    setCopyActivity: function (name, url) {
        const MAX_NAME_LENGTH_TO_COPY = 67; // to adjust ' [copy]' suffix within limit of 75 characters for activity name
        name = name.substring(0, MAX_NAME_LENGTH_TO_COPY); 
        $('#activitynamecopy').val(name + ' [copy]');
        $('#form-copy').attr('action', url);
    }
}

var AccountSetup = {
      
    sendVerificationEmail: function (redirectUrl) {
        Notifications.showBar({
            title: 'Success',
            message: 'Sending verification email...'
        });

        $.get(
            '/org/services/sendverificationemail/?redirectUrl=' + encodeURIComponent(redirectUrl),
            function (data) {
                if (data && data.status == 'ok') {
                    Notifications.showBar({
                        title: 'Success',
                        message: 'A verification email has been sent. Please check your email.'
                    });

                } else {
                    Notifications.showBar({
                        title: 'Error',
                        message: 'Error sending verification email. Please <a href="mailto:support@afterschoolhq.com" class="white">contact support</a>.'
                    });
                }
            }
        );
    }
}

PageEvent.add(PageEvent.AFTER_INIT, function () {
    if (WebPage.getUrlParamValue('saved') == '1') {
        Notifications.showMessage('Success', 'Saved', true);
    }
})