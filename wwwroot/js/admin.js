// ==================== State ====================
const state = {
    currentPage: 1,
    currentPageSize: 20,
    currentMeta: null,
    currentPostPage: 1,
    currentPostPageSize: 20,
    currentPostMeta: null,
    editingCategoryId: null,
    currentPostDetailId: null,
    editingPostData: null
};

// ==================== Utility Functions ====================

// Shared pagination renderer
function renderPagination(meta, containerId, tableSelector, loadFn) {
    let container = document.getElementById(containerId);
    if (!container) {
        const table = document.querySelector(tableSelector);
        if (table) {
            container = document.createElement('div');
            container.id = containerId;
            container.className = 'wwg-pagination';
            table.parentNode.insertBefore(container, table.nextSibling);
        }
    }
    if (!container) return;

    container.innerHTML = '';
    if (meta.lastPage <= 1) return;

    // Previous button
    const prevBtn = createPaginationButton('‹', meta.prevPage, () => loadFn(meta.prevPage, meta.pageSize), !meta.prevPage);
    container.appendChild(prevBtn);

    // Page numbers with ellipsis
    const radius = 2;
    const pages = new Set([1, meta.lastPage]);
    for (let p = Math.max(1, meta.currentPage - radius); p <= Math.min(meta.lastPage, meta.currentPage + radius); p++) {
        pages.add(p);
    }

    const sortedPages = Array.from(pages).sort((a, b) => a - b);
    let prev = null;

    for (const p of sortedPages) {
        if (prev !== null && p - prev > 1) {
            const ellipsis = document.createElement('span');
            ellipsis.className = 'wwg-page-btn is-ellipsis';
            ellipsis.textContent = '...';
            container.appendChild(ellipsis);
        }

        const pageBtn = createPaginationButton(p, p, () => loadFn(p, meta.pageSize), false, p === meta.currentPage);
        container.appendChild(pageBtn);
        prev = p;
    }

    // Next button
    const nextBtn = createPaginationButton('›', meta.nextPage, () => loadFn(meta.nextPage, meta.pageSize), !meta.nextPage);
    container.appendChild(nextBtn);
}

function createPaginationButton(text, pageNum, onClick, isDisabled, isActive = false) {
    const btn = document.createElement('a');
    btn.className = 'wwg-page-btn' + (isDisabled ? ' is-disabled' : '') + (isActive ? ' is-active' : '');
    btn.href = '#';
    btn.textContent = text;
    btn.setAttribute('aria-label', pageNum ? `Page ${pageNum}` : text);
    if (!isDisabled && onClick) {
        btn.onclick = (e) => { e.preventDefault(); onClick(); };
    }
    return btn;
}

// Shared modal handler
function setupModalClose(modalId, closeFn) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal._closeFn = closeFn;
    }
}

// Get CSRF token
function getCsrfToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        console.error('CSRF token not found');
        return null;
    }
    return tokenInput.value;
}

// Handle fetch response
async function handleResponse(response, successMsg) {
    if (response.ok) {
        if (successMsg) alert(successMsg);
        return true;
    }
    const text = await response.text();
    alert(`Error: ${text}`);
    return false;
}

// ==================== Tab Management ====================

function initTabs() {
    const tabs = document.querySelectorAll('.admin-tab');
    tabs.forEach(tab => {
        tab.addEventListener('click', function() {
            const targetTab = this.getAttribute('data-tab');
            
            tabs.forEach(t => t.classList.remove('active'));
            this.classList.add('active');
            
            document.querySelectorAll('.tab-content').forEach(content => {
                content.classList.remove('active');
            });
            
            const targetContent = document.getElementById(targetTab);
            if (targetContent) {
                targetContent.classList.add('active');
            }

            // Load data for specific tabs
            if (targetTab === 'categories') loadCategories();
            if (targetTab === 'posts') loadPosts(1, 20);
        });
    });
}

