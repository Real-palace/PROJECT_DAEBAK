document.addEventListener("DOMContentLoaded", function () {
    const dropdown = document.getElementById("management-dropdown");
    const button = document.getElementById("management-btn");

    let timeout;

    function showDropdown() {
        clearTimeout(timeout);
        dropdown.classList.remove("hidden");
    }

    function hideDropdown() {
        timeout = setTimeout(() => {
            dropdown.classList.add("hidden");
        }, 300); // Delays hiding, making it easier to navigate
    }

    button.addEventListener("mouseenter", showDropdown);
    dropdown.addEventListener("mouseenter", showDropdown);
    button.addEventListener("mouseleave", hideDropdown);
    dropdown.addEventListener("mouseleave", hideDropdown);
});
