function checkStrength() {
    $('#popover-password').css("display", "block");
    var strength = 0;
    var password = $('#password1').val();

    //If password contains both lower and uppercase characters, increase strength value.
    if (password.match(/([a-z].*[A-Z])|([A-Z].*[a-z])/)) {
        strength += 1;
        $('.low-upper-case').addClass('text-success');
        $('.low-upper-case i').removeClass('fa-times').addClass('fa-check');
        $('#popover-password-top').addClass('hide');


    } else {
        $('.low-upper-case').removeClass('text-success').addClass('text-danger');
        $('.low-upper-case i').addClass('fa-times').removeClass('fa-check');
        $('#popover-password-top').removeClass('hide');
    }

    //If it has numbers and characters, increase strength value.
    if (password.match(/([0-9])/)) {
        strength += 1;
        $('.one-number').addClass('text-success');
        $('.one-number i').removeClass('fa-times').addClass('fa-check');
        $('#popover-password-top').addClass('hide');

    } else {
        $('.one-number').removeClass('text-success').addClass('text-danger');
        $('.one-number i').addClass('fa-times').removeClass('fa-check');
        $('#popover-password-top').removeClass('hide');
    }

    //If it has one special character, increase strength value.
    if (password.match(/([!,%,&,@@,#,$,^,*,?,_,~])/)) {
        strength += 1;
        $('.one-special-char').addClass('text-success');
        $('.one-special-char i').removeClass('fa-times').addClass('fa-check');
        $('#popover-password-top').addClass('hide');

    } else {
        $('.one-special-char').removeClass('text-success').addClass('text-danger');
        $('.one-special-char i').addClass('fa-times').removeClass('fa-check');
        $('#popover-password-top').removeClass('hide');
    }

    if (password.length > 7) {
        strength += 1;
        $('.eight-character').addClass('text-success');
        $('.eight-character i').removeClass('fa-times').addClass('fa-check');
        $('#popover-password-top').addClass('hide');

    } else {
        $('.eight-character').removeClass('text-success').addClass('text-danger');
        $('.eight-character i').addClass('fa-times').removeClass('fa-check');
        $('#popover-password-top').removeClass('hide');
    }

    // If value is less than 2

    if (strength < 2) {
        $('#result').removeClass();
        $('#password-strength').addClass('progress-bar-danger');
        $('#result').addClass('text-danger').text('Weak');
        $('#password-strength').css('width', '10%');
        $('#form-submit').prop("disabled", true);
    }
    else if (strength < 4) {
        $('#result').removeClass();
        $('#password-strength').removeClass('progress-bar-danger');
        $('#password-strength').addClass('progress-bar-warning');
        $('#result').addClass('text-warning').text('Medium');
        $('#password-strength').css('width', '60%');
        $('#form-submit').prop("disabled", true);
        return 'Medium'
    }
    else {
        $('#result').removeClass();
        $('#result').addClass('strong');
        $('#password-strength').removeClass('progress-bar-warning progress-bar-danger');
        $('#password-strength').addClass('progress-bar-success');
        $('#result').addClass('text-success').text('Strong');
        $('#password-strength').css('width', '100%');
        $('#form-submit').prop("disabled", false);
        return 'Strong';
    }
    if (!password) {
        $('#popover-password').css("display", "none");
        $('#form-submit').prop("disabled", false);
    }

}