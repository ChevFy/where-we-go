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
        if (user.isAdmin) {
            actionButton.className = 'btn-admin';
            actionButton.textContent = 'Admin';
            actionButton.disabled = true;
            actionButton.style.opacity = '0.6';
            actionButton.style.cursor = 'not-allowed';
        } else if (user.isBanned) {
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

// ==================== Category Management ====================

async function loadCategories() {
    console.log('Fetching categories from /admin/categories...');
    try {
        const response = await fetch('/admin/categories');
        console.log('Response status:', response.status);
        if (response.ok) {
            const categories = await response.json();
            console.log('Categories loaded:', categories.length);
            renderCategories(categories);
        } else {
            console.error('Error loading categories:', response.statusText);
            alert('Error loading categories. Status: ' + response.status);
        }
    } catch (error) {
        console.error('Error loading categories:', error);
        alert('Error loading categories: ' + error.message);
    }
}

function renderCategories(categories) {
    console.log('Rendering', categories.length, 'categories');
    const tbody = document.getElementById('categoryList');
    if (!tbody) {
        console.error('categoryList element not found!');
        return;
    }
    
    tbody.innerHTML = '';
    
    if (categories.length === 0) {
        const tr = document.createElement('tr');
        const td = document.createElement('td');
        td.colSpan = 4;
        td.textContent = 'No categories found';
        td.style.textAlign = 'center';
        td.style.padding = '2rem';
        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }
    
    for (let i = 0; i < categories.length; i++) {
        const category = categories[i];
        
        const tr = document.createElement('tr');
        
        const nameTd = document.createElement('td');
        nameTd.textContent = category.name;
        nameTd.style.fontWeight = 'bold';
        
        const descTd = document.createElement('td');
        descTd.textContent = category.description || '-';
        
        const actionTd = document.createElement('td');
        actionTd.className = 'action-buttons';
        
        // Edit button
        const editButton = document.createElement('button');
        editButton.className = 'btn-edit';
        editButton.textContent = 'Edit';
        editButton.onclick = function() { openEditCategoryModal(category); };
        actionTd.appendChild(editButton);
        
        // Delete button
        const deleteButton = document.createElement('button');
        deleteButton.className = 'btn-delete';
        deleteButton.textContent = 'Delete';
        deleteButton.onclick = function() { deleteCategory(category.categoryId); };
        actionTd.appendChild(deleteButton);
        
        tr.appendChild(nameTd);
        tr.appendChild(descTd);
        tr.appendChild(actionTd);
        
        tbody.appendChild(tr);
    }
    console.log('Categories rendered');
}

function openCategoryModal() {
    console.log('Opening category modal');
    document.getElementById('categoryModal').style.display = 'block';
}

function closeCategoryModal() {
    document.getElementById('categoryModal').style.display = 'none';
    document.getElementById('categoryForm').reset();
}

window.onclick = function(event) {
    const modal = document.getElementById('banModal');
    if (event.target == modal) {
        closeBanModal();
    }
    
    const categoryModal = document.getElementById('categoryModal');
    if (event.target == categoryModal) {
        closeCategoryModal();
    }
    
    const editCategoryModal = document.getElementById('editCategoryModal');
    if (event.target == editCategoryModal) {
        closeEditCategoryModal();
    }
}

function createCategory() {
    console.log('Creating category...');
    const name = document.getElementById('categoryName').value;
    const description = document.getElementById('categoryDescription').value;
    
    console.log('Category Name:', name);
    console.log('Category Description:', description);
    
    if (!name || !name.trim()) {
        alert('Please enter a category name');
        return;
    }
    
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        alert('Security token not found. Please refresh the page.');
        return;
    }
    const token = tokenInput.value;
    
    const requestBody = JSON.stringify({
        name: name,
        description: description || null
    });
    
    console.log('Sending request to /admin/categories...');
    
    fetch('/admin/categories', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: requestBody
    })
    .then(function(response) {
        console.log('Create category response status:', response.status);
        if (response.ok) {
            closeCategoryModal();
            loadCategories();
            alert('Category created successfully!');
        } else {
            response.json().then(function(data) {
                alert('Error creating category: ' + (data.details || response.statusText));
            }).catch(function() {
                alert('Error creating category: ' + response.statusText);
            });
        }
    })
    .catch(function(error) {
        console.error('Create category error:', error);
        alert('Error creating category: ' + error.message);
    });
}

