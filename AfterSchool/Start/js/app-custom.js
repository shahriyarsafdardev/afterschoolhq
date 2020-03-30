$(document).ready(function(){
    SignUpProcess.init();
});

var SignUpProcess = {

    hasErrors: false,
    submitButton: null,

    init: function () {
        SignUpProcess.initForm();
        SignUpProcess.initCheckboxLabels();
    },

    isLive: function () {
        return window.location.protocol === 'file:' ? false : true;
    },

    initForm: function () {
        SignUpProcess.initButton();
        SignUpProcess.initFields();
    },

    initCheckboxLabels: function () {
        $('input[type="checkbox"], input[type="radio"]').each(function () {
            $(this).next().attr('for', $(this).attr('id'));
        });
    },

    initButton: function () {

        SignUpProcess.submitButton = $('#submit-buton');
        SignUpProcess.submitButton.attr('data-href', SignUpProcess.submitButton.attr('href'));
        SignUpProcess.submitButton.attr('data-text', SignUpProcess.submitButton.html());
        SignUpProcess.submitButton.attr('href', '#');
        SignUpProcess.disableSubmitButton();

        SignUpProcess.submitButton.click(function () {

            hasErrors = false;

            SignUpProcess.checkForm();

            if (!hasErrors) {

                SignUpProcess.submitButton.html('Saving...');
                SignUpProcess.disableSubmitButton();

                try {
                    if (SignUpProcess.isLive()) {
                        SignUpProcess.callApis(SignUpProcess.submitButton.attr('button-action'));
                    } else {
                        location.href = url;
                    }

                } catch (err) {
                    SignUpProcess.showErrors();
                    btn.prop('disabled', false);
                }
            }

        });

        const choiceButtons = $('.choice-buttons');

        choiceButtons.each(function () {
            $(this).attr('data-href', $(this).attr('href'));
            $(this).attr('href', '#');
        });

        choiceButtons.click(function () {

            if (SignUpProcess.isLive()) {
                SignUpProcess.callApis($(this).attr('button-action'), $(this).attr('data-href'));
            } else {
                location.href = $(this).attr('data-href');
            }
        });

    },

    disableSubmitButton: function () {

        SignUpProcess.submitButton.css('pointer-events', 'none');
        $('#div-btn-disable').remove();
        $('<div id="div-btn-disable" />').css({
            position: "absolute",
            width: "100%",
            height: "100%",
            left: 0,
            top: 0,
            zIndex: 1000000,  // to be on the safe side
            background: "rgba(0,0,0,.7)"
        }).appendTo(SignUpProcess.submitButton.css("position", "relative"));
    },

    enableSubmitButton: function () {
        SignUpProcess.submitButton.css('pointer-events', 'auto');
        $('#div-btn-disable').remove();
    },

    callApis: function (action, url = "") {

        switch (action) {
            case 'personal-info':
                API.savePersonalInfo();
                break;
            case 'profile-type-individual':
                API.saveProfileTypeIndividual(url);
                break;
            case 'profile-type-organization':
                API.saveProfileTypeOrganization(url);
                break;
            case 'user-type-teacher':
            case 'user-type-freelance':
            case 'org-type-school':
            case 'org-type-community-center':
            case 'org-type-nonprofit':
            case 'org-type-business':
            case 'program-name':
            case 'program-contact':
            case 'social-links':
            case 'program-classifications':
            case 'program-offerings':
            case 'awards':
            case 'certifications':
            case 'create-password':
                break;
        }
    },

    getParams: function () {
        let params = '';

        $('input, select, textarea').each(function () {

            let idParts = $(this).attr('id').split('-');
            let id = "";

            for (let i = 0; i < idParts.length; i++) {
                if (i > 0) {
                    id += idParts[i];
                }
            }

            if (params.length > 0) params += '&';

            if ($(this).attr('type') === 'checkbox' || $(this).attr('type') === 'radio') {
                if ($(this).is(':checked')) {
                    params += id + '=1';
                }
            } else {
                params += id + '=' + encodeURIComponent($(this).val());
            }
        });

        return params;
    },

    showErrors: function () {

    },

    initFields: function () {
        $('input, textarea').blur(function () {
            SignUpProcess.checkInput($(this));
            SignUpProcess.checkReadyToSubmit();
        });

        $('select').change(function () {
            SignUpProcess.checkInput($(this));
            SignUpProcess.checkReadyToSubmit();
        });

        $('input[type="checkbox"], input[type="radio"]').click(function () {
            SignUpProcess.checkInput($(this).parents('div[required]'));
            SignUpProcess.checkReadyToSubmit();
        });

        $('input[on-blur-validation], textarea[on-blur-validation]').blur(function () {
            eval($(this).attr('on-blur-validation'));
        });
    },

    checkReadyToSubmit: function () {
        if (SignUpProcess.isReadyToSubmit()) {
            console.log('is ready');
            SignUpProcess.enableSubmitButton();
        } else {
            console.log('is not ready');
            SignUpProcess.disableSubmitButton();
        }
    },

    isReadyToSubmit: function () {

        let readyToSubmit = true;

        $('input, select, textarea, div').each(function () {
            if (!SignUpProcess.isValidInput($(this))) {
                readyToSubmit = false;
            }
        });

        return readyToSubmit;
    },

    checkForm: function () {
        $('input, select, textarea, div').each(function () {
            SignUpProcess.checkInput($(this));
        });
    },

    checkInput: function (field) {
        if (!SignUpProcess.isValidInput(field)) {
            SignUpProcess.inputError(field);
        } else {
            SignUpProcess.inputReset(field);
        }
    },

    isValidInput: function (input) {

        if (SignUpProcess.inputHasAttribute(input, 'required')) {

            // checkboxes and radio buttons
            if (input.prop('tagName').toLowerCase() === 'div') {

                const requiredCount = parseInt(input.attr('required-count'));
                let checkedCount = 0;

                input.find('input').each(function () {
                    if ($(this).is(':checked')) {
                        checkedCount += 1;
                    }
                });

                if (checkedCount < requiredCount) {
                    return false;
                }

            } else {
                const val = input.val();

                if (val.length === 0) {
                    return false;
                }

                if (input.attr('type') === 'email') {
                    return SignUpProcess.validateEmail(val);
                }

                if (input.attr('type') === 'password') {
                    return SignUpProcess.validatePassword(val);
                }
            }
        }

        if (SignUpProcess.inputHasAttribute(input, 'data-validate')) {

            const val = input.val();

            if (!SignUpProcess.inputHasAttribute(input, 'required') && val.length === 0) {
                return true;
            }

            if (input.attr('data-validate') === 'email') {
                return SignUpProcess.validateEmail(input.val());
            }

            if (input.attr('data-validate') === 'url') {
                return SignUpProcess.validateUrl(input.val());
            }

            if (input.attr('data-validate') === 'phone') {
                return SignUpProcess.validatePhone(input.val());
            }
        }

        return true;
    },

    validatePassword: function (val) {
        return val.length >= 8;
    },

    validatePhone: function (val) {
        return val.length >= 10;
    },

    validateUrl: function (val) {
        return val.indexOf('.') > 0;
    },

    validateEmail: function (val) {
        return val.indexOf('@') > 0 && val.indexOf('.') > 0;
    },

    inputHasAttribute: function (input, attribute) {
        var attr = input.attr(attribute);

        if (typeof attr !== typeof undefined && attr !== false) {
            return true;
        }

        return false;
    },

    inputError: function (input, altText) {
        hasErrors = true;

        const id = input.attr('id');
        const errorId = id + '-error';
        const errorBlock = $('#' + errorId);

        let errorText = input.attr('data-validate-error');

        if (altText) {
            errorText = altText;
        }

        input.css('border-color', '#F00');
        input.css('border-width', '1px');
        input.css('border-style', 'solid');

        if (input.attr('data-role') === 'tagsinput') {
            SignUpProcess.inputError(input.prev());
        }

        if (SignUpProcess.inputHasAttribute(input, 'data-validate-error') && errorBlock.length === 0) {
            input.after('<p id="' + errorId + '" class="input-error text-danger">' + errorText + '</p>');
        }
    },

    inputReset: function (input) {

        const id = input.attr('id');

        input.css('border-color', '');
        input.css('border-width', '');
        input.css('border-style', '');

        if (input.attr('data-role') === 'tagsinput') {
            SignUpProcess.inputReset(input.prev());
        }

        $('#' + id + '-error').remove();
    },

    checkEmailAvailability: function (id) {

        if (SignUpProcess.isLive()) {

            const input = $('#' + id);
            const value = input.val();

            SignUpProcess.disableSubmitButton();

            if (value.length > 0 && SignUpProcess.validateEmail(value)) { 
                API.checkEmailAvailability();
            }

        } else {
            return false;
        }
    }

};


