


let UpdateUserDtoJson = {
    name : "",
    userName : "",
    bio : "",
    profileUrl : ""
};

/* Name */
let isToggleEditName = false;

const clickEditName = () => {
    const defaultName = document.getElementById("profile-name");
    const fromInputBar = document.querySelector(".frominput-name-display")
    if (!isToggleEditName)
    {
        fromInputBar.style.display = "block"
        defaultName.style.display = "none";
    }
    else 
    {
        fromInputBar.style.display = "none";
        defaultName.style.display = "block";
        
        fromInputName = document.getElementById("frominput-name")
        newValue = fromInputName.value;
        defaultName.innerText = newValue;
        UpdateUserDtoJson.name = newValue
        // console.log(UpdateUserDtoJson)
    }

    isToggleEditName = !isToggleEditName
}

/*Bio*/
let isToggleEditBio = false;

function clickUpdateBio() {
     const defaultName = document.getElementById("profile-bio");
    const fromInputBar = document.querySelector(".frominput-bio-display")
    if (!isToggleEditBio)
    {
        fromInputBar.style.display = "flex"
        defaultName.style.display = "none";
    }
    else 
    {
        fromInputBar.style.display = "none";
        defaultName.style.display = "flex";
        
        fromInputName = document.getElementById("frominput-bio")
        newValue = fromInputName.value;
        defaultName.innerText = newValue;
        UpdateUserDtoJson.bio = newValue
        // console.log(UpdateUserDtoJson)
    }

    isToggleEditBio = !isToggleEditBio


}


/* Username  */
let isToggleEditUsername = false;

function clickUpdateUsername() {
    const defaultName = document.getElementById("profile-username");
    const fromInputBar = document.querySelector(".frominput-username-display");
    const fromInputName = document.getElementById("frominput-username");

    if (!isToggleEditUsername) {
        fromInputBar.style.display = "flex"; 
        defaultName.style.display = "none";
    } else {
        fromInputBar.style.display = "none";
        defaultName.style.display = "flex";
        
        const newValue = fromInputName.value;
        defaultName.innerText = newValue;
        UpdateUserDtoJson.userName = newValue;
        
       
    }

    isToggleEditUsername = !isToggleEditUsername;
}


const UpdateProfileSubmit = async () => {
    try {
        UpdateUserDtoJson.name = document.getElementById("frominput-name").value;
        UpdateUserDtoJson.userName = document.getElementById("frominput-username").value;
        UpdateUserDtoJson.bio = document.getElementById("frominput-bio").value;
        UpdateUserDtoJson.profileUrl = document.querySelector(".profile-top img").src;

        console.log(UpdateUserDtoJson)
        const res = await fetch('/api/user/update', { 
                method: 'PUT', 
                headers: {
                    'Content-Type': 'application/json' 
                },
                body: JSON.stringify(UpdateUserDtoJson) 
            });
        if(res.ok){
            const data = await res.json();
            window.location.href = data.redirectUrl;
            alert("Success");
        }
        else
        {
            const errorDetail = await res.json();
            alert("something went wrong.")
        }
    }
    catch (e){
        console.error(e)
        alert("kak")
    }
}