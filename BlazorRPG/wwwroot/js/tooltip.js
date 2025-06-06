export function getTooltipPositionRelativeToElement(elementId) {
    const element = document.getElementById(elementId);

    if (!element) return { tooltipX: 0, tooltipY: 0 };

    return {
        tooltipX: element.clientLeft + element.clientWidth + 50,
        tooltipY: element.clientTop + element.clientHeight + 350
    };
}
