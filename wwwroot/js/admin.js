console.log('Admin page JavaScript starting...');

// Tab switching functionality
function initTabs() {
    var tabs = document.querySelectorAll('.admin-tab');
    tabs.forEach(function(tab) {
        tab.addEventListener('click', function() {
            var targetTab = this.getAttribute('data-tab');
            
            // Remove active class from all tabs
            tabs.forEach(function(t) {
                t.classList.remove('active');
            });
            
            // Add active class to clicked tab
            this.classList.add('active');
            
            // Hide all tab contents
            var tabContents = document.querySelectorAll('.tab-content');
            tabContents.forEach(function(content) {
                content.classList.remove('active');
            });
            
            // Show target tab content
            var targetContent = document.getElementById(targetTab);
            if (targetContent) {
                targetContent.classList.add('active');
            }
        });
    });
}

console.log('Admin page JavaScript starting...');


var allUsers = [];


window.onload = function() {
    console.log('Window loaded, initializing...');
    
    initTabs();
    
    loadUsers();
    
    var searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            doSearch();
        });
    }
};


async function loadUsers() {
    console.log('Fetching users from /Admin/GetUsers...');
    try {
        var response = await fetch('/Admin/GetUsers');
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
    var tbody = document.getElementById('userList');
    if (!tbody) {
        console.error('userList element not found!');
        return;
    }
    
    var html = '';
    for (var i = 0; i < users.length; i++) {
        var user = users[i];
        var actionButton = user.isBanned 
            ? '<button class="btn-unban" onclick="unbanUser(\'' + user.id + '\')">Unban</button>' 
            : '<button class="btn-ban" onclick="openBanModal(\'' + user.id + '\')">Ban</button>';
        
        html += '<tr>' +
            '<td>' + user.email + '</td>' +
            '<td>' + user.name + '</td>' +
            '<td><span class="ban-status ' + (user.isBanned ? 'active' : 'inactive') + '">' + 
                (user.isBanned ? 'Banned' : 'Active') + '</span></td>' +
            '<td>' + (user.banReason || '-') + '</td>' +
            '<td>' + (user.banExpiresAt ? new Date(user.banExpiresAt).toLocaleDateString() : '-') + '</td>' +
            '<td class="action-buttons">' + actionButton + '</td>' +
            '</tr>';
    }
    tbody.innerHTML = html;
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
    var modal = document.getElementById('banModal');
    if (event.target == modal) {
        closeBanModal();
    }
}

function performBan() {
    console.log('Performing ban...');
    var userId = document.getElementById('banUserId').value;
    var reason = document.getElementById('banReason').value;
    var durationDays = parseInt(document.getElementById('durationDays').value);
    
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
    
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        alert('Security token not found. Please refresh the page.');
        return;
    }
    var token = tokenInput.value;
    
    var formData = 'userId=' + encodeURIComponent(userId) + 
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


function doSearch() {
    var searchInput = document.getElementById('searchInput');
    if (!searchInput) return;
    
    var query = searchInput.value.toLowerCase().trim();
    
    if (query === '') {
        renderUsers(allUsers);
    } else {
        var filtered = allUsers.filter(function(u) {
            return u.email.toLowerCase().includes(query) || u.name.toLowerCase().includes(query);
        });
        renderUsers(filtered);
    }
}

console.log('JavaScript setup complete');
