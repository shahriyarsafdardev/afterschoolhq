var map;
var service;
var infowindow;
var marker = [];

function initMap() {
    var noPOILabels = [{featureType: "poi", elementType: "labels", stylers: [{visibility: "off"}]}];
    var noPOIMapType = new google.maps.StyledMapType(noPOILabels, {name: "Map"});
    infowindow = new google.maps.InfoWindow();

    map = new google.maps.Map(document.getElementById("map"),
        {
            center: {lat: 39.76, lng: -86.15},
            zoom: 10,
            mapTypeControlOptions: {
                mapTypeIds: [google.maps.MapTypeId.ROADMAP, 'no_poi']
            },
            disableDefaultUI: true,
            scaleControl: true,
            zoomControl: true
        });

    map.mapTypes.set('no_poi', noPOIMapType);
    map.setMapTypeId('no_poi');

    google.maps.event.addListener(map, 'click', function () {
        removeIcons();
    });
}

function addMarkersOnMap(places) {
    for (i = 0; i < places.length; i++) {
        if (places[i].Latitude != null && places[i].Longitude != "") {
            var newmarker = new google.maps.Marker({
                position: new google.maps.LatLng(places[i].Latitude, places[i].Longitude),
                map: map,
                title: places[i].AccountName,
                icon: '../Assets/img/red-icon-map.png'
            });
            newmarker['infowindow'] = new google.maps.InfoWindow({
                content: places[i].AccountName
            });

            google.maps.event.addListener(newmarker['infowindow'], 'closeclick', function () {
                removeIcons();
            });

            google.maps.event.addListener(map, 'click', function () {
                newmarker['infowindow'].close();
            });
            google.maps.event.addListener(newmarker, 'click', function () {
                removeIcons();
                this['infowindow'].open(map, this);
                this.setIcon('../Assets/img/red-map-icon-large.png');
                var mainDiv = $("div").find(`[name='${this['infowindow'].content}']`);
                $("#directory-panel", mainDiv).addClass('primary-color-border');
                var container = $("#results-container");
                var element = $(mainDiv);
                var height = container.height() / 2;
                var elementTop = element.position().top;
                var position = container.scrollTop() + elementTop - height;
                container.animate({scrollTop: position});
            });

            marker.push(newmarker);
        }
    }
}

function removeIcons() {
    for (var j = 0; j < marker.length; j++) {
        marker[j].setIcon("../Assets/img/red-icon-map.png");
        marker[j]['infowindow'].close();
    }
    $(".participation-object div").removeClass('primary-color-border');
}

function deleteAllMarkersOnMap() {
    for (var i = 0; i < marker.length; i++) {
        marker[i].setMap(null);
    }
    marker = [];
}