// ==================== User Management ====================

async function loadUsers(page, pageSize) {
    try {
        const response = await fetch(`/admin/users?page=${page}&pageSize=${pageSize}`);
        if (response.ok) {
            const result = await response.json();
            state.currentMeta = result.meta;
            state.currentPage = result.meta.currentPage;
            renderUsers(result.data);
            renderPagination(result.meta, 'paginationContainer', '.user-table', loadUsers);
        } else {
            console.error('Error loading users:', response.statusText);
        }
    } catch (error) {
        console.error('Error loading users:', error);
    }
}

function renderUsers(users) {
    const tbody = document.getElementById('userList');
    if (!tbody) return;

    tbody.innerHTML = '';

    if (users.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;padding:2rem">No users found</td></tr>`;
        return;
    }

    tbody.innerHTML = users.map(user => `
        <tr>
            <td>${user.email}</td>
            <td>${user.name}</td>
            <td><span class="ban-status ${user.isBanned ? 'active' : 'inactive'}">${user.isBanned ? 'Banned' : 'Active'}</span></td>
            <td>${user.banReason || '-'}</td>
            <td>${user.banExpiresAt ? new Date(user.banExpiresAt).toLocaleDateString() : '-'}</td>
            <td class="action-buttons">${renderUserActionButton(user)}</td>
        </tr>
    `).join('');
}

function renderUserActionButton(user) {
    if (user.isAdmin) {
        return `<button class="btn-admin" disabled style="opacity:0.6;cursor:not-allowed">Admin</button>`;
    }
    if (user.isBanned) {
        return `<button class="btn-unban" onclick="unbanUser('${user.id}')">Unban</button>`;
    }
    return `<button class="btn-ban" onclick="openBanModal('${user.id}')">Ban</button>`;
}

function openBanModal(userId) {
    document.getElementById('banUserId').value = userId;
    document.getElementById('banModal').style.display = 'block';
}

function closeBanModal() {
    document.getElementById('banModal').style.display = 'none';
    document.getElementById('banForm').reset();
}

function performBan() {
    const userId = document.getElementById('banUserId').value;
    const reason = document.getElementById('banReason').value;
    const durationDays = parseInt(document.getElementById('durationDays').value);
    const token = getCsrfToken();

    if (!userId || !reason?.trim()) {
        alert('Please enter a ban reason');
        return;
    }
    if (!token) {
        alert('Security token not found. Please refresh the page.');
        return;
    }

    fetch('/admin/users/ban', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
        body: JSON.stringify({ userId, reason, durationDays })
    })
    .then(response => {
        if (response.ok) {
            closeBanModal();
            loadUsers(state.currentPage, state.currentPageSize);
            alert('User banned successfully!');
        } else {
            response.text().then(text => alert('Error banning user: ' + text));
        }
    })
    .catch(error => {
        console.error('Ban error:', error);
        alert('Error banning user: ' + error.message);
    });
}

function unbanUser(userId) {
    if (!confirm('Are you sure you want to unban this user?')) return;
    const token = getCsrfToken();
    if (!token) {
        alert('Security token not found. Please refresh the page.');
        return;
    }

    fetch('/admin/users/unban', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
        body: JSON.stringify({ userId })
    })
    .then(response => {
        if (response.ok) {
            loadUsers(state.currentPage, state.currentPageSize);
            alert('User unbanned successfully!');
        } else {
            alert('Error unbanning user');
        }
    })
    .catch(error => {
        console.error('Unban error:', error);
        alert('Error unbanning user');
    });
}

async function doSearch() {
    const query = document.getElementById('searchInput')?.value?.trim() || '';
    const url = query
        ? `/admin/users?page=1&pageSize=${state.currentPageSize}&NameFilter=${encodeURIComponent(query)}`
        : `/admin/users?page=1&pageSize=${state.currentPageSize}`;

    try {
        const response = await fetch(url);
        if (response.ok) {
            const result = await response.json();
            state.currentMeta = result.meta;
            state.currentPage = 1;
            renderUsers(result.data);
            renderPagination(result.meta, 'paginationContainer', '.user-table', loadUsers);
        }
    } catch (error) {
        console.error('Search error:', error);
    }
}

