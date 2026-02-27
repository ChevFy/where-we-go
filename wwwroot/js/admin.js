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


// Pagination state
let currentPage = 1;
let currentPageSize = 20;
let currentMeta = null;


window.onload = async function() {
    console.log('Window loaded, initializing...');
    
    initTabs();
    
    await loadUsers(1, currentPageSize);
    
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            doSearch();
        });
    }
};


async function loadUsers(page, pageSize) {
    console.log('Fetching users from /admin/users?page=' + page + '&pageSize=' + pageSize + '...');
    try {
        const response = await fetch('/admin/users?page=' + page + '&pageSize=' + pageSize);
        console.log('Response status:', response.status);
        if (response.ok) {
            const result = await response.json();
            console.log('Users loaded:', result.data.length, 'Total:', result.meta.total);
            currentMeta = result.meta;
            currentPage = result.meta.currentPage;
            renderUsers(result.data);
            renderPagination(result.meta);
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
    
    if (users.length === 0) {
        const tr = document.createElement('tr');
        const td = document.createElement('td');
        td.colSpan = 6;
        td.textContent = 'No users found';
        td.style.textAlign = 'center';
        td.style.padding = '2rem';
        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }
    
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


function renderPagination(meta) {
    let paginationContainer = document.getElementById('paginationContainer');
    if (!paginationContainer) {
        // Create pagination container if it doesn't exist
        const table = document.querySelector('.user-table');
        if (table) {
            paginationContainer = document.createElement('div');
            paginationContainer.id = 'paginationContainer';
            paginationContainer.className = 'wwg-pagination';
            table.parentNode.insertBefore(paginationContainer, table.nextSibling);
        }
    }
    
    if (!paginationContainer) return;
    
    paginationContainer.innerHTML = '';
    
    if (meta.lastPage <= 1) return;
    
    // Previous button
    const prevBtn = document.createElement('a');
    prevBtn.className = 'wwg-page-btn' + (meta.prevPage ? '' : ' is-disabled');
    prevBtn.href = meta.prevPage ? '#' : '#';
    prevBtn.setAttribute('aria-label', 'Previous page');
    prevBtn.textContent = '‹';
    if (meta.prevPage) {
        prevBtn.onclick = function(e) { e.preventDefault(); loadUsers(meta.prevPage, meta.pageSize); };
    }
    paginationContainer.appendChild(prevBtn);
    
    // Page numbers with ellipsis
    const radius = 2;
    const pages = new Set([1, meta.lastPage]);
    
    for (let p = Math.max(1, meta.currentPage - radius); p <= Math.min(meta.lastPage, meta.currentPage + radius); p++) {
        pages.add(p);
    }
    
    const sortedPages = Array.from(pages).sort(function(a, b) { return a - b; });
    let prev = null;
    
    for (let i = 0; i < sortedPages.length; i++) {
        const p = sortedPages[i];
        
        if (prev !== null && p - prev > 1) {
            const ellipsis = document.createElement('span');
            ellipsis.className = 'wwg-page-btn is-ellipsis';
            ellipsis.textContent = '...';
            paginationContainer.appendChild(ellipsis);
        }
        
        const pageBtn = document.createElement('a');
        pageBtn.className = 'wwg-page-btn' + (p === meta.currentPage ? ' is-active' : '');
        pageBtn.href = '#';
        pageBtn.textContent = p;
        pageBtn.onclick = function(e) {
            e.preventDefault();
            loadUsers(p, meta.pageSize);
        };
        paginationContainer.appendChild(pageBtn);
        
        prev = p;
    }
    
    // Next button
    const nextBtn = document.createElement('a');
    nextBtn.className = 'wwg-page-btn' + (meta.nextPage ? '' : ' is-disabled');
    nextBtn.href = meta.nextPage ? '#' : '#';
    nextBtn.setAttribute('aria-label', 'Next page');
    nextBtn.textContent = '›';
    if (meta.nextPage) {
        nextBtn.onclick = function(e) { e.preventDefault(); loadUsers(meta.nextPage, meta.pageSize); };
    }
    paginationContainer.appendChild(nextBtn);
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
    
    const requestBody = JSON.stringify({
        userId: userId,
        reason: reason,
        durationDays: durationDays
    });
    
    console.log('Sending request to /admin/users/ban...');
    
    fetch('/admin/users/ban', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: requestBody
    })
    .then(function(response) {
        console.log('Ban response status:', response.status);
        if (response.ok) {
            closeBanModal();
            loadUsers(currentPage, currentPageSize);
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
    
    const requestBody = JSON.stringify({
        userId: userId
    });
    
    fetch('/admin/users/unban', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: requestBody
    })
    .then(function(response) {
        if (response.ok) {
            loadUsers(currentPage, currentPageSize);
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
    
    console.log('Searching users with query:', query);
    try {
        // Use the same endpoint with NameFilter parameter, always go to page 1
        const url = query
            ? '/admin/users?page=1&pageSize=' + currentPageSize + '&NameFilter=' + encodeURIComponent(query)
            : '/admin/users?page=1&pageSize=' + currentPageSize;
        
        const response = await fetch(url);
        if (response.ok) {
            const result = await response.json();
            console.log('Search results:', result.data.length, 'Total:', result.meta.total);
            currentMeta = result.meta;
            currentPage = 1;
            renderUsers(result.data);
            renderPagination(result.meta);
        } else {
            console.error('Search error:', response.statusText);
        }
    } catch (error) {
        console.error('Search error:', error);
    }
}

console.log('JavaScript setup complete');
