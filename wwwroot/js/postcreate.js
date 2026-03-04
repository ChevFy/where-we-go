const UploadPostImage = async () => {
    try {
        const imageInput = document.getElementById("post-imageInput");
        const file = imageInput.files[0];
        if (!file) {
            return { success: true, fileName: null };
        }

        const formData = new FormData();
        formData.append('file', file);

        const res = await fetch('/api/File/upload', {
            method: 'post',
            body: formData
        });

        if (!res.ok) {
            const errorText = await res.text();
            return { success: false, error: `Upload file Error : ${errorText || res.statusText}` };
        }

        const result = await res.json();
        return { success: true, fileName: result.fileName };
    }
    catch (e) {
        console.error('UploadPostImage error:', e);
        return { success: false, error: 'Something went wrong!' };
    }
}

const LocationValidate = async (lat, lon) => {
    try {


        if (!lat || !lng) {
            return { success: false, error: 'Please Select location on the map' };
        }



        return { success: true, data: result };
    }
    catch (e) {
        console.error('LocationSave error:', e);
        return { success: false, error: 'Something went wrong!!' };
    }
}

document.getElementById("post-imageInput").addEventListener("change", (e) => {
    if (e.target.files.length > 0) {
        const file = e.target.files[0];
        const previewUrl = URL.createObjectURL(file);
        document.getElementById("post-img-url").src = previewUrl;
    }
});

document.getElementById("submit-btn").addEventListener('click', async (e) => {
    e.preventDefault();

    const form = document.querySelector('form[action*="PostCreate"]');
    if (!form) {
        alert('เกิดข้อผิดพลาด: ไม่พบฟอร์ม');
        return;
    }

    const imageResult = await UploadPostImage();
    if (!imageResult.success) {
        alert(imageResult.error);
        return;
    }


    const imgKeyInput = document.getElementById("PostImgkey");
    if (imgKeyInput) {
        imgKeyInput.value = imageResult.fileName || "";
    }

    form.submit();
});


var map = L.map('map').setView([13.7563, 100.5018], 10); // bangkok

L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);


let marker;

map.on('click', async function (e) {
    const lat = e.latlng.lat;
    const lng = e.latlng.lng;
    const locationInput = document.getElementById('LocationName');
    locationInput.value = "Searching...";
    if (marker) {
        marker.setLatLng(e.latlng);
    } else {
        marker = L.marker(e.latlng).addTo(map);
    }

    const result = await LocationValidate(lat, lng);


    fetch(`https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lng}`)
        .then(response => {
            if (!response.ok) throw new Error('Geocoding request failed');
            return response.json();
        })
        .then(data => {
            let placeName = data.display_name;
            locationInput.value = placeName;
            marker.bindPopup("<b>Place :</b><br>" + placeName).openPopup();
            locationInput.dispatchEvent(new Event('change'));
            locationInput.dispatchEvent(new Event('input'));
        })
        .catch(error => {
            console.error('Geocoding error:', error);
            locationInput.value = "";
            alert("Can't find location. Please Try again!");
        });


    document.getElementById("location-lat").value = lat
    document.getElementById("location-lon").value = lng
});

const geocoder = L.Control.geocoder({
    defaultMarkGeocode: false
})
    .on('markgeocode', function (e) {
        var bbox = e.geocode.bbox;
        var poly = L.polygon([
            bbox.getSouthEast(),
            bbox.getNorthEast(),
            bbox.getNorthWest(),
            bbox.getSouthWest()
        ]);

        map.fitBounds(poly.getBounds());
    })
    .addTo(map);