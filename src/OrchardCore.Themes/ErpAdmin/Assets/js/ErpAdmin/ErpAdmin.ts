///<reference path="@types/bootstrap/index.d.ts" />

function confirmDialog({ callback, ...options }: { callback: (response: boolean) => void; [key: string]: any }) {
    const defaultOptions = $("#confirmRemoveModalMetadata").data();
    const { title, message, okText, cancelText, okClass, cancelClass } = $.extend({}, defaultOptions, options);

    $(
        '<div id="confirmRemoveModal" class="modal" tabindex="-1" role="dialog">\
        <div class="modal-dialog modal-dialog-centered" role="document">\
            <div class="modal-content">\
                <div class="modal-header">\
                    <h5 class="modal-title">' +
        title +
        '</h5>\
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>\
                </div>\
                <div class="modal-body">\
                    <p>' +
        message +
        '</p>\
                </div>\
                <div class="modal-footer">\
                    <button id="modalOkButton" type="button" class="btn ' +
        okClass +
        '">' +
        okText +
        '</button>\
                    <button id="modalCancelButton" type="button" class="btn ' +
        cancelClass +
        '" data-bs-dismiss="modal">' +
        cancelText +
        "</button>\
                </div>\
            </div>\
        </div>\
    </div>",
    ).appendTo("body");

    const modalElement = document.getElementById("confirmRemoveModal");

    if (modalElement) {
        const confirmModal = new bootstrap.Modal(modalElement, {
            backdrop: "static",
            keyboard: false,
        });

        confirmModal.show();

        document.getElementById("confirmRemoveModal")?.addEventListener("hidden.bs.modal", function () {
            document.getElementById("confirmRemoveModal")?.remove();
            confirmModal.dispose();
        });

        $("#modalOkButton").click(function () {
            callback(true);
            confirmModal.hide();
        });

        $("#modalCancelButton").click(function () {
            callback(false);
            confirmModal.hide();
        });
    }
}

(function () {
    // Prevents page flickering while downloading css
    document.addEventListener("DOMContentLoaded", () => {
        document.body.classList.remove("preload");
    });
})();

$(function () {
    $("body").on("click", '[data-url-af~="RemoveUrl"], a[itemprop~="RemoveUrl"]', function () {
        const _this = $(this);
        if (_this.filter('a[itemprop~="UnsafeUrl"]').length == 1) {
            console.warn("Please use data-url-af instead of itemprop attribute for confirm modals.");
        }
        if (_this.filter('[data-url-af~="UnsafeUrl"], a[itemprop~="UnsafeUrl"]').length == 1) {
            return false;
        }
        confirmDialog({
            ..._this.data(),
            callback: function (resp: any) {
                if (resp) {
                    const url = _this.attr("href");
                    if (url == undefined) {
                        let form = _this.parents("form");
                        form.append($('<input type="hidden" name="' + _this.attr("name") + '" value="' + _this.attr("value") + '" />'));
                        form.submit();
                    } else {
                        window.location.href = url;
                    }
                }
            },
        });
        return false;
    });
});

$(function () {
    const magicToken = $("input[name=__RequestVerificationToken]").first();
    if (magicToken) {
        $("body").on("click", 'a[data-url-af~="UnsafeUrl"], a[itemprop~="UnsafeUrl"]', function () {
            const _this = $(this);
            const hrefParts = _this.attr("href")?.split("?");

            if (hrefParts == undefined) {
                return false;
            }

            let form = $('<form action="' + hrefParts[0] + '" method="POST" />');
            form.append(magicToken.clone());
            if (hrefParts.length > 1) {
                const queryParts = hrefParts[1].split("&");
                for (let i = 0; i < queryParts.length; i++) {
                    const queryPartKVP = queryParts[i].split("=");
                    form.append($('<input type="hidden" name="' + decodeURIComponent(queryPartKVP[0]) + '" value="' + decodeURIComponent(queryPartKVP[1]) + '" />'));
                }
            }

            form.css({ position: "absolute", left: "-9999em" });
            $("body").append(form);

            const unsafeUrlPrompt = _this.data("unsafe-url");

            if (unsafeUrlPrompt && unsafeUrlPrompt.length > 0) {
                confirmDialog({
                    ..._this.data(),
                    callback: function (resp: any) {
                        if (resp) {
                            form.submit();
                        }
                    },
                });
                return false;
            }

            if (_this.filter('[data-url-af~="RemoveUrl"], a[itemprop~="RemoveUrl"]').length == 1) {
                confirmDialog({
                    ..._this.data(),
                    callback: function (resp: any) {
                        if (resp) {
                            form.submit();
                        }
                    },
                });
                return false;
            }

            form.submit();
            return false;
        });
    }
});