// Store the category being edited
let editingCategoryId = null;

function openEditCategoryModal(category) {
    console.log('Opening edit category modal for:', category);
    editingCategoryId = category.categoryId;
    document.getElementById('editCategoryName').value = category.name;
    document.getElementById('editCategoryDescription').value = category.description || '';
    document.getElementById('editCategoryModal').style.display = 'block';
}

function closeEditCategoryModal() {
    document.getElementById('editCategoryModal').style.display = 'none';
    document.getElementById('editCategoryForm').reset();
    editingCategoryId = null;
}

function updateCategory() {
    console.log('Updating category...');
    const name = document.getElementById('editCategoryName').value;
    const description = document.getElementById('editCategoryDescription').value;
    
    console.log('Category ID:', editingCategoryId);
    console.log('Category Name:', name);
    console.log('Category Description:', description);
    
    if (!editingCategoryId) {
        alert('No category selected for editing');
        return;
    }
    
    if (!name || !name.trim()) {
        alert('Please enter a category name');
        return;
    }
    
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        alert('Security token not found. Please refresh the page.');
        return;
    }
    const token = tokenInput.value;
    
    const requestBody = JSON.stringify({
        name: name,
        description: description || null
    });
    
    console.log('Sending request to /admin/categories/' + editingCategoryId + '...');
    
    fetch('/admin/categories/' + editingCategoryId, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: requestBody
    })
    .then(function(response) {
        console.log('Update category response status:', response.status);
        if (response.ok) {
            closeEditCategoryModal();
            loadCategories();
            alert('Category updated successfully!');
        } else {
            response.json().then(function(data) {
                alert('Error updating category: ' + (data.details || response.statusText));
            }).catch(function() {
                alert('Error updating category: ' + response.statusText);
            });
        }
    })
    .catch(function(error) {
        console.error('Update category error:', error);
        alert('Error updating category: ' + error.message);
    });
}

function deleteCategory(categoryId) {
    console.log('Deleting category:', categoryId);
    if (!confirm('Are you sure you want to delete this category?')) return;
    
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        alert('Security token not found. Please refresh the page.');
        return;
    }
    const token = tokenInput.value;
    
    fetch('/admin/categories/' + categoryId, {
        method: 'DELETE',
        headers: {
            'RequestVerificationToken': token
        }
    })
    .then(function(response) {
        console.log('Delete category response status:', response.status);
        if (response.ok) {
            loadCategories();
            alert('Category deleted successfully!');
        } else {
            response.json().then(function(data) {
                alert('Error deleting category: ' + (data.details || response.statusText));
            }).catch(function() {
                alert('Error deleting category: ' + response.statusText);
            });
        }
    })
    .catch(function(error) {
        console.error('Delete category error:', error);
        alert('Error deleting category: ' + error.message);
    });
}

// Override initTabs to load categories when the tab is clicked
const originalInitTabs = initTabs;
initTabs = function() {
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
            
            // Load categories when the categories tab is clicked
            if (targetTab === 'categories') {
                loadCategories();
            }
            
            // Load posts when the posts tab is clicked
            if (targetTab === 'posts') {
                loadPosts(1, 20);
            }
        });
    });
};

// ==================== Post Management ====================

let currentPostPage = 1;
let currentPostPageSize = 20;

async function loadPosts(page, pageSize) {
    console.log('Fetching posts from /admin/posts?page=' + page + '&pageSize=' + pageSize + '...');
    const searchQuery = document.getElementById('postSearchInput')?.value || '';
    const statusFilter = document.getElementById('postStatusFilter')?.value || '';
    
    let url = '/admin/posts?page=' + page + '&pageSize=' + pageSize;
    if (searchQuery) url += '&NameFilter=' + encodeURIComponent(searchQuery);
    if (statusFilter) url += '&StatusFilter=' + statusFilter;
    
    try {
        const response = await fetch(url);
        console.log('Response status:', response.status);
        if (response.ok) {
            const result = await response.json();
            console.log('Posts loaded:', result.data.length, 'Total:', result.meta.total);
            currentPostPage = result.meta.currentPage;
            renderPosts(result.data);
            renderPostPagination(result.meta);
        } else {
            console.error('Error loading posts:', response.statusText);
            alert('Error loading posts. Status: ' + response.status);
        }
    } catch (error) {
        console.error('Error loading posts:', error);
        alert('Error loading posts: ' + error.message);
    }
}

