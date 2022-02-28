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
            google.maps.event.addListener(newmarker, 'click', function () {
                this['infowindow'].open(map, this);
            });

            marker.push(newmarker);
        }
    }
}