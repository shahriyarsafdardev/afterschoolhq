var map;
var service;
var infowindow;
var marker = [];

function initMap() {
    var noPOILabels = [{ featureType: "poi", elementType: "labels", stylers: [{ visibility: "off" }] }];
    var noPOIMapType = new google.maps.StyledMapType(noPOILabels, { name: "Map" });
    infowindow = new google.maps.InfoWindow();

    map = new google.maps.Map(document.getElementById("map"),
        {
            center: { lat: 39.76, lng: -86.15 },
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
                icon: '../img/red-icon-map.png'
            });
            newmarker['infowindow'] = new google.maps.InfoWindow({
                content: places[i].AccountName
            });
            google.maps.event.addListener(map, 'click', function () {
                newmarker['infowindow'].close();
            });
            google.maps.event.addListener(newmarker, 'click', function () {
                removeIcons();
                this['infowindow'].open(map, this);
                this.setIcon('https://www.google.com/mapfiles/marker_green.png');
                var mainDiv = $("div").find(`[name='${this['infowindow'].content}']`);
                $("#directory-panel", mainDiv).attr('style', 'border: 5px solid red;');
                $('#search-container').animate({
                    scrollTop: parseInt($("#directory-panel", mainDiv).offset().top) + 50
                }, 500);
            });

            marker.push(newmarker);
        }
    }
}

function removeIcons() {
    for (var j = 0; j < marker.length; j++) {
        marker[j].setIcon("../img/red-icon-map.png");
        marker[j]['infowindow'].close();
    }
    $(".participation-object div").removeAttr('style');
}

function deleteAllMarkersOnMap() {
    for (var i = 0; i < marker.length; i++) {
        marker[i].setMap(null);
    }
    marker = [];
}