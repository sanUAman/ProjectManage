window.onload = function () {
    setTimeout(() => {
        document.getElementById("intro").style.transform = "translateY(-100%)";

        document.querySelectorAll(".role-button").forEach(button => {
            button.style.opacity = "1";
            button.style.transform = "none";
        });
    }, 100);

    AOS.init({
        once: true
    });
};