// ==================== Category Management ====================

async function loadCategories() {
    try {
        const response = await fetch('/admin/categories');
        if (response.ok) {
            const categories = await response.json();
            renderCategories(categories);
        }
    } catch (error) {
        console.error('Error loading categories:', error);
    }
}

function renderCategories(categories) {
    const tbody = document.getElementById('categoryList');
    if (!tbody) return;

    tbody.innerHTML = '';

    if (categories.length === 0) {
        tbody.innerHTML = `<tr><td colspan="4" style="text-align:center;padding:2rem">No categories found</td></tr>`;
        return;
    }

    tbody.innerHTML = categories.map(category => `
        <tr>
            <td style="font-weight:bold">${category.name}</td>
            <td>${category.description || '-'}</td>
            <td class="action-buttons">
                <button class="btn-edit" onclick='openEditCategoryModal(${JSON.stringify(category)})'>Edit</button>
                <button class="btn-delete" onclick="deleteCategory('${category.categoryId}')">Delete</button>
            </td>
        </tr>
    `).join('');
}

function openCategoryModal() {
    document.getElementById('categoryModal').style.display = 'block';
}

function closeCategoryModal() {
    document.getElementById('categoryModal').style.display = 'none';
    document.getElementById('categoryForm').reset();
}

function createCategory() {
    const name = document.getElementById('categoryName').value;
    const description = document.getElementById('categoryDescription').value;
    const token = getCsrfToken();

    if (!name?.trim()) {
        alert('Please enter a category name');
        return;
    }
    if (!token) return;

    fetch('/admin/categories', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
        body: JSON.stringify({ name, description: description || null })
    })
    .then(response => {
        if (response.ok) {
            closeCategoryModal();
            loadCategories();
            alert('Category created successfully!');
        } else {
            response.json().then(data => alert('Error: ' + (data.details || response.statusText)))
                .catch(() => alert('Error: ' + response.statusText));
        }
    })
    .catch(error => {
        console.error('Create category error:', error);
        alert('Error: ' + error.message);
    });
}

function openEditCategoryModal(category) {
    state.editingCategoryId = category.categoryId;
    document.getElementById('editCategoryName').value = category.name;
    document.getElementById('editCategoryDescription').value = category.description || '';
    document.getElementById('editCategoryModal').style.display = 'block';
}

function closeEditCategoryModal() {
    document.getElementById('editCategoryModal').style.display = 'none';
    document.getElementById('editCategoryForm').reset();
    state.editingCategoryId = null;
}

