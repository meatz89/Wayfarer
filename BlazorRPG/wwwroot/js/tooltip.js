window.getDimensions = () => {
    return {
        windowHeight: window.innerHeight,
        tooltipHeight: document.querySelector('.tooltip')?.offsetHeight || 0
    };
};