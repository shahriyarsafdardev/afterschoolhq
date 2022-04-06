let map;
let service;
let infowindow;
let markerList = [];
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
        closeInfoWindows();
    });
}

function addMarkersOnMap(places) {
    for (let i = 0; i < places.length; i++) {
        addMarker(places[i].Latitude, places[i].Longitude, places[i].AccountName, false);
    }
}

function addMarker(latitude, longitude, accountName, isSchool) {
    if (!latitude || !longitude) {
        return;
    }

    let newMarker = new google.maps.Marker({
        position: new google.maps.LatLng(latitude, longitude),
        map: map,
        title: accountName,
        icon: isSchool
            ? {url: '../Assets/img/school-map-icon.svg', scaledSize: new google.maps.Size(20, 20)}
            : '../Assets/img/red-map-icon.png',
    });
    newMarker['infowindow'] = new google.maps.InfoWindow({
        content: accountName
    });

    google.maps.event.addListener(newMarker['infowindow'], 'closeclick', function () {
        closeInfoWindows();
    });
    google.maps.event.addListener(map, 'click', function () {
        newMarker['infowindow'].close();
    });
    google.maps.event.addListener(newMarker, 'click', function () {
        closeInfoWindows();

        this['infowindow'].open(map, this);
        this.setIcon(isSchool
            ? {url: '../Assets/img/school-map-icon.svg', scaledSize: new google.maps.Size(32, 32)}
            : '../Assets/img/red-map-icon-large.png');

        if (isSchool)
            return;
        let mainDiv = $("div").find(`[name='${this['infowindow'].content}']`);
        $("#directory-panel", mainDiv).addClass('primary-color-border');
        let container = $("#results-container");
        let element = $(mainDiv);
        let height = container.height() / 2;
        let elementTop = element.position().top;
        let position = container.scrollTop() + elementTop - height;
        container.animate({scrollTop: position});
    });

    if (isSchool)
        schoolMarkers.push(newMarker);
    else
        markerList.push(newMarker);
}

function closeInfoWindows() {
    for (let marker of markerList) {
        marker.setIcon("../Assets/img/red-map-icon.png");
        marker['infowindow'].close();
    }
    for (let schoolMarker of schoolMarkers) {
        let markerIcon = {
            url: '../Assets/img/school-map-icon.svg',
            scaledSize: new google.maps.Size(20, 20)
        }
        schoolMarker.setIcon(markerIcon);
        schoolMarker['infowindow'].close();
    }
    $(".participation-object div").removeClass('primary-color-border');
}

function deleteAllMarkersOnMap() {
    for (let i = 0; i < markerList.length; i++) {
        markerList[i].setMap(null);
    }
    markerList = [];
}

function setMapCenter(latitude, longitude) {
    map.panTo(new google.maps.LatLng(latitude, longitude));
}