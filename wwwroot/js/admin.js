console.log('Admin page JavaScript starting...');


function initTabs() {
    const tabs = document.querySelectorAll('.admin-tab');
    tabs.forEach(function(tab) {
        tab.addEventListener('click', function() {
            const targetTab = this.getAttribute('data-tab');
            
            tabs.forEach(function(t) {
                t.classList.remove('active');
            });
            
            this.classList.add('active');
            
            const tabContents = document.querySelectorAll('.tab-content');
            tabContents.forEach(function(content) {
                content.classList.remove('active');
            });
            
            const targetContent = document.getElementById(targetTab);
            if (targetContent) {
                targetContent.classList.add('active');
            }
        });
    });
}

console.log('Admin page JavaScript starting...');


let allUsers = [];


window.onload = function() {
    console.log('Window loaded, initializing...');
    
    initTabs();
    
    loadUsers();
    
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            doSearch();
        });
    }
};


async function loadUsers() {
    console.log('Fetching users from /Admin/GetUsers...');
    try {
        const response = await fetch('/Admin/GetUsers');
        console.log('Response status:', response.status);
        if (response.ok) {
            allUsers = await response.json();
            console.log('Users loaded:', allUsers.length);
            renderUsers(allUsers);
        } else {
            console.error('Error loading users:', response.statusText);
            alert('Error loading users. Status: ' + response.status);
        }
    } catch (error) {
        console.error('Error loading users:', error);
        alert('Error loading users: ' + error.message);
    }
}


function renderUsers(users) {
    console.log('Rendering', users.length, 'users');
    const tbody = document.getElementById('userList');
    if (!tbody) {
        console.error('userList element not found!');
        return;
    }
    
    tbody.innerHTML = '';
    
    for (let i = 0; i < users.length; i++) {
        const user = users[i];
        
        const tr = document.createElement('tr');
        
        const emailTd = document.createElement('td');
        emailTd.textContent = user.email;
        
        const nameTd = document.createElement('td');
        nameTd.textContent = user.name;
        
        const statusTd = document.createElement('td');
        const statusSpan = document.createElement('span');
        statusSpan.className = 'ban-status ' + (user.isBanned ? 'active' : 'inactive');
        statusSpan.textContent = user.isBanned ? 'Banned' : 'Active';
        statusTd.appendChild(statusSpan);
        
        const banReasonTd = document.createElement('td');
        banReasonTd.textContent = user.banReason || '-';
        
        const banExpiresTd = document.createElement('td');
        banExpiresTd.textContent = user.banExpiresAt ? new Date(user.banExpiresAt).toLocaleDateString() : '-';
        
        const actionTd = document.createElement('td');
        actionTd.className = 'action-buttons';
        
        const actionButton = document.createElement('button');
        if (user.isBanned) {
            actionButton.className = 'btn-unban';
            actionButton.textContent = 'Unban';
            actionButton.onclick = function() { unbanUser(user.id); };
        } else {
            actionButton.className = 'btn-ban';
            actionButton.textContent = 'Ban';
            actionButton.onclick = function() { openBanModal(user.id); };
        }
        actionTd.appendChild(actionButton);
        
        tr.appendChild(emailTd);
        tr.appendChild(nameTd);
        tr.appendChild(statusTd);
        tr.appendChild(banReasonTd);
        tr.appendChild(banExpiresTd);
        tr.appendChild(actionTd);
        
        tbody.appendChild(tr);
    }
    console.log('Users rendered');
}


function openBanModal(userId) {
    console.log('Opening ban modal for user:', userId);
    document.getElementById('banUserId').value = userId;
    document.getElementById('banModal').style.display = 'block';
}

function closeBanModal() {
    document.getElementById('banModal').style.display = 'none';
    document.getElementById('banForm').reset();
}


window.onclick = function(event) {
    const modal = document.getElementById('banModal');
    if (event.target == modal) {
        closeBanModal();
    }
}

function performBan() {
    console.log('Performing ban...');
    const userId = document.getElementById('banUserId').value;
    const reason = document.getElementById('banReason').value;
    const durationDays = parseInt(document.getElementById('durationDays').value);
    
    console.log('User ID:', userId);
    console.log('Reason:', reason);
    console.log('Duration:', durationDays);
    
    if (!userId) {
        alert('No user selected');
        return;
    }
    
    if (!reason || !reason.trim()) {
        alert('Please enter a ban reason');
        return;
    }
    
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        alert('Security token not found. Please refresh the page.');
        return;
    }
    const token = tokenInput.value;
    
    const formData = 'userId=' + encodeURIComponent(userId) +
                   '&reason=' + encodeURIComponent(reason) + 
                   '&durationDays=' + durationDays;
    
    console.log('Sending request to /Admin/BanUser...');
    
    fetch('/Admin/BanUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: formData
    })
    .then(function(response) {
        console.log('Ban response status:', response.status);
        if (response.ok) {
            closeBanModal();
            loadUsers();
            alert('User banned successfully!');
        } else {
            response.text().then(function(text) {
                alert('Error banning user: ' + text);
            });
        }
    })
    .catch(function(error) {
        console.error('Ban error:', error);
        alert('Error banning user: ' + error.message);
    });
}

function unbanUser(userId) {
    console.log('Unbanning user:', userId);
    if (!confirm('Are you sure you want to unban this user?')) return;
    
    fetch('/Admin/UnbanUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: 'userId=' + encodeURIComponent(userId)
    })
    .then(function(response) {
        if (response.ok) {
            loadUsers();
            alert('User unbanned successfully!');
        } else {
            alert('Error unbanning user');
        }
    })
    .catch(function(error) {
        console.error('Unban error:', error);
        alert('Error unbanning user');
    });
}


async function doSearch() {
    const searchInput = document.getElementById('searchInput');
    if (!searchInput) return;
    
    const query = searchInput.value.trim();
    
    if (query === '') {
        renderUsers(allUsers);
    } else {
        console.log('Searching users with query:', query);
        try {
            const response = await fetch('/Admin/SearchUsers?query=' + encodeURIComponent(query));
            if (response.ok) {
                const filtered = await response.json();
                console.log('Search results:', filtered.length);
                renderUsers(filtered);
            } else {
                console.error('Search error:', response.statusText);
            }
        } catch (error) {
            console.error('Search error:', error);
        }
    }
}

console.log('JavaScript setup complete');
