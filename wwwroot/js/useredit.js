


let UpdateUserDtoJson = {
    Name : "",
    userName : "",
    Bio : "",
    ProfileUrl : ""
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
    
    if (!data.ProfileUrl || data.ProfileUrl.trim() === "") 
        return "Profile URL is required.";
    
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
        UpdateUserDtoJson.ProfileUrl = document.querySelector(".profile-top img").src;

        const errorMessage = validateUpdateData(UpdateUserDtoJson);
        if (errorMessage) {
            alert(errorMessage); 
            return; 
        }
        const res = await fetch('/api/user/update', { 
                method: 'PUT', 
                headers: {
                    'Content-Type': 'application/json' ,
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(UpdateUserDtoJson) 
            });
        const result = await res.json();
        if(res.ok){
            window.location.href = result.redirectUrl;
            alert("Success");
        }
        else
        {
            console.log(result)
            
            alert(result.message || "Something went wrong.");
        }
    }
    catch (e){
        console.error(e);
        alert("Connetion Failed!")
    }
}

const NameEditor = FieldEditorFactory("name", "name");
const BioEditor = FieldEditorFactory("bio", "bio", "flex");
const UsernameEditor = FieldEditorFactory("username", "userName", "flex");

const clickEditName = () => NameEditor.toggle();
const clickUpdateBio = () => BioEditor.toggle();
const clickUpdateUsername = () => UsernameEditor.toggle();