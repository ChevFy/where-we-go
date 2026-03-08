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

const validateForm = () => {
    const title = (document.getElementById("Title")?.value || "").trim();
    const desc = (document.getElementById("Description")?.value || "").trim();
    const locName = (document.getElementById("LocationName")?.value || "").trim();
    const dateDeadline = document.getElementById("DateDeadline")?.value || "";
    const timeDeadline = document.getElementById("TimeDeadline")?.value || "";
    const eventDate = document.getElementById("EventDate")?.value || "";
    const eventTime = document.getElementById("EventTime")?.value || "";
    const minPart = parseInt(document.getElementById("MinParticipants")?.value, 10) || 0;
    const maxPart = parseInt(document.getElementById("MaxParticipants")?.value, 10) || 0;
    const categories = document.querySelectorAll('input[name="CategoryIds"]:checked');
    const lat = document.getElementById("location-lat")?.value || "";
    const lon = document.getElementById("location-lon")?.value || "";
    const errors = [];

    if (!title || title.length < 1 || title.length > 200) errors.push({ field: "Title", message: "Title is required (1-200 characters)." });
    if (!desc || desc.length < 1 || desc.length > 1000) errors.push({ field: "Description", message: "Description is required (1-1000 characters)." });
    if (!locName || locName.length < 1 || locName.length > 1000) errors.push({ field: "LocationName", message: "Location name is required (1-1000 characters)." });
    if (locName && (!lat || !lon)) errors.push({ field: "LocationName", message: "Please select a location on the map." });
    if (!dateDeadline) errors.push({ field: "DateDeadline", message: "Registration deadline date is required." });
    if (!timeDeadline) errors.push({ field: "TimeDeadline", message: "Registration deadline time is required." });
    if (!eventDate) errors.push({ field: "EventDate", message: "Event date is required." });
    if (!eventTime) errors.push({ field: "EventTime", message: "Event time is required." });
    if (dateDeadline && timeDeadline && new Date(dateDeadline + "T" + timeDeadline) <= new Date()) errors.push({ field: "TimeDeadline", message: "Registration deadline must be in the future." });
    if (dateDeadline && timeDeadline && eventDate && eventTime && new Date(eventDate + "T" + eventTime) <= new Date(dateDeadline + "T" + timeDeadline)) errors.push({ field: "EventTime", message: "Event date and time must be after the deadline." });
    if (minPart < 1 || maxPart < 1 || minPart >= maxPart) errors.push({ field: "MinParticipants", message: "Min participants must be less than max participants (both at least 1)." });
    if (categories.length === 0) errors.push({ field: "check-category", message: "Please select at least one category." });

    return { valid: errors.length === 0, errors };
};

const showFieldError = (field, message) => {
    const container = field === "check-category" ? document.querySelector(".check-category") : document.getElementById(field)?.parentNode;
    if (!container) return;
    let span = container.querySelector(".validation-error-msg");
    if (!span) {
        span = document.createElement("span");
        span.className = "validation-error-msg";
        span.style.cssText = "color: red; font-size: 12px; display: block; margin-top: 5px;";
        container.appendChild(span);
    }
    span.textContent = message;
};

const clearFieldErrors = () => {
    document.querySelectorAll(".validation-error-msg").forEach(el => el.remove());
};

const updateSubmitButton = () => {
    const result = validateForm();
    const btn = document.getElementById("submit-btn");
    if (btn) btn.disabled = !result.valid;
    clearFieldErrors();
    result.errors.forEach(e => showFieldError(e.field, e.message));
};

const LocationValidate = async (lat, lon) => {
    try {
        if (!lat || !lon) {
            return { success: false, error: 'Please Select location on the map' };
        }



        return { success: true, data: result };
    }
    catch (e) {
        console.error('LocationSave error:', e);
        return { success: false, error: 'Something went wrong!!' };
    }
}

const form = document.querySelector('form[action*="PostCreate"]');
if (form) {
    form.addEventListener("input", updateSubmitButton);
    form.addEventListener("change", updateSubmitButton);
    updateSubmitButton();
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
    const btn = document.getElementById("submit-btn");
    if (!validateForm().valid) return;
    btn.disabled = true;

    const form = document.querySelector('form[action*="PostCreate"]');
    if (!form) {
        btn.disabled = false;
        alert('เกิดข้อผิดพลาด: ไม่พบฟอร์ม');
        return;
    }

    const imageResult = await UploadPostImage();
    if (!imageResult.success) {
        btn.disabled = false;
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
            updateSubmitButton();
        })
        .catch(error => {
            console.error('Geocoding error:', error);
            locationInput.value = "";
            document.getElementById("location-lat").value = "";
            document.getElementById("location-lon").value = "";
            alert("Can't find location. Please Try again!");
            updateSubmitButton();
        });


    document.getElementById("location-lat").value = lat
    document.getElementById("location-lon").value = lng
    updateSubmitButton();
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