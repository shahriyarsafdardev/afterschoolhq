let map;
let service;
let infowindow;
let marker = [];
let schoolMarkers = [];

function initMap() {
    let noPOILabels = [{featureType: "poi", elementType: "labels", stylers: [{visibility: "off"}]}];
    let noPOIMapType = new google.maps.StyledMapType(noPOILabels, {name: "Map"});
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
    for (let i = 0; i < places.length; i++) {
        addMarker(places[i].Latitude, places[i].Longitude, places[i].AccountName, marker)
    }
}

function addMarker(lat, long, accountName, markerList) {
    if (lat != null && long !== "") {
        let newmarker = new google.maps.Marker({
            position: new google.maps.LatLng(lat, long),
            map: map,
            title: accountName,
            icon: '../Assets/img/red-icon-map.png'
        });
        newmarker['infowindow'] = new google.maps.InfoWindow({
            content: accountName
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
            let mainDiv = $("div").find(`[name='${this['infowindow'].content}']`);
            $("#directory-panel", mainDiv).addClass('primary-color-border');
            let container = $("#results-container");
            let element = $(mainDiv);
            let height = container.height() / 2;
            let elementTop = element.position().top;
            let position = container.scrollTop() + elementTop - height;
            container.animate({scrollTop: position});
        });

        markerList.push(newmarker);
    }
}

function removeIcons() {
    for (let j = 0; j < marker.length; j++) {
        marker[j].setIcon("../Assets/img/red-icon-map.png");
        marker[j]['infowindow'].close();
    }
    $(".participation-object div").removeClass('primary-color-border');
}

function deleteAllMarkersOnMap() {
    for (let i = 0; i < marker.length; i++) {
        marker[i].setMap(null);
    }
    marker = [];
}