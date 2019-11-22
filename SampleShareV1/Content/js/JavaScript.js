// Creating the waveform with the following options
var wavesurfer = WaveSurfer.create({
    container: '#waveform',
    backend: 'MediaElement',
    progressColor: '#007bff',
    cursorColor: '#fff',
    hideScrollbar: true,
    responsive: true,
});

//function CreateWavesurfer(url) {
//    var wavesurfer = WaveSurfer.create({
//        container: '#waveform',
//        backend: 'MediaElement',
//        progressColor: '#007bff',
//        cursorColor: '#fff',
//        hideScrollbar: true,
//        responsive: true,
//    });

   // wavesurfer.load(document.querySelector(url))

//}

//CreateWavesurfer('#song');
//CreateWavesurfer('#song');
//CreateWavesurfer('#song');
//CreateWavesurfer('#song');

//Loading waveform via ID tag from HTML
wavesurfer.load(document.querySelector('#song'))

// PLAY / PAUSE Function
function playAudio() {
    wavesurfer.playPause();
    updateMetadata();
}

// PLAY
$('body').on('click', '#playpause', function () {
    playAudio();
});

// Play Waveform
navigator.mediaSession.setActionHandler('play', function () {
    wavesurfer.play();
});

// Pause Waveform
navigator.mediaSession.setActionHandler('pause', function () {
    wavesurfer.playPause();
});

// If audio is playing then replace icon to pause 
wavesurfer.on('play', function () {
    $('#playpause').html('<i class="fa fa-pause"></i>');
});

// If audio is not playing then replace icon to Play
wavesurfer.on('pause', function () {
    $('#playpause').html('<i class="fa fa-play"></i>');
});

// When audio finish playing then reset waveform
wavesurfer.on('finish', function () {
    wavesurfer.stop();
});