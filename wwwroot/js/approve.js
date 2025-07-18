document.addEventListener("DOMContentLoaded", () => {
    const filterButtons = document.querySelectorAll(".filter-btn");

    // Helper: Get all rows
    const getRows = () => Array.from(document.querySelectorAll(".approval-table tbody tr"));

    // Update counts for each filter
    function updateCounts() {
        const rows = getRows();
        const counts = { all: rows.length, Pending: 0, Approved: 0, Rejected: 0 };

        rows.forEach((row) => {
            const status = row.querySelector("td.status").textContent;
            counts[status] = (counts[status] || 0) + 1;
        });

        filterButtons.forEach((btn) => {
            const filter = btn.dataset.filter;
            const span = btn.querySelector(".count");
            span.textContent = filter === "all" ? counts.all : counts[filter] || 0;
        });
    }

    // Apply filter to rows
    function applyFilter(filter) {
        getRows().forEach((row) => {
            const status = row.querySelector("td.status").textContent;
            row.style.display = filter === "all" || status === filter ? "" : "none";
        });
    }

    // Wire up filter buttons
    filterButtons.forEach((btn) => {
        btn.addEventListener("click", () => {
            filterButtons.forEach((b) => b.classList.remove("active"));
            btn.classList.add("active");
            applyFilter(btn.dataset.filter);
        });
    });

    // Enhanced approval/reject with SweetAlert2
    document.querySelectorAll(".btn-approve").forEach((btn) => {
        btn.addEventListener("click", async () => {
            const row = btn.closest("tr");
            const projectName = row.querySelector("td:nth-child(2)").textContent;
            const company = row.querySelector("td:nth-child(3)").textContent;

            const { isConfirmed } = await Swal.fire({
                title: "Approve Project?",
                html: `
          <div style="text-align: left; margin: 10px 0;">
            <p><strong>Project:</strong> ${projectName}</p>
            <p><strong>Company:</strong> ${company}</p>
          </div>
          <p>Are you sure you want to approve this project?</p>
        `,
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "Yes, Approve",
                cancelButtonText: "Cancel",
                customClass: {
                    confirmButton: "swal-confirm",
                    cancelButton: "swal-cancel"
                }
            });

            if (isConfirmed) {
                finalize(row, "Approved", "status-approved");
                await Swal.fire({
                    title: "Approved!",
                    text: "The project has been approved.",
                    icon: "success",
                    timer: 1500,
                    showConfirmButton: false
                });
            }
        });
    });

    document.querySelectorAll(".btn-reject").forEach((btn) => {
        btn.addEventListener("click", async () => {
            const row = btn.closest("tr");
            const projectName = row.querySelector("td:nth-child(2)").textContent;
            const company = row.querySelector("td:nth-child(3)").textContent;

            const { value: reason, isConfirmed } = await Swal.fire({
                title: "Reject Project?",
                html: `
          <div style="text-align: left; margin: 10px 0;">
            <p><strong>Project:</strong> ${projectName}</p>
            <p><strong>Company:</strong> ${company}</p>
          </div>
          <p>Please provide a reason for rejection:</p>
        `,
                input: "textarea",
                inputPlaceholder: "Enter reason...",
                inputAttributes: {
                    "aria-label": "Type your rejection reason here"
                },
                showCancelButton: true,
                confirmButtonText: "Confirm Rejection",
                cancelButtonText: "Cancel",
                inputValidator: (value) => {
                    if (!value) {
                        return "You need to provide a reason!";
                    }
                }
            });

            if (isConfirmed) {
                finalize(row, "Rejected", "status-rejected");
                await Swal.fire({
                    title: "Rejected!",
                    text: "The project has been rejected.",
                    icon: "success",
                    timer: 1500,
                    showConfirmButton: false
                });
            }
        });
    });

    // Update UI after approval/rejection
    function finalize(row, newStatus, className) {
        const statusCell = row.querySelector("td.status");
        statusCell.textContent = newStatus;
        statusCell.className = `status ${className}`;

        // Disable action buttons
        row.querySelectorAll(".btn-approve, .btn-reject").forEach((b) => {
            b.disabled = true;
            b.style.opacity = "0.6";
            b.style.cursor = "not-allowed";
        });

        updateCounts();
        applyFilter(document.querySelector(".filter-btn.active").dataset.filter);
    }

    // Initial setup
    updateCounts();
    applyFilter("all");
});