window.scrollSectionIntoView = function (sectionName) {
    const sectionHeader = document.getElementById('section-' + sectionName);
    if (sectionHeader) {
        // Find the parent sidebar
        const sidebar = sectionHeader.closest('.game-sidebar');
        if (sidebar) {
            // Calculate how far to scroll - find the expanded content
            const sectionContent = sectionHeader.nextElementSibling;
            if (sectionContent) {
                // Calculate where the bottom of the expanded section will be
                const expandedBottom = sectionHeader.offsetTop + sectionHeader.offsetHeight + sectionContent.scrollHeight;

                // Determine how much to scroll to see the entire expanded section
                const scrollAmount = Math.min(
                    expandedBottom - (sidebar.scrollTop + sidebar.clientHeight) + 20, // +20 for padding
                    sidebar.scrollHeight - sidebar.clientHeight
                );

                // Only scroll if necessary
                if (scrollAmount > 0) {
                    // Smooth scroll animation
                    sidebar.scrollBy({
                        top: scrollAmount,
                        behavior: 'smooth'
                    });
                }
            }
        }
    }
};