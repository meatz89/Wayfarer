window.musicPlayer = {
    audioElement: null,
    play: function (filePath) {
        if (!this.audioElement) {
            this.audioElement = new Audio();
        }
        if (this.audioElement.src !== filePath) {
            this.audioElement.src = filePath;
        }
        this.audioElement.play();
    },
    pause: function () {
        if (this.audioElement) {
            this.audioElement.pause();
        }
    },
    stop: function () {
        if (this.audioElement) {
            this.audioElement.pause();
            this.audioElement.currentTime = 0;
        }
    },
    setPosition: function (seconds) {
        if (this.audioElement) {
            this.audioElement.currentTime = seconds;
        }
    }
};