function updateCategory() {
    const name = document.getElementById('editCategoryName').value;
    const description = document.getElementById('editCategoryDescription').value;
    const token = getCsrfToken();

    if (!state.editingCategoryId || !name?.trim()) {
        alert('Please enter a category name');
        return;
    }
    if (!token) return;

    fetch(`/admin/categories/${state.editingCategoryId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
        body: JSON.stringify({ name, description: description || null })
    })
    .then(response => {
        if (response.ok) {
            closeEditCategoryModal();
            loadCategories();
            alert('Category updated successfully!');
        } else {
            response.json().then(data => alert('Error: ' + (data.details || response.statusText)))
                .catch(() => alert('Error: ' + response.statusText));
        }
    })
    .catch(error => {
        console.error('Update category error:', error);
        alert('Error: ' + error.message);
    });
}

function deleteCategory(categoryId) {
    if (!confirm('Are you sure you want to delete this category?')) return;
    const token = getCsrfToken();
    if (!token) return;

    fetch(`/admin/categories/${categoryId}`, {
        method: 'DELETE',
        headers: { 'RequestVerificationToken': token }
    })
    .then(response => {
        if (response.ok) {
            loadCategories();
            alert('Category deleted successfully!');
        } else {
            response.json().then(data => alert('Error: ' + (data.details || response.statusText)))
                .catch(() => alert('Error: ' + response.statusText));
        }
    })
    .catch(error => {
        console.error('Delete category error:', error);
        alert('Error: ' + error.message);
    });
}

// ==================== Post Management ====================

async function loadPosts(page, pageSize) {
    const searchQuery = document.getElementById('postSearchInput')?.value || '';
    const statusFilter = document.getElementById('postStatusFilter')?.value || '';

    let url = `/admin/posts?page=${page}&pageSize=${pageSize}`;
    if (searchQuery) url += `&NameFilter=${encodeURIComponent(searchQuery)}`;
    if (statusFilter) url += `&StatusFilter=${statusFilter}`;

    try {
        const response = await fetch(url);
        if (response.ok) {
            const result = await response.json();
            state.currentPostPage = result.meta.currentPage;
            state.currentPostMeta = result.meta;
            renderPosts(result.data);
            renderPagination(result.meta, 'postPaginationContainer', '.post-table', loadPosts);
        }
    } catch (error) {
        console.error('Error loading posts:', error);
    }
}

function renderPosts(posts) {
    const tbody = document.getElementById('postList');
    if (!tbody) return;

    tbody.innerHTML = '';

    if (posts.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;padding:2rem">No posts found</td></tr>`;
        return;
    }

    tbody.innerHTML = posts.map(post => {
        const eventDate = new Date(post.eventDate).toLocaleDateString('th-TH');
        return `
            <tr>
                <td style="max-width:200px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap">${post.title}</td>
                <td>${post.ownerName || post.ownerEmail}</td>
                <td><span class="post-status ${post.status.toLowerCase()}">${post.status}</span></td>
                <td>${post.currentParticipants}/${post.maxParticipants}</td>
                <td>${eventDate}</td>
                <td class="action-buttons">
                    <button class="btn-details" onclick="openPostDetailModal('${post.postId}')">Details</button>
                </td>
            </tr>
        `;
    }).join('');
}

async function openPostDetailModal(postId) {
    state.currentPostDetailId = postId;
    const modal = document.getElementById('postDetailModal');
    const contentDiv = document.getElementById('postDetailContent');
    const actionsDiv = document.getElementById('postDetailActions');

    contentDiv.innerHTML = '<div class="loading">Loading...</div>';
    actionsDiv.innerHTML = '';
    modal.style.display = 'block';

    try {
        const response = await fetch(`/admin/posts/${postId}`);
        if (response.ok) {
            const post = await response.json();
            renderPostDetail(post);
        } else {
            contentDiv.innerHTML = '<div class="error">Error loading post details</div>';
        }
    } catch (error) {
        console.error('Error loading post detail:', error);
        contentDiv.innerHTML = `<div class="error">Error: ${error.message}</div>`;
    }
}

function closePostDetailModal() {
    document.getElementById('postDetailModal').style.display = 'none';
    state.currentPostDetailId = null;
}