function renderPosts(posts) {
    console.log('Rendering', posts.length, 'posts');
    const tbody = document.getElementById('postList');
    if (!tbody) {
        console.error('postList element not found!');
        return;
    }
    
    tbody.innerHTML = '';
    
    if (posts.length === 0) {
        const tr = document.createElement('tr');
        const td = document.createElement('td');
        td.colSpan = 6;
        td.textContent = 'No posts found';
        td.style.textAlign = 'center';
        td.style.padding = '2rem';
        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }
    
    for (let i = 0; i < posts.length; i++) {
        const post = posts[i];
        
        const tr = document.createElement('tr');
        
        // Title
        const titleTd = document.createElement('td');
        titleTd.textContent = post.title;
        titleTd.style.maxWidth = '200px';
        titleTd.style.overflow = 'hidden';
        titleTd.style.textOverflow = 'ellipsis';
        titleTd.style.whiteSpace = 'nowrap';
        
        // Owner
        const ownerTd = document.createElement('td');
        ownerTd.textContent = post.ownerName || post.ownerEmail;
        
        // Status
        const statusTd = document.createElement('td');
        const statusSpan = document.createElement('span');
        statusSpan.className = 'post-status ' + post.status.toLowerCase();
        statusSpan.textContent = post.status;
        statusTd.appendChild(statusSpan);
        
        // Participants
        const participantsTd = document.createElement('td');
        participantsTd.textContent = post.currentParticipants + '/' + post.maxParticipants;
        
        // Event Date
        const eventDateTd = document.createElement('td');
        const eventDate = new Date(post.eventDate);
        eventDateTd.textContent = eventDate.toLocaleDateString('th-TH');
        
        // Actions
        const actionTd = document.createElement('td');
        actionTd.className = 'action-buttons';
        
        // Details button - always visible
        const detailsBtn = document.createElement('button');
        detailsBtn.className = 'btn-details';
        detailsBtn.textContent = 'Details';
        detailsBtn.onclick = function() { openPostDetailModal(post.postId); };
        actionTd.appendChild(detailsBtn);
        
        tr.appendChild(titleTd);
        tr.appendChild(ownerTd);
        tr.appendChild(statusTd);
        tr.appendChild(participantsTd);
        tr.appendChild(eventDateTd);
        tr.appendChild(actionTd);
        
        tbody.appendChild(tr);
    }
    console.log('Posts rendered');
}

