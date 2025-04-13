window.getDimensions = function () {
    return {
        windowWidth: window.innerWidth,
        windowHeight: window.innerHeight,
        tooltipHeight: document.querySelector('.tooltip') ? document.querySelector('.tooltip').offsetHeight : 200
    };
};