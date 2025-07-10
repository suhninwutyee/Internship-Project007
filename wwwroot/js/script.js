// script.js
document.addEventListener("DOMContentLoaded", () => {
    // trigger hero fade-in
    document.body.classList.add("loaded");

    // setup scroll animations
    const observer = new IntersectionObserver(
        (entries, obs) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("visible");
                    obs.unobserve(entry.target);
                }
            });
        },
        { threshold: 0.1 }
    );

    document.querySelectorAll(".fade-up").forEach((el) => {
        observer.observe(el);
    });
});