(function () {
    // Tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
})();

// Prevent multi submissions on forms
$("body").on("submit", "form.no-multisubmit", function (e) {
    const submittingClass = "submitting";
    const form = $(this);

    if (form.hasClass(submittingClass)) {
        e.preventDefault();
        return;
    }

    form.addClass(submittingClass);
    setTimeout(function () {
        form.removeClass(submittingClass);
    }, 5000);
});

// ===== ErpAdmin-specific interactions =====

// Sidebar toggle
function toggleSidebar() {
    const sidebar = document.getElementById("erp-sidebar");
    if (!sidebar) return;
    sidebar.classList.toggle("collapsed");
    const btn = sidebar.querySelector(".sidebar-toggle-btn i");
    if (btn) {
        btn.classList.toggle("bi-chevron-double-left");
        btn.classList.toggle("bi-chevron-double-right");
    }
    // Persist preference
    const isCollapsed = sidebar.classList.contains("collapsed");
    try {
        const prefs = JSON.parse(localStorage.getItem("erpadmin_prefs") || "{}");
        prefs.sidebarCollapsed = isCollapsed;
        localStorage.setItem("erpadmin_prefs", JSON.stringify(prefs));
    } catch (e) { /* ignore */ }
}

// Mobile sidebar
function openMobileSidebar() {
    const sidebar = document.getElementById("erp-sidebar");
    const overlay = document.getElementById("erp-sidebar-overlay");
    if (sidebar) sidebar.classList.add("mobile-open");
    if (overlay) overlay.classList.add("show");
}

function closeMobileSidebar() {
    const sidebar = document.getElementById("erp-sidebar");
    const overlay = document.getElementById("erp-sidebar-overlay");
    if (sidebar) sidebar.classList.remove("mobile-open");
    if (overlay) overlay.classList.remove("show");
}

// Theme toggle
function toggleErpTheme() {
    const html = document.documentElement;
    const current = html.getAttribute("data-bs-theme");
    const next = current === "dark" ? "light" : "dark";
    html.setAttribute("data-bs-theme", next);
    const icon = document.querySelector(".erp-theme-toggle-btn i");
    if (icon) {
        icon.classList.toggle("bi-moon-stars", next === "dark");
        icon.classList.toggle("bi-sun", next === "light");
    }
}

// User dropdown
function toggleUserDropdown() {
    const dropdown = document.getElementById("erp-user-dropdown");
    if (dropdown) dropdown.classList.toggle("open");
}

// Filter panel
function toggleFilterPanel() {
    const panel = document.getElementById("erp-filter-panel");
    const overlay = document.getElementById("erp-filter-overlay");
    if (panel) panel.classList.toggle("open");
    if (overlay) overlay.classList.toggle("open");
}

// Close user dropdown on outside click
document.addEventListener("click", function (e) {
    const userMenu = (e.target as HTMLElement).closest(".topbar-user");
    if (!userMenu) {
        const dropdown = document.getElementById("erp-user-dropdown");
        if (dropdown) dropdown.classList.remove("open");
    }
});

// Restore sidebar state on load
document.addEventListener("DOMContentLoaded", function () {
    try {
        const prefs = JSON.parse(localStorage.getItem("erpadmin_prefs") || "{}");
        if (prefs.sidebarCollapsed) {
            const sidebar = document.getElementById("erp-sidebar");
            if (sidebar) {
                sidebar.classList.add("collapsed");
                const btn = sidebar.querySelector(".sidebar-toggle-btn i");
                if (btn) {
                    btn.classList.remove("bi-chevron-double-left");
                    btn.classList.add("bi-chevron-double-right");
                }
            }
        }
    } catch (e) { /* ignore */ }
});

// Expose globally
(window as any).toggleSidebar = toggleSidebar;
(window as any).openMobileSidebar = openMobileSidebar;
(window as any).closeMobileSidebar = closeMobileSidebar;
(window as any).toggleErpTheme = toggleErpTheme;
(window as any).toggleUserDropdown = toggleUserDropdown;
(window as any).toggleFilterPanel = toggleFilterPanel;

declare global {
    interface Window {
        confirmDialog: typeof confirmDialog;
        toggleSidebar: typeof toggleSidebar;
        openMobileSidebar: typeof openMobileSidebar;
        closeMobileSidebar: typeof closeMobileSidebar;
        toggleErpTheme: typeof toggleErpTheme;
        toggleUserDropdown: typeof toggleUserDropdown;
        toggleFilterPanel: typeof toggleFilterPanel;
    }
}

window.confirmDialog = confirmDialog;

export { confirmDialog, toggleSidebar, openMobileSidebar, closeMobileSidebar, toggleErpTheme, toggleUserDropdown, toggleFilterPanel };