function renderPostDetail(post) {
    const contentDiv = document.getElementById('postDetailContent');
    const actionsDiv = document.getElementById('postDetailActions');

    const eventDate = new Date(post.eventDate).toLocaleString('th-TH');
    const deadlineDate = new Date(post.dateDeadline).toLocaleString('th-TH');
    const createdDate = new Date(post.dateCreated).toLocaleString('th-TH');

    const categoriesHtml = post.categories?.length > 0
        ? `<div class="category-tags">${post.categories.map(c => `<span class="category-tag">${c.name}</span>`).join('')}</div>`
        : '<span style="color:#666">No categories</span>';

    const participantsHtml = post.participants?.length > 0
        ? `<div class="participant-list">${post.participants.map(p => `
            <div class="participant-item">
                <div class="participant-info">
                    <span class="participant-name">${p.userName || 'Unknown'}</span>
                    <span class="participant-email">${p.userEmail || ''}</span>
                </div>
                <span class="participant-status ${p.status}">${p.status}</span>
            </div>
        `).join('')}</div>`
        : '<span style="color:#666">No participants yet</span>';

    contentDiv.innerHTML = `
        <div class="post-detail-section">
            <h4>Basic Information</h4>
            <div class="post-detail-grid">
                <div class="post-detail-item"><span class="post-detail-label">Title</span><span class="post-detail-value">${post.title}</span></div>
                <div class="post-detail-item"><span class="post-detail-label">Status</span><span class="post-detail-value"><span class="post-status ${post.status.toLowerCase()}">${post.status}</span></span></div>
                <div class="post-detail-item"><span class="post-detail-label">Owner</span><span class="post-detail-value">${post.ownerName || post.ownerEmail}</span></div>
                <div class="post-detail-item"><span class="post-detail-label">Owner Email</span><span class="post-detail-value">${post.ownerEmail}</span></div>
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
                <div class="post-detail-item"><span class="post-detail-label">Location</span><span class="post-detail-value">${post.locationName}</span></div>
                <div class="post-detail-item"><span class="post-detail-label">Event Date</span><span class="post-detail-value">${eventDate}</span></div>
                <div class="post-detail-item"><span class="post-detail-label">Deadline</span><span class="post-detail-value">${deadlineDate}</span></div>
                <div class="post-detail-item"><span class="post-detail-label">Created</span><span class="post-detail-value">${createdDate}</span></div>
            </div>
        </div>
        <div class="post-detail-section">
            <h4>Participants (${post.currentParticipants}/${post.maxParticipants})</h4>
            ${participantsHtml}
        </div>
        <div class="post-detail-section">
            <h4>Additional Info</h4>
            <div class="post-detail-grid">
                <div class="post-detail-item"><span class="post-detail-label">Min Participants</span><span class="post-detail-value">${post.minParticipants}</span></div>
                <div class="post-detail-item"><span class="post-detail-label">Max Participants</span><span class="post-detail-value">${post.maxParticipants}</span></div>
                <div class="post-detail-item"><span class="post-detail-label">Invite Code</span><span class="post-detail-value">${post.inviteCode || 'N/A'}</span></div>
                <div class="post-detail-item"><span class="post-detail-label">Post ID</span><span class="post-detail-value" style="font-size:0.75rem">${post.postId}</span></div>
            </div>
        </div>
    `;

    window.currentPostForEdit = post;

    // Check if deadline has passed
    const deadlineDateObj = new Date(post.dateDeadline);
    const now = new Date();
    const isDeadlinePassed = deadlineDateObj < now;

    if (post.status === 'Cancelled') {
        const isRestoreDisabled = isDeadlinePassed;
        actionsDiv.innerHTML = `
            <button type="button" class="btn-cancel" onclick="closePostDetailModal()">Close</button>
            <button type="button" class="btn-restore" ${isRestoreDisabled ? 'disabled style="opacity:0.5;cursor:not-allowed" title="Cannot restore: deadline has passed"' : ''} onclick="${isRestoreDisabled ? '' : 'restorePostFromModal()'}">Restore Post</button>
        `;
    } else if (post.status === 'Completed') {
        // Completed posts are read-only - no edit/cancel allowed
        actionsDiv.innerHTML = `
            <button type="button" class="btn-cancel" onclick="closePostDetailModal()">Close</button>
        `;
    } else {
        actionsDiv.innerHTML = `
            <button type="button" class="btn-cancel" onclick="closePostDetailModal()">Close</button>
            <button type="button" class="btn-edit" onclick="closePostDetailModal();openEditPostModal(window.currentPostForEdit)">Edit Post</button>
            <button type="button" class="btn-delete" onclick="deletePostFromModal()">Cancel</button>
        `;
    }
}

