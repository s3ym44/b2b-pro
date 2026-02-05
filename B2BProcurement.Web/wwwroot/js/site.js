// ========================================
// B2B Procurement - Admin Panel Scripts
// ========================================

document.addEventListener('DOMContentLoaded', function() {
    
    // ========== Sidebar Toggle ==========
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarToggleClose = document.getElementById('sidebarToggleClose');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    
    function toggleSidebar() {
        sidebar.classList.toggle('active');
        sidebarOverlay.classList.toggle('active');
        document.body.classList.toggle('sidebar-open');
    }
    
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', toggleSidebar);
    }
    
    if (sidebarToggleClose) {
        sidebarToggleClose.addEventListener('click', toggleSidebar);
    }
    
    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', toggleSidebar);
    }
    
    // ========== Submenu Toggle ==========
    const submenuLinks = document.querySelectorAll('.sidebar .nav-link[data-bs-toggle="collapse"]');
    
    submenuLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            // Collapse other open submenus
            const currentSubmenu = this.getAttribute('href');
            submenuLinks.forEach(otherLink => {
                const otherSubmenu = otherLink.getAttribute('href');
                if (otherSubmenu !== currentSubmenu) {
                    const submenu = document.querySelector(otherSubmenu);
                    if (submenu && submenu.classList.contains('show')) {
                        bootstrap.Collapse.getInstance(submenu)?.hide();
                    }
                }
            });
        });
    });
    
    // ========== Active Menu Item ==========
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.sidebar .nav-link, .sidebar .submenu a');
    
    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath === href.toLowerCase()) {
            link.classList.add('active');
            
            // If it's a submenu item, open parent
            const parentSubmenu = link.closest('.submenu');
            if (parentSubmenu) {
                parentSubmenu.classList.add('show');
                const parentToggle = document.querySelector(`[href="#${parentSubmenu.id}"]`);
                if (parentToggle) {
                    parentToggle.setAttribute('aria-expanded', 'true');
                }
            }
        }
    });
    
    // ========== Tooltips ==========
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(el => {
        new bootstrap.Tooltip(el);
    });
    
    // ========== Notifications Mark as Read ==========
    const markAllRead = document.querySelector('.notification-header a');
    if (markAllRead) {
        markAllRead.addEventListener('click', function(e) {
            e.preventDefault();
            const unreadItems = document.querySelectorAll('.notification-item.unread');
            unreadItems.forEach(item => {
                item.classList.remove('unread');
            });
            const badge = document.querySelector('.notification-badge');
            if (badge) {
                badge.style.display = 'none';
            }
        });
    }
    
    // ========== Card Animations ==========
    const cards = document.querySelectorAll('.card, .stat-card');
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);
    
    cards.forEach(card => observer.observe(card));
    
    // ========== Form Validation Styles ==========
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
    
    // ========== Search Functionality ==========
    const searchInput = document.querySelector('.search-box input');
    if (searchInput) {
        let debounceTimer;
        searchInput.addEventListener('input', function() {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                const query = this.value.trim();
                if (query.length >= 2) {
                    console.log('Searching for:', query);
                    // TODO: Implement search API call
                }
            }, 300);
        });
    }
    
    // ========== Responsive Table Wrapper ==========
    const tables = document.querySelectorAll('.table');
    tables.forEach(table => {
        if (!table.closest('.table-responsive')) {
            const wrapper = document.createElement('div');
            wrapper.className = 'table-responsive';
            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
        }
    });
    
    // ========== Copy to Clipboard ==========
    window.copyToClipboard = function(text, btn) {
        navigator.clipboard.writeText(text).then(() => {
            const originalText = btn.innerHTML;
            btn.innerHTML = '<i class="fas fa-check"></i> Kopyalandı!';
            setTimeout(() => {
                btn.innerHTML = originalText;
            }, 2000);
        });
    };
    
    // ========== Confirm Delete ==========
    window.confirmDelete = function(message = 'Bu kaydı silmek istediğinizden emin misiniz?') {
        return confirm(message);
    };
    
    // ========== Print Page ==========
    window.printPage = function() {
        window.print();
    };
    
    console.log('B2B Procurement Admin Panel initialized');
});
