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

wavesurfer.load(document.querySelector('#song'))

// PLAY PAUSE
function playAudio() {
    wavesurfer.playPause();
    updateMetadata();
}

// PLAY
$('body').on('click', '#playpause', function () {
    playAudio();
});

navigator.mediaSession.setActionHandler('play', function () {
    wavesurfer.play();
});

navigator.mediaSession.setActionHandler('pause', function () {
    wavesurfer.playPause();
});

wavesurfer.on('play', function () {
    $('#playpause').html('<i class="fa fa-pause"></i>');
});

wavesurfer.on('pause', function () {
    $('#playpause').html('<i class="fa fa-play"></i>');
});

wavesurfer.on('finish', function () {
    wavesurfer.stop();
});
