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