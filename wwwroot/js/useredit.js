


let UpdateUserDtoJson = {
    Name: "",
    userName: "",
    Bio: "",
    ProfileUrl: ""
};

const FieldEditorFactory = (htmlId, dtoKey, displayType = "block") => {
    let isEditing = false;

    return {
        toggle: () => {
            const displayLabel = document.getElementById(`profile-${htmlId}`);
            const inputContainer = document.querySelector(`.frominput-${htmlId}-display`);
            const inputField = document.getElementById(`frominput-${htmlId}`);

            if (!isEditing) {
                inputContainer.style.display = displayType;
                displayLabel.style.display = "none";
            } else {
                const newValue = inputField.value;
                displayLabel.innerText = newValue;
                UpdateUserDtoJson[dtoKey] = newValue;

                inputContainer.style.display = "none";
                displayLabel.style.display = displayType;
            }
            isEditing = !isEditing;
        }
    };
};

const validateUpdateData = (data) => {
    if (!data.Name || data.Name.trim().length < 1 || data.Name.length > 100)
        return "Name must be between 1 and 100 characters.";

    const usernameRegex = /^[a-zA-Z0-9_.-]+$/;
    if (!data.userName || data.userName.trim().length < 1 || data.userName.length > 100)
        return "Username must be between 1 and 100 characters.";

    if (!usernameRegex.test(data.userName))
        return "Username can only contain letters, numbers, '.', '-', and '_' with no spaces.";

    if (data.Bio.length > 50)
        return "Bio cannot exceed 50 characters.";

    return null;
};

const UpdateProfileSubmit = async () => {
    try {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!tokenElement) {
            console.error("Antiforgery token not found!");
            return;
        }
        const token = tokenElement.value;

        UpdateUserDtoJson.Name = document.getElementById("frominput-name").value;
        UpdateUserDtoJson.userName = document.getElementById("frominput-username").value;
        UpdateUserDtoJson.Bio = document.getElementById("frominput-bio").value;

        UpdateUserDtoJson.ProfileUrl = await UploadImgProfie();

        if(UpdateUserDtoJson.ProfileUrl == null)
            UpdateUserDtoJson.ProfileUrl = document.getElementById("profile-img-display").src


        const errorMessage = validateUpdateData(UpdateUserDtoJson);
        if (errorMessage) {
            alert(errorMessage);
            return;
        }
        const res = await fetch('/api/user/update', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(UpdateUserDtoJson)
        });
        const result = await res.json();
        if (res.ok) {
            window.location.href = result.redirectUrl;
            alert("Success");
        }
        else {
            console.log(result)

            alert(result.message || "Something went wrong.");
        }
    }
    catch (e) {
        console.error(e);
        alert("Connetion failed!")
    }
}

const NameEditor = FieldEditorFactory("name", "Name");
const BioEditor = FieldEditorFactory("bio", "Bio", "flex");
const UsernameEditor = FieldEditorFactory("username", "userName", "flex");

const clickEditName = () => NameEditor.toggle();
const clickUpdateBio = () => BioEditor.toggle();
const clickUpdateUsername = () => UsernameEditor.toggle();


/** Img file */

const UploadImgProfie = async () => {
    try {
        const imageInput = document.getElementById("imageInput");
        const file = imageInput.files[0];
        if (!file) {
            console.log("No file selected");
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

        // console.log(res.json().fileName);


    }
    catch (e) {
        console.log(e);
        return null;
    }

} 

document.getElementById("imageInput").addEventListener("change", (e) => {
    if (e.target.files.length > 0) {
        const file = e.target.files[0];
                const previewUrl = URL.createObjectURL(file);
        document.getElementById("profile-img-display").src = previewUrl;
    }
});