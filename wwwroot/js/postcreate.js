const UploadPostImage = async () => {
    try {
        const imageInput = document.getElementById("post-imageInput");
        const file = imageInput.files[0];
        if (!file) {
            return null;
        }

        const formData = new FormData();

        formData.append('file', file);

        const res = await fetch('/api/File/upload', {
            method: 'post',
            body: formData
        })

        const result = await res.json()
        return result.fileName;
    }
    catch (e) {
        return null;
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
    const imageKey = await UploadPostImage();

    const imgKeyInput = document.getElementById("PostImgkey");
    if (imgKeyInput) {
        imgKeyInput.value = imageKey || "";
    }

    form.submit();
});


var map = L.map('map').setView([13.7563, 100.5018], 13); // พิกัดกรุงเทพฯ

L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);

// เพิ่มหมุด (Marker)
// 1. สร้างตัวแปรเก็บ Marker ไว้ตัวเดียว (เพื่อไม่ให้หมุดซ้ำกันหลายอันเวลาคลิกใหม่)
var selectionMarker;

// 2. ดักจับเหตุการณ์การคลิกบนแผนที่
map.on('click', function(e) {
    var lat = e.latlng.lat; // ละติจูด
    var lng = e.latlng.lng; // ลองจิจูด

    // 3. ถ้ามีหมุดเดิมอยู่แล้ว ให้ลบออกก่อน หรือย้ายตำแหน่ง
    if (selectionMarker) {
        selectionMarker.setLatLng(e.latlng);
    } else {
        selectionMarker = L.marker(e.latlng).addTo(map);
    }

    // 4. แสดง Popup บอกพิกัด (หรือเอาไปใส่ใน Input ของ Form)
    selectionMarker.bindPopup("คุณเลือกที่: " + lat.toFixed(6) + ", " + lng.toFixed(6)).openPopup();

    // 5. ส่งค่าพิกัดไปเก็บไว้ใน Input HTML (เพื่อเตรียมส่งไป .NET)
    document.getElementById('latInput').value = lat;
    document.getElementById('lngInput').value = lng;
    
    console.log("Selected coordinates:", lat, lng);
});