const sidebar = document.getElementById("sidebar");
const menuBtn = document.getElementById("menuBtn");
const collapseBtn = document.getElementById("collapseBtn");
const managementMenu = document.getElementById("managementMenu");
const submenuHeader = managementMenu.querySelector(".submenu-header");

// toggle sidebar open/closed
menuBtn.addEventListener("click", () => {
    sidebar.classList.toggle("collapsed");
    managementMenu.classList.remove("open");
});
collapseBtn.addEventListener("click", () => {
    sidebar.classList.toggle("collapsed");
    managementMenu.classList.remove("open");
});

// toggle Management submenu
submenuHeader.addEventListener("click", (e) => {
    e.stopPropagation();
    managementMenu.classList.toggle("open");

});