function renderPostPagination(meta) {
    let paginationContainer = document.getElementById('postPaginationContainer');
    if (!paginationContainer) {
        const table = document.querySelector('.post-table');
        if (table) {
            paginationContainer = document.createElement('div');
            paginationContainer.id = 'postPaginationContainer';
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
        prevBtn.onclick = function(e) { e.preventDefault(); loadPosts(meta.prevPage, meta.pageSize); };
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
            loadPosts(p, meta.pageSize);
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
        nextBtn.onclick = function(e) { e.preventDefault(); loadPosts(meta.nextPage, meta.pageSize); };
    }
    paginationContainer.appendChild(nextBtn);
}

function deletePost(postId) {
    console.log('Deleting post:', postId);
    if (!confirm('Are you sure you want to delete this post?')) return;
    
    fetch('/admin/posts/' + postId + '/delete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
    .then(function(response) {
        console.log('Delete post response status:', response.status);
        if (response.ok) {
            loadPosts(currentPostPage, currentPostPageSize);
            alert('Post deleted successfully!');
        } else {
            response.text().then(function(text) {
                alert('Error deleting post: ' + text);
            });
        }
    })
    .catch(function(error) {
        console.error('Delete post error:', error);
        alert('Error deleting post: ' + error.message);
    });
}

// Store current post details for actions
let currentPostDetailId = null;

async function openPostDetailModal(postId) {
    console.log('Opening post detail modal for:', postId);
    currentPostDetailId = postId;
    
    const modal = document.getElementById('postDetailModal');
    const contentDiv = document.getElementById('postDetailContent');
    const actionsDiv = document.getElementById('postDetailActions');
    
    contentDiv.innerHTML = '<div class="loading">Loading...</div>';
    actionsDiv.innerHTML = '';
    modal.style.display = 'block';
    
    try {
        const response = await fetch('/admin/posts/' + postId);
        if (response.ok) {
            const post = await response.json();
            renderPostDetail(post);
        } else {
            contentDiv.innerHTML = '<div class="error">Error loading post details</div>';
        }
    } catch (error) {
        console.error('Error loading post detail:', error);
        contentDiv.innerHTML = '<div class="error">Error loading post details: ' + error.message + '</div>';
    }
}

function closePostDetailModal() {
    document.getElementById('postDetailModal').style.display = 'none';
    currentPostDetailId = null;
}

function renderPostDetail(post) {
    const contentDiv = document.getElementById('postDetailContent');
    const actionsDiv = document.getElementById('postDetailActions');
    
    // Format dates
    const eventDate = new Date(post.eventDate).toLocaleString('th-TH');
    const deadlineDate = new Date(post.dateDeadline).toLocaleString('th-TH');
    const createdDate = new Date(post.dateCreated).toLocaleString('th-TH');
    
    // Build categories HTML
    let categoriesHtml = '';
    if (post.categories && post.categories.length > 0) {
        categoriesHtml = '<div class="category-tags">' +
            post.categories.map(c => '<span class="category-tag">' + c.name + '</span>').join('') +
            '</div>';
    } else {
        categoriesHtml = '<span style="color: #666;">No categories</span>';
    }
    
    // Build participants HTML
    let participantsHtml = '';
    if (post.participants && post.participants.length > 0) {
        participantsHtml = '<div class="participant-list">' +
            post.participants.map(p => `
                <div class="participant-item">
                    <div class="participant-info">
                        <span class="participant-name">${p.userName || 'Unknown'}</span>
                        <span class="participant-email">${p.userEmail || ''}</span>
                    </div>
                    <span class="participant-status ${p.status}">${p.status}</span>
                </div>
            `).join('') +
            '</div>';
    } else {
        participantsHtml = '<span style="color: #666;">No participants yet</span>';
    }
    
    // Build content HTML
    contentDiv.innerHTML = `
        <div class="post-detail-section">
            <h4>Basic Information</h4>
            <div class="post-detail-grid">
                <div class="post-detail-item">
                    <span class="post-detail-label">Title</span>
                    <span class="post-detail-value">${post.title}</span>
                </div>
                <div class="post-detail-item">
                    <span class="post-detail-label">Status</span>
                    <span class="post-detail-value"><span class="post-status ${post.status.toLowerCase()}">${post.status}</span></span>
                </div>
                <div class="post-detail-item">
                    <span class="post-detail-label">Owner</span>
                    <span class="post-detail-value">${post.ownerName || post.ownerEmail}</span>
                </div>
                <div class="post-detail-item">
                    <span class="post-detail-label">Owner Email</span>
                    <span class="post-detail-value">${post.ownerEmail}</span>
                </div>
            </div>
        </div>
        
        <div class="post-detail-section">
            <h4>Description</h4>
            <p class="post-detail-description">${post.description || 'No description'}</p>
        </div>
        
        <div class="post-detail-section">
            <h4>Categories</h4>
            ${categoriesHtml}
        </div>
        
        <div class="post-detail-section">
            <h4>Event Details</h4>
            <div class="post-detail-grid">
                <div class="post-detail-item">
                    <span class="post-detail-label">Location</span>
                    <span class="post-detail-value">${post.locationName}</span>
                </div>
                <div class="post-detail-item">
                    <span class="post-detail-label">Event Date</span>
                    <span class="post-detail-value">${eventDate}</span>
                </div>
                <div class="post-detail-item">
                    <span class="post-detail-label">Deadline</span>
                    <span class="post-detail-value">${deadlineDate}</span>
                </div>
                <div class="post-detail-item">
                    <span class="post-detail-label">Created</span>
                    <span class="post-detail-value">${createdDate}</span>
                </div>
            </div>
        </div>
        
        <div class="post-detail-section">
            <h4>Participants (${post.currentParticipants}/${post.maxParticipants})</h4>
            ${participantsHtml}
        </div>
        
        <div class="post-detail-section">
            <h4>Additional Info</h4>
            <div class="post-detail-grid">
                <div class="post-detail-item">
                    <span class="post-detail-label">Min Participants</span>
                    <span class="post-detail-value">${post.minParticipants}</span>
                </div>
                <div class="post-detail-item">
                    <span class="post-detail-label">Max Participants</span>
                    <span class="post-detail-value">${post.maxParticipants}</span>
                </div>
                <div class="post-detail-item">
                    <span class="post-detail-label">Invite Code</span>
                    <span class="post-detail-value">${post.inviteCode || 'N/A'}</span>
                </div>
                <div class="post-detail-item">
                    <span class="post-detail-label">Post ID</span>
                    <span class="post-detail-value" style="font-size: 0.75rem;">${post.postId}</span>
                </div>
            </div>
        </div>
    `;
    
    // Build action buttons based on status
    if (post.status === 'Delete') {
        actionsDiv.innerHTML = `
            <button type="button" class="btn-cancel" onclick="closePostDetailModal()">Close</button>
            <button type="button" class="btn-restore" onclick="restorePostFromModal()">Restore Post</button>
        `;
    } else {
        actionsDiv.innerHTML = `
            <button type="button" class="btn-cancel" onclick="closePostDetailModal()">Close</button>
            <button type="button" class="btn-delete" onclick="deletePostFromModal()">Delete Post</button>
        `;
    }
}

function deletePostFromModal() {
    if (!currentPostDetailId) return;
    
    if (!confirm('Are you sure you want to delete this post?')) return;
    
    fetch('/admin/posts/' + currentPostDetailId + '/delete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
    .then(function(response) {
        console.log('Delete post response status:', response.status);
        if (response.ok) {
            closePostDetailModal();
            loadPosts(currentPostPage, currentPostPageSize);
            alert('Post deleted successfully!');
        } else {
            response.text().then(function(text) {
                alert('Error deleting post: ' + text);
            });
        }
    })
    .catch(function(error) {
        console.error('Delete post error:', error);
        alert('Error deleting post: ' + error.message);
    });
}

function restorePostFromModal() {
    if (!currentPostDetailId) return;
    
    fetch('/admin/posts/' + currentPostDetailId + '/restore', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
    .then(function(response) {
        console.log('Restore post response status:', response.status);
        if (response.ok) {
            closePostDetailModal();
            loadPosts(currentPostPage, currentPostPageSize);
            alert('Post restored successfully!');
        } else {
            response.text().then(function(text) {
                alert('Error restoring post: ' + text);
            });
        }
    })
    .catch(function(error) {
        console.error('Restore post error:', error);
        alert('Error restoring post: ' + error.message);
    });
}

// Update window.onclick to handle post detail modal
const originalWindowOnclick = window.onclick;
window.onclick = function(event) {
    // Call original handlers
    if (originalWindowOnclick) {
        originalWindowOnclick(event);
    }
    
    const postDetailModal = document.getElementById('postDetailModal');
    if (event.target == postDetailModal) {
        closePostDetailModal();
    }
};

async function doPostSearch() {
    const searchInput = document.getElementById('postSearchInput');
    if (!searchInput) return;
    
    const query = searchInput.value.trim();
    
    console.log('Searching posts with query:', query);
    try {
        const statusFilter = document.getElementById('postStatusFilter')?.value || '';
        const url = query
            ? '/admin/posts?page=1&pageSize=' + currentPostPageSize + '&NameFilter=' + encodeURIComponent(query) + (statusFilter ? '&StatusFilter=' + statusFilter : '')
            : '/admin/posts?page=1&pageSize=' + currentPostPageSize + (statusFilter ? '&StatusFilter=' + statusFilter : '');
        
        const response = await fetch(url);
        if (response.ok) {
            const result = await response.json();
            console.log('Search results:', result.data.length, 'Total:', result.meta.total);
            currentPostPage = 1;
            renderPosts(result.data);
            renderPostPagination(result.meta);
        } else {
            console.error('Search error:', response.statusText);
        }
    } catch (error) {
        console.error('Search error:', error);
    }
}

// Add search input listener for posts
const originalOnload = window.onload;
window.onload = async function() {
    if (originalOnload) {
        await originalOnload();
    }
    
    const postSearchInput = document.getElementById('postSearchInput');
    if (postSearchInput) {
        postSearchInput.addEventListener('input', function() {
            doPostSearch();
        });
    }
};

console.log('JavaScript setup complete');