var API = {

    checkEmailAvailability: function() {
        $.get('/services/isinuse/?email=' + encodeURIComponent(value), function (data) {
            if (data && data.status === 'ok') {

                if (data.isInUse) {
                    SignUpProcess.inputError(input, 'This email is already in use, do you need to <a href="/login">login</a>?');
                } else {
                    SignUpProcess.checkReadyToSubmit();
                }

            } else {
                SignUpProcess.showErrors();
            }
            
        });
    },

    savePersonalInfo: function () {

        const params = SignUpProcess.getParams();

        $.post('/signup/personal', params, function (data) {

            if (data && data.status === 'ok') {

                localStorage.setItem('ahq-profile-token', data.token);
                localStorage.setItem('ahq-current-page', SignUpProcess.submitButton.attr('data-href'));

                location.href = SignUpProcess.submitButton.attr('data-href');
            } else {
                SignUpProcess.checkReadyToSubmit();
                SignUpProcess.submitButton.html(SignUpProcess.submitButton.attr('data-text'));
                SignUpProcess.showErrors();
            }
        });
    },

    saveProfileTypeIndividual: function (url) {
        localStorage.setItem('ahq-profile-type', 'individual');
        localStorage.setItem('ahq-current-page', url);
        location.href = url;
    },

    saveProfileTypeOrganization: function (url) {
        localStorage.setItem('ahq-profile-type', 'organization');
        localStorage.setItem('ahq-current-page', url);
        location.href = url;
    }

}