function deletePostFromModal() {
    if (!state.currentPostDetailId || !confirm('Are you sure you want to cancel this post?')) return;
    const token = getCsrfToken();
    if (!token) {
        alert('Security token not found. Please refresh the page.');
        return;
    }

    fetch(`/admin/posts/${state.currentPostDetailId}/cancel`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token }
    })
    .then(response => {
        if (response.ok) {
            closePostDetailModal();
            loadPosts(state.currentPostPage, state.currentPostPageSize);
            alert('Post cancelled successfully!');
        } else {
            response.text().then(text => alert('Error: ' + text));
        }
    })
    .catch(error => console.error('Delete post error:', error));
}

function restorePostFromModal() {
    if (!state.currentPostDetailId) return;
    const token = getCsrfToken();
    if (!token) {
        alert('Security token not found. Please refresh the page.');
        return;
    }

    fetch(`/admin/posts/${state.currentPostDetailId}/restore`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token }
    })
    .then(response => {
        if (response.ok) {
            closePostDetailModal();
            loadPosts(state.currentPostPage, state.currentPostPageSize);
            alert('Post restored successfully!');
        } else {
            response.text().then(text => alert('Error: ' + text));
        }
    })
    .catch(error => console.error('Restore post error:', error));
}

// ==================== Post Edit ====================

function openEditPostModal(post) {
    state.editingPostData = post;

    document.getElementById('editPostId').value = post.postId;
    document.getElementById('editPostMinParticipants').value = post.minParticipants || 1;
    document.getElementById('editPostMaxParticipants').value = post.maxParticipants || 10;

    loadCategoriesForEdit(post);

    const participantsContainer = document.getElementById('editPostParticipants');
    if (post.participants?.length > 0) {
        participantsContainer.innerHTML = post.participants.map(p => `
            <div class="participant-edit-item">
                <span class="participant-name">${p.userName || p.userEmail || 'Unknown'}</span>
                <span class="participant-status ${p.status.toLowerCase()}">${p.status}</span>
                <button type="button" class="btn-remove-participant" onclick="removeParticipant('${post.postId}','${p.participantId}')">Remove</button>
            </div>
        `).join('');
    } else {
        participantsContainer.innerHTML = '<div class="no-participants">No participants yet</div>';
    }

    document.getElementById('editPostModal').style.display = 'block';
}

async function loadCategoriesForEdit(post) {
    const container = document.getElementById('editPostCategories');

    try {
        const response = await fetch('/admin/categories');
        if (response.ok) {
            const categories = await response.json();
            const postCategoryIds = post.categories?.map(c => c.categoryId) || [];

            if (categories.length > 0) {
                container.innerHTML = categories.map(cat => `
                    <label class="category-checkbox">
                        <input type="checkbox" name="editCategories" value="${cat.categoryId}" ${postCategoryIds.includes(cat.categoryId) ? 'checked' : ''}>
                        <span>${cat.name}</span>
                    </label>
                `).join('');
            } else {
                container.innerHTML = '<div class="no-categories">No categories available</div>';
            }
        } else {
            container.innerHTML = '<div class="no-categories">Error loading categories</div>';
        }
    } catch (error) {
        console.error('Error loading categories:', error);
        container.innerHTML = '<div class="no-categories">Error loading categories</div>';
    }
}

function closeEditPostModal() {
    document.getElementById('editPostModal').style.display = 'none';
    state.editingPostData = null;
    document.getElementById('editPostForm').reset();
}

