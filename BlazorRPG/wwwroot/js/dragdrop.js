// wwwroot/js/dragdrop.js
window.setDragImage = function (e, cardType) {
    var img = new Image();
    img.src = `/images/cards/${cardType.toLowerCase()}_card.png`;
    e.dataTransfer.setDragImage(img, 50, 75);
};

// Card drag and drop functionality
window.initializeDraggable = function (elementId, cardId) {
    const element = document.getElementById(elementId);
    if (!element) return;

    element.setAttribute('draggable', 'true');

    element.addEventListener('dragstart', function (e) {
        e.dataTransfer.setData('text/plain', cardId);
        e.dataTransfer.effectAllowed = 'move';
        element.classList.add('dragging');
        setTimeout(() => {
            window.cardDraggedId = cardId;
            if (window.cardDragStartCallback) {
                window.cardDragStartCallback.invokeMethodAsync('HandleDragStartFromJS', cardId);
            }
        }, 10);
    });

    element.addEventListener('dragend', function () {
        element.classList.remove('dragging');
        setTimeout(() => {
            if (window.cardDragEndCallback) {
                window.cardDragEndCallback.invokeMethodAsync('HandleDragEndFromJS');
            }
            window.cardDraggedId = null;
        }, 10);
    });
};

window.registerDragCallbacks = function (dotNetHelper) {
    window.cardDragStartCallback = dotNetHelper;
    window.cardDragEndCallback = dotNetHelper;
};