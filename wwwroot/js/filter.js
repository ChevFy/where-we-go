const categoryChips = document.querySelectorAll('.category-chip');
categoryChips.forEach(chip => {
    chip.addEventListener('click', () => {
        const checkbox = chip.querySelector('input[type="checkbox"]');
        if (checkbox.checked) {
            checkbox.checked = false;
            chip.classList.remove('selected');
        } else {
            checkbox.checked = true;
            chip.classList.add('selected');
        }
    });
});