function removeParticipant(postId, participantId) {
    if (!confirm('Are you sure you want to remove this participant?')) return;
    const token = getCsrfToken();
    if (!token) {
        alert('Security token not found. Please refresh the page.');
        return;
    }

    fetch(`/admin/posts/${postId}/participants/${participantId}`, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token }
    })
    .then(response => {
        if (response.ok) {
            openPostDetailModal(postId).then(() => {
                fetch(`/admin/posts/${postId}`)
                    .then(res => res.json())
                    .then(post => openEditPostModal(post));
            });
            alert('Participant removed successfully!');
        } else {
            response.text().then(text => alert('Error: ' + text));
        }
    })
    .catch(error => console.error('Remove participant error:', error));
}

function savePostEdit() {
    if (!state.editingPostData) {
        alert('No post selected for editing');
        return;
    }

    const postId = state.editingPostData.postId;
    const minParticipants = parseInt(document.getElementById('editPostMinParticipants').value) || 1;
    const maxParticipants = parseInt(document.getElementById('editPostMaxParticipants').value) || 10;

    const categoryCheckboxes = document.querySelectorAll('input[name="editCategories"]:checked');
    const categoryIds = Array.from(categoryCheckboxes).map(cb => cb.value);

    if (minParticipants < 1) {
        alert('Minimum participants must be at least 1');
        return;
    }
    if (maxParticipants < minParticipants) {
        alert('Maximum participants must be >= minimum');
        return;
    }
    if (state.editingPostData.currentParticipants > maxParticipants) {
        alert(`Max participants cannot be less than current (${state.editingPostData.currentParticipants})`);
        return;
    }

    fetch(`/admin/posts/${postId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ minParticipants, maxParticipants, categoryIds })
    })
    .then(response => {
        if (response.ok) {
            closeEditPostModal();
            closePostDetailModal();
            loadPosts(state.currentPostPage, state.currentPostPageSize);
            alert('Post updated successfully!');
        } else {
            response.json().then(data => alert('Error: ' + (data.details || response.statusText)))
                .catch(() => alert('Error: ' + response.statusText));
        }
    })
    .catch(error => {
        console.error('Update post error:', error);
        alert('Error: ' + error.message);
    });
}

async function doPostSearch() {
    const query = document.getElementById('postSearchInput')?.value?.trim() || '';
    const statusFilter = document.getElementById('postStatusFilter')?.value || '';

    const url = query
        ? `/admin/posts?page=1&pageSize=${state.currentPostPageSize}&NameFilter=${encodeURIComponent(query)}${statusFilter ? '&StatusFilter=' + statusFilter : ''}`
        : `/admin/posts?page=1&pageSize=${state.currentPostPageSize}${statusFilter ? '&StatusFilter=' + statusFilter : ''}`;

    try {
        const response = await fetch(url);
        if (response.ok) {
            const result = await response.json();
            state.currentPostPage = 1;
            renderPosts(result.data);
            renderPagination(result.meta, 'postPaginationContainer', '.post-table', loadPosts);
        }
    } catch (error) {
        console.error('Search error:', error);
    }
}

// ==================== Event Handlers ====================

function initEventHandlers() {
    // Window click - close modals
    window.onclick = function(event) {
        const modals = [
            { id: 'banModal', close: closeBanModal },
            { id: 'categoryModal', close: closeCategoryModal },
            { id: 'editCategoryModal', close: closeEditCategoryModal },
            { id: 'editPostModal', close: closeEditPostModal },
            { id: 'postDetailModal', close: closePostDetailModal }
        ];

        modals.forEach(({ id, close }) => {
            const modal = document.getElementById(id);
            if (event.target === modal && close) close();
        });
    };

    // Window load
    window.onload = async function() {
        initTabs();
        await loadUsers(1, state.currentPageSize);

        // Search input listeners
        const searchInput = document.getElementById('searchInput');
        if (searchInput) searchInput.addEventListener('input', doSearch);

        const postSearchInput = document.getElementById('postSearchInput');
        if (postSearchInput) postSearchInput.addEventListener('input', doPostSearch);
    };
}

// Initialize
initEventHandlers();
