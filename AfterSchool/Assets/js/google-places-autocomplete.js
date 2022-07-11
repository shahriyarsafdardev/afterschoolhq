$(document).ready(function () {
    let $form = $('form');
    let input = $form.find('input:first')[0];

    let streetNumber = '';
    let mainAddress = '';
    let state = '';

    let autocomplete = new google.maps.places.Autocomplete(input, null);
    autocomplete.setFields(['place_id', 'name', 'address_components', 'geometry']);
    autocomplete.addListener('place_changed', function () {
        const place = autocomplete.getPlace();
        const components = place.address_components;

        if (typeof components !== 'undefined') {
            for (let component of components) {

                const type = component.types[0];
                const longName = component.long_name;
                const shortName = component.short_name;
                switch (type) {
                    case 'street_number':
                        streetNumber = longName
                        break;
                    case 'route':
                        mainAddress = streetNumber + " " + shortName;
                        break;
                    case 'sublocality_level_2':
                        mainAddress = mainAddress + " " + shortName;
                        break;
                    case 'sublocality_level_1':
                        mainAddress = mainAddress + " " + shortName;
                        break;
                    case 'locality':
                        $(input).val(mainAddress);
                        $form.find('input').eq(2).val(shortName);
                        break;
                    case 'postal_code':
                        $form.find('input').eq(3).val(longName);
                        break;
                    case 'administrative_area_level_2':
                        break;
                    case 'administrative_area_level_1':
                        $form.find('select').eq(0).find("option:contains(" + longName + ")").prop('selected', true);
                        state = longName;
                        break;
                    case 'country':
                        break;
                }
            }
        }
    });
    $(input).attr('placeholder', '');
})